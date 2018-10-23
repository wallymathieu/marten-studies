namespace SomeBasicMartenApp

open System
open System.Collections.Generic
type Order = {mutable Id:string;mutable OrderDate:DateTime;mutable CustomerId:string;Products:List<string>;mutable Version:int }
and Customer= {mutable Id:string;mutable Firstname:string;mutable Lastname:string;mutable Version:int}
and Product= {mutable Id:string;mutable Cost:float;mutable Name:string;mutable Version:int}

module Product=let id (p:Product) = p.Id
module Order=let id (p:Order) = p.Id
module Customer=let id (p:Customer) = p.Id
open FSharpPlus
open FSharpPlus.Operators

open Marten
open Marten.Events
open Marten.Services.Includes

module Session=

    open System.Linq
    
    type IDocumentSession with
        member session.GetOrder (id:string)=
            Session.loadByString<Order> id session
    
        member session.GetProduct id=
            Session.loadByString<Product> id session
    
        member session.GetCustomer id=
            Session.loadByString<Customer> id session
    
        member session.GetCustomerOrders (predicate) =
            let dict = new Dictionary<string, Customer>();
            let query=session.Query<Order>() |> Queryable.filter predicate
            query.Include<Order,String, Customer>( (fun o -> o.CustomerId :> obj), dict, JoinType.LeftOuter)
            |> Seq.map (fun o->(o, Dict.tryGetValue o.CustomerId dict ))

        member session.GetOrderProducts id=
            let orderProducts (order:Order)=
                                session 
                                   |> Session.query<Product>
                                   |> Queryable.filter <@ fun p-> order.Products.Contains( p.Id) @>
                                   |> Queryable.toList
            Session.loadByString<Order> id session
            |> Option.map (fun order->(order, orderProducts order) )
module Store=

    let create (connectionString:string) (configure:StoreOptions->unit)=
            DocumentStore.For(fun opt ->
                opt.Connection(connectionString)
                opt.AutoCreateSchemaObjects <- AutoCreate.All
                configure(opt)
                opt.Events.StreamIdentity <- StreamIdentity.AsString
            )
