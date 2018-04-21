using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FoodBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;

namespace FoodBot.Dialogs
{
    public class HelpScorable : ScorableBase<IActivity, string, double>
    {
        private readonly IDialogTask _task;

        public HelpScorable(IDialogTask task)
        {
            SetField.NotNull(out this._task, nameof(task), task);
        }

        protected override Task<string> PrepareAsync(IActivity item, CancellationToken token)
        {
            string result = null;

            if (item is IMessageActivity messageActivity && !string.IsNullOrWhiteSpace(messageActivity.Text))
            {
                var text = messageActivity.Text;

                if (/*Regex.IsMatch(text, @"Bye\s+.*", RegexOptions.IgnoreCase)*/ text.StartsWith("help", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = text;
                }
            }

            return Task.FromResult(result);
        }

        protected override bool HasScore(IActivity item, string state)
        {
            return null != state;
        }

        protected override double GetScore(IActivity item, string state)
        {
            return 1;
        }

        protected override Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var activity = (Activity)item;

            var dialog = new HelpDialog();
            var interruption = dialog.Void(_task);
            _task.Call(interruption, null);

            return Task.CompletedTask;
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}