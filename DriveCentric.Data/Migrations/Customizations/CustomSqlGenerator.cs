using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Utilities;
using System.Data.Entity.SqlServer;

namespace DriveCentric.Data.Migrations.Customizations
{
    public class CustomSqlGenerator : SqlServerMigrationSqlGenerator
    {
        protected override void Generate(MigrationOperation migrationOperation)
        {
            if (migrationOperation is CreateViewOperation)
            {
                var operation = (CreateViewOperation)migrationOperation;

                using (IndentedTextWriter writer = Writer())
                {
                    writer.WriteLine("CREATE VIEW {0} AS {1} ; ",
                                      operation.ViewName,
                                      operation.ViewString);
                    Statement(writer);
                }
            }
            else if (migrationOperation is DropViewOperation)
            {
                var operation = (DropViewOperation)migrationOperation;

                using (IndentedTextWriter writer = Writer())
                {
                    writer.WriteLine("DROP VIEW {0} ; ", operation.ViewName);
                    Statement(writer);
                }
            }
            else if (migrationOperation is CreateSequenceOperation)
            {
                var operation = (CreateSequenceOperation)migrationOperation;

                using (IndentedTextWriter writer = Writer())
                {
                    writer.WriteLine("CREATE SEQUENCE {0} AS {1} START WITH {2} INCREMENT BY {3} MINVALUE 0 CACHE ; ",
                        operation.SequenceName, operation.DataType, operation.StartValue, operation.Increment);
                    Statement(writer);
                }
            }
            else if (migrationOperation is DropSequenceOperation)
            {
                var operation = (DropSequenceOperation)migrationOperation;

                using (IndentedTextWriter writer = Writer())
                {
                    writer.WriteLine("DROP SEQUENCE {0} ; ", operation.SequenceName);
                    Statement(writer);
                }
            }

        }
    }
}
