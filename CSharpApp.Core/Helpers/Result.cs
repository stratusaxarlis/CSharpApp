using CSharpApp.Infrastructure.Interfaces;

namespace CSharpApp.Infrastructure.Helpers;

    public record Result : IResult
    {
        internal Result()
        {
            Errors = [];
        }

        internal Result(bool succeeded, IEnumerable<string> errors)
        {
            Succeeded = succeeded;
            Errors = errors?.ToList().AsReadOnly() ?? ((IReadOnlyList<string>)[]);
        }

        public string ErrorMessage => string.Join(", ", Errors ?? new string[] { });

        public bool Succeeded { get; init; }

        public IReadOnlyList<string> Errors { get; init; }

        public static Result Success() => new(true, []);
        public static Task<Result> SuccessAsync() => Task.FromResult(Success());
        public static Result Failure(params string[] errors) => new(false, errors);
        public static Task<Result> FailureAsync(params string[] errors) => Task.FromResult(Failure(errors));
        /// <summary>
        /// Executes the corresponding action based on whether the operation succeeded.
        /// </summary>
        /// <param name="onSuccess">Action to execute on success.</param>
        /// <param name="onFailure">Action to execute on failure (receives the error message).</param>
        public void Match(Action onSuccess, Action<string> onFailure)
        {
            if (Succeeded)
                onSuccess();
            else
                onFailure(ErrorMessage);
        }
        /// <summary>
        /// Asynchronously executes the corresponding action based on whether the operation succeeded.
        /// </summary>
        /// <param name="onSuccess">Async action to execute on success.</param>
        /// <param name="onFailure">Async action to execute on failure (receives the error message).</param>
        public Task MatchAsync(Func<Task> onSuccess, Func<string, Task> onFailure) => Succeeded ? onSuccess() : onFailure(ErrorMessage);
        // Functional extensions
        public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure) =>
            Succeeded ? onSuccess() : onFailure(ErrorMessage);

        public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> onSuccess, Func<string, Task<TResult>> onFailure) =>
            Succeeded ? await onSuccess() : await onFailure(ErrorMessage);
    }

    public record Result<T> : Result, IResult<T>
    {
        public T? Data { get; set; }

        /// <summary>
        /// Creates a successful <see cref="Result{T}"/> instance with data.
        /// </summary>
        /// <param name="data">The data to include in the result.</param>
        public static Result<T> Success(T data) => new(true, [], data);
        public static Task<Result<T>> SuccessAsync(T data) => Task.FromResult(Success(data));
        public static Result<T> Failure(params IEnumerable<string> errors) => new(false, errors, default);
        public static Task<Result<T>> FailureAsync(params IEnumerable<string> errors) => Task.FromResult(Failure(errors));


        /// <summary>
        /// Protected constructor to initialize a result with data.
        /// </summary>
        /// <param name="succeeded">Indicates if the operation succeeded.</param>
        /// <param name="errors">A collection of error messages.</param>
        /// <param name="data">The data returned by the operation.</param>
        protected Result(bool succeeded, IEnumerable<string>? errors, T? data)
#pragma warning disable CS8604 // Possible null reference argument.
            : base(succeeded, errors: errors)
#pragma warning restore CS8604 // Possible null reference argument.
        {
            Data = data;
        }

        // Functional extensions
        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure) =>
            Succeeded ? onSuccess(Data!) : onFailure(ErrorMessage);

        public async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> onSuccess, Func<string, Task<TResult>> onFailure) =>
            Succeeded ? await onSuccess(Data!) : await onFailure(ErrorMessage);

        public Result<TResult> Map<TResult>(Func<T, TResult> map) =>
            Succeeded ? Result<TResult>.Success(map(Data!)) : Result<TResult>.Failure(Errors);

        public async Task<Result<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> map) =>
        Succeeded ? Result<TResult>.Success(await map(Data!)) : await Result<TResult>.FailureAsync(Errors);

        public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> bind) =>
            Succeeded ? bind(Data!) : Result<TResult>.Failure(Errors);

        public async Task<Result<TResult>> BindAsync<TResult>(Func<T, Task<Result<TResult>>> bind) =>
        Succeeded ? await bind(Data!) : await Result<TResult>.FailureAsync(Errors);

    }
