using System;
using System.Collections.Generic;

namespace personapi_dotnet.Back.Persistencia.Entities;

public partial class Estudio
{
    public int IdProf { get; set; }//clave foranea de profesion

    public int CcPer { get; set; }//clave foranea de persona

    public DateOnly? Fecha { get; set; }//fecha de estudio

    public string? Univer { get; set; }//universidad donde estudio

    public virtual Persona CcPerNavigation { get; set; } = null!;//relacion con persona

    public virtual Profesion IdProfNavigation { get; set; } = null!;//relacion con profesion
}
