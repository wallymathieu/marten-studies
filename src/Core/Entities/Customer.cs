namespace SomeBasicMartenApp.Core.Entities
{
    public class Customer
    {
        public virtual string Id { get; set; }

        public virtual int Number { get; set; }

        public virtual string Firstname { get; set; }

        public virtual string Lastname { get; set; }

        public virtual string Email { get; set; }

        public virtual int Version { get; set; }
    }
}
