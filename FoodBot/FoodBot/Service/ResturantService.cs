using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FoodBot.Service
{
    [Serializable]
    public class ResturantService
    {
        private readonly List<ResturantItem> _items = new List<ResturantItem>
        {
            new ResturantItem(ResturantItemType.Meal, "Hamburger", 50, new Uri("https://www.thesun.co.uk/wp-content/uploads/2016/09/nintchdbpict000264481984.jpg"), "One of the best burgers you'll ever tatse!"),
            new ResturantItem(ResturantItemType.Meal, "Pizza", 45, new Uri("http://www.famouspizzaexpress.com/images/pizza35.jpg"), "Hot, crunchy with great cheese!"),
            new ResturantItem(ResturantItemType.Beverage, "Water", 7, new Uri("http://images.esellerpro.com/2363/I/243/48/lrgscaleVolvic%20Water.jpg"), ""),
            new ResturantItem(ResturantItemType.Beverage, "Soda", 10, new Uri("https://tse3.mm.bing.net/th?id=OIP.ub13aVaEG0LO45tIm3pa4gHaHa&pid=Api"), "Coca Cola"),
            new ResturantItem(ResturantItemType.Beverage, "Juice", 13, new Uri("http://www.fdaimports.com/blog/wp-content/uploads/2012/01/Orange_Juice_OJ1.jpg"), "Orange Juice"),
            new ResturantItem(ResturantItemType.Beverage, "Beer", 30, new Uri("https://www.singleblackmale.org/wp-content/uploads/2013/05/glass-of-beer.jpg"), "Our own beer!"),
        };

        public Task<IEnumerable<ResturantItem>> GetAllItemsAsync()
            => Task.FromResult((IEnumerable<ResturantItem>)_items);

        public Task<IEnumerable<ResturantItem>> GetMealsAsync()
            => Task.FromResult(_items.Where(item => ResturantItemType.Meal == item.Type));

        public Task<IEnumerable<ResturantItem>> GetBeveragesAsync()
            => Task.FromResult(_items.Where(item => ResturantItemType.Beverage == item.Type));
    }
}
