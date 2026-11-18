namespace Finance.Application.ViewModel
{
    public class LoginDto
    {
        // Defaults (not "required") so missing JSON fields surface as VALIDATION_FAILED, not a binding error.
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
