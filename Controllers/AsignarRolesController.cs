using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TPLudoteca.Controllers
{
    public class AsignarRolesController : Controller
    {
        // GET: AsignarRolesController
        public ActionResult Index()
        {
            return View();
        }

        // GET: AsignarRolesController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AsignarRolesController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AsignarRolesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AsignarRolesController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AsignarRolesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AsignarRolesController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AsignarRolesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
