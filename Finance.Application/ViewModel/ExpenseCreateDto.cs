namespace Finance.Application.ViewModel
{
    public class ExpenseCreateDto
    {
        public string? Description { get; set; }
        public DateOnly MovimentDate { get; set; }
        public bool IsRecurring { get; set; }
        public int CategoryId { get; set; }
        public required decimal Value { get; set; }
    }
}
