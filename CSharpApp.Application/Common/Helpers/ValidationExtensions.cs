using FluentValidation;
using FluentValidation.Results;

namespace CSharpApp.Application.Common.Helpers;

public static class ValidationExtensions
{
    public static async Task<List<ValidationFailure>> ValidateAsync<TRequest>(
        this IEnumerable<IValidator<TRequest>> validators, ValidationContext<TRequest> validationContext,
        CancellationToken cancellationToken = default)
    {
        if (!validators.Any()) return [];

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(validationContext, cancellationToken)));

        return validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();
    }

    public static Dictionary<string, string[]> ToDictionary(this List<ValidationFailure>? failures)
    {
        return failures is not null && failures.Count != 0
            ? failures.GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray())
            : [];
    }
}
