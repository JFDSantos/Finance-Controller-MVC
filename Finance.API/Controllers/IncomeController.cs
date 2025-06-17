using System.Threading.Tasks;
using Finance.Web.Data;
using Finance.Web.Models;
using Finance.Web.Models.Enums;
using Finance.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Finance.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IncomeController : ControllerBase
    {
        private readonly FinanceContext _context;
        public IncomeController(FinanceContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Income>>> GetAll()
        {
            var incomes = await _context.Incomes.Include(i => i.Category).Select(i => new IncomeDto
            {
                Id = i.id,
                Description = i.description,
                MovimentDate = i.movimentDate,
                IsAppellant = i.isAppellant,
                TypeIncome = i.typeIncome,
                CategoryId = i.categoryId,
                CategoryName = i.Category.Name
            }).ToListAsync();

            return Ok(incomes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Income>> GetById(int id)
        {
            var income = await _context.Incomes.Include(i => i.Category).FirstOrDefaultAsync(i => i.id == id);

            if (income == null)
            {
                return NotFound();
            }

            return Ok(income);
        }

        [HttpPost]
        public async Task<ActionResult<Income>> Create(IncomeCreateDto vm) {


                var income = new Income
                {

                    description = vm.Description,
                    isAppellant = vm.IsAppellant,
                    movimentDate = vm.MovimentDate,
                    typeIncome = vm.TypeIncome,
                    type = MovimentType.Income,
                    categoryId = vm.CategoryId

                };

                _context.Add(income);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = income.id }, income);

        }


    }
}
