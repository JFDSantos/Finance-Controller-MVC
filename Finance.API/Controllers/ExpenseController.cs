using Finance.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expService;
        public ExpenseController(IExpenseService expService)
        {
            _expService = expService;
        }


    }
}
