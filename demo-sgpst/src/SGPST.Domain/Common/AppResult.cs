using System.Text.Json.Serialization;
using SGPST.Domain.Interfaces;

namespace SGPST.Domain.Common;

public class AppResult : IAppResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    
    [JsonIgnore]
    public Exception? Error { get; set; }

    [JsonConstructor]
    public AppResult(bool success, string message, object? data = null)
    {
        Success = success;
        Message = message;
        Data = data;
    }

    protected AppResult(bool success, string message, object? data, Exception? error)
    {
        Success = success;
        Message = message;
        Data = data;
        Error = error;
    }

    // Metodo para criar um resultado de sucesso
    public static IAppResult Ok(string mensagem = "Operacao realizada com sucesso", object? dados = null)
    {
        return new AppResult(true, mensagem, dados);
    }

    // Metodo para criar um resultado de erro
    public static IAppResult Failure(string mensagem, Exception? erro = null)
    {
        return new AppResult(false, mensagem, null, erro);
    }
}

public class AppResult<T> : AppResult, IAppResult<T>
{
    public new T? Data { get; set; }

    [JsonConstructor]
    public AppResult(bool success, string message, T? data = default) 
        : base(success, message, data)
    {
        Data = data;
    }

    private AppResult(bool success, string message, T? data, Exception? error) 
        : base(success, message, data, error)
    {
        Data = data;
    }

    // Metodo para criar um resultado de sucesso com dados tipados
    public static IAppResult<T> Ok(T dados, string mensagem = "Operacao realizada com sucesso")
    {
        return new AppResult<T>(true, mensagem, dados);
    }

    // Metodo para criar um resultado de erro tipado
    public static new IAppResult<T> Failure(string mensagem, Exception? erro = null)
    {
        return new AppResult<T>(false, mensagem, default, erro);
    }
}
