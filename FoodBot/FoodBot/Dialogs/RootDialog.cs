using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace FoodBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var text = activity.Text;

            if (text.Equals("Where are we", StringComparison.CurrentCultureIgnoreCase))
            {
                var reply = activity.CreateReply();

                reply.Attachments.Add(new Attachment()
                {
                    ContentUrl = "https://global.azurebootcamp.net/wp-content/uploads/2014/11/logo-2018-500x444-300x266.png",
                    ContentType = "image/png",
                    Name = "AzureBootcamp 2018"
                });

                await context.PostAsync(reply);
            }
            else
            {

                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                // return our reply to the user
                await context.PostAsync($"You sent {activity.Text} which was {length} characters");
            }


            context.Wait(MessageReceivedAsync);
        }
    }
}