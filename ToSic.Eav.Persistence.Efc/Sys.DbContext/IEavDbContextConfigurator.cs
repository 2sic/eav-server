namespace ToSic.Eav.Persistence.Efc.Sys.DbContext;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IEavDbContextConfigurator
{
    void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString);

    string RewriteName(string name);
}

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class SqlServerEavDbContextConfigurator : IEavDbContextConfigurator
{
    public void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
#if NETFRAMEWORK
        optionsBuilder.UseSqlServer(
            connectionString,
            options => options.CommandTimeout(90));
#else
        optionsBuilder.UseSqlServer(
            connectionString,
            options => options
                .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                .CommandTimeout(180));
#endif
    }

    public string RewriteName(string name) => name;
}
