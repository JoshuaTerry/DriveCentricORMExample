using System.Data.Entity.Migrations.Model;

namespace DriveCentric.Data.Migrations.Customizations
{
    public class DropViewOperation : MigrationOperation
    {
        public DropViewOperation(string viewName)
          : base(null)
        {
            ViewName = viewName;
        }

        public string ViewName { get; private set; }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
