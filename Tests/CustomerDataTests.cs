using System.IO;
using System.Xml.Linq;
using Order = SomeBasicMartenApp.Core.Entities.Order;
using System;
using System.Linq;
using System.Reflection;
using Marten;
using SomeBasicMartenApp.Core;
using SomeBasicMartenApp.Core.Entities;
using SomeBasicMartenApp.Core.Extensions;
using Xunit;

namespace SomeBasicMartenApp.Tests
{
    public static class DocumentStoreConnection
    {
        public static readonly IDocumentStore Store =
            AppStore.Create(Environment.GetEnvironmentVariable("MARTEN_STUDIES"),
                opt => { opt.DatabaseSchemaName = "marten_studies"; });
    }

    public class CustomerDataTests : IDisposable
    {
        private static IDocumentStore _store;
        private IDocumentSession _session;


        [Fact]
        public void CanGetCustomerById()
        {
            var customer = _session.Load<Customer>("1");

            Assert.NotNull(customer);
        }

        [Fact]
        public void CustomerHasOrders()
        {
            var customerOrder = _session.GetCustomerOrders(order => order.Number == 1)
                .First();

            Assert.True(customerOrder.Item2.Any());
        }

        [Fact]
        public void CanGetCustomerByFirstname()
        {
            var customers = _session.Query<Customer>()
                .Where(c => c.Firstname == "Steve")
                .ToList();
            Assert.Equal(2, customers.Count);
        }

        [Fact]
        public void CanGetCustomerByEmail()
        {
            var customer = _session.Query<Customer>().Single(x => x.Email ==
                                                                  "peter@sylvester.com");
            Assert.Equal(51, customer.Number);
        }

        [Fact]
        public void CanSearchForCustomerByName()
        {
            var customers = _session.Query<Customer>()
                .Where(c => c.Firstname == "Steve")
                .ToList();
            Assert.Equal(2, customers.Count);
        }


        [Fact]
        public void CantInsertARecordWithSameId()
        {
            var customer = new Customer
            {
                Id = "65",
                Number = 61,
                Firstname = "Peter John",
                Lastname = "Sylvester1",
                Email = "peter1@sylvester.com"
            };
            _session.Store(customer);
            var customer_2 = new Customer
            {
                Id = customer.Id,
                Number = 61,
                Firstname = "Peter John",
                Lastname = "Sylvester2",
                Email = "peter2@sylvester.com"
            };
            Assert.Throws<InvalidOperationException>(() => _session.Store(customer_2));
            var load = _session.Load<Customer>(customer.Id);
            Assert.NotNull(load);
            Assert.Equal(customer.Lastname, load.Lastname);
            Assert.Equal(customer.Email, load.Email);
        }

        [Fact]
        public void CanGetProductById()
        {
            var product = _session.Load<Product>("1");

            Assert.NotNull(product);
        }

        [Fact]
        public void OrderContainsProduct()
        {
            var orderProducts = _session.GetOrderProducts("1");
            Assert.True(orderProducts.Item2.Any(p => p.Number == 1));
        }

        public CustomerDataTests()
        {
            _session = _store.OpenSession();
        }


        public void Dispose()
        {
            _session.Dispose();
        }

        static CustomerDataTests()
        {
            void OnIgnore(Type type, PropertyInfo property) => 
                Console.WriteLine("ignoring property {1} on {0}", type.Name, property.PropertyType.Name);

            _store = DocumentStoreConnection.Store;
            _store.Advanced.Clean.DeleteAllDocuments();
            //_store.index
            var doc = XDocument.Load(Path.Combine("TestData", "TestData.xml"));
            var import = new XmlImport(doc, "http://tempuri.org/Database.xsd");
            
            
            using (var session = _store.OpenSession())
            {
                import.Parse<Customer>(new[] {typeof(Customer)},
                    (type, obj) =>
                    {
                        obj.Id = obj.Number.ToString();
                        session.Store(obj);
                    },
                    OnIgnore);
                import.Parse<Product>(new[] {typeof(Product)},
                    (type, obj) =>
                    {
                        obj.Id = obj.Number.ToString();
                        session.Store(obj);
                    },
                    OnIgnore);
                import.Parse<Order>(new[] {typeof(Order)},
                    (type, obj) =>
                    {
                        obj.Id = obj.Number.ToString();
                        session.Store(obj);
                    },
                    OnIgnore);
                session.SaveChanges();
            }

            using (var session = _store.OpenSession())
            {
                import.ParseConnections("OrderProduct", "Product", "Order", (productId, orderId) =>
                {
                    var product = session.Load<Product>(productId.ToString());
                    var order = session.Load<Order>(orderId.ToString());
                    order.Products.Add(product.Id);
                    session.Update(order);
                });

                import.ParseIntProperty("Order", "Customer", (orderId, customerId) =>
                {
                    var order = session.Load<Order>(orderId.ToString());
                    order.CustomerId = customerId.ToString();
                    session.Update(order);
                });
                session.SaveChanges();
            }
        }
    }
}