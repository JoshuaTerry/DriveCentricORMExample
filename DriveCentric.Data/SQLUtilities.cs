using DriveCentric.Shared.Enums;
using DriveCentric.Shared.Interfaces;
using System;
using System.Data.Entity;
using System.Linq;

namespace DriveCentric.Data
{
    public class SQLUtilities : ISQLUtilities
    {
        private DbContext context = null;

        public SQLUtilities(DbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Set the next value for a database sequence.
        /// </summary>
        public void SetNextSequenceValue(DatabaseSequence sequence, Int64 newValue)
        {
            context.Database.ExecuteSqlCommand($"ALTER SEQUENCE {GetSequenceName(sequence)} RESTART WITH {newValue};");
        }

        /// <summary>
        /// Get the next value for a database sequence.
        /// </summary>
        public Int64 GetNextSequenceValue(DatabaseSequence sequence)
        {
            return context.Database.SqlQuery<Int64>($"SELECT NEXT VALUE FOR {GetSequenceName(sequence)};").FirstOrDefault();
        }

        /// <summary>
        /// Execute a SQL statement.
        /// </summary>
        /// <param name="sql">SQL statement.</param>
        /// <param name="parameters">Optional parameters.</param>
        public int ExecuteSQL(string sql, params object[] parameters)
        {
            return context.Database.ExecuteSqlCommand(sql, parameters);
        }

        private string GetSequenceName(DatabaseSequence sequence)
        {
            switch (sequence)
            {
                case DatabaseSequence.TransactionNumber: return Sequences.TransactionNumber;
            }

            throw new ArgumentException("Invalid database sequence", nameof(sequence));
        }
    }
}
