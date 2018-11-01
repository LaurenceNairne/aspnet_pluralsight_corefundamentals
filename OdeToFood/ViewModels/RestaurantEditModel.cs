using OdeToFood.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OdeToFood.ViewModels
{
    public class RestaurantEditModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public CuisineOrigin Cuisine { get; set; }
    }
}
