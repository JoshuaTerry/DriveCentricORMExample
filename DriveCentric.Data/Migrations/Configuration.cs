using DriveCentric.Shared.Models;
using System;
using System.Data.Entity.Migrations;

namespace DriveCentric.Data.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<DriveCentric.Data.DomainContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            
        }

        protected override void Seed(DriveCentric.Data.DomainContext context)
        {
            context.Customers.Add(new Customer() { FirstName = "Joshua", LastName = "Terry", Email = "jterry@drivecentric.com", Id = Guid.NewGuid(), DateOfBirth = DateTime.Now.AddDays(-1) });
            context.Customers.Add(new Customer() { FirstName = "Bob", LastName = "Alouie", Email = "bob@drivecentric.com", Id = Guid.NewGuid(), DateOfBirth = DateTime.Now });
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
