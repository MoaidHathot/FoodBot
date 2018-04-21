using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;

namespace FoodBot.Dialogs
{
    public class HelpDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Write 'Order' for ordering food or beverage.");

            context.Call(new RootDialog(), Done);
        }

        private Task Done(IDialogContext context, IAwaitable<object> argument)
        {
            context.Done((object)null);
            return Task.CompletedTask;
        }
    }
}