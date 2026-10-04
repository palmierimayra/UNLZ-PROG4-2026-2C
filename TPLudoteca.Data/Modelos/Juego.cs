using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TPLudoteca.Data.Modelos.Helpers;

namespace TPLudoteca.Data.Modelos
{
    public class Juego
    {
        [Key]
        public int IdJuego { get; set; }
        public string DescripcionJuego { get; set; }
        [ForeignKey(nameof(CategoriaJuego))]
        public int IdCategoriaJuego { get; set; }
        public CategoriaJuego? CategoriaJuego { get; set; }
        [Column(TypeName = "decimal(7,2)")]
        public decimal? MontoAlquiler { get; set; }
        public int Cantidad { get; set; }
        public int Disponible { get; set; }
        public Audit Audit { get; set; }
    }
}
