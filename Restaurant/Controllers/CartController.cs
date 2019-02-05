using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Models.OrderDetailsViewModel;
using Restaurant.Utility;

namespace Restaurant.Controllers
{
    public class CartController : Controller
    {

        private readonly ApplicationDbContext _context;
        [BindProperty]
        public OrderDetailsCart detailsCart { get; set; }
        public CartController(ApplicationDbContext context)
        {
            _context = context;
         
        }
        public IActionResult Index()
        {
            detailsCart = new OrderDetailsCart
            {
                OrderHeader = new OrderHeader()
            };
            detailsCart.OrderHeader.OrderTotal = 0;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var cart = _context.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);

            if (cart != null)
            {
                detailsCart.ListCart = cart.ToList();
            }

            foreach (var list in detailsCart.ListCart)
            {
                list.MenuItem = _context.MenuItems.FirstOrDefault(m => m.Id == list.MenuItemId);
                detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
                if (list.MenuItem.Description.Length > 0)
                {
                    list.MenuItem.Description = list.MenuItem.Description.Substring(0, 99) + "....";
                }
            }
            detailsCart.OrderHeader.PickUpTime = DateTime.Now;

            return View(detailsCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]

        public IActionResult IndexPost()
        {

            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            detailsCart.ListCart = _context.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToList();

            detailsCart.OrderHeader.OrderDate = DateTime.Now;
            detailsCart.OrderHeader.UserId = claim.Value;
            detailsCart.OrderHeader.Status = SD.StatusSubmitted;

            OrderHeader orderHeader = detailsCart.OrderHeader;
            _context.OrderHeaders.Add(orderHeader);
            _context.SaveChanges();

            foreach (var item in detailsCart.ListCart)
            {
                item.MenuItem = _context.MenuItems.FirstOrDefault(m => m.Id == item.MenuItemId);
                OrderDetails orderDetails = new OrderDetails
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = orderHeader.Id,
                    Description = item.MenuItem.Description,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count

                };

                _context.orderDetails.Add(orderDetails);

            }
            _context.ShoppingCart.RemoveRange(detailsCart.ListCart);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("CartCout", 0);
            return RedirectToAction("Confirm", "Order",new { id = orderHeader.Id});


        }


        public IActionResult Plus(int cartId)
        {
            var cart = _context.ShoppingCart.Where(c => c.Id == cartId).FirstOrDefault();
            cart.Count += 1;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _context.ShoppingCart.Where(c => c.Id == cartId).FirstOrDefault();

            if (cart.Count == 1)
            {
                _context.ShoppingCart.Remove(cart);
                _context.SaveChanges();

                var cart1 = _context.ShoppingCart.Where(s => s.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32("CartCout", cart1);

            }
            else
            {
                cart.Count -= 1;
                _context.SaveChanges();

            }
            return RedirectToAction(nameof(Index));
        }






    }
}