using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        readonly IUnitOfWork _unit;
        readonly IEmailSender _emailSender;
        [BindProperty]
        public ShoppingCartViewModel ShoppingCartVM { get; set; }

        public int OrderTotal { get; set; }

        public CartController(IUnitOfWork unit, IEmailSender emailSender)
        {
            _unit = unit;
            _emailSender = emailSender;
        }


        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartViewModel() {
                ListCart = _unit.ShoppingCartRepo.GetAll("Product").Where(s => s.ApplicationUserId == claim.Value).ToList(),
                OrderHeader = new OrderHeader()
            };

            foreach(var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }


            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartViewModel()
            {
                ListCart = _unit.ShoppingCartRepo.GetAll("Product").Where(s => s.ApplicationUserId == claim.Value).ToList(),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unit.ApplicationUserRepo.GetFirstOrDefault(u => u.Id == claim.Value);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }


            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST([FromServices] IHttpContextAccessor accessor)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.ListCart = _unit.ShoppingCartRepo.GetAll(includeProperties:"Product").Where(u => u.ApplicationUserId == claim.Value);

            
            ShoppingCartVM.OrderHeader.OrderDay = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

            ApplicationUser applicationUser = _unit.ApplicationUserRepo.GetFirstOrDefault(u => u.Id == claim.Value);
            if (applicationUser.CompanyId != null)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            _unit.OrderHeaderRepo.Add(ShoppingCartVM.OrderHeader);
            _unit.Save();

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                OrderDetails orderDetails = new() { 
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };
                _unit.OrderDetailsRepo.Add(orderDetails);
                _unit.Save();

            }


            if(ShoppingCartVM.OrderHeader.PaymentStatus == SD.PaymentStatusPending)
            {
                //Stripe Settings
                var domain = "https://" + accessor.HttpContext.Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>(),

                    Mode = "payment",
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                };

                foreach (var item in ShoppingCartVM.ListCart)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            },
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                _unit.OrderHeaderRepo.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, null);
                _unit.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }


            return RedirectToAction("OrderConfirmation", new { id = ShoppingCartVM.OrderHeader.Id });

        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader header = _unit.OrderHeaderRepo.GetFirstOrDefault(x => x.Id == id, includeProperties:"ApplicationUser");

            if(header.PaymentStatus == SD.PaymentStatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(header.SessionId);

                //check stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unit.OrderHeaderRepo.UpdateStripePaymentId(id, header.SessionId, session.PaymentIntentId);
                    _unit.OrderHeaderRepo.UpdateStatus(header.Id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unit.Save();
                }
            }

            _emailSender.SendEmailAsync(header.ApplicationUser.Email, "New Order in WebStore1", "<p>New order Created</p>");

            List<ShoppingCart> shoppingCarts = _unit.ShoppingCartRepo.GetAll()
                .Where(u => u.ApplicationUserId == header.ApplicationUserId).ToList();
            HttpContext.Session.SetInt32(SD.SessionCart, 0);

            _unit.ShoppingCartRepo.RemoveRange(shoppingCarts);
            _unit.Save();
            return View(header.Id);
        }



        public IActionResult Plus(int cartId)
        {
            var cart = _unit.ShoppingCartRepo.GetFirstOrDefault(c => c.Id == cartId);
            if(cart == null)
            {
                return NotFound();
            }
            _unit.ShoppingCartRepo.IncrementCount(cart, 1);
            _unit.Save();
            return RedirectToAction("Index");
        }

        
        public IActionResult Minus(int cartId)
        {
            var cart = _unit.ShoppingCartRepo.GetFirstOrDefault(c => c.Id == cartId);
            if (cart == null)
            {
                return NotFound();
            }
            _unit.ShoppingCartRepo.DecrementCount(cart, 1);
            if(cart.Count == 0)
            {
                _unit.ShoppingCartRepo.Remove(cart);
                int count = _unit.ShoppingCartRepo.GetAll().Where(u => u.ApplicationUserId == cart.ApplicationUserId).Count() - 1;
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }
            _unit.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unit.ShoppingCartRepo.GetFirstOrDefault(c => c.Id == cartId);
            if (cart == null)
            {
                return NotFound();
            }
            _unit.ShoppingCartRepo.Remove(cart);
            _unit.Save();
            int count = _unit.ShoppingCartRepo.GetAll().Where(u => u.ApplicationUserId == cart.ApplicationUserId).Count();
            HttpContext.Session.SetInt32(SD.SessionCart, count);
            return RedirectToAction("Index");
        }


        double GetPriceBasedOnQuantity(ShoppingCart cart)
        {
            return cart.Count switch
            {
                1 => cart.Product.ListPrice,
                > 1 and < 50 => cart.Product.Price,
                >=50 and < 100 => cart.Product.Price50,
                >= 100 => cart.Product.Price100,
                _ => 0
            };

        }
    }
}
