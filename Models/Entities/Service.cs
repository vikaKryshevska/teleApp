namespace Models.Entities
{
    public class Service
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public double Prise { get; set; }
        public string Description { get; set; }
        public ICollection<Subscriber> Subscribers { get; set; }
        //public ICollection<SubscriberServices> Subscribers { get; set; }

    }
}
