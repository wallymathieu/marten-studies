using SomeBasicMartenApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Marten;

namespace SomeBasicMartenApp.Core.Extensions
{
    public static class OrderExtensions
    {
        public static IEnumerable<(Customer, Order[])> GetCustomerOrders(this IDocumentSession session,
            Expression<Func<Order, bool>> predicate)
        {
            var dict = new Dictionary<string, Customer>();
            return session.Query<Order>()
                .Where(predicate)
                .Include(o => o.CustomerId, dict)
                .ToList()
                .Select(o => new
                {
                    Order = o,
                    Customer = dict.TryGetValue(o.CustomerId, out var customer) ? customer : null
                })
                .GroupBy(c => c.Customer?.Number ?? 0)
                .Select(c => (c.First().Customer, c.Select(o => o.Order).ToArray()));
        }

        public static (Order, Product[]) GetOrderProducts(this IDocumentSession session,
            string orderId)
        {
            var order = session.Load<Order>(orderId);
            var products = session.Query<Product>()
                .Where(p => order.Products.Contains(p.Id))
                .ToArray();
            return (order, products);
        }
    }
}
