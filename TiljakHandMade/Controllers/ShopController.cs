﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TiljakHandMade.Models.Data;
using TiljakHandMade.Models.ViewModels.Shop;

namespace TiljakHandMade.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            //declare lst of categoryVM
            List<CategoryVM> categoryVMList;
            //init the list
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
                    
            }

            //return partial view w/ list
            return PartialView(categoryVMList);
        }

        public ActionResult Category (string name)
        {
            //declare a list of product vms
            List<ProductVM> productVMList;
            
            using (Db db = new Db())
            {

                //get catID
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                //init the lis
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();
                //get category name
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;
            }

            //retutn view with list
            return View(productVMList);
        }

        [ActionName("products-details")]
        public ActionResult ProductDetails(string name)
        {
            //declare the vm and the dto
            ProductVM model;
            ProductDTO dto;

            //init product id
            int id = 0;

            using (Db db = new Db())
            {

                //check if product exists
                if (!db.Products.Any(x =>x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }
                //init productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();
                //get id
                id = dto.Id;

                //init model
                model = new ProductVM(dto);
            }

            //get the gallery images
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            //return view w/ model
            return View("ProductDetails", model);
        }
    }
}