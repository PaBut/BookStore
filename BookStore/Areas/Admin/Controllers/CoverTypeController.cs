using Microsoft.AspNetCore.Mvc;
using BookStore.DataAccess;
using BookStore.Models;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class CoverTypeController : Controller
    {
        readonly IUnitOfWork _unit;

        public CoverTypeController(IUnitOfWork unit)
        {
            _unit = unit;
        }
        
        public IActionResult Index()
        {
            IEnumerable<CoverType> objCategoryList = _unit.CoverTypeRepo.GetAll();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _unit.CoverTypeRepo.Add(obj);
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
            var coverTypeFromDb = _unit.CoverTypeRepo.GetFirstOrDefault(x => x.Id == id);
            //var caategoryFromDbFirst = _dbContextApp.Categories.FirstOrDefault(x => x.Id == id);

            if(coverTypeFromDb == null)
            {
                return NotFound();  
            }

            return View(coverTypeFromDb);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _unit.CoverTypeRepo.Update(obj);
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
            var coverTypeFromDb = _unit.CoverTypeRepo.GetFirstOrDefault(x => x.Id == id);
            //var caategoryFromDbFirst = _dbContextApp.Categories.FirstOrDefault(x => x.Id == id);

            if (coverTypeFromDb == null)
            {
                return NotFound();
            }

            return View(coverTypeFromDb);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var coverTypeFromDb = _unit.CoverTypeRepo.GetFirstOrDefault(x => x.Id == id);
            //var caategoryFromDbFirst = _dbContextApp.Categories.FirstOrDefault(x => x.Id == id);

            if (coverTypeFromDb == null)
            {
                return NotFound();
            }
            _unit.CoverTypeRepo.Remove(coverTypeFromDb);
            _unit.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
