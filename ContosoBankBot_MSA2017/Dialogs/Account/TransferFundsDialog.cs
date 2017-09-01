using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ContosoBankBot_MSA2017.Dialogs.Account
{
    [Serializable]
    public class TransferFundsDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Sorry this functionality is not yet implemented.");
            context.Done<object>(null);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {

        }
    }
}