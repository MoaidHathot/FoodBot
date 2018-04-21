using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;

namespace FoodBot.Model
{
    [Serializable]
    public enum HamburgerCheese { Cheddar, Gouada, Mozzarella}

    [Serializable]
    public enum Doneness { Rare, [Terms("Medium")] Medium, [Terms("Well Done", "Burnt", "Done")] WellDone }

    [Serializable]
    public enum HamburgerTopping
    {
        [Terms("except", "but", "not", "no", "all", "everything", "without")]
        Everything = 1,
        Pickles,
        Lettuce,
        Tomatoes,
        Onions
    }

    [Serializable]
    public class Hamburger : IOrderItem
    {
        public string Name => "Hamburger";

        public Hamburger()
        {
            
        }

        [Prompt("What kind of {&} would you like? {||}", ChoiceFormat = "{1}")]
        [Template(TemplateUsage.NoPreference, "None")]
        [Optional]
        public HamburgerCheese? Cheese { get; set; }

        [Prompt("How do you like your burger's {&}? {||}")]
        public Doneness Doneness { get; set; }

        [Prompt("Would you like to add {&}? {||}")]
        [Optional]
        public List<HamburgerTopping> Toppings { get; set; }

        public override string ToString() 
            => $"A {Doneness} Hamburger {(Cheese.HasValue ? $"With {Cheese}" : string.Empty)} {(0 == (Toppings?.Count ?? 0) ? string.Empty : $"with {string.Join(", ", Toppings.Select(t => t.ToString("g")))}")}";
    }
}