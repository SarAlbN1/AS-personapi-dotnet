using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Back.Aplicacion.Services;
using personapi_dotnet.Back.Aplicacion.Services.Interfaces;
using personapi_dotnet.Back.Borde.Dtos;

namespace personapi_dotnet.Back.Borde.ControllersApi;

/// <summary>
/// Canal JSON de estudios (SDD §5). Detalle, edición y baja se identifican con los dos
/// segmentos de la PK compuesta, <c>{idProf}/{ccPer}</c>, nunca con un id artificial.
/// </summary>
[ApiController]
[Route("api/estudios")]
[Produces("application/json")]
public class EstudiosApiController : ControllerBase
{
    private readonly IEstudioService _estudios;

    public EstudiosApiController(IEstudioService estudios) => _estudios = estudios;

    /// <summary>Lista todos los estudios.</summary>
    /// <response code="200">Los estudios registrados; sin filas, <c>[]</c>.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EstudioDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EstudioDto>>> GetAll() =>
        Ok(await _estudios.GetAllAsync());

    /// <summary>Consulta un estudio por el par (profesión, persona).</summary>
    /// <param name="idProf">Id de la profesión, primera mitad de la clave.</param>
    /// <param name="ccPer">Cédula de la persona, segunda mitad de la clave.</param>
    /// <response code="200">El estudio encontrado.</response>
    /// <response code="404">No existe un estudio para ese par.</response>
    [HttpGet("{idProf:int}/{ccPer:int}")]
    [ProducesResponseType<EstudioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int idProf, int ccPer) =>
        Traducir(await _estudios.GetByIdAsync(idProf, ccPer));

    /// <summary>Registra que una persona estudió una profesión.</summary>
    /// <param name="entrada">El par (<c>idProf</c>, <c>ccPer</c>) y, opcionalmente, la fecha y la universidad.</param>
    /// <response code="201">Estudio creado; <c>Location</c> apunta a <c>/api/estudios/{idProf}/{ccPer}</c>.</response>
    /// <response code="400">Falta una mitad de la clave, la fecha no es válida, la universidad supera los 50 caracteres sin contar los espacios de los extremos, o la profesión o la persona no existen.</response>
    /// <response code="409">Ya existe un estudio para ese par.</response>
    [HttpPost]
    [ProducesResponseType<EstudioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] EstudioEntradaDto entrada)
    {
        var resultado = await _estudios.CreateAsync(entrada);

        if (!resultado.IsSuccess)
        {
            return Traducir(resultado);
        }

        var creado = resultado.Value!;
        return CreatedAtAction(nameof(GetById), new { idProf = creado.IdProf, ccPer = creado.CcPer }, creado);
    }

    /// <summary>Actualiza la fecha y la universidad de un estudio; el par no se puede cambiar.</summary>
    /// <param name="idProf">Id de la profesión, primera mitad de la clave.</param>
    /// <param name="ccPer">Cédula de la persona, segunda mitad de la clave.</param>
    /// <param name="entrada">Datos completos del estudio, con el mismo par de la ruta.</param>
    /// <response code="204">Estudio actualizado.</response>
    /// <response code="400">Datos inválidos o el par del cuerpo no coincide con el de la ruta.</response>
    /// <response code="404">No existe un estudio para ese par.</response>
    [HttpPut("{idProf:int}/{ccPer:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int idProf, int ccPer, [FromBody] EstudioEntradaDto entrada) =>
        Traducir(await _estudios.UpdateAsync(idProf, ccPer, entrada));

    /// <summary>Elimina un estudio. Nada depende de un estudio, así que no hay conflicto al borrarlo.</summary>
    /// <param name="idProf">Id de la profesión, primera mitad de la clave.</param>
    /// <param name="ccPer">Cédula de la persona, segunda mitad de la clave.</param>
    /// <response code="204">Estudio eliminado.</response>
    /// <response code="404">No existe un estudio para ese par.</response>
    [HttpDelete("{idProf:int}/{ccPer:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int idProf, int ccPer) =>
        Traducir(await _estudios.DeleteAsync(idProf, ccPer));

    // Mismo mapeo que en personas, profesiones y teléfonos: los errores salen como
    // ProblemDetails, la forma que ya produce [ApiController] cuando el cuerpo no valida.
    private IActionResult Traducir(ServiceResult resultado) => resultado.Status switch
    {
        ServiceResultStatus.Success => NoContent(),
        ServiceResultStatus.NotFound => Problem(detail: resultado.Message, statusCode: StatusCodes.Status404NotFound),
        ServiceResultStatus.ValidationError => Problem(detail: resultado.Message, statusCode: StatusCodes.Status400BadRequest),
        ServiceResultStatus.Conflict => Problem(detail: resultado.Message, statusCode: StatusCodes.Status409Conflict),
        _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
    };

    private IActionResult Traducir<T>(ServiceResult<T> resultado) =>
        resultado.IsSuccess ? Ok(resultado.Value) : Traducir((ServiceResult)resultado);
}
