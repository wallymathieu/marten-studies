using System;
using Marten;
using Marten.Events;
using Weasel.Core;

namespace SomeBasicMartenApp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public static class AppStore
    {
        public static IDocumentStore Create(string connectionString, Action<StoreOptions> configure)
        {
            return DocumentStore.For(opt =>
            {
                opt.Connection(connectionString);
                opt.AutoCreateSchemaObjects = AutoCreate.All;
                configure(opt);
                opt.Events.StreamIdentity = StreamIdentity.AsString;
            });
        }
    }
}
