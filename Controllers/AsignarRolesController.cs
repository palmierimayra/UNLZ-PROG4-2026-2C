using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TPLudoteca.Data;
using TPLudoteca.Models;

namespace TPLudoteca.Controllers
{
    [Authorize(Roles = "Administrador", AuthenticationSchemes = "Identity.Application")]
    public class AsignarRolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AsignarRolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = _userManager.Users.OrderBy(u => u.Email).ToList();
            var listaUsuarios = new List<UsuarioRolVM>();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                listaUsuarios.Add(new UsuarioRolVM
                {
                    IdUsuario = usuario.Id,
                    Email = usuario.Email ?? usuario.UserName ?? string.Empty,
                    Rol = roles.FirstOrDefault(),
                    Desactivado = await _userManager.IsLockedOutAsync(usuario)
                });
            }

            return View(listaUsuarios);
        }

        public async Task<IActionResult> Editar(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(usuario);
            var usuarioRol = new UsuarioRolVM
            {
                IdUsuario = usuario.Id,
                Email = usuario.Email ?? usuario.UserName ?? string.Empty,
                Rol = roles.FirstOrDefault(),
                RolesDisponibles = ObtenerRolesDisponibles()
            };

            return View(usuarioRol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(UsuarioRolVM usuarioRol)
        {
            usuarioRol.RolesDisponibles = ObtenerRolesDisponibles();

            if (!ModelState.IsValid)
            {
                return View(usuarioRol);
            }

            var usuario = await _userManager.FindByIdAsync(usuarioRol.IdUsuario);
            if (usuario == null)
            {
                return NotFound();
            }

            if (!await _roleManager.RoleExistsAsync(usuarioRol.Rol!))
            {
                ModelState.AddModelError(string.Empty, "El rol elegido no existe.");
                return View(usuarioRol);
            }

            if (usuario.Id == _userManager.GetUserId(User) && usuarioRol.Rol != "Administrador")
            {
                ModelState.AddModelError(string.Empty, "No podés quitarte el rol de Administrador a vos mismo.");
                return View(usuarioRol);
            }

            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            var resultado = await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            if (resultado.Succeeded)
            {
                resultado = await _userManager.AddToRoleAsync(usuario, usuarioRol.Rol!);
            }

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(usuarioRol);
            }

            TempData["Mensaje"] = $"Se asignó el rol {usuarioRol.Rol} a {usuarioRol.Email}";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Desactivar(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(await ConvertirAUsuarioRolVM(usuario));
        }

        [HttpPost]
        [ActionName("Desactivar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarConfirmado(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            if (usuario.Id == _userManager.GetUserId(User))
            {
                ModelState.AddModelError(string.Empty, "No podés desactivar tu propio usuario.");
                return View(await ConvertirAUsuarioRolVM(usuario));
            }

            await _userManager.SetLockoutEnabledAsync(usuario, true);
            var resultado = await _userManager.SetLockoutEndDateAsync(usuario, DateTimeOffset.MaxValue);
            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(await ConvertirAUsuarioRolVM(usuario));
            }

            await _userManager.UpdateSecurityStampAsync(usuario);

            TempData["Mensaje"] = $"Se desactivó el usuario {usuario.Email}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            await _userManager.SetLockoutEndDateAsync(usuario, null);
            await _userManager.ResetAccessFailedCountAsync(usuario);

            TempData["Mensaje"] = $"Se reactivó el usuario {usuario.Email}";
            return RedirectToAction(nameof(Index));
        }

        private async Task<UsuarioRolVM> ConvertirAUsuarioRolVM(ApplicationUser usuario)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            return new UsuarioRolVM
            {
                IdUsuario = usuario.Id,
                Email = usuario.Email ?? usuario.UserName ?? string.Empty,
                Rol = roles.FirstOrDefault(),
                Desactivado = await _userManager.IsLockedOutAsync(usuario)
            };
        }

        private List<SelectListItem> ObtenerRolesDisponibles()
        {
            return _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem(r.Name, r.Name))
                .ToList();
        }
    }
}
