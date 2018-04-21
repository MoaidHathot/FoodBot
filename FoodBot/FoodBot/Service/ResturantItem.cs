using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodBot.Service
{
    [Serializable]
    public class ResturantItem
    {
        public ResturantItemType Type { get; }
        public string Name { get; }
        public double Price { get; }

        public Uri Image { get; }
        public string Description { get; }

        public ResturantItem(ResturantItemType type, string name, double price, Uri image, string description)
        {
            Type = type;
            Name = name;
            Price = price;
            Image = image;
            Description = description;
        }
    }
}