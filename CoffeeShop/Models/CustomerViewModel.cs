using System;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Models
{
    public class CustomerViewModel
    {
        public int CustomerId { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        
        public int RewardBalance { get; set; }
    }
}
