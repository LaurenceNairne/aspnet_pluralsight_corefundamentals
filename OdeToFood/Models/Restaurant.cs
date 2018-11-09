using System.ComponentModel.DataAnnotations;

namespace OdeToFood.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        [Display(Name="Restaurant Name")]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public CuisineOrigin Cuisine { get; set; }
    }
}
