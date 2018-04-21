using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;

namespace FoodBot.Dialogs
{
    public class ByeScorable : ScorableBase<IActivity, string, double>
    {
        private readonly IDialogTask _task;

        public ByeScorable(IDialogTask task)
        {
            SetField.NotNull(out this._task, nameof(task), task);
        }

        protected override Task<string> PrepareAsync(IActivity item, CancellationToken token)
        {
            string result = null;

            if (item is IMessageActivity messageActivity && !string.IsNullOrWhiteSpace(messageActivity.Text))
            {
                var text = messageActivity.Text;

                if (text.StartsWith("bye bye", StringComparison.InvariantCultureIgnoreCase))
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

        protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var activity = (Activity)item;

            var reply = activity.CreateReply();

            reply.Text = "Bye Bye!! See you soon :)";

            var connector = new ConnectorClient(new Uri(item.ServiceUrl));
            await connector.Conversations.SendToConversationAsync(reply, token);
            _task.Reset();
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}