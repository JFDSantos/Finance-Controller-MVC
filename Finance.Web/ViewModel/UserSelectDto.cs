using Finance.Web.Models.Enums;

namespace Finance.Web.ViewModel
{
    public class UserSelectDto
    {
        public int Id { get; set; }
        public required string user { get; set; }
        public required string email { get; set; }
        public required EUserRole role { get; set; }
    }
}
