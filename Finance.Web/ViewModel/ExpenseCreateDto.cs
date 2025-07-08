namespace Finance.Web.ViewModel
{
    public class ExpenseCreateDto
    {
        public string? Description { get; set; }
        public DateTime MovimentDate { get; set; }
        public bool IsAppellant { get; set; }
        public int CategoryId { get; set; }
        public required decimal Value { get; set; }
    }
}
