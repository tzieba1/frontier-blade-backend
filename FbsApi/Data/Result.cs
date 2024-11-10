using System.Collections.Generic;

public class Result
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public static Result SuccessResult() =>
        new Result { Success = true };

    public static Result FailureResult(string error)
    {
        var result = new Result { Success = false };
        result.Errors.Add(error);
        return result;
    }

    public static Result FailureResult(IEnumerable<string> errors)
    {
        return new Result { Success = false, Errors = new List<string>(errors) };
    }

    public void AddError(string error) => Errors.Add(error);
}

public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> SuccessResult(T data) =>
        new Result<T> { Success = true, Data = data };

    public static new Result<T> FailureResult(string error)
    {
        var result = new Result<T> { Success = false };
        result.Errors.Add(error);
        return result;
    }

    public static new Result<T> FailureResult(IEnumerable<string> errors)
    {
        return new Result<T> { Success = false, Errors = new List<string>(errors) };
    }
}
