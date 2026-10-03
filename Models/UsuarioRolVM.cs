using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TPLudoteca.Models
{
    public class UsuarioRolVM
    {
        public string IdUsuario { get; set; }
        public string Email { get; set; }
        [DisplayName("Rol")]
        [Required(ErrorMessage = "Elegí un rol.")]
        public string? Rol { get; set; }
        public List<SelectListItem>? RolesDisponibles { get; set; }
        public bool Desactivado { get; set; }
    }
}
