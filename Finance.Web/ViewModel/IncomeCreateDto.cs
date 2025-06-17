namespace Finance.Web.ViewModel
{
    public class IncomeCreateDto
    {
        public string? Description { get; set; }
        public DateTime MovimentDate { get; set; }
        public bool IsAppellant { get; set; }
        public int TypeIncome { get; set; }
        public int CategoryId { get; set; }
    }

}
