using DriveCentric.Data.Migrations.Customizations;
using System.Data.Entity;

namespace DriveCentric.Data
{
    public class CustomDbConfiguration : DbConfiguration
    {
        public CustomDbConfiguration()
        {
            SetMigrationSqlGenerator("System.Data.SqlClient", () => new CustomSqlGenerator());
        }
    }
}
