using Microsoft.AspNetCore.Mvc;

namespace LanchesMac.Controllers
{
    public class CategoriaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
