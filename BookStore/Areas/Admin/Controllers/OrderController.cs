using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Stripe;
using Stripe.Checkout;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        readonly IUnitOfWork _unit;

        [BindProperty]
        public OrderViewModel OrderVM { get; set; }

        public OrderController(IUnitOfWork unit)
        {
            _unit = unit;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = _unit.OrderHeaderRepo.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = _unit.OrderDetailsRepo.GetAll("Product").Where(u => u.OrderId == orderId).ToList()
            };
            return View(OrderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details([FromServices] IHttpContextAccessor accessor)
        {
            OrderVM.OrderHeader = _unit.OrderHeaderRepo.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetails = _unit.OrderDetailsRepo.GetAll("Product").Where(u => u.OrderId == OrderVM.OrderHeader.Id).ToList();
            var domain = "https://" + accessor.HttpContext.Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?id={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"customer/order/details?id={OrderVM.OrderHeader.Id}",
            };

            foreach (var item in OrderVM.OrderDetails)
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
            _unit.OrderHeaderRepo.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, null);
            _unit.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
            return View(OrderVM);
        }


        public IActionResult PaymentConfirmation(int id)
        {
            OrderHeader header = _unit.OrderHeaderRepo.GetFirstOrDefault(x => x.Id == id);
      
            var service = new SessionService();          
            Session session = service.Get(header.SessionId);
                
            //check stripe status
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unit.OrderHeaderRepo.UpdateStripePaymentId(id, header.SessionId, session.PaymentIntentId);
                _unit.OrderHeaderRepo.UpdateStatus(header.Id, header.OrderStatus, SD.PaymentStatusApproved);
                _unit.Save();
            }


            return View(header.Id);
        }


        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult UpdateOrderDetail()
        {
            OrderHeader orderHeaderFromDb = _unit.OrderHeaderRepo.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if(OrderVM.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if(OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _unit.Save();
            TempData["Success"] = "Order Details Updated Successfully";
            return RedirectToAction("Details", new {orderId = OrderVM.OrderHeader.Id });
        }

        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult StartProcessing()
        {
            _unit.OrderHeaderRepo.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unit.Save();
            TempData["Success"] = "Order Status Updated Successfully";
            return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.Id });
        }

        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeaderFromDb = _unit.OrderHeaderRepo.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = OrderVM.OrderHeader.OrderStatus;
            orderHeaderFromDb.ShippingDay = DateTime.Now; 
            if(orderHeaderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            _unit.OrderHeaderRepo.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusShipped);
            _unit.Save();
            TempData["Success"] = "Order Shipped Successfully";
            return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.Id });
        }

        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult CancelOrder()
        {
            OrderHeader orderHeaderFromDb = _unit.OrderHeaderRepo.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            if(orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntentId,
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _unit.OrderHeaderRepo.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unit.OrderHeaderRepo.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            _unit.Save();
            TempData["Success"] = "Order Cancelled Successfully";
            return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.Id });
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string? status)
        {
            IEnumerable<OrderHeader> orderHeaders = _unit.OrderHeaderRepo.GetAll(includeProperties: "ApplicationUser");

            if (!(User.IsInRole(SD.RoleAdmin) || User.IsInRole(SD.RoleEmployee)))
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = orderHeaders.Where(u => u.ApplicationUserId == claim.Value);
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }


            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
