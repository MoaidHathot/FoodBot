using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;

namespace FoodBot.Model
{
    [Serializable]
    public enum PizzaCheese { Mozzarella, Romano, Parmesan }


    [Serializable]
    public enum PizzaTopping
    {
        Tomatoes,
        Onions,
        GrrenOlives,
        BlackOlives,
        Anchovy
    }

    [Serializable]
    public class Pizza : IOrderItem
    {
        public string Name => "Pizza";

        public Pizza()
        {
            
        }

        public PizzaCheese Cheese { get; set; }

        [Optional]
        [Template(TemplateUsage.NoPreference, "None")]
        [Prompt("What kind of {&} would you like? {||}", ChoiceFormat = "{1}")]
        public List<PizzaTopping> Toppings { get; set; }

        public override string ToString()
        {
            return $"A {Cheese:g} Pizza {(0 == (Toppings?.Count ?? 0) ? string.Empty : $"with {string.Join(", ", Toppings?.Select(t => t.ToString("g")))}")}";
        }
    }
}