using LanchesMac.Repositories.Interfaces;
using LanchesMac.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LanchesMac.Controllers
{
    public class LancheController : Controller
    {
        private readonly ILancheRepository _lancheRepository;
        public LancheController(ILancheRepository lancheRepository)
        {
            _lancheRepository = lancheRepository;
        }

        public IActionResult List()
        {
            //ViewData["Titulo"] = "Todos os Lanches";
            ViewData["Data"] = DateTime.Now;

            //var lanches = _lancheRepository.Lanches;
            //var totalLanches = lanches.Count();
            var lanchesListViewModel = new LancheListViewModel();
            lanchesListViewModel.Lanches = _lancheRepository.Lanches;
            lanchesListViewModel.CategoriaAtual = "Categoria Atual";

            ViewBag.Total = "Total de lanches";
            ViewBag.TotalLanches = lanchesListViewModel.Lanches.Count();   


            return View(lanchesListViewModel);

        }
    }
}
