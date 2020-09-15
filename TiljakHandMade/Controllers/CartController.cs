using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TiljakHandMade.Models.Data;
using TiljakHandMade.Models.ViewModels.Cart;

namespace TiljakHandMade.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //init the cart list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            //check if the cart is empty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }
            //calculate total and save to viewbag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;

            }
            ViewBag.GrandTotal = total;
            //return view with model

            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //init cart VM
            CartVM model = new CartVM();
            //init quantity
            int qty = 0;
            //init price
            decimal price = 0m;
            //check for cart section
            if (Session["cart"] != null)
            {
                //get total quantity and price
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }
                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                //or set quantity and price to 0
                model.Quantity = 0;
                model.Price = 0m;
            }


            //return view w/ model
            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            //init the cart vm list

            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //init cartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {

                //get the product
                ProductDTO product = db.Products.Find(id);

                //check if the product is already in cart
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);
                
                //if not, add new
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image=product.ImageName
                    });
                }
                else
                {
                    //if it is, increment
                    productInCart.Quantity++;

                }
            }

            //get total qty and price and add to model

            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;


            //save cart back to session

            Session["cart"] = cart;

            //return view with model

            return PartialView(model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Increment qty
                model.Quantity++;

                // Store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                // Return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult DecrementProduct(int productId)
        {
            // Init cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Decrement qty
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                // Store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                // Return json
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public void RemoveProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Remove model from list
                cart.Remove(model);
            }

        }

        [HttpPost]
        public ActionResult PlaceOrder()
        {
            // Get cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Get username
            string username = User.Identity.Name;

            int orderId = 0;

            using (Db db = new Db())
            {
                // Init OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                // Get user id
                var q = db.Users.FirstOrDefault(x => x.Username == username);
                int userId = q.Id;

                // Add to OrderDTO and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);

                db.SaveChanges();

                // Get inserted id
                orderId = orderDTO.OrderId;

                // Init OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                // Add to OrderDetailsDTO
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }
            }

            Session["cart"] = null;

            return RedirectToAction("Orders", "Account", new { area = "" });
        }
    }
}