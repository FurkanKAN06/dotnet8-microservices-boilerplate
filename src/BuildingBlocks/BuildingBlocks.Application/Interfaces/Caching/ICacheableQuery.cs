namespace BuildingBlocks.Application.Interfaces.Caching
{
    public interface ICacheableQuery
    {
        string CacheKey { get; }
        bool BypassCache { get; }
        System.TimeSpan? SlidingExpiration { get; }
    }
}
