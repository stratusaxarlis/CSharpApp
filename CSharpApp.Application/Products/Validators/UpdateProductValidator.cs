using CSharpApp.Application.Products.Commands;
using FluentValidation;

namespace CSharpApp.Application.Products.Validators;

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(v => v.Dto.Description).NotEmpty();
        RuleFor(v => v.Dto.Title).MaximumLength(100);
        RuleFor(v => v.Dto.Price).GreaterThan(0);
        RuleFor(v => v.Id).GreaterThan(0);

    }
}
