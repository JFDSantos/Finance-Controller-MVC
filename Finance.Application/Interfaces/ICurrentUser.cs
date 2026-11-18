namespace Finance.Application.Interfaces
{
    public interface ICurrentUser
    {
        /// <summary>Id of the authenticated user. Throws UnauthorizedAccessException when there is none.</summary>
        int UserId { get; }
    }
}
