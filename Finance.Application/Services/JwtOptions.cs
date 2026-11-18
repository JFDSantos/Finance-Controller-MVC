namespace Finance.Application.Services
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";
        public const int MinKeyLength = 32;

        public string Key { get; set; } = string.Empty;
    }
}
