using Microsoft.EntityFrameworkCore.Design;
using ToSic.Eav.Internal.Configuration;

namespace ToSic.Eav.Persistence.Efc.Models
{
    public class EavDbContextFactory : IDesignTimeDbContextFactory<EavDbContext>
    {
        public EavDbContext CreateDbContext(string[] args)
        {
            var dbConfiguration = new DbConfiguration();

            var optionsBuilder = new DbContextOptionsBuilder<EavDbContext>();
            optionsBuilder.UseSqlServer(dbConfiguration.ConnectionString);

            return new EavDbContext(optionsBuilder.Options, new DbConfiguration());
        }
    }

    class DbConfiguration : IDbConfiguration
    {
        public string ConnectionString { get; set; } = "Data Source=.;Initial Catalog=2sxc-dnn;User ID=test;Password=test";
    }
}
