using Microsoft.AspNetCore.Mvc;
using BookStore.DataAccess;
using BookStore.Models;
using BookStore.DataAccess.Repository.IRepository;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using BookStore.Utilities;

namespace BookStore.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class ProductController : Controller
    {
        readonly IUnitOfWork _unit;
        readonly IWebHostEnvironment _host; 

        public ProductController(IUnitOfWork unit, IWebHostEnvironment host)
        {
            _unit = unit;
            _host = host;
        }
        
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Upsert(int? id)
        {
            ProductViewModel productVM = new()
            {
                Product = new(),
                CategoryList = _unit.CategoryRepo.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                }),
                CoverTypeList = _unit.CoverTypeRepo.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                })
            };
            //Product product = new();
            //IEnumerable<SelectListItem> CategoryList = _unit.CategoryRepo.GetAll().Select( c => new SelectListItem {
            //    Text = c.Name,
            //    Value = c.Id.ToString(),
            //});
            //IEnumerable<SelectListItem> CoverTypeList = _unit.CoverTypeRepo.GetAll().Select(c => new SelectListItem
            //{
            //    Text = c.Name,
            //    Value = c.Id.ToString(),
            //});
            if (id == null || id == 0)
            {
                //Create product
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(productVM);
            }
            var productFromDb = _unit.ProductRepo.GetFirstOrDefault(x => x.Id == id);
            //var caategoryFromDbFirst = _dbContextApp.Categories.FirstOrDefault(x => x.Id == id);

            if(productFromDb == null)
            {
                return NotFound();  
            }
            productVM.Product = productFromDb;

            return View(productVM);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _host.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"Images\Products");
                    var extension = Path.GetExtension(file.FileName);

                    if(obj.Product.ImageUrl != null)
                    {
                        deleteImage(obj.Product);
                    }

                    using(var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }

                    obj.Product.ImageUrl = @"\Images\Products\" + fileName + extension;   

                }
                if(obj.Product.Id == 0)
                {
                    _unit.ProductRepo.Add(obj.Product);
                    TempData["success"] = "Product created successfully";
                }
                else
                {
                    _unit.ProductRepo.Update(obj.Product);
                    TempData["success"] = "Product updated successfully";
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
            var productList = _unit.ProductRepo.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = productList });
        }

        //POST
        [HttpDelete] 
        public IActionResult Delete(int? id)
        {
            var productFromDb = _unit.ProductRepo.GetFirstOrDefault(x => x.Id == id);

            if (productFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            deleteImage(productFromDb);

            _unit.ProductRepo.Remove(productFromDb);
            _unit.Save();
            return Json(new { success = true, message = "Deleted successfully" });
        }
        #endregion

        void deleteImage(Product obj)
        {
            var oldImagePath = Path.Combine(_host.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
        }
    }
}
