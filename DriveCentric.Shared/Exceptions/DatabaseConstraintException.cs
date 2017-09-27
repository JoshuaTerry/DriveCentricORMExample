using System;

namespace DriveCentric.Shared.Exceptions
{
    public class DatabaseConstraintException : Exception
    {
        public DatabaseConstraintException() : base("The record you are trying to add or edit conflicts with an existing record.") { }
    }

    public class DatabaseConstraintDeleteException : Exception
    {
        public DatabaseConstraintDeleteException() : base("The record you are trying to delete is currently in use.") { }
    }

    public class DatabaseConcurrencyException : Exception
    {
        public DatabaseConcurrencyException() : base("A record you are updating was changed before you save.  Refresh to get latest version.") { }
    }
}
