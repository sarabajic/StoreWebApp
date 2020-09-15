using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TiljakHandMade.Models.Data;
using TiljakHandMade.Models.ViewModels.Pages;

namespace TiljakHandMade.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //declare the list of VM
            List<PageVM> pagesList;

            using (Db db = new Db())
            {
                //init the list
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            //return view with list
            return View(pagesList);
        }
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {


                //declare slug
                string slug;
                //init pageDTO
                PageDTO dto = new PageDTO();
                //DTO title
                dto.Title = model.Title;
                //check slug and set slug
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                    slug = model.Slug.Replace(" ", "-").ToLower();
                //make sure title and slug are unique
                if (db.Pages.Any(x => x.Title == model.Title) || 
                    db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "that title or slug already exists.");
                    return View(model);
                }
                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;
                //save DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            //set tempData message
            TempData["SM"] = "You have added a new page.";
            //redirect

            return RedirectToAction("AddPage");
        }

        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //declare pageVM
            PageVM model;

            using (Db db= new Db())
            {

                //get the page
                PageDTO dto = db.Pages.Find(id);

                //confirm page exists
                if (dto==null)
                {
                    return Content("The page does not exist.");
                }
                //init pageVM
                model = new PageVM(dto);
            }

            //return view with model

            return View(model);
        }

        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db= new Db())
            {

                //get page id
                int id = model.Id;
                //init slug
                string slug= "home";
                //get the page
                PageDTO dto = db.Pages.Find(id);
                //DTO the title
                dto.Title = model.Title;
                //check slug and set slug
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                        slug = model.Slug.Replace(" ", "-").ToLower();
                }
                //make sure title and slug are unique
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) || 
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists.");
                    return View(model);
                }
                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                //save DTO
                db.SaveChanges();

            }
            //set tempData message
            TempData["SM"] = "You have edited the page.";


            //redirect

            return RedirectToAction("EditPage");
        }

        public ActionResult PageDetails(int id)
        {
            //declare pageVM
            PageVM model;

            using (Db db = new Db())
            {

                //get the page
                PageDTO dto = db.Pages.Find(id);
                //confirm the page exists
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }
                //init the pageVM
                model = new PageVM(dto);
            }

            //return the view with the model

            return View(model);
        }

        public ActionResult DeletePage(int id)
        {
            using (Db db= new Db())
            {

                //get the page
                PageDTO dto = db.Pages.Find(id);
                //delete the page
                db.Pages.Remove(dto);
                //save
                db.SaveChanges();
            }

            //redirect
            return RedirectToAction("Index");
        }

        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db= new Db())
            {

                //set inital count
                int count = 1;
                //declare the pageDTO
                PageDTO dto;
                //set sorting to each page

                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }


        }

        [HttpGet]
        public ActionResult EditSidebar()
        {
            //declare model
            SidebarVM model;

            using (Db db = new Db())
            {

                //get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);
                //init model
                model = new SidebarVM(dto);
            }

            //return view with the model

            return View(model);
        }
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {

            using (Db db = new Db())
            {

                //get the dto
                SidebarDTO dto = db.Sidebar.Find(1);
                //DTO the body
                dto.Body = model.Body;
                //save
                db.SaveChanges();
            }

            //set tempdata message
            TempData["SM"] = "You have edited the sidebar.";

            //redirect

            return RedirectToAction("EditSidebar");
        }

       

    }
}