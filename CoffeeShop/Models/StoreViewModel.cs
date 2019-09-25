using System;
using System.ComponentModel.DataAnnotations;
namespace CoffeeShop.Models
{
    public class StoreViewModel
    {
        public int StoreId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public int Zip { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
    }
}
