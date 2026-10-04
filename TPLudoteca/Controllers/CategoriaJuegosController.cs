using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TPLudoteca.Data;
using TPLudoteca.Data.Modelos;
using TPLudoteca.Data.Modelos.Helpers;
using TPLudoteca.Data.Repositorios;
using TPLudoteca.Models;

namespace TPLudoteca.Controllers
{
    [Authorize(Roles = "Administrador", AuthenticationSchemes = "Identity.Application")]
    public class CategoriaJuegosController : Controller
    {
        private readonly ICategoriaJuegoRepository _categoriaJuegoRepository;
        private readonly IJuegoRepository _juegoRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoriaJuegosController(ICategoriaJuegoRepository categoriaJuegoRepository, IJuegoRepository juegoRepository, UserManager<ApplicationUser> userManager)
        {
            _categoriaJuegoRepository = categoriaJuegoRepository;
            _juegoRepository = juegoRepository;
            _userManager = userManager;
        }

        // GET: CategoriaJuegosController
        public ActionResult Index()
        {
            List<CategoriaJuegoVM> listaCategoriaJuegos = new List<CategoriaJuegoVM>();
            var CategoriaJuegosDDBB = _categoriaJuegoRepository.ObtenerCategorias();

            CategoriaJuegosDDBB = CategoriaJuegosDDBB.Where(x => x.Audit.FechaBaja == null).ToList(); 
            listaCategoriaJuegos = CategoriaJuegosDDBB.Select(x => new CategoriaJuegoVM
            {
                IdCategoriaJuego = x.IdCategoriaJuego,
                DescripcionCategoria = x.DescripcionCategoria,
            }).ToList();

            return View(listaCategoriaJuegos);
        }

        // GET: CategoriaJuegosController/Detalle/5
        public ActionResult Detalle(int id)
        {
            return View();
        }

        // GET: CategoriaJuegosController/Crear
        public ActionResult Crear()
        {
            return View();
        }

        // POST: CategoriaJuegosController/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(CategoriaJuegoVM nuevaCategoriaJuego)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mensaje = "Error";

                return View(nuevaCategoriaJuego);
            }

            try
            {
                CategoriaJuego categoriaJuego = new CategoriaJuego();

                categoriaJuego.DescripcionCategoria = nuevaCategoriaJuego.DescripcionCategoria;

                categoriaJuego.Audit = new Audit
                {
                    FechaAlta = DateTime.Now,
                    IdUsuarioAlta = _userManager.GetUserId(User)
                };

                if (!_categoriaJuegoRepository.AgregarCategoria(categoriaJuego))
                {
                    ModelState.AddModelError(string.Empty, "No se pudo guardar la categoría. Probá de nuevo.");
                    return View(nuevaCategoriaJuego);
                }

                TempData["Mensaje"] = "Categoría creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar la categoría. Probá de nuevo.");
                return View(nuevaCategoriaJuego);
            }
        }

        // GET: CategoriaJuegosController/Editar/5
        public ActionResult Editar(int id)
        {
            CategoriaJuegoVM categoriaJuego = null;

            var categoriaJuegoDDBB = _categoriaJuegoRepository.ObtenerCategoriaPorId(id);
            categoriaJuego = new CategoriaJuegoVM
            {
                IdCategoriaJuego = categoriaJuegoDDBB.IdCategoriaJuego,
                DescripcionCategoria = categoriaJuegoDDBB.DescripcionCategoria
            };

            return View(categoriaJuego);
        }

        // POST: CategoriaJuegosController/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int id, CategoriaJuegoVM categoriaModificada)
        {
            if (!ModelState.IsValid)
            {
                return View(categoriaModificada);
            }

            try
            {
                var categoriaAModificar = _categoriaJuegoRepository.ObtenerCategoriaPorId(id);
                if (categoriaAModificar == null)
                {
                    return NotFound();
                }

                categoriaAModificar.DescripcionCategoria = categoriaModificada.DescripcionCategoria;
                categoriaAModificar.Audit.FechaModificacion = DateTime.Now;
                categoriaAModificar.Audit.IdUsuarioModificacion = _userManager.GetUserId(User);

                _categoriaJuegoRepository.ActualizarCategoria(categoriaAModificar);
                TempData["Mensaje"] = "Categoría modificada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar la categoría. Probá de nuevo.");
                return View(categoriaModificada);
            }
        }

        // GET: CategoriaJuegosController/Eliminar/5
        public ActionResult Eliminar(int id)
        {
            CategoriaJuegoVM categoriaJuego = null;

            var categoriaJuegoDDBB = _categoriaJuegoRepository.ObtenerCategoriaPorId(id);

            categoriaJuego = new CategoriaJuegoVM
            {
                IdCategoriaJuego = categoriaJuegoDDBB.IdCategoriaJuego,
                DescripcionCategoria = categoriaJuegoDDBB.DescripcionCategoria
            };

            return View(categoriaJuego);
        }

        // POST: CategoriaJuegosController/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id, IFormCollection collection)
        {
            try
            {
                var categoriaAEliminar = _categoriaJuegoRepository.ObtenerCategoriaPorId(id);
                if (categoriaAEliminar == null)
                {
                    return NotFound();
                }

                var juegosActivos = _juegoRepository.ObtenerJuegos().Count(x => x.IdCategoriaJuego == id && x.Audit.FechaBaja == null);
                if (juegosActivos > 0)
                {
                    TempData["Error"] = $"No se puede eliminar: tiene {juegosActivos} juegos asociados.";
                    return RedirectToAction(nameof(Index));
                }

                categoriaAEliminar.Audit.FechaBaja = DateTime.Now;
                categoriaAEliminar.Audit.IdUsuarioBaja = _userManager.GetUserId(User);

                if (!_categoriaJuegoRepository.BorrarCategoria(categoriaAEliminar))
                {
                    TempData["Error"] = "No se pudo eliminar la categoría. Probá de nuevo.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Mensaje"] = "Categoría eliminada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] = "No se pudo eliminar la categoría. Probá de nuevo.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
