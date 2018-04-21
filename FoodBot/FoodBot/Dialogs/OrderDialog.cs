using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using FoodBot.Model;
using FoodBot.Service;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;

namespace FoodBot.Dialogs
{
    [Serializable]
    public class OrderDialog : IDialog<Order>
    {
        private readonly bool _continueOrdering;
        private readonly ResturantService _resturant = new ResturantService();

        private readonly Order _order = new Order();

        private ResturantItemType? _wantToOrder;

        public OrderDialog(Order basedOn = null, bool continueOrdering = false)
        {
            _continueOrdering = continueOrdering;
            _order = basedOn ?? _order;
        }

        public OrderDialog(IOrderItem orderItem) 
            => _order.Add(orderItem);

        public OrderDialog(ResturantItemType item)
            : this((Order)null) => _wantToOrder = item;

        public Task StartAsync(IDialogContext context)
        {
            if (_order.ContainsItem && !_continueOrdering)
            {
                context.Done(_order);
                return Task.CompletedTask;
                //return Task.CompletedTask;
            }

            context.Wait(FirstOrderRequest);

            return Task.CompletedTask;
        }

        private Task FirstOrderRequest(IDialogContext context, IAwaitable<IMessageActivity> awaitable)
        {            
            context.Call(CreteOrderDialog(), OrderDoneAsync);
            return Task.CompletedTask;
        }

        private IDialog<Order> CreteOrderDialog()
        {
            return Chain.From(() => null != _wantToOrder ? Chain.Return(_wantToOrder.Value) : SelectionMenu())
                .Switch(
                    Chain.Case<ResturantItemType, IDialog<Order>>(type => ResturantItemType.Meal == type, (context, type) => OrderMeal()),
                    Chain.Case<ResturantItemType, IDialog<Order>>(type => ResturantItemType.Beverage == type, (context, type) => OrderBeverage())
                )
                .Unwrap()
                .ContinueWith(async (ctx, awaitableOrder) => Chain.Return(await awaitableOrder));
        }

        private IDialog<ResturantItemType> SelectionMenu()
            => new PromptDialog.PromptChoice<ResturantItemType>(new PromptOptions<ResturantItemType>(
                "What would you like to order?",
                "Oh, I didn't get that. Could you repeat please?",
                options: ((ResturantItemType[])Enum.GetValues(typeof(ResturantItemType))).ToList()));

        private IDialog<Hamburger> CreateHamburgerDialog()
            => Chain.From(() => FormDialog.FromForm(BuildHamburgerForm, FormOptions.PromptInStart));

        private IDialog<Pizza> CreatePizzaDialog()
            => Chain.From(() => FormDialog.FromForm(BuildPizzaForm, FormOptions.PromptInStart));

        private IDialog<Order> OrderMeal()
        {
            return Chain
                .Return(_order)
                .ContinueWith(async(context, message) =>
                {
                    var menuActivity = await CreateMealSelectionAttachment(context);

                    await context.PostAsync(menuActivity);
                    return Chain.Return(_order);
                })
                .WaitToBot()
                .Select(message => message.Text)
                .Select(choice => "Hamburger" == choice ? CreateHamburgerDialog().Select(AddToOrder) : CreatePizzaDialog().Select(AddToOrder))
                .Unwrap()
                .ContinueWith(async (ctx, awaitableOrder) => Chain.Return(await awaitableOrder));
        }

        private IDialog<Order> OrderBeverage()
        {
            return Chain
                .Return(_order)
                .ContinueWith(async (context, message) =>
                {
                    var menuActivity = await CreateBeverageSelectionAttachment(context);

                    await context.PostAsync(menuActivity);
                    return Chain.Return(_order);
                })
                .WaitToBot()
                .Select(message => message.Text)
                .ContinueWith(async (context, message) =>
                {
                    var name = await message;

                    var beverages = await _resturant.GetBeveragesAsync();
                    var beverage = beverages.Single(item => item.Name == name);

                    return Chain.Return(AddToOrder(new Beverage {Name = beverage.Name}));
                });
        }

        private async Task<IMessageActivity> CreateMealSelectionAttachment(IBotContext context)
        {
            var meals = await _resturant.GetMealsAsync();
            var list = meals.Select(item => GetMenuHeroCard(item.Name, item.Description, item.Price, item.Image)).ToList();

            //var reply = activity.CreateReply();
            var reply = context.MakeMessage();

            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = list;

            return reply;
        }

        private async Task<IMessageActivity> CreateBeverageSelectionAttachment(IBotContext context)
        {
            var meals = await _resturant.GetBeveragesAsync();
            var list = meals.Select(item => GetMenuHeroCard(item.Name, item.Description, item.Price, item.Image)).ToList();

            var reply = context.MakeMessage();

            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = list;

            return reply;
        }

        private Order AddToOrder(IOrderItem item)
        {
            _order.Add(item);

            return _order;
        }
        
        private static Attachment GetMenuHeroCard(string title, string description, double price, Uri image)
        {
            return new ThumbnailCard
            {
                Title = title,
                Subtitle = description,
                Text = $"Price: {price}",
                Images = new List<CardImage> { new CardImage(image.ToString()) },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Select", value: title) }
                
            }.ToAttachment();
        }

        private async Task OrderDoneAsync(IDialogContext context, IAwaitable<Order> argument)
        {
            var item = await argument;

            context.Done(item);
        }

        private static IForm<Hamburger> BuildHamburgerForm()
            => new FormBuilder<Hamburger>()
                .Message("Our Hamburger are great, you would like it!")
                .Field(nameof(Hamburger.Toppings), 
                    validate: async (state, value) =>
                    {
                        var toppings = ((List<object>) value);
                        var result = new ValidateResult {IsValid = true, Value = toppings};

                        if (null != toppings && toppings.Contains(HamburgerTopping.Everything))
                        {
                            result.Value = ((HamburgerTopping[]) Enum.GetValues(typeof(HamburgerTopping))).Where(topping => HamburgerTopping.Everything != topping && !toppings.Contains(topping)).ToList();
                        }

                        return result;
                    })
               .AddRemainingFields()
            .Confirm("Do you want to order your Hamburger as {Doneness} {?with {Cheese}} {?with [{Toppings}]}?")
            .Build();

        private static IForm<Pizza> BuildPizzaForm()
            => new FormBuilder<Pizza>()
                .Message("Our Pizza are great, you would like it!")
                .Build();
    }
}