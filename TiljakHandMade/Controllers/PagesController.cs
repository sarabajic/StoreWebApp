using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TiljakHandMade.Models.Data;
using TiljakHandMade.Models.ViewModels.Pages;

namespace TiljakHandMade.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/(page)
        public ActionResult Index(string page="")
        {
            //get or set the page slug
            if (page == "")
            {
                page = "home";
            }
            //declare the model and the DTO
            PageVM model;
            PageDTO dto;
            //check if page exists
            using (Db db= new Db())
            {
                if (!(db.Pages.Any(x => x.Slug.Equals(page))))
                {
                    return RedirectToAction("Index", new { page = "" });
                }
            }
            //get page DTO
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            //set page title
            ViewBag.PageTitle = dto.Title;
            //check for sidebar
            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";

            }

            //init model
            model = new PageVM(dto);
            //return view with model

            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            //declare of list of pageVM
            List<PageVM> pageVMList;
            //get all pages except home
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home").Select(x => new PageVM(x)).ToList();
            }
            //return the partial view with list
            return PartialView(pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            //declare model
            SidebarVM model;
            //init model
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);
                model = new SidebarVM(dto);
            }
            //return partial view
            return PartialView(model);
        }
    }
}