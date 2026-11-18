using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Application.Validators
{
    public class ExpenseCreateDtoValidator : AbstractValidator<ExpenseCreateDto>
    {
        public ExpenseCreateDtoValidator(ICategoryRepository categories)
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Value)
                .GreaterThan(0)
                .PrecisionScale(18, 2, false);

            RuleFor(x => x.CategoryId)
                .MustAsync(async (id, ct) => await categories.GetByIdAsync(id) != null)
                .WithErrorCode("CATEGORY_NOT_FOUND")
                .WithMessage("Category not found");
        }
    }
}
