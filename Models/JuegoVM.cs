using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPLudoteca.Models.Helpers;

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
        public CategoriaJuegoVM CategoriaJuegoVM { get; set; }
        [NotMapped]
        public Audit Audit { get; set; }
    }
}
