﻿using FluentValidation;

namespace IdentityService.WebAPI.Controllers.UserAdmin
{
    public record AddAdminUserRequest(string UserName, string PhoneNum,string Email);
    public class AddAdminUserRequestValidator : AbstractValidator<AddAdminUserRequest>
    {
        public AddAdminUserRequestValidator()
        {
            RuleFor(e => e.PhoneNum).NotNull().NotEmpty().MaximumLength(11);
            RuleFor(e => e.UserName).NotEmpty().NotEmpty().MaximumLength(20).MinimumLength(2);
            RuleFor(e => e.Email).NotEmpty().NotEmpty().MaximumLength(20).MinimumLength(2);
        }
    }
}
