using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Application.Interfaces.Caching;

namespace BuildingBlocks.Application.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>, ICacheableQuery
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

        public CachingBehavior(IDistributedCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request.BypassCache)
            {
                return await next();
            }

            byte[]? cachedBytes = await _cache.GetAsync(request.CacheKey, cancellationToken);
            if (cachedBytes != null)
            {
                _logger.LogInformation($"Fetched from Cache -> '{request.CacheKey}'.");
                string cachedResponse = System.Text.Encoding.UTF8.GetString(cachedBytes);
                return JsonSerializer.Deserialize<TResponse>(cachedResponse)!;
            }

            TResponse response = await next();

            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = request.SlidingExpiration ?? System.TimeSpan.FromMinutes(10)
            };

            string serializedData = JsonSerializer.Serialize(response);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(serializedData);

            await _cache.SetAsync(request.CacheKey, bytes, options, cancellationToken);
            _logger.LogInformation($"Added to Cache -> '{request.CacheKey}'.");

            return response;
        }
    }
}
