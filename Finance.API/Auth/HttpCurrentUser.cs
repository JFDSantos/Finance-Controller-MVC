using System.Security.Claims;
using Finance.Application.Interfaces;

namespace Finance.API.Auth
{
    public class HttpCurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        public HttpCurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

        public int UserId
        {
            get
            {
                var value = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(value, out var id))
                    throw new UnauthorizedAccessException("No authenticated user.");
                return id;
            }
        }
    }
}
