module Tests

open System
open Xunit
open FSharp.Data
open SomeBasicMartenApp

module TestData=
    open Marten
    open System.IO
    open System.Collections.Generic
    

    type TestData = XmlProvider<"../Tests/TestData/TestData.xml", Global=false>

    let fillDb (sessionFactory:IDocumentStore)=
        use session = sessionFactory.OpenSession()

        let toCustomer (o : TestData.Customer) :Customer=
            {Id=string o.Number;Version=o.Version;Firstname=o.Firstname;Lastname=o.Lastname}

        let toOrder (o : TestData.Order) :Order=
            {Id=string o.Number;Version=o.Version;CustomerId=string o.Customer; OrderDate=o.OrderDate.DateTime; Products=List<_>()}

        let toProduct (o : TestData.Product) :Product=
            {Id=string o.Number;Version=o.Version;Name=o.Name;Cost=float o.Cost}

        let toOrderProduct(o : TestData.OrderProduct)=
            let order=session.Load<Order>(string o.Order)
            (order, o.Product)

        use f = File.Open("TestData/TestData.xml", FileMode.Open, FileAccess.Read, FileShare.Read)
        let db = TestData.Load(f)

        for customer in db.Customers |> Array.map toCustomer do
            session.Store (customer)

        for order in db.Orders |> Array.map toOrder do
            session.Store (order)
        for product in db.Products |> Array.map toProduct do
            session.Store (product)
        for (order,product) in db.OrderProducts |> Array.map toOrderProduct do
            order.Products.Add <| string product
        session.SaveChanges()

open System
open SomeBasicMartenApp.Session
type CustomerDataTests()=
    let mutable sessionFactory =null
    let mutable session=null
    do
        sessionFactory <- Store.create 
                            (Environment.GetEnvironmentVariable "MARTEN_STUDIES")
                            (fun opt->opt.DatabaseSchemaName<-"marten_studies_fsharp")
        TestData.fillDb sessionFactory
        session <-sessionFactory.OpenSession()

    [<Fact>]
    member this.CanGetCustomerById()=
        Assert.True(session.GetCustomer "1" |> Option.isSome)

    [<Fact>]
    member this.CanGetProductById()=
        Assert.True(session.GetProduct "1" |> Option.isSome)

    [<Fact>]
    member this.AProductThatDoesNotExist()=
        Assert.True(session.GetProduct "1000" |> Option.isNone)

    [<Fact>]
    member this.OrderContainsProduct()=
        match session.GetOrderProducts "1" with
        | Some (order,products) -> 
            let productId = "1"
            Assert.True(order.Products |> Seq.tryFind( (=) productId) |> Option.isSome)
            Assert.True(products |> Seq.tryFind( Product.id >> (=) (productId)) |> Option.isSome)
        | None -> failwith "Could not find order"
