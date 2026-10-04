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
                Rol = roles.FirstOrDefault()
            };

            ViewBag.Roles = new SelectList(_roleManager.Roles.OrderBy(r => r.Name), "Name", "Name");

            return View(usuarioRol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(UsuarioRolVM usuarioRol)
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles.OrderBy(r => r.Name), "Name", "Name");

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            if (usuario.Id == _userManager.GetUserId(User))
            {
                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsLockedOutAsync(usuario))
            {
                await _userManager.SetLockoutEndDateAsync(usuario, null);
                await _userManager.ResetAccessFailedCountAsync(usuario);
                TempData["Mensaje"] = $"Se reactivó el usuario {usuario.Email}";
            }
            else
            {
                await _userManager.SetLockoutEnabledAsync(usuario, true);
                await _userManager.SetLockoutEndDateAsync(usuario, DateTimeOffset.MaxValue);
                await _userManager.UpdateSecurityStampAsync(usuario);
                TempData["Mensaje"] = $"Se desactivó el usuario {usuario.Email}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
