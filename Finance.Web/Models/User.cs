using Finance.Web.Models.Enums;

namespace Finance.Web.Models
{
    public class User
    {
        public int id { get; set; } 
        public required string user { get; set; }
        public required string password { get; set; }
        public required string email { get; set; }
        public EUserRole role { get; set; }
    }
}
