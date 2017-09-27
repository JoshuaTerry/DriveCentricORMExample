using System.Data.Entity.Migrations.Model;

namespace DriveCentric.Data.Migrations.Customizations
{
    public class CreateSequenceOperation : MigrationOperation
    {
        public CreateSequenceOperation(string sequenceName, string dataType, int startValue, int increment) : base(null)
        {
            SequenceName = sequenceName;
            DataType = dataType;
            StartValue = startValue;
            Increment = increment;
        }

        public string SequenceName { get; private set; }
        public string DataType { get; private set; }
        public int StartValue { get; set; }
        public int Increment { get; set; }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
