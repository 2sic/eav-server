using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
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
            //options.UseModel(EavDbContextModel.Instance);

            return new EavDbContext(optionsBuilder.Options, new DbConfiguration());
        }
    }

    class DbConfiguration : IDbConfiguration
    {
        public string ConnectionString { get; set; } = "Data Source=(local);Initial Catalog=2sxc-dnn9134-t05;Integrated Security=True;TrustServerCertificate=true;";
    }
}
