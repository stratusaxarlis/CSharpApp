using CSharpApp.Application.Products.Commands;
using FluentValidation;

namespace CSharpApp.Application.Products.Validators;


public sealed class DeleteProductValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductValidator()
    {

        RuleFor(v => v.Id).GreaterThan(0);
    }
}
