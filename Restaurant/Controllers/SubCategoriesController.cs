using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Models.SubCategoryViewModel;
using Restaurant.Utility;

namespace Restaurant.Controllers
{
    [Authorize(Roles = SD.AdminUser)]
    public class SubCategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        [TempData]
        public string StatusMessage { get; set; }

        public SubCategoriesController(ApplicationDbContext db)
        {
            _db = db; 
        }
        public async Task<IActionResult> Index()
        {
            var subcategory = _db.SubCategories.Include(s => s.Category);

            return View(await subcategory.ToListAsync());
        }

        //Get action for create
        public IActionResult Create()
        {
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel
            {
                CategoryList = _db.Categories.ToList(),
                SubCategory = new SubCategory(),
                SubCategoryList = _db.SubCategories.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToList()

            };
            return View(model);
        }
        //Post Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if(ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategories.Where(s => s.Name == model.SubCategory.Name).Count();
                var doesSubCatAndCatExists = _db.SubCategories.Where(s => s.Name == model.SubCategory.Name && s.CategoryId == model.SubCategory.CategoryId).Count();

                if(doesSubCategoryExists > 0 && model.isNew)
                {
                    //error
                    StatusMessage = "Error : Sub Category Already Exist";
                }
                else
                {
                    if (doesSubCategoryExists == 0 && !model.isNew)
                    {
                        //error 
                        StatusMessage = "Error : Sub Category does not exist";
                    }
                    else
                    {
                        if(doesSubCatAndCatExists>0)
                        {
                            //error
                            StatusMessage = "Error : Category and sub Category combination exists";
                        }
                        else
                        {
                            _db.Add(model.SubCategory);
                             await  _db.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));

                        }
                    }
                }
               
            }

            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel
            {
                CategoryList = _db.Categories.ToList(),
                SubCategory = model.SubCategory,
                SubCategoryList = _db.SubCategories.OrderBy(p => p.Name).Select(p => p.Name).ToList(),
                StatusMessage = StatusMessage
            };

            return View(modelVM);
        }

        //Get Edit
        public async Task<IActionResult> Edit(int?id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subcategory = await _db.SubCategories.SingleOrDefaultAsync(m => m.Id == id);
            if(subcategory == null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel
            {
                CategoryList = _db.Categories.ToList(),
                SubCategory = subcategory,
                SubCategoryList = _db.SubCategories.OrderBy(p => p.Name).Select(p => p.Name).ToList(),
            };

            return View(modelVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategories.Where(s => s.Name == model.SubCategory.Name).Count();
                var doesSubCatAndCatExists = _db.SubCategories.Where(s => s.Name == model.SubCategory.Name && s.CategoryId == model.SubCategory.CategoryId).Count();

                if (doesSubCategoryExists == 0)
                {
                    //error
                    StatusMessage = "Error : Sub Category does not exist . You cannot add sub category here";
                }
                else
                {
                    if (doesSubCatAndCatExists > 0)
                    {
                        //error 
                        StatusMessage = "Error : Category and sub Category combination exists";

                    }
                    else
                    {
                        var suCardformdb = _db.SubCategories.Find(id);
                        suCardformdb.Name = model.SubCategory.Name;
                        suCardformdb.CategoryId = model.SubCategory.CategoryId;
                        await _db.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }

            }

            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel
            {
                CategoryList = _db.Categories.ToList(),
                SubCategory = model.SubCategory,
                SubCategoryList = _db.SubCategories.OrderBy(p => p.Name).Select(p => p.Name).ToList(),
                StatusMessage = StatusMessage
            };

            return View(modelVM);
        }
        //Get Details
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var suCardformdb = await _db.SubCategories.Include(s => s.Category).SingleOrDefaultAsync(m => m.Id == id);

            if(suCardformdb == null)
            {
                return NotFound();
            }

            return View(suCardformdb);
        }
        //Get Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var suCardformdb = await _db.SubCategories.Include(s => s.Category).SingleOrDefaultAsync(m => m.Id == id);

            if (suCardformdb == null)
            {
                return NotFound();
            }

            return View(suCardformdb);
        }

        //Post delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var suCardformdb = await _db.SubCategories.SingleOrDefaultAsync(m => m.Id == id);
            _db.SubCategories.Remove(suCardformdb);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}