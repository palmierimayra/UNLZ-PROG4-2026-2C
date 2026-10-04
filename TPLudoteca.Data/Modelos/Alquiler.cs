using System.ComponentModel.DataAnnotations;
using TPLudoteca.Data.Modelos.Helpers;

namespace TPLudoteca.Data.Modelos
{
    public class Alquiler
    {
        [Key]
        public int IdAlquiler { get; set; }
        public string IdUsuario { get; set; }
        public int IdJuego { get; set; }
        public Juego? Juego { get; set; }
        public DateTime FechaRetiro { get; set; }
        public DateTime FechaDevolucionEstimada { get; set; }
        public DateTime? FechaDevolucionReal { get; set; }
        public decimal MontoAlquiler { get; set; }
        public int? DiasDemora { get; set; }
        public decimal? Recargo { get; set; }
        public decimal? MontoRecargo { get; set; }
        public decimal MontoTotal { get; set; }
        public Audit Audit { get; set; }
    }
}
