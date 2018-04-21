using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using FoodBot.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace FoodBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string LastOrderUserStorageKey = "LastOrder";
        private const string CurrentOrderOfUserStorageKey = "CurrentUserOrder";
        private const string RecommendedFoodOfUserStorageKey = "RecommendedForUser";

        public Task StartAsync(IDialogContext context)
        {
            //context.UserData.RemoveValue(LastOrderUserStorageKey);
            context.Wait(FirstMessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task FirstMessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> awaitableActivity)
        {
            var message = await awaitableActivity;

            if (Regex.IsMatch(message.Text, "^(bye).*"))
            {
                await context.PostAsync("Bye! Hoping to see you soon :)");
                context.Done(true);
            }

            await NotifyTyping((Activity) message);

            await context.PostAsync("Shalom!");
            if (TryGetLastOrder(context, out Order order))
            {
                PromptDialog.Confirm(context, ConfirmReorderLastOrderAsync, $"Would you like to reorder the last order? {order}");
                return;
            }

            await context.PostAsync("How can I help you?");
            

            context.Wait(MessageReceivedAsync);
        }

        private async Task NotifyTyping(Activity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            Activity isTypingReply = activity.CreateReply();
            isTypingReply.Type = ActivityTypes.Typing;
            await connector.Conversations.ReplyToActivityAsync(isTypingReply);
        }

        private async Task ConfirmReorderLastOrderAsync(IDialogContext context, IAwaitable<bool> isConfirmed)
        {
            if (await isConfirmed)
            {
                TryGetLastOrder(context, out Order lastOrder);
                context.Call(new OrderDialog(lastOrder), OrderDoneAsync);
            }
            else
            {
                await context.PostAsync("No problem. So how can I help you?");
                context.Wait(MessageReceivedAsync);
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

            PromptDialog.Confirm(context, ConfirmOrderMoreAsync, "Would you like to order More?");

            //await context.PostAsync("So what would you like to do next?");
            //context.Wait(MessageReceivedAsync);
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
                    var settings = new JsonSerializerSettings();
                    settings.TypeNameHandling = TypeNameHandling.Objects;

                    lastOrder = JsonConvert.DeserializeObject<Order>(json, settings);
                    return true;
                }
            }

            return false;
        }

        private async Task ConfirmOrderMoreAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var toOrderMore = await argument;

            TryGetLastOrder(context, out Order order);

            if (toOrderMore)
            {
                await context.Forward(new OrderDialog(order, true), OrderDoneAsync, context.MakeMessage());
                return;
            }

            await context.PostAsync($"Your current order is: {order}");

            await context.PostAsync("What do you want to do now? :)");
            context.Wait(MessageReceivedAsync);
        }

        private async Task RecommendationDoneAsync(IDialogContext context, IAwaitable<IOrderItem> argument)
        {
            var item = await argument;

            if (null != item)
            {
                context.PrivateConversationData.SetValue(RecommendedFoodOfUserStorageKey, item);
                PromptDialog.Confirm(context, ConfirmAddRecommendationToOrder, $"Would you like to add {item} to your order?", "Sorry, I didn't get that. Try again please");
            }
            else
            {
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task ConfirmAddRecommendationToOrder(IDialogContext context, IAwaitable<bool> argument)
        {
            var isConfirmed = await argument;

            if (isConfirmed && context.PrivateConversationData.TryGetValue(RecommendedFoodOfUserStorageKey, out IOrderItem item) && null != item)
            {
                context.PrivateConversationData.TryGetValue(CurrentOrderOfUserStorageKey, out Order order);
                order = order ?? new Order();

                order.Add(item);

                await context.PostAsync("Ok, {item} was added to your order.");
                await context.PostAsync("What would you want to do next?");
            }
            else
            {
                context.PrivateConversationData.SetValue<IOrderItem>(RecommendedFoodOfUserStorageKey, null);
            }

            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            await NotifyTyping((Activity)message);

            if (Regex.IsMatch(message.Text, ".*order.*", RegexOptions.IgnoreCase))
            {
                await context.Forward(new OrderDialog(), OrderDoneAsync, message);
                return;
            }

            if (Regex.IsMatch(message.Text, "^(help|how|support|faq|QnA).*", RegexOptions.IgnoreCase))
            {
                await context.PostAsync("You can type 'order' for ordering food");
                context.Wait(MessageReceivedAsync);
            }
            else if (Regex.IsMatch(message.Text, "^(bye|adios).*", RegexOptions.IgnoreCase))
            {
                await context.PostAsync("Bye! Hoping to see you soon :)");
                context.Done(true);
            }
            else
            {
                await context.PostAsync("I don't quite understand. Say 'help' for getting acquianted with me.");
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}