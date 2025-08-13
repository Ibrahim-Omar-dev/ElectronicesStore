using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElectronicsStore.Models;
using ElectronicsStore.RepositoryAndUnitOfWork;

namespace ElectronicsStore.Controllers;

public class HomeController : Controller
{
    private readonly IRepository<Product> repository;

    public HomeController(IRepository<Product> repository)
    {
        this.repository = repository;
    }

    public IActionResult Index()
    {
        var product = repository.GetAllAsync().Result;
        return View(product);
    }

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
