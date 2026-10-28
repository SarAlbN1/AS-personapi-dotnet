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

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProfesionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProfesionDto>>> GetAll() =>
        Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProfesionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfesionDto>> GetById(int id)
    {
        var resultado = await _service.GetByIdAsync(id);
        return resultado.IsSuccess ? Ok(resultado.Value) : AError(resultado);
    }

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

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, ProfesionDto dto)
    {
        var resultado = await _service.UpdateAsync(id, dto);
        return resultado.IsSuccess ? NoContent() : AError(resultado);
    }

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
