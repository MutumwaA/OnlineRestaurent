using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Models.HomeViewModel;

namespace Restaurant.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            IndexViewModel IndexVM = new IndexViewModel
            {
                MenuItems = await _context.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync(),
                Categories = _context.Categories.OrderBy(c => c.DisplayOrder),
                Coupons = _context.Coupons.Where(c => c.IsActive == true)
            };
            return View(IndexVM);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var MenuItemFromDB = await _context.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == id).FirstOrDefaultAsync();

            ShoppingCart Cartobj = new ShoppingCart()
            {
                MenuItem = MenuItemFromDB,
                MenuItemId = MenuItemFromDB.Id
            };

            return View(Cartobj);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart CrdObj)
        {
            CrdObj.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CrdObj.ApplicationUserId = claim.Value;

                ShoppingCart cardFromDb = _context.ShoppingCart.Where(c => c.ApplicationUserId == CrdObj.ApplicationUserId
                                             && c.MenuItemId == CrdObj.MenuItemId).FirstOrDefault();
                if (cardFromDb == null)
                {
                    //this menu item doesnot exist
                    _context.ShoppingCart.Add(CrdObj);

                }

                else
                {
                    cardFromDb.Count = cardFromDb.Count + CrdObj.Count;
                    //menu item exists
                }

                await _context.SaveChangesAsync();

                var count = _context.ShoppingCart.Where(c => c.ApplicationUserId == CrdObj.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32("CartCout", count);

                return RedirectToAction(nameof(Index));
            }

            var MenuItemFromDB = await _context.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == CrdObj.MenuItemId).FirstOrDefaultAsync();

            ShoppingCart Cartobj = new ShoppingCart()
            {
                MenuItem = MenuItemFromDB,
                MenuItemId = MenuItemFromDB.Id
            };
            return View(Cartobj);
        }

    }
}
