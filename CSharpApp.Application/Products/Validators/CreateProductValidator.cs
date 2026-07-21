using CSharpApp.Application.Products.Commands;
using FluentValidation;

namespace CSharpApp.Application.Products.Validators;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(v => v.Dto.Description).NotEmpty();
        RuleFor(v => v.Dto.Title).MaximumLength(100);
        RuleFor(v => v.Dto.Price).GreaterThan(0);
        RuleFor(v => v.Dto.CategoryId).GreaterThan(0);
    }
}
