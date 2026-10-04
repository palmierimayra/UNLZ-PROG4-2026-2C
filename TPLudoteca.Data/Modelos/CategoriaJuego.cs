using System.ComponentModel.DataAnnotations;
using TPLudoteca.Data.Modelos.Helpers;

namespace TPLudoteca.Data.Modelos
{
    public class CategoriaJuego
    {
        [Key]
        public int IdCategoriaJuego { get; set; }
        public string DescripcionCategoria { get; set; }
        public Audit Audit { get; set; }
    }
}
