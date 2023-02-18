using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BookStore.Models;
using BookStore.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookStore.Utilities;

namespace BookStore.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        readonly IUnitOfWork _unit;
        readonly IWebHostEnvironment _host;

        public HomeController(IUnitOfWork unit, IWebHostEnvironment host, ILogger<HomeController> logger)
        {
            _logger = logger;
            _unit = unit;
            _host = host;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unit.ProductRepo.GetAll(includeProperties: "Category,CoverType");

            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart obj = new()
            {
                Product = _unit.ProductRepo.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category,CoverType"), 
                ProductId = productId,
                Count = 1
            };

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart obj)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            obj.ApplicationUserId = claim.Value;

            ShoppingCart shoppingCartFromDb = _unit.ShoppingCartRepo.GetFirstOrDefault(
                s =>(s.ApplicationUserId == claim.Value) && (s.ProductId == obj.ProductId)
            );

            if(shoppingCartFromDb == null)
            {
                _unit.ShoppingCartRepo.Add(obj);
            }
            else
            {
                _unit.ShoppingCartRepo.IncrementCount(shoppingCartFromDb, obj.Count);
            }
            _unit.Save();
            HttpContext.Session.SetInt32(SD.SessionCart, _unit.ShoppingCartRepo.GetAll().
                    Where(u => u.ApplicationUserId == claim.Value).Count());

            return RedirectToAction(nameof(Index));
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
}