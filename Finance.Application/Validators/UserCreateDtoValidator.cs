using Finance.Application.ViewModel;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Application.Validators
{
    public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateDtoValidator()
        {
            RuleFor(x => x.email)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.password)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.user)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
