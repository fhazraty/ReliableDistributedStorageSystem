using FullNodeDataLogger.EF.Model;
using Microsoft.EntityFrameworkCore;

namespace FullNodeDataLogger.EF
{
    //The following packages is installed to use entity framework on SQL Server
    //Install-Package Microsoft.EntityFrameworkCore.SqlServer
    //Install-Package Microsoft.EntityFrameworkCore.Tools
    //Install-Package Microsoft.EntityFrameworkCore.Design

    //To convert model to db context :
    //Add-Migration InitialCreate
    
    //To update the database context with ef context:
    //Update-Database

    //To retrieve sql code :
    //Script-Migration

    public class FullNodeDataLoggerEntities : DbContext
    {
        public virtual DbSet<Log> Logs { get; set; }
        public FullNodeDataLoggerEntities()
        {
            Database.SetCommandTimeout(120);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(@"Data Source=.;Initial Catalog=FullNodeDataLogger;Integrated Security=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}