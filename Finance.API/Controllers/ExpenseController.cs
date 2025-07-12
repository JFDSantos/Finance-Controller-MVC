using Finance.Web.Patterns.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseRepository _expService;
        public ExpenseController(IExpenseRepository expService)
        {
            _expService = expService;
        }


    }
}
