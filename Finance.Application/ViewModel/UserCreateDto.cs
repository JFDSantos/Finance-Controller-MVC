namespace Finance.Application.ViewModel
{
    public class UserCreateDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
    }
}
