namespace personapi_dotnet.Back.Aplicacion.Services;

/// <summary>
/// Resultados discriminados de SDD §2. El servicio decide el resultado; 01-Borde lo
/// traduce a código HTTP (REST) o a mensaje visible (MVC) sin repetir la regla.
/// </summary>
public enum ServiceResultStatus
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

/// <summary>Resultado de una operación que no devuelve valor (edición y baja).</summary>
public class ServiceResult
{
    protected ServiceResult(ServiceResultStatus status, string? message)
    {
        Status = status;
        Message = message;
    }

    public ServiceResultStatus Status { get; }

    /// <summary>Explicación legible para el cliente; nula en caso de éxito.</summary>
    public string? Message { get; }

    public bool IsSuccess => Status == ServiceResultStatus.Success;

    public static ServiceResult Success() => new(ServiceResultStatus.Success, null);

    public static ServiceResult NotFound(string message) => new(ServiceResultStatus.NotFound, message);

    public static ServiceResult ValidationError(string message) => new(ServiceResultStatus.ValidationError, message);

    public static ServiceResult Conflict(string message) => new(ServiceResultStatus.Conflict, message);
}

/// <summary>Resultado de una operación que devuelve <typeparamref name="T"/> si tiene éxito.</summary>
public sealed class ServiceResult<T> : ServiceResult
{
    private ServiceResult(ServiceResultStatus status, T? value, string? message)
        : base(status, message) => Value = value;

    public T? Value { get; }

    public static ServiceResult<T> Success(T value) => new(ServiceResultStatus.Success, value, null);

    public static new ServiceResult<T> NotFound(string message) => new(ServiceResultStatus.NotFound, default, message);

    public static new ServiceResult<T> ValidationError(string message) => new(ServiceResultStatus.ValidationError, default, message);

    public static new ServiceResult<T> Conflict(string message) => new(ServiceResultStatus.Conflict, default, message);
}
