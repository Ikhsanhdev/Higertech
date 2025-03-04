using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Higertech.Models;
using System.Threading.Tasks;
using Higertech.Interfaces;

namespace Higertech.Controllers;

public class TutorialController : Controller
{
    
    private readonly ILogger<TutorialController> _logger;
    private readonly IUnitOfWorkRepository _unitOfWorkRepository;

    public TutorialController( IUnitOfWorkRepository unitOfWorkRepository)
    {
        this._unitOfWorkRepository = unitOfWorkRepository;
    }

    public async Task<IActionResult> Index()
    {
        
        var model = await _unitOfWorkRepository.Tutorial.GetListTutorialAsync();
        return View(model);
    }
    
    // [Route("/article/{slug}")]
    // public async Task<IActionResult> Detail(string slug)
    // {
    //     var model = await _unitOfWorkRepository.Article.GetArticleBySlugAsync(slug);

    //     if (model == null)
    //     {
    //         return View("~/Views/404/PageNotFound.cshtml");
    //     }
    //     return View(model);
    // }
    
    public IActionResult Privacy()
    {
        return View();
    }

    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
