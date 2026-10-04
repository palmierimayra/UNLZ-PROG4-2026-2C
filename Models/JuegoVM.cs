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
        [Required(ErrorMessage = "Ingresá la descripción del juego.")]
        [StringLength(100, ErrorMessage = "La descripción no puede superar los 100 caracteres.")]
        public string DescripcionJuego { get; set; }
        [ForeignKey(nameof(CategoriaJuegoVM))]
        [Required(ErrorMessage = "Elegí una categoría.")]
        public int IdCategoriaJuego { get; set; }
        [DisplayName("Categoría del Juego")]
        public CategoriaJuegoVM? CategoriaJuegoVM { get; set; }
        [DisplayName("Monto de alquiler")]
        [Required(ErrorMessage = "Ingresá el monto de alquiler.")]
        [Range(1, 99999.99, ErrorMessage = "El monto debe estar entre 1 y 99.999,99.")]
        public decimal? MontoAlquiler { get; set; }
        [DisplayName("Cantidad")]
        [Required(ErrorMessage = "Ingresá la cantidad.")]
        [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1000.")]
        public int Cantidad { get; set; }
        [DisplayName("Disponibles")]
        public int Disponible { get; set; }
        [NotMapped]
        public Audit? Audit { get; set; }
    }
}
