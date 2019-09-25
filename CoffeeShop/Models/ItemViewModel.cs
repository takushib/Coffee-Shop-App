using System;
using System.ComponentModel.DataAnnotations;
namespace CoffeeShop.Models
{
    public class ItemViewModel
    {
        public int ItemId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Type { get; set; }

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }
}
