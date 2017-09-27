using DriveCentric.Data.Conventions;
using DriveCentric.Logger;
using DriveCentric.Shared.Exceptions;
using DriveCentric.Shared.Models;
using DriveCentric.Shared.Models.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace DriveCentric.Data
{
    [DbConfigurationType(typeof(CustomDbConfiguration))]
    public class DomainContext : DbContext
    {
        private const string DOMAIN_CONTEXT_CONNECTION_KEY = "DomainContext";
        private readonly ILogger logger = LoggerManager.GetLogger(typeof(DomainContext));
        #region Public Properties

        #region Security Entities
        public DbSet<User> Users { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserClaim> UserClaims { get; set; }
        #endregion

        public DbSet<Customer> Customers { get; set; }

        public Action<DbContext> CustomSaveChangesLogic { get; set; }
        #endregion

        #region Public Constructors
        public DomainContext() : this(null)
        { 
        }
        public DomainContext(Action<DbContext> customSaveChangesLogic = null) : base("name=DomainContext")
        { 
            CustomSaveChangesLogic = customSaveChangesLogic;
            this.Configuration.LazyLoadingEnabled = false; 
            this.Configuration.ProxyCreationEnabled = false;
        }
        #endregion Public Constructors

        #region Method Overrides  
        
        public override int SaveChanges()
        {
            try
            {
                CustomSaveChangesLogic?.Invoke(this);

                return base.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                if (ex?.InnerException?.InnerException != null && ex.InnerException.InnerException is SqlException)
                {
                    var sqlException = ex.InnerException.InnerException as SqlException;
                    if (sqlException.Number == 2627 || sqlException.Number == 2601)
                    {
                        logger.LogInformation(ex);
                        throw new DatabaseConstraintException();
                    }
                    else if (sqlException.Number == 547)
                    {
                        logger.LogInformation(ex);
                        throw new DatabaseConstraintDeleteException();
                    }
                    else
                    {
                        logger.LogError(ex);
                        throw ex;
                    }
                }
                else
                {
                    logger.LogError(ex);
                    throw ex;
                }
            }
            catch
            {
                throw;
            }
        }

        private void ProcessDBExceptions(Exception ex)
        {
            var updateException = ex as DbUpdateException;
            if (updateException != null && updateException?.InnerException?.InnerException != null && updateException.InnerException.InnerException is SqlException)
            {
                var sqlException = updateException.InnerException.InnerException as SqlException;

                var sqlConstraintErrorNumbers = new List<int>(new int[] { 2627, 547, 2601 });
                if (sqlConstraintErrorNumbers.Contains(sqlException.Number))
                {
                    logger.LogInformation(ex);
                    throw new DatabaseConstraintException();
                }
            }
            else
            {
                logger.LogError(ex);
                throw ex;
            }

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new DecimalPrecisionAttributeConvention());
            modelBuilder.Conventions.Add(new StringLengthRequiredConvention());
            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
        }

        
        #endregion
    }
}
