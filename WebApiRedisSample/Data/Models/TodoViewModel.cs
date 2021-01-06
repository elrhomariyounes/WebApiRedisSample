using System.ComponentModel.DataAnnotations;

namespace WebApiRedisSample.Data.Models
{
    public class TodoViewModel
    {
        [Required]
        public string Content { get; set; }
    }
}