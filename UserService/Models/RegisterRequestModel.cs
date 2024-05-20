using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class RegisterRequestModel
    {


        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get; set; }
    }
}