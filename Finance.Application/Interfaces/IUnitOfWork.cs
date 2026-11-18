namespace Finance.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task CommitAsync();

        /// <summary>Runs <paramref name="work"/> in one database transaction: commits if it returns, rolls back if it throws.</summary>
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> work);
    }
}
