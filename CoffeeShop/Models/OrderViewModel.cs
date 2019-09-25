using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Models
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public int CustomerRewardBalance { get; set; }

        public int StoreId { get; set; }

        public string StoreName { get; set; }

        public DateTime Date { get; set; }

        public bool Completed { get; set; }

        public bool PickedUp { get; set; }

        public int RewardUsed { get; set; }

        public int RewardEarned { get; set; }

        public decimal SubTotal { get; set; }

        public decimal TaxTotal { get; set; }

        public decimal GrandTotal { get; set; }

        public ICollection<OrderItemViewModel> OrderItems { get; set; }

        public ICollection<ItemViewModel> AllItems { get; set; }

        public ICollection<StoreViewModel> AllStores { get; set; }

        public ICollection<CustomerViewModel> AllCustomers { get; set; }

        public int GrandTotalAsPoints => (int)Math.Ceiling(GrandTotal);

        public bool CanPayWithPoints => CustomerRewardBalance >= GrandTotalAsPoints;

        public bool PayWithPoints { get; set; }
    }
}
