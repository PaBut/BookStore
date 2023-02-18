using Microsoft.AspNetCore.Mvc;
using BookStore.DataAccess;
using BookStore.Models;
using BookStore.DataAccess.Repository.IRepository;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookStore.Models.ViewModels;
using BookStore.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace BookStore.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class CompanyController : Controller
    {
        readonly IUnitOfWork _unit;

        public CompanyController(IUnitOfWork unit)
        {
            _unit = unit;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }


        public IActionResult Upsert(int? id)
        {
            Company obj = new();
            if (id == null || id == 0)
            {
                //Create product
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(obj);
            }
            obj = _unit.CompanyRepo.GetFirstOrDefault(x => x.Id == id);
            //var caategoryFromDbFirst = _dbContextApp.Categories.FirstOrDefault(x => x.Id == id);

            if(obj == null)
            {
                return NotFound();  
            }

            return View(obj);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                
                if(obj.Id == 0)
                {
                    _unit.CompanyRepo.Add(obj);
                    TempData["success"] = "Company added successfully";
                }
                else
                {
                    _unit.CompanyRepo.Update(obj);
                    TempData["success"] = "Company updated successfully";
                }
                _unit.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        

        #region API Call
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unit.CompanyRepo.GetAll();
            return Json(new { data = companyList });
        }

        //POST
        [HttpDelete] 
        public IActionResult Delete(int? id)
        {
            var companyFromDb = _unit.CompanyRepo.GetFirstOrDefault(x => x.Id == id);

            if (companyFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unit.CompanyRepo.Remove(companyFromDb);
            _unit.Save();
            return Json(new { success = true, message = "Deleted successfully" });
        }
        #endregion
    }
}
