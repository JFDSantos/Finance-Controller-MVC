using Finance.Web.Models;

namespace Finance.Web.ViewModel
{
    public class IncomeViewModel
    {
        public int id { get; set; }
        public string? description { get; set; }
        public DateTime movimentDate { get; set; }
        public bool isAppellant { get; set; }
        public int categoryId { get; set; }
        public int typeIncome { get; set; }
        public IEnumerable<Category>? Categories { get; set; }

    }
}
