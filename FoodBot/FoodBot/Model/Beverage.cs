using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodBot.Model
{
    [Serializable]
    public class Beverage : IOrderItem
    {
        public string Name { get; set; } = "";

        public Beverage()
        {
            
        }

        public override string ToString()
            => $"A {Name}";
    }
}