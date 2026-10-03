using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using TPLudoteca.Data.Modelos.Helpers;

namespace TPLudoteca.Models
{
    public class CategoriaJuegoVM
    {
        [Key]
        public int IdCategoriaJuego { get; set; }
        public string DescripcionCategoria {  get; set; }
        [NotMapped]
        public Audit? Audit { get; set; }
    }
}
