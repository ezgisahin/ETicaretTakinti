﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Takinti.Models;

namespace Takinti.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Cart()
        {
            if (Request.IsAjaxRequest())
            {
                return View("LayoutCart");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Cart(FormCollection form)
        {
            if (Request.IsAjaxRequest())
            {
                return View("LayoutCart");
            }
            if(Session["Cart"] == null)
            {
                Session["Cart"] = new Cart();
            }
            var cartItems = ((Cart)Session["Cart"]).CartItems.ToArray();
            foreach (var item in  cartItems)
            {
                if (!String.IsNullOrEmpty(form["Quantity_" + item.Product.Slug.ToLower()]))
                {
                    var sessionCartItem=((Cart)Session["Cart"]).CartItems.FirstOrDefault(c => c.Product.Slug.ToLower() == item.Product.Slug.ToLower());
                    sessionCartItem.Quantity = Convert.ToInt32(form["Quantity_" + item.Product.Slug.ToLower()]);
                    if (sessionCartItem.Quantity <= 0)
                    {
                        ((Cart)Session["Cart"]).CartItems.Remove(sessionCartItem);
                    }
                }
            }
            return View();
        }

        [Authorize]
        public ActionResult Checkout()
        {
            using (var db = new ApplicationDbContext())
            {
                ViewBag.Countries =new SelectList(db.Countries.OrderBy(c => c.Name).ToList(),"Id","Name");
                ViewBag.Cities = new SelectList(db.Cities.OrderBy(c => c.Name).ToList(), "Id", "Name");
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Checkout(CheckoutViewModel checkout)
        {
            using (var db = new ApplicationDbContext())
            {
                ViewBag.Countries = new SelectList(db.Countries.OrderBy(c => c.Name).ToList(), "Id", "Name",checkout.CountryId);
                ViewBag.Cities = new SelectList(db.Cities.OrderBy(c => c.Name).ToList(), "Id", "Name",checkout.CityId);
            }
            return View(checkout);
        }


        public JsonResult AddToCart(string slug)
        {
            using (var db = new ApplicationDbContext())
            {
                if(Session["Cart"] == null)
                {
                    Session["Cart"] = new Cart();
                    ((Cart)Session["Cart"]).CreateDate = DateTime.Now;
                }
                ((Cart)Session["Cart"]).UserName = User.Identity.Name;
                ((Cart)Session["Cart"]).UpdateDate = DateTime.Now;


                CartItem cartItem= ((Cart)Session["Cart"]).CartItems.FirstOrDefault(c => c.Product.Slug.ToLower() == slug.ToLower());
                if (cartItem == null)
                {
                    cartItem = new CartItem();
                    cartItem.Quantity = 1;
                
                
                var product = db.Products.FirstOrDefault(p => p.Slug.ToLower() == slug.ToLower() && p.IsInStock == true && p.Quantity > 0 && p.IsPublished == true);
                if(product == null)
                {
                    return Json(false);
                }
                cartItem.ProductId = product.Id;
                cartItem.Product = product;
                cartItem.CreateDate = DateTime.Now;

                ((Cart)Session["Cart"]).CartItems.Add(cartItem);
                }
                else
                {
                    cartItem.Quantity += 1;
                }
                return Json(CartProductCount());

            }
        }
        public JsonResult RemoveFromCart(string slug)
        {
           

            if (Session["Cart"] == null)
            {
                Session["Cart"] = new Cart();
                
            }

            else
            {
                var cartItem = ((Cart)Session["Cart"]).CartItems.FirstOrDefault(p => p.Product.Slug.ToLower() == slug.ToLower());

                if (cartItem != null)
                     {
                        ((Cart)Session["Cart"]).CartItems.Remove(cartItem);
                    }
                    
            }
            return Json(CartProductCount());
            
        }
        

        public int CartProductCount()
        {
            if(Session["Cart"] != null)
            {
                return ((Cart)Session["Cart"]).ProductCount;
            }
            return 0;
        }
    }
}