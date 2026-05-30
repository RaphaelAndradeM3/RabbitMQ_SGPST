namespace SGPST.Domain.Interfaces;

public interface IAppResult
{
    bool Success { get; }
    string Message { get; }
    object? Data { get; }
    Exception? Error { get; }
}

public interface IAppResult<T> : IAppResult
{
    new T? Data { get; }
}
