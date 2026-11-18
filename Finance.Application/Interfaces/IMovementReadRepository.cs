namespace Finance.Application.Interfaces
{
    public interface IMovementReadRepository
    {
        Task<bool> AnyByCategoryAsync(int categoryId);
    }
}
