using System.ComponentModel.DataAnnotations;

namespace AuthorizeErrorEFCoreSample.Models
{
    public class LoginViewModel
    {
        [Required]
        public string account { get; set; }

        [Required]
        public string pwd { get; set; }
    }
}