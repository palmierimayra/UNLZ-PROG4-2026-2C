using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPLudoteca.Data.Modelos.Helpers;

namespace TPLudoteca.Models
{
    public class JuegoVM
    {
        [Key]
        public int IdJuego { get; set; }
        [DisplayName("Descripción del Juego")]
        public string DescripcionJuego { get; set; }
        [ForeignKey(nameof(CategoriaJuegoVM))]
        public int IdCategoriaJuego { get; set; }
        [DisplayName("Categoría del Juego")]
        public CategoriaJuegoVM? CategoriaJuegoVM { get; set; }
        [DisplayName("Monto de alquiler")]
        [Range(0, 99999.99, ErrorMessage = "Ingresá un monto entre 0 y 99.999,99.")]
        public decimal? MontoAlquiler { get; set; }
        [NotMapped]
        public Audit? Audit { get; set; }
    }
}
