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
    public class JuegosController : Controller
    {
        private readonly IJuegoRepository _juegoRepository;
        private readonly ICategoriaJuegoRepository _categoriaJuegoRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public JuegosController(IJuegoRepository juegoRepository, ICategoriaJuegoRepository categoriaJuegoRepository, UserManager<ApplicationUser> userManager)
        {
            _juegoRepository = juegoRepository;
            _categoriaJuegoRepository = categoriaJuegoRepository;
            _userManager = userManager;
        }

        // GET: JuegosController
        public ActionResult Index()
        {
            List<JuegoVM> listaJuegos = new List<JuegoVM>();
            var juegosDDBB = _juegoRepository.ObtenerJuegos();

            juegosDDBB = juegosDDBB.Where(x => x.Audit.FechaBaja == null).ToList(); listaJuegos = juegosDDBB.Select(x => new JuegoVM
            {
                IdJuego = x.IdJuego,
                DescripcionJuego = x.DescripcionJuego,
                IdCategoriaJuego = x.IdCategoriaJuego,
                MontoAlquiler = x.MontoAlquiler,
                Cantidad = x.Cantidad,
                Disponible = x.Disponible,
                CategoriaJuegoVM = new CategoriaJuegoVM
                {
                    IdCategoriaJuego = x.CategoriaJuego.IdCategoriaJuego,
                    DescripcionCategoria = x.CategoriaJuego.DescripcionCategoria
                }
            }).ToList();

            return View(listaJuegos);
        }

        // GET: JuegosController/Detalle/5
        public ActionResult Detalle(int id)
        {
            return View();
        }

        // GET: JuegosController/Crear
        public ActionResult Crear()
        {
            ViewBag.Categorias = new SelectList(_categoriaJuegoRepository.ObtenerCategorias(), "IdCategoriaJuego", "DescripcionCategoria");

            return View();
        }

        // POST: JuegosController/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(JuegoVM nuevoJuego)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mensaje = "Error";
                ViewBag.Categorias = new SelectList(_categoriaJuegoRepository.ObtenerCategorias(), "IdCategoriaJuego", "DescripcionCategoria");

                return View(nuevoJuego);
            }

            try
            {
                Juego juego = new Juego();

                juego.DescripcionJuego = nuevoJuego.DescripcionJuego;
                juego.IdCategoriaJuego = nuevoJuego.IdCategoriaJuego;
                juego.MontoAlquiler = nuevoJuego.MontoAlquiler;
                juego.Cantidad = nuevoJuego.Cantidad;
                juego.Disponible = nuevoJuego.Cantidad;

                juego.Audit = new Audit
                {
                    FechaAlta = DateTime.Now,
                    IdUsuarioAlta = _userManager.GetUserId(User)
                };

                if (!_juegoRepository.AgregarJuego(juego))
                {
                    ModelState.AddModelError(string.Empty, "No se pudo guardar el juego. Probá de nuevo.");
                    ViewBag.Categorias = new SelectList(_categoriaJuegoRepository.ObtenerCategorias(), "IdCategoriaJuego", "DescripcionCategoria");
                    return View(nuevoJuego);
                }

                TempData["Mensaje"] = "Juego creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewBag.Categorias = new SelectList(_categoriaJuegoRepository.ObtenerCategorias(), "IdCategoriaJuego", "DescripcionCategoria");
                return View(nuevoJuego);
            }
        }

        // GET: JuegosController/Editar/5
        public ActionResult Editar(int id)
        {
            JuegoVM juego = null;
            
            ViewBag.Categorias = new SelectList(_categoriaJuegoRepository.ObtenerCategorias(), "IdCategoriaJuego", "DescripcionCategoria");

            var juegoDDBB = _juegoRepository.ObtenerJuegoPorId(id);
            juego = new JuegoVM
            {
                IdJuego = juegoDDBB.IdJuego,
                DescripcionJuego = juegoDDBB.DescripcionJuego,
                IdCategoriaJuego = juegoDDBB.IdCategoriaJuego,
                MontoAlquiler = juegoDDBB.MontoAlquiler,
                Cantidad = juegoDDBB.Cantidad,
                Disponible = juegoDDBB.Disponible
            };

            return View(juego);
        }

        // POST: JuegosController/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int id, JuegoVM juegoModificado)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = new SelectList(_categoriaJuegoRepository.ObtenerCategorias(), "IdCategoriaJuego", "DescripcionCategoria");
                return View(juegoModificado);
            }

            try
            {
                var juegoaModificar = _juegoRepository.ObtenerJuegoPorId(id);
                if (juegoaModificar == null)
                {
                    return NotFound();
                }

                var alquilados = juegoaModificar.Cantidad - juegoaModificar.Disponible;
                if (juegoModificado.Cantidad < alquilados)
                {
                    ModelState.AddModelError(nameof(JuegoVM.Cantidad), $"Hay {alquilados} copias alquiladas, la cantidad no puede ser menor.");
                    ViewBag.Categorias = new SelectList(_categoriaJuegoRepository.ObtenerCategorias(), "IdCategoriaJuego", "DescripcionCategoria");
                    return View(juegoModificado);
                }

                juegoaModificar.DescripcionJuego = juegoModificado.DescripcionJuego;
                juegoaModificar.IdCategoriaJuego = juegoModificado.IdCategoriaJuego;
                juegoaModificar.MontoAlquiler = juegoModificado.MontoAlquiler;
                juegoaModificar.Cantidad = juegoModificado.Cantidad;
                juegoaModificar.Disponible = juegoModificado.Cantidad - alquilados;
                juegoaModificar.Audit.FechaModificacion = DateTime.Now;
                juegoaModificar.Audit.IdUsuarioModificacion = _userManager.GetUserId(User);

                _juegoRepository.ActualizarJuego(juegoaModificar);
                TempData["Mensaje"] = "Juego modificado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "No se pudo guardar el juego. Probá de nuevo.");
                ViewBag.Categorias = new SelectList(_categoriaJuegoRepository.ObtenerCategorias(), "IdCategoriaJuego", "DescripcionCategoria");
                return View(juegoModificado);
            }
        }

        // GET: JuegosController/Eliminar/5
        public ActionResult Eliminar(int id)
        {
            JuegoVM juego = null;

            var juegoDDBB = _juegoRepository.ObtenerJuegoPorId(id);

            juego = new JuegoVM
            {
                IdJuego = juegoDDBB.IdJuego,
                DescripcionJuego = juegoDDBB.DescripcionJuego,
                IdCategoriaJuego = juegoDDBB.IdCategoriaJuego,
                MontoAlquiler = juegoDDBB.MontoAlquiler,
                Cantidad = juegoDDBB.Cantidad,
                Disponible = juegoDDBB.Disponible
            };

            ViewBag.NombreCategoria = juegoDDBB.CategoriaJuego.DescripcionCategoria;

            return View(juego);
        }

        // POST: JuegosController/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id, IFormCollection collection)
        {
            try
            {
                var juegoaEliminar = _juegoRepository.ObtenerJuegoPorId(id);
                if (juegoaEliminar == null)
                {
                    return NotFound();
                }

                var alquilados = juegoaEliminar.Cantidad - juegoaEliminar.Disponible;
                if (alquilados > 0)
                {
                    TempData["Error"] = $"No se puede eliminar: hay {alquilados} copias alquiladas.";
                    return RedirectToAction(nameof(Index));
                }

                juegoaEliminar.Audit.FechaBaja = DateTime.Now;
                juegoaEliminar.Audit.IdUsuarioBaja = _userManager.GetUserId(User);

                if (!_juegoRepository.BorrarJuego(juegoaEliminar))
                {
                    TempData["Error"] = "No se pudo eliminar el juego. Probá de nuevo.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Mensaje"] = "Juego eliminado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] = "No se pudo eliminar el juego. Probá de nuevo.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
