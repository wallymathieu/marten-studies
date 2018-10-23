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
    let getOrder id session = Session.loadByString<Order> id session
    let getProduct id session = Session.loadByString<Product> id session
    let getCustomer id session = Session.loadByString<Customer> id session
    let getCustomerOrders predicate session =
        let dict = new Dictionary<string, Customer>();
        let query=session |> Session.query<Order> |> Queryable.filter predicate
        query.Include<Order,String, Customer>( (fun o -> o.CustomerId :> obj), dict, JoinType.LeftOuter)
        |> Seq.map (fun o->(o, Dict.tryGetValue o.CustomerId dict ))
    let getOrderProducts id session=
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
