using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Models.OrderDetailsViewModel;
using Restaurant.Utility;

namespace Restaurant.Controllers
{
    public class OrderController : Controller
    {

        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context)
        {
            _context = context;

        }

        //Confrim Get
        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {

            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel OrderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = _context.OrderHeaders.Where(o => o.Id == id && o.UserId == claim.Value).FirstOrDefault(),
                OrderDetails = _context.orderDetails.Where(o => o.Id == id).ToList()
            };
            return View(OrderDetailsViewModel);
        }

        [Authorize]
        public IActionResult OrderHistory()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            List<OrderDetailsViewModel> OrderDetailsVM = new List<OrderDetailsViewModel>();
            List<OrderHeader> OrderHeadersList = _context.OrderHeaders.Where(o => o.UserId == claim.Value).OrderByDescending(o => o.OrderDate).ToList();

            foreach (OrderHeader item in OrderHeadersList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel();
                individual.OrderHeader = item;
                individual.OrderDetails = _context.orderDetails.Where(o => o.OrderId == item.Id).ToList();
                OrderDetailsVM.Add(individual);

            }

            return View(OrderDetailsVM);
        }

        [Authorize(Roles = SD.AdminUser)]
        public IActionResult ManageOrder()

        {
            List<OrderDetailsViewModel> OrderDetailsVM = new List<OrderDetailsViewModel>();

            List<OrderHeader> OrderHeadersList = _context.OrderHeaders.Where(o => o.Status == SD.StatusSubmitted || o.Status == SD.StatusInProcess)
                .OrderByDescending(o => o.PickUpTime).ToList();

            foreach (OrderHeader item in OrderHeadersList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel();
                individual.OrderHeader = item;
                individual.OrderDetails = _context.orderDetails.Where(o => o.OrderId == item.Id).ToList();
                OrderDetailsVM.Add(individual);

            }
            return View(OrderDetailsVM);
        }

        [Authorize(Roles =(SD.AdminUser))]
        public async Task<IActionResult> OrderPrepare(int orderId)
        {

            OrderHeader orderHeader = _context.OrderHeaders.Find(orderId);
            orderHeader.Status = SD.StatusInProcess;
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = (SD.AdminUser))]
        public async Task<IActionResult> Ordercancel(int orderId)
        {

            OrderHeader orderHeader = _context.OrderHeaders.Find(orderId);
            orderHeader.Status = SD.StatusCancelled;
           await  _context.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = (SD.AdminUser))]
        public async Task< IActionResult> OrderReady(int orderId)
        {

            OrderHeader orderHeader = _context.OrderHeaders.Find(orderId);
            orderHeader.Status = SD.StatusReady;
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }

        //Get Order Pickup

        public IActionResult OrderPickup(string searchOrder = null, string searchPhone = null, string searchEmail = null)
        {
            List<OrderDetailsViewModel> OrderDetailsVM = new List<OrderDetailsViewModel>();

            if (searchEmail != null || searchPhone != null || searchOrder != null)
            {

                //filtering criteria
                var user = new ApplicationUser();
                List<OrderHeader> OrderHeaderList = new List<OrderHeader>();

                if (searchOrder != null)
                {
                    OrderHeaderList = _context.OrderHeaders.Where(o => o.Id == Convert.ToInt32(searchOrder)).ToList();
                }
                else
                {
                    if (searchEmail != null)
                    {
                        user = _context.Users.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower())).FirstOrDefault();
                    }
                    else
                    {
                        if (searchPhone != null)
                        {
                            user = _context.Users.Where(u => u.PhoneNumber.ToLower().Contains(searchPhone.ToLower())).FirstOrDefault();
                        }

                    }
                }
                if (user != null || OrderHeaderList.Count > 0)
                {

                    if (OrderHeaderList.Count == 0)
                    {
                        OrderHeaderList = _context.OrderHeaders.Where(o => o.UserId == user.Id).OrderByDescending(o => o.OrderDate).ToList();
                    }

                    foreach (OrderHeader item in OrderHeaderList)
                    {
                        OrderDetailsViewModel individual = new OrderDetailsViewModel
                        {
                            OrderHeader = item,
                            OrderDetails = _context.orderDetails.Where(o => o.OrderId == item.Id).ToList()
                        };
                        OrderDetailsVM.Add(individual);

                    }

                }
            }

            else
            {
                List<OrderHeader> OrderHeadersList = _context.OrderHeaders.Where(o => o.Status == SD.StatusReady)
                 .OrderByDescending(o => o.PickUpTime).ToList();

                foreach (OrderHeader item in OrderHeadersList)
                {
                    OrderDetailsViewModel individual = new OrderDetailsViewModel
                    {
                        OrderHeader = item,
                        OrderDetails = _context.orderDetails.Where(o => o.OrderId == item.Id).ToList()
                    };
                    OrderDetailsVM.Add(individual);

                }
            }
         
            return View(OrderDetailsVM);
        }


        [Authorize(Roles = SD.AdminUser)]
        public IActionResult OrderPickupDetails(int orderId)
        {

            OrderDetailsViewModel OrderDetatilsVM = new OrderDetailsViewModel
            {
                OrderHeader = _context.OrderHeaders.Where(o => o.Id == orderId).FirstOrDefault()
            };
            OrderDetatilsVM.OrderHeader.ApplicationUser = _context.Users.Where(u => u.Id == OrderDetatilsVM.OrderHeader.UserId).FirstOrDefault();
            OrderDetatilsVM.OrderDetails = _context.orderDetails.Where(o => o.OrderId == OrderDetatilsVM.OrderHeader.Id).ToList();

            return View(OrderDetatilsVM);
        }
    }
}