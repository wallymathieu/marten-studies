namespace SomeBasicMartenApp.Core.Entities
{
    public class Product 
    {
        public virtual string Id { get; set; }

        public virtual float Cost { get; set; }

        public virtual string Name { get; set; }

        public virtual string Tags { get; set; }
        public virtual string Description { get; set; }
        public virtual int Number { get; set; }

        public virtual int Version { get; set; }
    }
}
