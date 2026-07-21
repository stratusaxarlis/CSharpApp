using CSharpApp.Application.Categories.Commands;
using FluentValidation;

namespace CSharpApp.Application.Categories.Validators;


public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {

        RuleFor(v => v.Dto.Name).NotEmpty();
        RuleFor(v => v.Id).GreaterThan(0);
        RuleFor(v => v.Dto.Name).MaximumLength(100);
    }
}
