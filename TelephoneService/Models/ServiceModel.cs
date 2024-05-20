using Models.Entities;

namespace TelephoneServices.Models
{
    public class ServiceModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public double Prise { get; set; }
        public string? Description { get; set; }
    }
}
