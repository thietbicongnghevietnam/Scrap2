using FluentValidation;
using ScrapSystem.Api.Application.DTOs.UserDtos;

namespace ScrapSystem.Api.Application.DTOs.UserDtos.Validators
{
    public class UserDtoValidator : AbstractValidator<IUserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(user => user.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password is required.");
                //.MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
        }
    }

    public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
    {
        public UserRegisterDtoValidator()
        {
            Include(new UserDtoValidator());

            RuleFor(x => x.Department)
                .NotEmpty().WithMessage("Department is required.");

            RuleFor(x => x.Section)
                .NotEmpty().WithMessage("Section is required.");
        }
    }

    public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginDtoValidator()
        {
            Include(new UserDtoValidator());

        }
    }
}
