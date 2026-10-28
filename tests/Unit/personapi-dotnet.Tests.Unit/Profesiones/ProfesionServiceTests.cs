using personapi_dotnet.Back.AccesoDatos.Repository.Interfaces;
using personapi_dotnet.Back.Aplicacion.Services;
using personapi_dotnet.Back.Aplicacion.Services.Implementations;
using personapi_dotnet.Back.Borde.Dtos;
using personapi_dotnet.Back.Persistencia.Entities;

namespace personapi_dotnet.Tests.Unit.Profesiones;

/// <summary>
/// Reglas de SDD §7 para profesiones (HU-005) con DAO dobles en memoria: no necesitan
/// SQL Server.
/// </summary>
public class ProfesionServiceTests
{
    private readonly ProfesionesEnMemoria _profesiones = new();
    private readonly EstudiosEnMemoria _estudios = new();
    private readonly ProfesionService _service;

    public ProfesionServiceTests()
    {
        _profesiones.Filas.Add(new Profesion { Id = 1, Nom = "Ingeniería de Sistemas" });
        _profesiones.Filas.Add(new Profesion { Id = 2, Nom = "Medicina", Des = "Salud" });
        _estudios.Filas.Add(new Estudio { IdProf = 1, CcPer = 1010101010 });
        _service = new ProfesionService(_profesiones, _estudios);
    }

    [Fact]
    public async Task Crear_con_id_libre_guarda_la_profesion()
    {
        var resultado = await _service.CreateAsync(new ProfesionDto { Id = 3, Nom = "Derecho", Des = null });

        Assert.Equal(ServiceResultStatus.Success, resultado.Status);
        Assert.Equal(3, resultado.Value!.Id);
        Assert.Contains(_profesiones.Filas, p => p.Id == 3 && p.Nom == "Derecho" && p.Des == null);
    }

    [Fact]
    public async Task Crear_con_id_duplicado_es_conflicto_y_no_escribe()
    {
        var resultado = await _service.CreateAsync(new ProfesionDto { Id = 2, Nom = "Otra" });

        Assert.Equal(ServiceResultStatus.Conflict, resultado.Status);
        Assert.Equal("Medicina", _profesiones.Filas.Single(p => p.Id == 2).Nom);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Crear_sin_nombre_es_error_de_validacion(string? nom)
    {
        var resultado = await _service.CreateAsync(new ProfesionDto { Id = 3, Nom = nom });

        Assert.Equal(ServiceResultStatus.ValidationError, resultado.Status);
        Assert.DoesNotContain(_profesiones.Filas, p => p.Id == 3);
    }

    [Fact]
    public async Task Crear_con_nombre_de_mas_de_90_caracteres_es_error_de_validacion()
    {
        var resultado = await _service.CreateAsync(new ProfesionDto { Id = 3, Nom = new string('a', 91) });

        Assert.Equal(ServiceResultStatus.ValidationError, resultado.Status);
    }

    [Fact]
    public async Task Crear_recorta_el_nombre_y_mide_la_longitud_sin_espacios()
    {
        var nom = new string('a', 90);

        var resultado = await _service.CreateAsync(new ProfesionDto { Id = 3, Nom = $"  {nom}  " });

        Assert.Equal(ServiceResultStatus.Success, resultado.Status);
        Assert.Equal(nom, _profesiones.Filas.Single(p => p.Id == 3).Nom);
    }

    [Theory]
    [InlineData("  Diseño de sistemas  ", "Diseño de sistemas")]
    [InlineData("   ", null)]
    public async Task Crear_recorta_la_descripcion(string des, string? guardada)
    {
        var resultado = await _service.CreateAsync(new ProfesionDto { Id = 3, Nom = "Derecho", Des = des });

        Assert.Equal(ServiceResultStatus.Success, resultado.Status);
        Assert.Equal(guardada, _profesiones.Filas.Single(p => p.Id == 3).Des);
    }

    [Fact]
    public async Task Crear_sin_id_es_error_de_validacion()
    {
        var resultado = await _service.CreateAsync(new ProfesionDto { Id = null, Nom = "Derecho" });

        Assert.Equal(ServiceResultStatus.ValidationError, resultado.Status);
    }

    [Fact]
    public async Task Crear_admite_descripcion_larga()
    {
        var larga = new string('d', 20_000);

        var resultado = await _service.CreateAsync(new ProfesionDto { Id = 3, Nom = "Derecho", Des = larga });

        Assert.Equal(ServiceResultStatus.Success, resultado.Status);
        Assert.Equal(larga, _profesiones.Filas.Single(p => p.Id == 3).Des);
    }

    [Fact]
    public async Task Consultar_inexistente_es_no_encontrado()
    {
        var resultado = await _service.GetByIdAsync(99);

        Assert.Equal(ServiceResultStatus.NotFound, resultado.Status);
    }

    [Fact]
    public async Task Editar_cambia_nombre_y_descripcion()
    {
        var resultado = await _service.UpdateAsync(2, new ProfesionDto { Id = 2, Nom = "Medicina general", Des = null });

        Assert.Equal(ServiceResultStatus.Success, resultado.Status);
        var fila = _profesiones.Filas.Single(p => p.Id == 2);
        Assert.Equal("Medicina general", fila.Nom);
        Assert.Null(fila.Des);
    }

    [Fact]
    public async Task Editar_con_id_distinto_al_de_la_ruta_es_error_de_validacion()
    {
        var resultado = await _service.UpdateAsync(2, new ProfesionDto { Id = 1, Nom = "Medicina" });

        Assert.Equal(ServiceResultStatus.ValidationError, resultado.Status);
        Assert.Equal(2, _profesiones.Filas.Count);
    }

    [Fact]
    public async Task Editar_sin_id_en_el_cuerpo_es_error_de_validacion()
    {
        var resultado = await _service.UpdateAsync(2, new ProfesionDto { Id = null, Nom = "Medicina" });

        Assert.Equal(ServiceResultStatus.ValidationError, resultado.Status);
        Assert.Equal("El id es obligatorio.", resultado.Message);
    }

    [Fact]
    public async Task Editar_recorta_nombre_y_descripcion()
    {
        var resultado = await _service.UpdateAsync(2, new ProfesionDto { Id = 2, Nom = "  Medicina general  ", Des = " Salud " });

        Assert.Equal(ServiceResultStatus.Success, resultado.Status);
        var fila = _profesiones.Filas.Single(p => p.Id == 2);
        Assert.Equal("Medicina general", fila.Nom);
        Assert.Equal("Salud", fila.Des);
    }

    [Fact]
    public async Task Editar_inexistente_es_no_encontrado()
    {
        var resultado = await _service.UpdateAsync(99, new ProfesionDto { Id = 99, Nom = "Nada" });

        Assert.Equal(ServiceResultStatus.NotFound, resultado.Status);
    }

    [Fact]
    public async Task Borrar_profesion_con_estudios_es_conflicto_y_no_borra()
    {
        var resultado = await _service.DeleteAsync(1);

        Assert.Equal(ServiceResultStatus.Conflict, resultado.Status);
        Assert.Contains(_profesiones.Filas, p => p.Id == 1);
    }

    [Fact]
    public async Task Borrar_profesion_sin_estudios_la_elimina()
    {
        var resultado = await _service.DeleteAsync(2);

        Assert.Equal(ServiceResultStatus.Success, resultado.Status);
        Assert.DoesNotContain(_profesiones.Filas, p => p.Id == 2);
    }

    [Fact]
    public async Task Borrar_inexistente_es_no_encontrado()
    {
        var resultado = await _service.DeleteAsync(99);

        Assert.Equal(ServiceResultStatus.NotFound, resultado.Status);
    }

    private sealed class ProfesionesEnMemoria : IProfesionRepository
    {
        public List<Profesion> Filas { get; } = [];

        public Task<IReadOnlyList<Profesion>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<Profesion>>(Filas.OrderBy(p => p.Id).ToList());

        public Task<Profesion?> GetByIdAsync(int id) => Task.FromResult(Filas.SingleOrDefault(p => p.Id == id));

        public Task<bool> ExistsAsync(int id) => Task.FromResult(Filas.Any(p => p.Id == id));

        public Task AddAsync(Profesion entity)
        {
            Filas.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Profesion entity) => Task.CompletedTask;

        public Task DeleteAsync(Profesion entity)
        {
            Filas.Remove(entity);
            return Task.CompletedTask;
        }
    }

    private sealed class EstudiosEnMemoria : IEstudioRepository
    {
        public List<Estudio> Filas { get; } = [];

        public Task<IReadOnlyList<Estudio>> GetAllAsync() => Task.FromResult<IReadOnlyList<Estudio>>(Filas.ToList());

        public Task<Estudio?> GetByIdAsync(int idProf, int ccPer) =>
            Task.FromResult(Filas.SingleOrDefault(e => e.IdProf == idProf && e.CcPer == ccPer));

        public Task<bool> ExistsAsync(int idProf, int ccPer) =>
            Task.FromResult(Filas.Any(e => e.IdProf == idProf && e.CcPer == ccPer));

        public Task AddAsync(Estudio entity) => throw new NotSupportedException();

        public Task UpdateAsync(Estudio entity) => throw new NotSupportedException();

        public Task DeleteAsync(Estudio entity) => throw new NotSupportedException();
    }
}
