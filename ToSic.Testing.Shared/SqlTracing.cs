namespace ToSic.Testing.Shared
{
    internal class SqlTracing
    {
        // this helps debug in advanced scenarios
        // hasn't been used since ca. 2017, but keep in case we ever do deep work on the DB again
        // ReSharper disable once UnusedMember.Global
        //private static void AddSqlLogger()
        //{
        //    // try to add logger
        //    var db = Factory.Resolve<EavDbContext>();
        //    var serviceProvider = db.GetInfrastructure();
        //    var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        //    loggerFactory.AddProvider(new EfCoreLoggerProvider());
        //}

    }
}
