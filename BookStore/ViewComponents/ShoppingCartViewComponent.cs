using BookStore.DataAccess.Repository.IRepository;
using BookStore.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStore.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        readonly IUnitOfWork _unit;

        public ShoppingCartViewComponent(IUnitOfWork unit)
        {
            _unit = unit;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim != null)
            {
                if (HttpContext.Session.GetInt32(SD.SessionCart) != null){
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }
                else
                {
                    HttpContext.Session.SetInt32(SD.SessionCart,
                        _unit.ShoppingCartRepo.GetAll().Where(u => u.ApplicationUserId == claim.Value).Count());
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
