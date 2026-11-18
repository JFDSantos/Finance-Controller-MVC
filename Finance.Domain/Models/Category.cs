namespace Finance.Domain.Models
{
    public class Category : IOwnedEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int UserId { get; set; }
        public ICollection<Moviment>? Moviments { get; set; }
    }
}
