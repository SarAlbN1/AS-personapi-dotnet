using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Back.Aplicacion.Services.Implementations;

public class TelefonoService : ITelefonoService
{
    /// <summary>Longitud de telefono.num en el DDL (VARCHAR(15)).</summary>
    public const int NumLongitudMaxima = 15;

    private readonly ITelefonoRepository _telefonos;
    private readonly IPersonaRepository _personas;

    // El repositorio de personas solo se consulta para validar el dueño: la FK
    // telefono_persona_fk es la defensa final, pero el servicio da el 400 antes.
    public TelefonoService(ITelefonoRepository telefonos, IPersonaRepository personas)
    {
        _telefonos = telefonos;
        _personas = personas;
    }

    public async Task<IReadOnlyList<TelefonoDto>> GetAllAsync() =>
        (await _telefonos.GetAllAsync()).Select(ADto).ToList();

    public async Task<ServiceResult<TelefonoDto>> GetByIdAsync(string num)
    {
        var telefono = await _telefonos.GetByIdAsync(num);

        return telefono is null
            ? ServiceResult<TelefonoDto>.NotFound(NoExiste(num))
            : ServiceResult<TelefonoDto>.Success(ADto(telefono));
    }

    public async Task<ServiceResult<TelefonoDto>> CreateAsync(TelefonoEntradaDto entrada)
    {
        if (await Validar(entrada) is string error)
        {
            return ServiceResult<TelefonoDto>.ValidationError(error);
        }

        var num = entrada.Num.Trim();
        if (await _telefonos.ExistsAsync(num))
        {
            return ServiceResult<TelefonoDto>.Conflict($"Ya existe un teléfono con número {num}.");
        }

        var telefono = new Telefono
        {
            Num = num,
            Oper = entrada.Oper.Trim(),
            Duenio = entrada.Duenio!.Value
        };

        await _telefonos.AddAsync(telefono);

        return ServiceResult<TelefonoDto>.Success(ADto(telefono));
    }

    public async Task<ServiceResult> UpdateAsync(string num, TelefonoEntradaDto entrada)
    {
        // El número es la identidad del recurso: el PUT no la cambia (api-rest.md).
        if (!string.Equals(num, entrada.Num?.Trim(), StringComparison.Ordinal))
        {
            return ServiceResult.ValidationError(
                $"El número de la ruta ({num}) no coincide con el del cuerpo ({entrada.Num}).");
        }

        if (await Validar(entrada) is string error)
        {
            return ServiceResult.ValidationError(error);
        }

        var telefono = await _telefonos.GetByIdAsync(num);
        if (telefono is null)
        {
            return ServiceResult.NotFound(NoExiste(num));
        }

        telefono.Oper = entrada.Oper.Trim();
        telefono.Duenio = entrada.Duenio!.Value;

        await _telefonos.UpdateAsync(telefono);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(string num)
    {
        var telefono = await _telefonos.GetByIdAsync(num);
        if (telefono is null)
        {
            return ServiceResult.NotFound(NoExiste(num));
        }

        // Nada depende de un teléfono: no hay caso de conflicto al borrarlo.
        await _telefonos.DeleteAsync(telefono);

        return ServiceResult.Success();
    }

    private async Task<string?> Validar(TelefonoEntradaDto entrada)
    {
        var num = entrada.Num?.Trim();
        if (string.IsNullOrEmpty(num))
        {
            return "El número (num) es obligatorio.";
        }

        if (num.Length > NumLongitudMaxima)
        {
            return $"El número (num) admite hasta {NumLongitudMaxima} caracteres.";
        }

        // La clave del recurso viaja en la ruta: un carácter como '/' dejaba la fila
        // creada pero inalcanzable por GET y DELETE. Se restringe a dígitos, que es lo
        // que la tabla guarda, sin dejar de ser cadena para conservar los ceros.
        if (!num.All(char.IsAsciiDigit))
        {
            return "El número (num) solo admite dígitos.";
        }

        if (string.IsNullOrWhiteSpace(entrada.Oper))
        {
            return "El operador (oper) es obligatorio.";
        }

        // El borde ya lo rechaza con [Required]; el servicio tampoco acepta el dueño
        // ausente, porque MVC comparte estos servicios.
        if (entrada.Duenio is null)
        {
            return "El dueño (duenio) es obligatorio.";
        }

        // Referencia inexistente: error de entrada, no recurso ausente (SDD §7).
        if (!await _personas.ExistsAsync(entrada.Duenio.Value))
        {
            return $"No existe una persona con cédula {entrada.Duenio} para ser dueña del teléfono.";
        }

        return null;
    }

    private static string NoExiste(string num) => $"No existe un teléfono con número {num}.";

    private static TelefonoDto ADto(Telefono telefono) => new()
    {
        Num = telefono.Num,
        Oper = telefono.Oper,
        Duenio = telefono.Duenio
    };
}
