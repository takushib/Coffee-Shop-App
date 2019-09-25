using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Models
{
    public class OrderItemViewModel
    {
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int ItemId { get; set; }

        public string ItemName { get; set; }

        public string ItemType { get; set; }

        [DataType(DataType.Currency)]
        public decimal ItemPrice { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Non-negative quantity required")]
        public int Quantity { get; set; }

        public ICollection<ItemViewModel> AllItems { get; set; }
    }
}
