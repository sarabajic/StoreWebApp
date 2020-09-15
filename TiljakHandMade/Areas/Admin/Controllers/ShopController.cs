using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using TiljakHandMade.Areas.Admin.Models.ViewModels.Shop;
using TiljakHandMade.Models.Data;
using TiljakHandMade.Models.ViewModels.Shop;

namespace TiljakHandMade.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //declare the list of models
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {

                //init the list
                categoryVMList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();

            }
                //return view with list
                return View(categoryVMList);
        }
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //declare id
            string id;
            using (Db db = new Db())
            {

                //check that the category name is unique
                if (db.Categories.Any(x => x.Name== catName))
                {
                    return "titletaken";
                }

                //init DTO
                CategoryDTO dto = new CategoryDTO();

                //add to DTO
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //save DTO
                db.Categories.Add(dto);
                db.SaveChanges();

                //get the id
                id = dto.Id.ToString();

            }


            //return id
            return id;
        }

        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {

                //set inital count
                int count = 1;
                //declare the pageDTO
                CategoryDTO dto;
                //set sorting to each page

                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }


        }

        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {

                //get the category
                CategoryDTO dto = db.Categories.Find(id);
                //delete the category
                db.Categories.Remove(dto);
                //save
                db.SaveChanges();
            }

            //redirect
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public string RenameCategory(string newCatName,int id)
        {
            using (Db db = new Db())
            {

                //check if the category name is unique
                if (db.Categories.Any(x => x.Name == newCatName))
                {
                    return "titletaken";
                }
                //get DTO
                CategoryDTO dto = db.Categories.Find(id);

                //edit DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //save
                db.SaveChanges();
            }
            //return
            return "ok";
        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            //init model
            ProductVM model = new ProductVM();

            //add select list of categories to model
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            }
            //return view
            return View(model);
        }

        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }
            //make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x =>x.Name== model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken.");
                    return View(model);
                }
               
            }
            //declare product id

            int id;

            //init and save productsDTO

            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ","-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                model.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //get the id
                id = product.Id;
            }


            //set tempdata message
            TempData["SM"] = "You have added a product!";

            //UPLOAD IMAGE//  

            //create directories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" +id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);
            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);
            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);
            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);
            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //check if the file was uploaded

            if (file != null&& file.ContentLength > 0)
            {

                //get file extension
                string ext = file.ContentType.ToLower();
                //verify extension
                if (ext!= "image/jpg"&&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" && 
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        
                       model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                       ModelState.AddModelError("", "That image has a wrong extension.");
                       return View(model);
                        
                    }
                }
                //init image name
                string imageName = file.FileName;
                //save image to DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }
                //set original and thumb image paths
                var path = string.Format("{0}\\{1}", pathString2,imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                //save original
                file.SaveAs(path);
                //create and save thumb

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }


            ///////////////

            //redirect

            return RedirectToAction("AddProduct");
        }

        public ActionResult Products(int? page, int? catId )
        {
            //declare a list of product VMs
            List<ProductVM> listOfProductVM;
            //set the page number
            var pageNumber = page ?? 1;
            using (Db db = new Db())
            {

                //init the list
                listOfProductVM = db.Products.ToArray()
                    .Where(x=> catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                //populate category select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //set selected category
                ViewBag.SelectedCat = catId.ToString();
            }

            //set pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 5);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            //return the view
            return View(listOfProductVM);
        }
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //declare productVM
            ProductVM model;

            using (Db db = new Db())
            {

                //get the product
                ProductDTO dto = db.Products.Find(id);
                //make sure it exists
                if (dto==null)
                {
                    return Content("That product does not exist.");
                    
                }
                //init model
                model = new ProductVM(dto);
                //make a select list
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                //get all gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }


            //return view with model
            return View(model);
        }
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //get product id
            int id = model.Id;
            //populate category selectlist and gallery images
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Where(x=> x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken.");
                    return View(model);
                }
            }
            //update product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Price = model.Price;
                dto.Description = model.Description;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }
            //set tempdata message
            TempData["SM"] = "You have edited the product";

            //IMAGE UPLOAD//

            //check for file upload
            if (file !=null && file.ContentLength >0)
            {

                //get the extension 
                string ext = file.ContentType.ToLower();
                //and verify it
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {

                        ModelState.AddModelError("", "That image has a wrong extension.");
                        return View(model);

                    }
                }
                //set upload directory paths
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
                //delete files from directories
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles())
                {
                    file2.Delete();
                }
                foreach (FileInfo file3 in di2.GetFiles())
                {
                    file3.Delete();
                }
                //save image name
                string imageName = file.FileName;
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }
                //save original and thumb images
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);
                
                file.SaveAs(path);
                

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }



            //////////////

            //redirect
            return RedirectToAction("EditProduct");
        }

        public ActionResult DeleteProduct(int id)
        {
            //delete the product from db
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }
            //delete product folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            string pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString,true);

            //redirect
            return RedirectToAction("Products");
        }

        public ActionResult Orders()
        {
            // Init list of OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                // Init list of OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                // Loop through list of OrderVM
                foreach (var order in orders)
                {
                    // Init product dict
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    // Declare total
                    decimal total = 0m;

                    // Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(X => X.OrderId == order.OrderId).ToList();

                    // Get username
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.Username;

                    // Loop through list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        // Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get product price
                        decimal price = product.Price;

                        // Get product name
                        string productName = product.Name;

                        // Add to product dict
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * price;
                    }

                    // Add to ordersForAdminVM list
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        Username = username,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            // Return view with OrdersForAdminVM list
            return View(ordersForAdmin);
        }

    }

}