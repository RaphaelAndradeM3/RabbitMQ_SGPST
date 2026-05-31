namespace SGPST.Domain.Interfaces;

// Interface para padronizacao do retorno de servicos e casos de uso
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
