using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BuildingBlocks.Presentation.Middleware
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string IdempotencyHeaderName = "X-Idempotency-Key";

        public IdempotencyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IDistributedCache cache)
        {
            if (context.Request.Method != HttpMethods.Post && 
                context.Request.Method != HttpMethods.Put && 
                context.Request.Method != HttpMethods.Patch)
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(IdempotencyHeaderName, out Microsoft.Extensions.Primitives.StringValues idempotencyKeys))
            {
                await _next(context);
                return;
            }

            string idempotencyKey = idempotencyKeys.ToString();
            string cacheKey = $"Idempotency_{idempotencyKey}";

            string? cachedResponse = await cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedResponse))
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync(cachedResponse);
                return;
            }

            System.IO.Stream originalBodyStream = context.Response.Body;
            using System.IO.MemoryStream responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                };
                await cache.SetStringAsync(cacheKey, responseText, options);
            }

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}
