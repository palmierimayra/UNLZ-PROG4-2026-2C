namespace TPLudoteca.Data.Modelos.Helpers
{
    public class Audit
    {
        public string IdUsuarioAlta { get; set; }
        public DateTime FechaAlta { get; set; }
        public string? IdUsuarioModificacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? IdUsuarioBaja { get; set; }
        public DateTime? FechaBaja { get; set; }
    }
}
