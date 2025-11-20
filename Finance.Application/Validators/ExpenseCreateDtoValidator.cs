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
        public ExpenseCreateDtoValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Value)
                .GreaterThan(0);
        }
    }
}
