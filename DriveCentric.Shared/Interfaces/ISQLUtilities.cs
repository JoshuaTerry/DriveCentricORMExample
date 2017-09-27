using DriveCentric.Shared.Enums;
using System;

namespace DriveCentric.Shared.Interfaces
{
    public interface ISQLUtilities
    {
        void SetNextSequenceValue(DatabaseSequence sequence, Int64 newValue);
        Int64 GetNextSequenceValue(DatabaseSequence sequence);
        int ExecuteSQL(string sql, params object[] parameters);
    }
}
