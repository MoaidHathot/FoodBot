using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace FoodBot.Model
{
    [Serializable]
    public class Order
    {
        private readonly List<IOrderItem> _items;

        public IEnumerable<IOrderItem> OrderItems => _items;

        public Order(IEnumerable<IOrderItem> orderItems = null)
            => _items = new List<IOrderItem>(orderItems ?? Enumerable.Empty<IOrderItem>());

        public void Add(IOrderItem orderItem)
            => _items.Add(orderItem);

        public bool ContainsItem => _items.Any();

        public override string ToString()
            => string.Join(", ", OrderItems);
    }
}