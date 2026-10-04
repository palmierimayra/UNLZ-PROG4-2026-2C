using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPLudoteca.Data;
using TPLudoteca.Data.Repositorios;
using TPLudoteca.Models;

namespace TPLudoteca.Controllers
{
    [Authorize(Roles = "Administrador", AuthenticationSchemes = "Identity.Application")]
    public class ReportesController : Controller
    {
        private const decimal PorcentajeMoraPorDia = 0.05m;

        private readonly IAlquilerRepository _alquilerRepository;
        private readonly IJuegoRepository _juegoRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportesController(IAlquilerRepository alquilerRepository, IJuegoRepository juegoRepository, UserManager<ApplicationUser> userManager)
        {
            _alquilerRepository = alquilerRepository;
            _juegoRepository = juegoRepository;
            _userManager = userManager;
        }

        public async Task<ActionResult> Index()
        {
            var sociosDDBB = await _userManager.GetUsersInRoleAsync("Socio");
            var juegosDDBB = _juegoRepository.ObtenerJuegos().Where(x => x.Audit.FechaBaja == null).ToList();
            var alquileresDDBB = _alquilerRepository.ObtenerAlquileres().Where(x => x.Audit.FechaBaja == null).ToList();

            var hoy = DateTime.Today;
            var alquileresEnCurso = alquileresDDBB.Where(x => x.FechaDevolucionReal == null).ToList();

            var alquileresPorJuego = alquileresDDBB
                .GroupBy(x => x.Juego!.DescripcionJuego)
                .OrderByDescending(g => g.Count())
                .ToList();

            var alquileresPorCategoria = alquileresDDBB
                .GroupBy(x => x.Juego!.CategoriaJuego!.DescripcionCategoria)
                .OrderByDescending(g => g.Count())
                .ToList();

            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimosMeses = Enumerable.Range(0, 6)
                .Select(i => primerDiaDelMes.AddMonths(i - 5))
                .ToList();
            var cultura = new CultureInfo("es-AR");

            ReporteTiendaVM reporte = new ReporteTiendaVM
            {
                CantidadSocios = sociosDDBB.Count,
                SociosActivos = sociosDDBB.Count(x => x.LockoutEnd == null || x.LockoutEnd <= DateTimeOffset.Now),
                SociosConAlquilerEnCurso = alquileresEnCurso.Select(x => x.IdUsuario).Distinct().Count(),
                CantidadJuegos = juegosDDBB.Count,
                CopiasTotales = juegosDDBB.Sum(x => x.Cantidad),
                CopiasAlquiladas = alquileresEnCurso.Count,
                AlquileresVencidos = alquileresEnCurso.Count(x => hoy > x.FechaDevolucionEstimada.Date),
                TotalAlquileres = alquileresDDBB.Count,
                JuegoMasAlquilado = alquileresPorJuego.FirstOrDefault()?.Key,
                VecesJuegoMasAlquilado = alquileresPorJuego.FirstOrDefault()?.Count() ?? 0,
                IngresosDelMes = alquileresDDBB.Where(x => x.FechaRetiro >= primerDiaDelMes).Sum(x => x.MontoTotal),
                RecaudadoPorMora = alquileresDDBB.Sum(x => x.MontoRecargo ?? 0),
                CategoriasNombres = alquileresPorCategoria.Select(g => g.Key).ToList(),
                CategoriasCantidades = alquileresPorCategoria.Select(g => g.Count()).ToList(),
                TopJuegosNombres = alquileresPorJuego.Take(5).Select(g => g.Key).ToList(),
                TopJuegosCantidades = alquileresPorJuego.Take(5).Select(g => g.Count()).ToList(),
                MesesNombres = ultimosMeses.Select(m => cultura.TextInfo.ToTitleCase(m.ToString("MMM yyyy", cultura))).ToList(),
                MesesCantidades = ultimosMeses.Select(m => alquileresDDBB.Count(x => x.FechaRetiro.Year == m.Year && x.FechaRetiro.Month == m.Month)).ToList()
            };

            return View(reporte);
        }

        public ActionResult ListadoTransacciones()
        {
            var alquileresDDBB = _alquilerRepository.ObtenerAlquileres();
            var usuariosDDBB = _userManager.Users.ToDictionary(x => x.Id, x => x.Email ?? x.UserName ?? string.Empty);

            alquileresDDBB = alquileresDDBB.Where(x => x.Audit.FechaBaja == null).ToList();
            List<AlquilerVM> listaAlquileres = alquileresDDBB.Select(x => new AlquilerVM
            {
                IdAlquiler = x.IdAlquiler,
                IdUsuario = x.IdUsuario,
                EmailUsuario = usuariosDDBB.GetValueOrDefault(x.IdUsuario),
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
                DiasDemora = x.DiasDemora,
                Recargo = x.Recargo,
                MontoRecargo = x.MontoRecargo,
                MontoTotal = x.MontoTotal
            }).ToList();

            ViewBag.PorcentajeMora = PorcentajeMoraPorDia;

            return View(listaAlquileres);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrarDevolucion(int id, DateTime fechaDevolucionReal)
        {
            var alquilerDDBB = _alquilerRepository.ObtenerAlquilerPorId(id);
            if (alquilerDDBB == null || alquilerDDBB.Audit.FechaBaja != null)
            {
                return NotFound();
            }

            if (alquilerDDBB.FechaDevolucionReal != null)
            {
                TempData["Error"] = "Ese alquiler ya tiene registrada la devolución";
                return RedirectToAction(nameof(ListadoTransacciones));
            }

            if (fechaDevolucionReal.Date < alquilerDDBB.FechaRetiro.Date || fechaDevolucionReal.Date > DateTime.Today)
            {
                TempData["Error"] = "La fecha de devolución tiene que estar entre la fecha de retiro y hoy";
                return RedirectToAction(nameof(ListadoTransacciones));
            }

            var diasDemora = Math.Max(0, (fechaDevolucionReal.Date - alquilerDDBB.FechaDevolucionEstimada.Date).Days);

            alquilerDDBB.FechaDevolucionReal = fechaDevolucionReal.Date;
            alquilerDDBB.DiasDemora = diasDemora;
            alquilerDDBB.Recargo = diasDemora > 0 ? PorcentajeMoraPorDia * 100 : 0;
            alquilerDDBB.MontoRecargo = alquilerDDBB.MontoAlquiler * PorcentajeMoraPorDia * diasDemora;
            alquilerDDBB.MontoTotal = alquilerDDBB.MontoAlquiler + alquilerDDBB.MontoRecargo.Value;
            alquilerDDBB.Audit.FechaModificacion = DateTime.Now;
            alquilerDDBB.Audit.IdUsuarioModificacion = _userManager.GetUserId(User);

            _alquilerRepository.ActualizarAlquiler(alquilerDDBB);

            var juego = alquilerDDBB.Juego!;
            juego.Disponible += 1;
            _juegoRepository.ActualizarJuego(juego);

            TempData["Mensaje"] = diasDemora > 0
                ? $"Devolución registrada con {diasDemora} días de demora. Recargo: $ {alquilerDDBB.MontoRecargo:N2}"
                : "Devolución registrada sin demora";
            return RedirectToAction(nameof(ListadoTransacciones));
        }
    }
}
