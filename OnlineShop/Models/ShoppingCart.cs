﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineShop.Models
{
    public partial class ShoppingCart
    {
        ApplicationDbContext db = new ApplicationDbContext();
        string ShoppingCartId { get; set; }
       // public const string CartSessionKey = "CartId";

        public static ShoppingCart GetCart(string cartId)
        {
            var cart = new ShoppingCart();
            cart.ShoppingCartId = cartId;
            return cart;
        }
        // Helper method to simplify shopping cart calls
       /* public static ShoppingCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }
       */
        public void AddToCart(Book book)
        {
            // Get the matching cart and book instances
            var cartItem = db.Carts.SingleOrDefault(
                c => c.CartId == ShoppingCartId
                && c.BookId == book.BookId);

            if (cartItem == null)
            {
                // Create a new cart item if no cart item exists
                cartItem = new Cart
                {
                    BookId = book.BookId,
                    CartId = ShoppingCartId,
                    Count = 1,
                    DateCreated = DateTime.Now
                };
                db.Carts.Add(cartItem);
            }
            else
            {
                // If the item does exist in the cart, 
                // then add one to the quantity
                cartItem.Count++;
            }
            // Save changes
            db.SaveChanges();
        }
        public int RemoveFromCart(int id)
        {
            // Get the cart
            var cartItem = db.Carts.Single(
                cart => cart.CartId == ShoppingCartId
                && cart.RecordId == id);

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }
                else
                {
                    db.Carts.Remove(cartItem);
                }
                // Save changes
                db.SaveChanges();
            }
            return itemCount;
        }
        public void EmptyCart()
        {
            var cartItems = db.Carts.Where(
                cart => cart.CartId == ShoppingCartId);

            foreach (var cartItem in cartItems)
            {
                db.Carts.Remove(cartItem);
            }
            // Save changes
            db.SaveChanges();
        }
        public List<Cart> GetCartItems()
        {
            return db.Carts.Where(
                cart => cart.CartId == ShoppingCartId).ToList();
        }
        public int GetCount()
        {
            // Get the count of each item in the cart and sum them up
            int? count = (from cartItems in db.Carts
                          where cartItems.CartId == ShoppingCartId
                          select (int?)cartItems.Count).Sum();
            // Return 0 if all entries are null
            return count ?? 0;
        }
        public decimal GetTotal()
        {
            // Multiply book price by count of that book to get 
            // the current price for each of those books in the cart
            // sum all book price totals to get the cart total
            decimal? total = (from cartItems in db.Carts
                              where cartItems.CartId == ShoppingCartId
                              select (int?)cartItems.Count *
                              cartItems.Book.Price).Sum();

            return total ?? decimal.Zero;
        }
        public int CreateOrder(Order order)
        {
            decimal orderTotal = 0;

            var cartItems = GetCartItems();
            // Iterate over the items in the cart, 
            // adding the order details for each
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    BookId = item.BookId,
                    OrderId = order.OrderId,
                    UnitPrice = item.Book.Price,
                    Quantity = item.Count
                };
                // Set the order total of the shopping cart
                orderTotal += item.Count * item.Book.Price;

                db.OrderDetails.Add(orderDetail);

            }
            // Set the order's total to the orderTotal count
            order.Total = orderTotal;

            // Save the order
            db.SaveChanges();
            // Empty the shopping cart
            EmptyCart();
            // Return the OrderId as the confirmation number
            return order.OrderId;
        }
      
    }
}