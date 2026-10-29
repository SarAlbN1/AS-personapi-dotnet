using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Aplicacion.Services;
using personapi_dotnet.Back.Aplicacion.Services.Implementations;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Tests.Unit.Estudios;

/// <summary>
/// Reglas de SDD §7 para estudios (HU-011) con DAO dobles en memoria: no necesitan
/// SQL Server.
/// </summary>
public class EstudioServiceTests
{
    private const int CcAna = 1010101010;
    private const int CcLuis = 1020202020;

    private readonly EstudiosEnMemoria _estudios = new();
    private readonly EstudioService _service;

    public EstudioServiceTests()
    {
        _estudios.Filas.Add(new Estudio
        {
            IdProf = 1,
            CcPer = CcAna,
            Fecha = new DateOnly(2020, 12, 15),
            Univer = "Javeriana"
        });

        _service = new EstudioService(_estudios, new ProfesionesEnMemoria(1, 2), new PersonasEnMemoria(CcAna, CcLuis));
    }

    private static EstudioEntradaDto Entrada(int? idProf, int? ccPer, DateOnly? fecha = null, string? univer = null) =>
        new() { IdProf = idProf, CcPer = ccPer, Fecha = fecha, Univer = univer };

    [Fact]
    public async Task Crear_con_profesion_y_persona_existentes_guarda_el_par()
    {
        var resultado = await _service.CreateAsync(Entrada(2, CcLuis, new DateOnly(2015, 6, 20), "Nacional"));

        Assert.Equal(ServiceResultStatus.Success, resultado.Status);
        Assert.Equal((2, CcLuis), (resultado.Value!.IdProf, resultado.Value.CcPer));
        Assert.Contains(_estudios.Filas, e => e.IdProf == 2 && e.CcPer == CcLuis && e.Univer == "Nacional");
    }

    [Fact]
    public async Task Crear_admite_fecha_y_universidad_nulas()
    {
        var resultado = await _service.CreateAsync(Entrada(2, CcAna));

        Assert.Equal(ServiceResultStatus.Success, resultado.Status);
        var fila = _estudios.Filas.Single(e => e.IdProf == 2 && e.CcPer == CcAna);
        Assert.Null(fila.Fecha);
        Assert.Null(fila.Univer);
    }

    [Fact]
    public async Task Crear_con_profesion_inexistente_es_error_de_validacion_y_no_escribe()
    {
        var resultado = await _service.CreateAsync(Entrada(99, CcAna));

        Assert.Equal(ServiceResultStatus.ValidationError, resultado.Status);
        Assert.Single(_estudios.Filas);
    }

    [Fact]
    public async Task Crear_con_persona_inexistente_es_error_de_validacion_y_no_escribe()
    {
        var resultado = await _service.CreateAsync(Entrada(2, 777000777));

        Assert.Equal(ServiceResultStatus.ValidationError, resultado.Status);
        Assert.Single(_estudios.Filas);
    }

    [Fact]
    public async Task Crear_par_duplicado_es_conflicto_y_no_cambia_la_fila()
    {
        var resultado = await _service.CreateAsync(Entrada(1, CcAna, univer: "Otra"));

        Assert.Equal(ServiceResultStatus.Conflict, resultado.Status);
        Assert.Equal("Javeriana", _estudios.Filas.Single().Univer);
    }

    [Theory]
    [InlineData(null, CcAna)]
    [InlineData(1, null)]
    public async Task Crear_sin_alguna_mitad_de_la_clave_es_error_de_validacion(int? idProf, int? ccPer)
    {
        var resultado = await _service.CreateAsync(Entrada(idProf, ccPer));

        Assert.Equal(ServiceResultStatus.ValidationError, resultado.Status);
    }

    [Fact]
    public async Task Crear_recorta_la_universidad_y_mide_la_longitud_sin_espacios()
    {
        var univer = new string('u', 50);

        var conEspacios = await _service.CreateAsync(Entrada(2, CcAna, univer: $"  {univer}  "));
        var soloEspacios = await _service.CreateAsync(Entrada(2, CcLuis, univer: "   "));

        Assert.Equal(ServiceResultStatus.Success, conEspacios.Status);
        Assert.Equal(univer, _estudios.Filas.Single(e => e.IdProf == 2 && e.CcPer == CcAna).Univer);
        Assert.Equal(ServiceResultStatus.Success, soloEspacios.Status);
        Assert.Null(_estudios.Filas.Single(e => e.IdProf == 2 && e.CcPer == CcLuis).Univer);
    }

    [Fact]
    public async Task Crear_con_universidad_de_mas_de_50_caracteres_es_error_de_validacion()
    {
        var resultado = await _service.CreateAsync(Entrada(2, CcAna, univer: new string('u', 51)));

        Assert.Equal(ServiceResultStatus.ValidationError, resultado.Status);
    }

    [Fact]
    public async Task Consultar_par_inexistente_es_no_encontrado()
    {
        var resultado = await _service.GetByIdAsync(2, CcAna);

        Assert.Equal(ServiceResultStatus.NotFound, resultado.Status);
    }

    [Fact]
    public async Task Editar_cambia_solo_fecha_y_universidad()
    {
        var resultado = await _service.UpdateAsync(1, CcAna, Entrada(1, CcAna, new DateOnly(2021, 1, 10), " Andes "));

        Assert.Equal(ServiceResultStatus.Success, resultado.Status);
        var fila = _estudios.Filas.Single();
        Assert.Equal((1, CcAna), (fila.IdProf, fila.CcPer));
        Assert.Equal(new DateOnly(2021, 1, 10), fila.Fecha);
        Assert.Equal("Andes", fila.Univer);
    }

    [Theory]
    [InlineData(2, CcAna)]
    [InlineData(1, CcLuis)]
    [InlineData(null, CcAna)]
    public async Task Editar_con_par_distinto_al_de_la_ruta_es_error_de_validacion(int? idProf, int? ccPer)
    {
        var resultado = await _service.UpdateAsync(1, CcAna, Entrada(idProf, ccPer, univer: "Otra"));

        Assert.Equal(ServiceResultStatus.ValidationError, resultado.Status);
        Assert.Equal("Javeriana", _estudios.Filas.Single().Univer);
    }

    [Fact]
    public async Task Editar_par_inexistente_es_no_encontrado()
    {
        var resultado = await _service.UpdateAsync(2, CcLuis, Entrada(2, CcLuis));

        Assert.Equal(ServiceResultStatus.NotFound, resultado.Status);
    }

    [Fact]
    public async Task Borrar_elimina_el_par()
    {
        var resultado = await _service.DeleteAsync(1, CcAna);

        Assert.Equal(ServiceResultStatus.Success, resultado.Status);
        Assert.Empty(_estudios.Filas);
    }

    [Fact]
    public async Task Borrar_par_inexistente_es_no_encontrado()
    {
        var resultado = await _service.DeleteAsync(1, CcLuis);

        Assert.Equal(ServiceResultStatus.NotFound, resultado.Status);
        Assert.Single(_estudios.Filas);
    }

    private sealed class EstudiosEnMemoria : IEstudioRepository
    {
        public List<Estudio> Filas { get; } = [];

        public Task<IReadOnlyList<Estudio>> GetAllAsync() => Task.FromResult<IReadOnlyList<Estudio>>(Filas.ToList());

        public Task<Estudio?> GetByIdAsync(int idProf, int ccPer) =>
            Task.FromResult(Filas.SingleOrDefault(e => e.IdProf == idProf && e.CcPer == ccPer));

        public Task<bool> ExistsAsync(int idProf, int ccPer) =>
            Task.FromResult(Filas.Any(e => e.IdProf == idProf && e.CcPer == ccPer));

        public Task AddAsync(Estudio entity)
        {
            Filas.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Estudio entity) => Task.CompletedTask;

        public Task DeleteAsync(Estudio entity)
        {
            Filas.Remove(entity);
            return Task.CompletedTask;
        }
    }

    // Los dobles de profesión y persona solo responden ExistsAsync: es lo único que el
    // servicio de estudios les pide.
    private sealed class ProfesionesEnMemoria(params int[] ids) : IProfesionRepository
    {
        public Task<bool> ExistsAsync(int id) => Task.FromResult(ids.Contains(id));

        public Task<IReadOnlyList<Profesion>> GetAllAsync() => throw new NotSupportedException();

        public Task<Profesion?> GetByIdAsync(int id) => throw new NotSupportedException();

        public Task AddAsync(Profesion entity) => throw new NotSupportedException();

        public Task UpdateAsync(Profesion entity) => throw new NotSupportedException();

        public Task DeleteAsync(Profesion entity) => throw new NotSupportedException();
    }

    private sealed class PersonasEnMemoria(params int[] ccs) : IPersonaRepository
    {
        public Task<bool> ExistsAsync(int cc) => Task.FromResult(ccs.Contains(cc));

        public Task<IReadOnlyList<Persona>> GetAllAsync() => throw new NotSupportedException();

        public Task<Persona?> GetByIdAsync(int cc) => throw new NotSupportedException();

        public Task AddAsync(Persona entity) => throw new NotSupportedException();

        public Task UpdateAsync(Persona entity) => throw new NotSupportedException();

        public Task DeleteAsync(Persona entity) => throw new NotSupportedException();
    }
}
