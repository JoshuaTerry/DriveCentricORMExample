using System.Data.Entity.Migrations.Model;

namespace DriveCentric.Data.Migrations.Customizations
{
    public class CreateViewOperation : MigrationOperation
    {
        public CreateViewOperation(string viewName, string viewQueryString) : base(null)
        {
            ViewName = viewName;
            ViewString = viewQueryString;
        }

        public string ViewName { get; private set; }
        public string ViewString { get; private set; }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
