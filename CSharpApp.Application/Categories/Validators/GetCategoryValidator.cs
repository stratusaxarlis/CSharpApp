using CSharpApp.Application.Categories.Queries;
using FluentValidation;

namespace CSharpApp.Application.Categories.Validators;


public sealed class GetCategoryValidator : AbstractValidator<GetCategoryByIdQuery>
{
    public GetCategoryValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Id).GreaterThan(0);
    }
}
