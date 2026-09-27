namespace TPLudoteca.Models.Helpers
{
    public class Audit
    {
        public int IdUsuarioAlta { get; set; }
        public DateTime FechaAlta { get; set; }
        public int? IdUsuarioModificacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public int? IdUsuarioBaja { get; set; }
        public DateTime? FechaBaja { get; set; }
    }
}
