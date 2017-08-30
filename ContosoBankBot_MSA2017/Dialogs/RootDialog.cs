using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading;

namespace ContosoBankBot_MSA2017.Dialogs
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

            if (activity.Text == "menu")
            {
                PromptDialog.Choice(
                    context,
                    AfterMenuChoiceAsync,
                    (new string[] { "My Account", "FAQ", "Closest Banks", "Contact An Agent", "Exchange Rates", "Stocks" }),
                    "Please select one of this options: ");
            }
            else
            {
                await context.PostAsync("This functionality is not yet available, please type \"menu\".");
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task AfterMenuChoiceAsync(IDialogContext context, IAwaitable<string> result)
        {
            var choice = await result;
            
            switch (choice)
            {
                case "My Account":
                    context.Call(new Account.AccountRootDialog(), this.MessageReceivedAsync);
                    break;
                case "FAQ":
                    context.Call(new Account.AccountRootDialog(), this.MessageReceivedAsync);
                    break;
                case "Closest Banks":
                    context.Call(new Account.AccountRootDialog(), this.MessageReceivedAsync);
                    break;
                case "Contact An Agent":
                    context.Call(new Account.AccountRootDialog(), this.MessageReceivedAsync);
                    break;
                case "Exchange Rates":
                    context.Call(new Account.AccountRootDialog(), this.MessageReceivedAsync);
                    break;
                case "Stocks":
                    context.Call(new Account.AccountRootDialog(), this.MessageReceivedAsync);
                    break;
                default:
                    await context.PostAsync("This functionality is not yet available, please type \"menu\" and choose another option.");
                    break;
            }
            
        }
    }
}