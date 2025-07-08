namespace Finance.Web.ViewModel
{
    public class ExpenseViewModel
    {
        public int id { get; set; }
        public string? description { get; set; }
        public DateTime movimentDate { get; set; }
        public bool isAppellant { get; set; }
        public int categoryId { get; set; }
        public IEnumerable<Finance.Web.Models.Category>? Categories { get; set; }
        public required decimal value { get; set; }
    }
}
