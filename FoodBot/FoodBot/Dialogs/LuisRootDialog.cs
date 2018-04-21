using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using FoodBot.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace FoodBot.Dialogs
{
    [Serializable]
    public class LuisRootDialog : LuisDialog<object>
    {
        private const string LastOrderUserStorageKey = "LastOrder";
        private const string CurrentOrderOfUserStorageKey = "CurrentUserOrder";
        private const string RecommendedFoodOfUserStorageKey = "RecommendedForUser";

        public LuisRootDialog()
            : base(CreateLuisService())
        {
            
        }

        private static ILuisService CreateLuisService()
        {
            return new LuisService(
                new LuisModelAttribute(ConfigurationManager.AppSettings["LuisModelId"], ConfigurationManager.AppSettings["LuisSubscriptionKey"])
                {
                    SpellCheck = true
                });
        }

        [LuisIntent("None")]
        public async Task OnIntentUnidentifiedAsync(IDialogContext context, IAwaitable<IMessageActivity> messageAwaitable, LuisResult luisResult)
        {
            //await context.SpeakAsync("I'm sorry, I didn't get that. How can I help you?", InputHints.ExpectingInput);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Order")]
        public async Task OnOrderBeerAsync(IDialogContext context, IAwaitable<IMessageActivity> messageAwaitable, LuisResult luisResult)
        {
            var message = await messageAwaitable;
            //var beerName = GetEntity(luisResult, BeerNameEntityName);
            //var chaser = GetEntity(luisResult, ChaserEntityName);
            //var sideDish = GetEntity(luisResult, SideDishEntityName);

            //context.Call(OrderDialog.CreateDialog(beerName, chaser, sideDish), BeerOrderedAsync);
            await context.Forward(new OrderDialog(), OrderDoneAsync, message);
        }

        private async Task ConfirmReorderLastOrderAsync(IDialogContext context, IAwaitable<bool> isConfirmed)
        {
            if (await isConfirmed)
            {
                context.UserData.TryGetValue(LastOrderUserStorageKey, out Order lastOrder);
                context.Call(new OrderDialog(lastOrder), OrderDoneAsync);
            }
            else
            {
                await context.PostAsync("No problem. So how can I help you?");
                context.Wait(MessageReceived);
            }
        }

        private async Task OrderDoneAsync(IDialogContext context, IAwaitable<Order> argument)
        {
            var order = await argument;

            if (null != order)
            {
                await context.PostAsync($"Your order is coming right up!. You have ordered {order}");


                SaveOrder(context, order);
            }

            //PromptDialog.Confirm(context, ConfirmOrderMoreAsync, "Would you like to order More?");

            //await context.PostAsync("So what would you like to do next?");
            context.Wait(MessageReceived);
        }

        private static string GetEntity(LuisResult luisResult, string entityName)
        {
            var entityRecommendation = luisResult.Entities.FirstOrDefault(e => e.Type == entityName);
            object resolvedValue = null;
            return entityRecommendation?.Resolution?.TryGetValue("values", out resolvedValue) == true
                ? ((List<object>)resolvedValue)[0].ToString()
                : entityRecommendation?.Entity;
        }

        private void SaveOrder(IDialogContext context, Order order)
        {
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;

            context.UserData.SetValue(LastOrderUserStorageKey, JsonConvert.SerializeObject(order, Formatting.Indented, settings));
        }

        private bool TryGetLastOrder(IDialogContext context, out Order lastOrder)
        {
            lastOrder = null;


            if (context.UserData.TryGetValue(LastOrderUserStorageKey, out string json))
            {
                if (!string.IsNullOrWhiteSpace(json))
                {
                    lastOrder = JsonConvert.DeserializeObject<Order>(json);
                    return true;
                }
            }

            return false;
        }
    }
}