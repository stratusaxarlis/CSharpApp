using CSharpApp.Application.Products.Queries;
using FluentValidation;

namespace CSharpApp.Application.Products.Validators;


public sealed class GetProductValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Id).GreaterThan(0);
    }
}
