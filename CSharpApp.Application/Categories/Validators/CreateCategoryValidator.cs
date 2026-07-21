using CSharpApp.Application.Categories.Commands;
using FluentValidation;

namespace CSharpApp.Application.Categories.Validators;


public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(v => v.Dto.Name).NotEmpty();
        RuleFor(v => v.Dto.Name).MaximumLength(100);
        RuleFor(v => v.Dto.Image).MaximumLength(250);
    }
}
