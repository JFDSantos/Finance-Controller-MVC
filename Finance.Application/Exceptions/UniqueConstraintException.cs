namespace Finance.Application.Exceptions
{
    /// <summary>A database unique constraint was violated (raised by the unit of work).</summary>
    public class UniqueConstraintException : Exception
    {
        public UniqueConstraintException(Exception inner) : base("Unique constraint violated", inner) { }
    }
}
