using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPLudoteca.Data.Modelos.Helpers;

namespace TPLudoteca.Models
{
    public class AlquilerVM
    {
        [Key]
        public int IdAlquiler {  get; set; }
        public string? IdUsuario { get; set; }
        public string? EmailUsuario { get; set; }
        [ForeignKey(nameof(JuegoVM))]
        [Required(ErrorMessage = "Elegí un juego.")]
        public int IdJuego { get; set; }
        public JuegoVM? JuegoVM { get; set; }
        [DisplayName("Fecha de retiro")]
        [Required(ErrorMessage = "Ingresá la fecha de retiro.")]
        public DateTime FechaRetiro { get; set; }
        public DateTime FechaDevolucionEstimada { get; set; }
        public DateTime? FechaDevolucionReal {  get; set; }
        public decimal MontoAlquiler { get; set; }
        public int? DiasDemora { get; set; }
        public decimal? Recargo { get; set; }
        public decimal? MontoRecargo { get; set; }
        public decimal MontoTotal { get; set; }
        [NotMapped]
        public Audit? Audit { get; set; }
    }
}
