using Finance.Web.Models.Enums;

namespace Finance.Web.Models
{
    public class Moviment
    {
        public int id { get; set; }
        public EMovimentType type { get; set; }
        public string? description { get; set; }
        public DateTime movimentDate { get; set; }
        public Boolean isAppellant {  get; set; }
        public int categoryId { get; set; }
        public Category? Category { get; set; }
        public decimal value { get; set; }

    }
}
