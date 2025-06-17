namespace Finance.Web.ViewModel
{
    public class IncomeDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTime MovimentDate { get; set; }
        public bool IsAppellant { get; set; }
        public int TypeIncome { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }

}
