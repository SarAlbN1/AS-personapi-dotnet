using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Back.Aplicacion.Services.Interfaces;

/// <summary>
/// Casos de uso de teléfonos, compartidos por REST (HU-010) y MVC (HU-012). Aplica las
/// reglas de SDD §7: <c>num</c> único de hasta 15 caracteres y <c>duenio</c> existente;
/// una referencia inexistente es error de entrada, no un recurso ausente.
/// </summary>
public interface ITelefonoService
{
    Task<IReadOnlyList<TelefonoDto>> GetAllAsync();

    Task<ServiceResult<TelefonoDto>> GetByIdAsync(string num);

    Task<ServiceResult<TelefonoDto>> CreateAsync(TelefonoEntradaDto entrada);

    Task<ServiceResult> UpdateAsync(string num, TelefonoEntradaDto entrada);

    Task<ServiceResult> DeleteAsync(string num);
}
