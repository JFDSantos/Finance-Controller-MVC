namespace Finance.Domain.Models
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Moviment>? Moviments { get; set; }
    }
}
