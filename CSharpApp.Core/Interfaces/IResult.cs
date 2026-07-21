namespace CSharpApp.Infrastructure.Interfaces;

public interface IResult
{
    IReadOnlyList<string> Errors { get; init; }

    bool Succeeded { get; init; }
}

public interface IResult<out T> : IResult
{
    T? Data { get; }
}
