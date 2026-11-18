namespace Finance.Application.ViewModel
{
    public class IncomeDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateOnly MovimentDate { get; set; }
        public bool IsRecurring { get; set; }
        public int CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public required decimal Value { get; set; }
    }

}
