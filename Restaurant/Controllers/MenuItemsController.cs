using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Models.MenuItemViewModel;
using Restaurant.Utility;

namespace Restaurant.Controllers
{
    [Authorize(Roles = SD.AdminUser)]
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _hostingEnvironment;
        [BindProperty]
        public MenuItemViewModel MenuItemViewModel { get; set; }

        public MenuItemsController(ApplicationDbContext db, IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            MenuItemViewModel = new MenuItemViewModel()
            {
                Categories = _db.Categories.ToList(),
                MenuItem = new Models.MenuItem()
            };
        }

     //Get
        public async Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync();
            return View(menuItems);
        }

        //Get : Menu Item
        public IActionResult Create()
        {
            return View(MenuItemViewModel);
        }

        //Post : MenuItems create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            MenuItemViewModel.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if(!ModelState.IsValid)
            {
                return View(MenuItemViewModel);
            }
            _db.MenuItems.Add(MenuItemViewModel.MenuItem);
            await _db.SaveChangesAsync();

            //Image being saved

            //image saving
            string webrootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var menuItemsFromDb = _db.MenuItems.Find(MenuItemViewModel.MenuItem.Id);
            if (files[0]!= null && files[0].Length>0)
            {
                //image uploaded
                var uploads = Path.Combine(webrootPath, "images");
                var extension = files[0].FileName.Substring(files[0].FileName.LastIndexOf("."), files[0].FileName.Length - files[0].FileName.LastIndexOf("."));

                using (var filestream = new FileStream(Path.Combine(uploads, MenuItemViewModel.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }

                menuItemsFromDb.Image = @"/images/" + MenuItemViewModel.MenuItem.Id + extension;

            }

            else
            {
                //when user does not upload image
                var uploads = Path.Combine(webrootPath, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webrootPath + @"\images\" + MenuItemViewModel.MenuItem.Id + ".png");
                menuItemsFromDb.Image = @"/images/" + MenuItemViewModel.MenuItem.Id + ".png";

            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Edit(int?id)
        {
            if(id==null)
            {
                return NotFound();
            }
            MenuItemViewModel.MenuItem = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemViewModel.SubCategories = _db.SubCategories.Where(m => m.CategoryId == MenuItemViewModel.MenuItem.CategoryId).ToList();

            if (MenuItemViewModel.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemViewModel);
           

        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            MenuItemViewModel.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (id!= MenuItemViewModel.MenuItem.Id)
            {
                return NotFound();
            }

            if(ModelState.IsValid)
            {
                try
                {
                    string webrootpath = _hostingEnvironment.WebRootPath;
                    var files = HttpContext.Request.Form.Files;
                    var menuItemfromdb = _db.MenuItems.Where(m => m.Id == MenuItemViewModel.MenuItem.Id).FirstOrDefault();
                    if (files[0].Length > 0 && files[0] != null)
                    {
                        //if user uploads a new file
                        var uploads = Path.Combine(webrootpath, "images");
                        var extension_new = files[0].FileName.Substring(files[0].FileName.LastIndexOf("."), files[0].FileName.Length - files[0].FileName.LastIndexOf("."));
                        var extension_old = menuItemfromdb.Image.Substring(files[0].FileName.LastIndexOf("."), files[0].FileName.Length - files[0].FileName.LastIndexOf("."));

                        if (System.IO.File.Exists(Path.Combine(uploads, MenuItemViewModel.MenuItem.Id + extension_old)))
                        {
                            System.IO.File.Delete(Path.Combine(uploads, MenuItemViewModel.MenuItem.Id + extension_old));

                        }
                        using (var filestream = new FileStream(Path.Combine(uploads, MenuItemViewModel.MenuItem.Id + extension_new), FileMode.Create))
                        {
                            files[0].CopyTo(filestream);
                        }

                        MenuItemViewModel.MenuItem.Image = @"/images/" + MenuItemViewModel.MenuItem.Id + extension_new;
                    }

                    if (MenuItemViewModel.MenuItem.Image != null)
                    {
                        menuItemfromdb.Image = MenuItemViewModel.MenuItem.Image;
                    }

                    menuItemfromdb.Name = MenuItemViewModel.MenuItem.Name;
                    menuItemfromdb.Description = MenuItemViewModel.MenuItem.Description;
                    menuItemfromdb.Price = MenuItemViewModel.MenuItem.Price;
                    menuItemfromdb.Spicyness = MenuItemViewModel.MenuItem.Spicyness;
                    menuItemfromdb.CategoryId = MenuItemViewModel.MenuItem.CategoryId;
                    menuItemfromdb.SubCategoryId = MenuItemViewModel.MenuItem.SubCategoryId;


                    await _db.SaveChangesAsync();
                    

                }
                catch(Exception ex)
                {

                }
                return RedirectToAction(nameof(Index));

            }
            MenuItemViewModel.SubCategories = _db.SubCategories.Where(s => s.CategoryId == MenuItemViewModel.MenuItem.CategoryId).ToList();
            return View(MenuItemViewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemViewModel.MenuItem = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            if (MenuItemViewModel.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemViewModel);


        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemViewModel.MenuItem = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            if (MenuItemViewModel.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemViewModel);


        }

        //Post delete
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>DeleteConfirmed(int id)
        {

            string webrootpath = _hostingEnvironment.WebRootPath;
            MenuItem menuItem = await _db.MenuItems.FindAsync(id);

            if (menuItem != null)
            { 
                var uploads = Path.Combine(webrootpath, "images");
                var extension = menuItem.Image.Substring(menuItem.Image.LastIndexOf("."), menuItem.Image.Length - menuItem.Image.LastIndexOf("."));

              var imagepath = Path.Combine(uploads, menuItem.Id + extension);
              if (System.IO.File.Exists(imagepath))
                {
                    System.IO.File.Delete(imagepath);
                }
                _db.MenuItems.Remove(menuItem);
                await _db.SaveChangesAsync();
                
            }
            return RedirectToAction(nameof(Index));
        }

        public JsonResult GetSubCategory(int CategoryId)
        {
            List<SubCategory> subCategorylist = new List<SubCategory>();
            subCategorylist = (from s in _db.SubCategories
                               where s.CategoryId == CategoryId
                               select s).ToList();
            return Json(new SelectList(subCategorylist, "Id", "Name"));
        }
    }
}