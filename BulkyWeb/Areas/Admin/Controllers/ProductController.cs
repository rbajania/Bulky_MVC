﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(products);
        }
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll()
                .Select(u => new SelectListItem()
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

            ProductVM productVM = new()
            {
                CategoryList = CategoryList,
                Product = new Product()
            };

            if (id == null || id == 0) //create
            {
                return View(productVM);
            }
            else //update
            {
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Category");

                //string wwwRootPath = _webHostEnvironment.WebRootPath;
                //string productPath = Path.Combine(wwwRootPath, @"images\product");
                //string filePath = Path.Combine(productPath, productVM.Product.ImageUrl);
                //productVM.Product.ImageUrl = filePath;
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if(!string.IsNullOrWhiteSpace(productVM.Product.ImageUrl))
                    {
                        //delete the old image
                        var oldImagePath = Path.Combine(
                            wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\')
                            );

                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"images\product\" + fileName;
                }

                if(productVM.Product.Id == 0)
                    _unitOfWork.Product.Add(productVM.Product);
                else
                    _unitOfWork.Product.Update(productVM.Product);

                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll()
               .Select(u => new SelectListItem()
               {
                   Text = u.Name,
                   Value = u.Id.ToString()
               });

                productVM.CategoryList = CategoryList;
                return View(productVM);
            }
           
        }

        //public IActionResult Delete(int? Id)
        //{
        //    if (Id == null || Id == 0)
        //        return NotFound();

        //    Product? product = _unitOfWork.Product.Get(u => u.Id == Id, includeProperties: "Category");
        //    if (product == null)
        //        return NotFound();

        //    return View(product);
        //}

        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int? Id)
        //{
        //    Product? category = _unitOfWork.Product.Get(u => u.Id == Id, includeProperties: "Category");
        //    if (category == null)
        //        return NotFound();

        //    _unitOfWork.Product.Remove(category);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Product deleted successfully";
        //    return RedirectToAction("Index");
        //}

        #region " API Calls "

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return Json(new { data = products });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var prodToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (prodToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            //delete the old image
            var oldImagePath = Path.Combine(
                _webHostEnvironment.WebRootPath, prodToBeDeleted.ImageUrl.TrimStart('\\')
                );

            if (System.IO.File.Exists(oldImagePath))
                System.IO.File.Delete(oldImagePath);

            _unitOfWork.Product.Remove(prodToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion
    }
}
