using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Models
{
    public class CustomerFavoriteViewModel
    {
        public int CustomerFavoriteId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int ItemId { get; set; }

        public string ItemName { get; set; }

        public ICollection<ItemViewModel> AllItems { get; set; }

        public ICollection<CustomerViewModel> AllCustomers { get; set; }
    }
}
