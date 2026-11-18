using Finance.Domain.Models.Enums;

namespace Finance.Domain.Models
{
    public abstract class Moviment : IOwnedEntity
    {
        public int Id { get; set; }
        public EMovimentType Type { get; set; }
        public string? Description { get; set; }
        public DateOnly MovimentDate { get; set; }
        public bool IsRecurring { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public decimal Value { get; set; }
        public int UserId { get; set; }
    }
}
