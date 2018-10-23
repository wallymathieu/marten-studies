using System;
using System.Collections.Generic;

namespace SomeBasicMartenApp.Core.Entities
{
    public class Order 
    {
        public virtual string Id { get; set; }

        public virtual DateTime OrderDate { get; set; }
        public virtual int Number { get; set; }

        public virtual string CustomerId { get; set; }

        public virtual List<string> Products { get; set; } = new List<string>();

        public virtual int Version { get; set; }

    }
}
