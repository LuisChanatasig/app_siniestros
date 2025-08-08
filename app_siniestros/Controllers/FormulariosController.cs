using Microsoft.AspNetCore.Mvc;

namespace app_siniestros.Controllers
{
    public class FormulariosController : Controller
    {
        public IActionResult Industrias(int? step)
        {
            if (step.HasValue)
            {
                // Lógica para manejar el paso
                // Por ejemplo, pasar el paso a la vista a través del ViewBag o un ViewModel
                ViewBag.CurrentStep = step.Value;
            }
            return View();
        }
    }
}
