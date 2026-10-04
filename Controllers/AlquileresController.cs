using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TPLudoteca.Data;
using TPLudoteca.Data.Modelos;
using TPLudoteca.Data.Modelos.Helpers;
using TPLudoteca.Data.Repositorios;
using TPLudoteca.Models;

namespace TPLudoteca.Controllers
{
    [Authorize(Roles = "Socio", AuthenticationSchemes = "Identity.Application")]
    public class AlquileresController : Controller
    {
        private const int DiasDePlazo = 7;
        private const decimal PorcentajeMoraPorDia = 0.05m;

        private readonly IAlquilerRepository _alquilerRepository;
        private readonly IJuegoRepository _juegoRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AlquileresController(IAlquilerRepository alquilerRepository, IJuegoRepository juegoRepository, UserManager<ApplicationUser> userManager)
        {
            _alquilerRepository = alquilerRepository;
            _juegoRepository = juegoRepository;
            _userManager = userManager;
        }

        public ActionResult Index()
        {
            var alquileresDDBB = _alquilerRepository.ObtenerAlquileresPorUsuario(_userManager.GetUserId(User)!);

            alquileresDDBB = alquileresDDBB.Where(x => x.Audit.FechaBaja == null).ToList();

            List<AlquilerVM> listaAlquileres = alquileresDDBB.Select(x => new AlquilerVM
            {
                IdAlquiler = x.IdAlquiler,
                IdUsuario = x.IdUsuario,
                IdJuego = x.IdJuego,
                JuegoVM = new JuegoVM
                {
                    IdJuego = x.IdJuego,
                    DescripcionJuego = x.Juego!.DescripcionJuego
                },
                FechaRetiro = x.FechaRetiro,
                FechaDevolucionEstimada = x.FechaDevolucionEstimada,
                FechaDevolucionReal = x.FechaDevolucionReal,
                MontoAlquiler = x.MontoAlquiler,
                MontoTotal = x.MontoTotal
            }).ToList();

            return View(listaAlquileres);
        }

        public ActionResult Detalle(int id)
        {
            var alquilerDDBB = _alquilerRepository.ObtenerAlquilerPorId(id);
            if (alquilerDDBB == null || alquilerDDBB.IdUsuario != _userManager.GetUserId(User) || alquilerDDBB.Audit.FechaBaja != null)
            {
                return NotFound();
            }

            AlquilerVM alquiler = new AlquilerVM
            {
                IdAlquiler = alquilerDDBB.IdAlquiler,
                IdUsuario = alquilerDDBB.IdUsuario,
                IdJuego = alquilerDDBB.IdJuego,
                JuegoVM = new JuegoVM
                {
                    IdJuego = alquilerDDBB.IdJuego,
                    DescripcionJuego = alquilerDDBB.Juego!.DescripcionJuego
                },
                FechaRetiro = alquilerDDBB.FechaRetiro,
                FechaDevolucionEstimada = alquilerDDBB.FechaDevolucionEstimada,
                FechaDevolucionReal = alquilerDDBB.FechaDevolucionReal,
                MontoAlquiler = alquilerDDBB.MontoAlquiler,
                DiasDemora = alquilerDDBB.DiasDemora,
                Recargo = alquilerDDBB.Recargo,
                MontoRecargo = alquilerDDBB.MontoRecargo,
                MontoTotal = alquilerDDBB.MontoTotal
            };

            if (alquiler.FechaDevolucionReal == null)
            {
                var diasDemora = (DateTime.Today - alquiler.FechaDevolucionEstimada.Date).Days;
                if (diasDemora > 0)
                {
                    alquiler.DiasDemora = diasDemora;
                    alquiler.Recargo = PorcentajeMoraPorDia * 100;
                    alquiler.MontoRecargo = alquiler.MontoAlquiler * PorcentajeMoraPorDia * diasDemora;
                    alquiler.MontoTotal = alquiler.MontoAlquiler + alquiler.MontoRecargo.Value;
                }
            }

            return View(alquiler);
        }

        public ActionResult Crear()
        {
            var juegosDDBB = _juegoRepository.ObtenerJuegos();

            juegosDDBB = juegosDDBB.Where(x => x.Audit.FechaBaja == null).OrderBy(x => x.DescripcionJuego).ToList();
            List<JuegoVM> listaJuegos = juegosDDBB.Select(x => new JuegoVM
            {
                IdJuego = x.IdJuego,
                DescripcionJuego = x.DescripcionJuego,
                IdCategoriaJuego = x.IdCategoriaJuego,
                MontoAlquiler = x.MontoAlquiler,
                Cantidad = x.Cantidad,
                Disponible = x.Disponible,
                CategoriaJuegoVM = new CategoriaJuegoVM
                {
                    IdCategoriaJuego = x.CategoriaJuego!.IdCategoriaJuego,
                    DescripcionCategoria = x.CategoriaJuego.DescripcionCategoria
                }
            }).ToList();

            ViewBag.DiasDePlazo = DiasDePlazo;
            ViewBag.PorcentajeMora = PorcentajeMoraPorDia * 100;

            return View(listaJuegos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(int idJuego)
        {
            var juego = _juegoRepository.ObtenerJuegoPorId(idJuego);
            if (juego == null || juego.Audit.FechaBaja != null)
            {
                return NotFound();
            }

            if (juego.Disponible <= 0 || juego.MontoAlquiler == null)
            {
                TempData["Error"] = $"{juego.DescripcionJuego} no está disponible para alquilar en este momento";
                return RedirectToAction(nameof(Crear));
            }

            var idUsuario = _userManager.GetUserId(User)!;
            var ahora = DateTime.Now;

            Alquiler alquiler = new Alquiler
            {
                IdUsuario = idUsuario,
                IdJuego = juego.IdJuego,
                FechaRetiro = ahora,
                FechaDevolucionEstimada = ahora.AddDays(DiasDePlazo),
                MontoAlquiler = juego.MontoAlquiler.Value,
                MontoTotal = juego.MontoAlquiler.Value,
                Audit = new Audit
                {
                    FechaAlta = ahora,
                    IdUsuarioAlta = idUsuario
                }
            };

            if (!_alquilerRepository.AgregarAlquiler(alquiler))
            {
                TempData["Error"] = "No se pudo registrar el alquiler. Probá de nuevo";
                return RedirectToAction(nameof(Crear));
            }

            juego.Disponible -= 1;
            _juegoRepository.ActualizarJuego(juego);

            TempData["Mensaje"] = $"Alquilaste {juego.DescripcionJuego}. Devolvelo antes del {alquiler.FechaDevolucionEstimada:dd/MM/yyyy}";
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Editar(int id)
        {
            var alquilerDDBB = _alquilerRepository.ObtenerAlquilerPorId(id);
            if (alquilerDDBB == null || alquilerDDBB.IdUsuario != _userManager.GetUserId(User) || alquilerDDBB.Audit.FechaBaja != null)
            {
                return NotFound();
            }

            if (!EstaEnCurso(alquilerDDBB))
            {
                TempData["Error"] = "Solo se pueden modificar los alquileres en curso";
                return RedirectToAction(nameof(Index));
            }

            AlquilerVM alquiler = new AlquilerVM
            {
                IdAlquiler = alquilerDDBB.IdAlquiler,
                IdJuego = alquilerDDBB.IdJuego,
                FechaRetiro = alquilerDDBB.FechaRetiro,
                FechaDevolucionEstimada = alquilerDDBB.FechaDevolucionEstimada,
                MontoAlquiler = alquilerDDBB.MontoAlquiler,
                MontoTotal = alquilerDDBB.MontoTotal
            };

            CargarJuegosDisponibles(alquilerDDBB.IdJuego);
            ViewBag.FechaRetiroMinima = alquilerDDBB.Audit.FechaAlta.ToString("yyyy-MM-dd");
            ViewBag.FechaRetiroMaxima = DateTime.Today.AddDays(DiasDePlazo).ToString("yyyy-MM-dd");

            return View(alquiler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int id, AlquilerVM alquilerModificado)
        {
            var alquilerDDBB = _alquilerRepository.ObtenerAlquilerPorId(id);
            if (alquilerDDBB == null || alquilerDDBB.IdUsuario != _userManager.GetUserId(User) || alquilerDDBB.Audit.FechaBaja != null)
            {
                return NotFound();
            }

            if (!EstaEnCurso(alquilerDDBB))
            {
                TempData["Error"] = "Solo se pueden modificar los alquileres en curso";
                return RedirectToAction(nameof(Index));
            }

            if (alquilerModificado.FechaRetiro.Date < alquilerDDBB.Audit.FechaAlta.Date)
            {
                ModelState.AddModelError(nameof(AlquilerVM.FechaRetiro), "La fecha de retiro no puede ser anterior al día en que hiciste el alquiler.");
            }

            if (alquilerModificado.FechaRetiro.Date > DateTime.Today.AddDays(DiasDePlazo))
            {
                ModelState.AddModelError(nameof(AlquilerVM.FechaRetiro), $"La fecha de retiro no puede ser posterior al {DateTime.Today.AddDays(DiasDePlazo):dd/MM/yyyy}.");
            }

            Juego? juegoNuevo = null;
            if (alquilerModificado.IdJuego != alquilerDDBB.IdJuego)
            {
                juegoNuevo = _juegoRepository.ObtenerJuegoPorId(alquilerModificado.IdJuego);
                if (juegoNuevo == null || juegoNuevo.Audit.FechaBaja != null || juegoNuevo.Disponible <= 0 || juegoNuevo.MontoAlquiler == null)
                {
                    ModelState.AddModelError(nameof(AlquilerVM.IdJuego), "El juego elegido no está disponible para alquilar.");
                }
            }

            if (!ModelState.IsValid)
            {
                CargarJuegosDisponibles(alquilerDDBB.IdJuego);
                ViewBag.FechaRetiroMinima = alquilerDDBB.Audit.FechaAlta.ToString("yyyy-MM-dd");
                ViewBag.FechaRetiroMaxima = DateTime.Today.AddDays(DiasDePlazo).ToString("yyyy-MM-dd");
                return View(alquilerModificado);
            }

            var juegoAnterior = alquilerDDBB.Juego!;
            if (juegoNuevo != null)
            {
                juegoAnterior.Disponible += 1;
                juegoNuevo.Disponible -= 1;

                alquilerDDBB.IdJuego = juegoNuevo.IdJuego;
                alquilerDDBB.Juego = juegoNuevo;
                alquilerDDBB.MontoAlquiler = juegoNuevo.MontoAlquiler!.Value;
                alquilerDDBB.MontoTotal = juegoNuevo.MontoAlquiler.Value;
            }

            if (alquilerModificado.FechaRetiro.Date != alquilerDDBB.FechaRetiro.Date)
            {
                alquilerDDBB.FechaRetiro = alquilerModificado.FechaRetiro.Date;
                alquilerDDBB.FechaDevolucionEstimada = alquilerDDBB.FechaRetiro.AddDays(DiasDePlazo);
            }

            alquilerDDBB.Audit.FechaModificacion = DateTime.Now;
            alquilerDDBB.Audit.IdUsuarioModificacion = _userManager.GetUserId(User);

            _alquilerRepository.ActualizarAlquiler(alquilerDDBB);
            if (juegoNuevo != null)
            {
                _juegoRepository.ActualizarJuego(juegoAnterior);
                _juegoRepository.ActualizarJuego(juegoNuevo);
            }

            TempData["Mensaje"] = $"Alquiler modificado. Devolvelo antes del {alquilerDDBB.FechaDevolucionEstimada:dd/MM/yyyy}";
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Eliminar(int id)
        {
            var alquilerDDBB = _alquilerRepository.ObtenerAlquilerPorId(id);
            if (alquilerDDBB == null || alquilerDDBB.IdUsuario != _userManager.GetUserId(User) || alquilerDDBB.Audit.FechaBaja != null)
            {
                return NotFound();
            }

            if (!EstaEnCurso(alquilerDDBB))
            {
                TempData["Error"] = "Solo se pueden eliminar los alquileres en curso";
                return RedirectToAction(nameof(Index));
            }

            AlquilerVM alquiler = new AlquilerVM
            {
                IdAlquiler = alquilerDDBB.IdAlquiler,
                IdJuego = alquilerDDBB.IdJuego,
                JuegoVM = new JuegoVM
                {
                    IdJuego = alquilerDDBB.IdJuego,
                    DescripcionJuego = alquilerDDBB.Juego!.DescripcionJuego
                },
                FechaRetiro = alquilerDDBB.FechaRetiro,
                FechaDevolucionEstimada = alquilerDDBB.FechaDevolucionEstimada,
                MontoAlquiler = alquilerDDBB.MontoAlquiler,
                MontoTotal = alquilerDDBB.MontoTotal
            };

            return View(alquiler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id, IFormCollection collection)
        {
            var alquilerAEliminar = _alquilerRepository.ObtenerAlquilerPorId(id);
            if (alquilerAEliminar == null || alquilerAEliminar.IdUsuario != _userManager.GetUserId(User) || alquilerAEliminar.Audit.FechaBaja != null)
            {
                return NotFound();
            }

            if (!EstaEnCurso(alquilerAEliminar))
            {
                TempData["Error"] = "Solo se pueden eliminar los alquileres en curso";
                return RedirectToAction(nameof(Index));
            }

            alquilerAEliminar.Audit.FechaBaja = DateTime.Now;
            alquilerAEliminar.Audit.IdUsuarioBaja = _userManager.GetUserId(User);

            if (!_alquilerRepository.BorrarAlquiler(alquilerAEliminar))
            {
                TempData["Error"] = "No se pudo eliminar el alquiler. Probá de nuevo";
                return RedirectToAction(nameof(Index));
            }

            var juego = alquilerAEliminar.Juego!;
            juego.Disponible += 1;
            _juegoRepository.ActualizarJuego(juego);

            TempData["Mensaje"] = $"Se eliminó el alquiler de {juego.DescripcionJuego}";
            return RedirectToAction(nameof(Index));
        }

        private static bool EstaEnCurso(Alquiler alquiler)
        {
            return alquiler.FechaDevolucionReal == null && DateTime.Today <= alquiler.FechaDevolucionEstimada.Date;
        }

        private void CargarJuegosDisponibles(int idJuegoActual)
        {
            var juegosDDBB = _juegoRepository.ObtenerJuegos()
                .Where(x => x.Audit.FechaBaja == null && x.MontoAlquiler != null && (x.Disponible > 0 || x.IdJuego == idJuegoActual))
                .OrderBy(x => x.DescripcionJuego)
                .ToList();

            ViewBag.Juegos = new SelectList(juegosDDBB, "IdJuego", "DescripcionJuego");
            ViewBag.DiasDePlazo = DiasDePlazo;
        }
    }
}
