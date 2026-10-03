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
        [NotMapped]
        public Audit? Audit { get; set; }
    }
}
