namespace Qonote.Infrastructure.Infrastructure.Caching;

public sealed class CacheOptions
{
    public bool Enabled { get; set; } = true;
    public RedisOptions Redis { get; set; } = new();
    public SectionOptions Sidebar { get; set; } = new();
    public SectionOptions Me { get; set; } = new();

    public sealed class RedisOptions
    {
        public string? ConnectionString { get; set; }
    }

    public sealed class SectionOptions
    {
        public bool Enabled { get; set; } = true;
        public int TtlSeconds { get; set; } = 300;
    }
}
