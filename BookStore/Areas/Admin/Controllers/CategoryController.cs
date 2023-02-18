using Microsoft.AspNetCore.Mvc;
using BookStore.Models;
using BookStore.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using BookStore.Utilities;

namespace BookStore.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class CategoryController : Controller
    {
        readonly IUnitOfWork _unit;

        public CategoryController(IUnitOfWork unit)
        {
            _unit = unit;
        }
        
        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList = _unit.CategoryRepo.GetAll();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if(obj.Name == obj.dispalyOrder.ToString())
            {
                TempData["error"] = "Display order can\'t be the same as name";    
                return View(obj);
            }
            if (ModelState.IsValid)
            {
                _unit.CategoryRepo.Add(obj);
                _unit.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _unit.CategoryRepo.GetFirstOrDefault(x => x.Id == id);
            //var caategoryFromDbFirst = _dbContextApp.Categories.FirstOrDefault(x => x.Id == id);

            if(categoryFromDb == null)
            {
                return NotFound();  
            }

            return View(categoryFromDb);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.dispalyOrder.ToString())
            {
                TempData["error"] = "Display Order can\'t be the same as name";
                return View(obj);
            }
            if (ModelState.IsValid)
            {
                _unit.CategoryRepo.Update(obj);
                _unit.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _unit.CategoryRepo.GetFirstOrDefault(x => x.Id == id);
            //var caategoryFromDbFirst = _dbContextApp.Categories.FirstOrDefault(x => x.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var categoryFromDb = _unit.CategoryRepo.GetFirstOrDefault(x => x.Id == id);
            //var caategoryFromDbFirst = _dbContextApp.Categories.FirstOrDefault(x => x.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            _unit.CategoryRepo.Remove(categoryFromDb);
            _unit.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
