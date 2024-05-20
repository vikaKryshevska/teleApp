using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities
{
    public class Bill
    {
        
        public string Id { get; set; }
        public double Prise { get; set; }
        public bool Status { get; set; }
        public DateTime DueDate { get; set; }
        public string SubscriberId { get; set; }
        public Subscriber Subscriber { get; set; }
    }
}
