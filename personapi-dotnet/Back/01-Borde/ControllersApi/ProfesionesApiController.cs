using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Back.Aplicacion.Services;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Back.Borde.ControllersApi;

/// <summary>
/// Las cinco operaciones de /api/profesiones (api-rest.md, SDD §5). Solo traduce: la
/// regla la decide <see cref="IProfesionService"/> y aquí se convierte en código HTTP.
/// </summary>
[ApiController]
[Route("api/profesiones")]
[Produces("application/json")]
public class ProfesionesApiController : ControllerBase
{
    private readonly IProfesionService _service;

    public ProfesionesApiController(IProfesionService service) => _service = service;

    /// <summary>Lista todas las profesiones.</summary>
    /// <response code="200">Las profesiones registradas; sin filas, <c>[]</c>.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProfesionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProfesionDto>>> GetAll() =>
        Ok(await _service.GetAllAsync());

    /// <summary>Consulta una profesión por su id.</summary>
    /// <param name="id">Id de la profesión (clave).</param>
    /// <response code="200">La profesión encontrada.</response>
    /// <response code="404">No existe una profesión con ese id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ProfesionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfesionDto>> GetById(int id)
    {
        var resultado = await _service.GetByIdAsync(id);
        return resultado.IsSuccess ? Ok(resultado.Value) : AError(resultado);
    }

    /// <summary>Crea una profesión.</summary>
    /// <param name="dto">Datos de la profesión; <c>des</c> es opcional y admite texto largo.</param>
    /// <response code="201">Profesión creada; <c>Location</c> apunta a <c>/api/profesiones/{id}</c>.</response>
    /// <response code="400">Falta el id o el nombre, o el nombre supera los 90 caracteres.</response>
    /// <response code="409">Ya existe una profesión con ese id.</response>
    [HttpPost]
    [ProducesResponseType<ProfesionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProfesionDto>> Create(ProfesionDto dto)
    {
        var resultado = await _service.CreateAsync(dto);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = resultado.Value!.Id }, resultado.Value)
            : AError(resultado);
    }

    /// <summary>Actualiza una profesión; el id no se puede cambiar.</summary>
    /// <param name="id">Id de la profesión (clave).</param>
    /// <param name="dto">Datos completos de la profesión, con el mismo id de la ruta.</param>
    /// <response code="204">Profesión actualizada.</response>
    /// <response code="400">Datos inválidos o el id del cuerpo no coincide con el de la ruta.</response>
    /// <response code="404">No existe una profesión con ese id.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, ProfesionDto dto)
    {
        var resultado = await _service.UpdateAsync(id, dto);
        return resultado.IsSuccess ? NoContent() : AError(resultado);
    }

    /// <summary>Elimina una profesión.</summary>
    /// <param name="id">Id de la profesión (clave).</param>
    /// <response code="204">Profesión eliminada.</response>
    /// <response code="404">No existe una profesión con ese id.</response>
    /// <response code="409">La profesión tiene estudios asociados; no se elimina.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var resultado = await _service.DeleteAsync(id);
        return resultado.IsSuccess ? NoContent() : AError(resultado);
    }

    // Los errores salen como ProblemDetails, el mismo formato que [ApiController] usa para
    // los 400 de validación automática, para que todas las respuestas de error sean iguales.
    private ObjectResult AError(ServiceResult resultado) => Problem(
        detail: resultado.Message,
        statusCode: resultado.Status switch
        {
            ServiceResultStatus.NotFound => StatusCodes.Status404NotFound,
            ServiceResultStatus.ValidationError => StatusCodes.Status400BadRequest,
            ServiceResultStatus.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        });
}
