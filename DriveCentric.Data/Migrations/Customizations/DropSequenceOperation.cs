using System.Data.Entity.Migrations.Model;

namespace DriveCentric.Data.Migrations.Customizations
{
    public class DropSequenceOperation : MigrationOperation
    {
        public DropSequenceOperation(string sequenceName)
          : base(null)
        {
            SequenceName = sequenceName;
        }

        public string SequenceName { get; private set; }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
