using CSharpApp.Application.Categories.Commands;
using FluentValidation;

namespace CSharpApp.Application.Categories.Validators;


public sealed class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryValidator()
    {

        RuleFor(v => v.Id).GreaterThan(0);
    }
}
