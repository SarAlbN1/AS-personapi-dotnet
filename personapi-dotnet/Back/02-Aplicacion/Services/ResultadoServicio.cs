namespace personapi_dotnet.Back.Aplicacion.Services;

/// <summary>
/// Resultado discriminado de un servicio (SDD §2). La capa 02 no conoce HTTP: devuelve
/// uno de estos estados y <c>01-Borde</c> los traduce a código de estado o a mensaje
/// Razor. Lo comparten los cuatro servicios del proyecto.
/// </summary>
public enum EstadoResultado
{
    Success,
    NotFound,
    ValidationError,
    Conflict
}

/// <summary>
/// Resultado sin valor de retorno, para actualizar y eliminar.
/// </summary>
public class ResultadoServicio
{
    protected ResultadoServicio(EstadoResultado estado, string? mensaje)
    {
        Estado = estado;
        Mensaje = mensaje;
    }

    public EstadoResultado Estado { get; }

    /// <summary>
    /// Explicación para el cliente. Nunca lleva SQL, trazas ni datos de conexión, según
    /// el contrato de validaciones y errores.
    /// </summary>
    public string? Mensaje { get; }

    public bool EsExito => Estado == EstadoResultado.Success;

    public static ResultadoServicio Success() => new(EstadoResultado.Success, null);

    public static ResultadoServicio NotFound(string mensaje) => new(EstadoResultado.NotFound, mensaje);

    public static ResultadoServicio ValidationError(string mensaje) => new(EstadoResultado.ValidationError, mensaje);

    public static ResultadoServicio Conflict(string mensaje) => new(EstadoResultado.Conflict, mensaje);
}

/// <summary>
/// Resultado con valor, para consultar y crear. <see cref="Valor"/> solo tiene contenido
/// cuando el estado es <see cref="EstadoResultado.Success"/>.
/// </summary>
public sealed class ResultadoServicio<T> : ResultadoServicio
{
    private ResultadoServicio(EstadoResultado estado, T? valor, string? mensaje)
        : base(estado, mensaje) => Valor = valor;

    public T? Valor { get; }

    public static ResultadoServicio<T> Success(T valor) => new(EstadoResultado.Success, valor, null);

    public static new ResultadoServicio<T> NotFound(string mensaje) => new(EstadoResultado.NotFound, default, mensaje);

    public static new ResultadoServicio<T> ValidationError(string mensaje) => new(EstadoResultado.ValidationError, default, mensaje);

    public static new ResultadoServicio<T> Conflict(string mensaje) => new(EstadoResultado.Conflict, default, mensaje);
}
