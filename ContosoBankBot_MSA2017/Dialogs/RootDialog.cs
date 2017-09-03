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
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (activity == null)
            {
                await context.PostAsync("How can I help you?");
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                try
                {
                    PromptDialog.Choice(
                        context,
                        AfterMenuChoiceAsync,
                        (new string[] { "My account", "Ask a question", "Closest banks", "Contact an agent", "Exchange rates", "Stocks" }),
                        "What do you want to do today?");
                }
                catch (TooManyAttemptsException e)
                {
                    await context.PostAsync("You attempted too many times, please try again.");
                    context.Wait(MessageReceivedAsync);
                }
            }
        }

        public async Task AfterMenuChoiceAsync(IDialogContext context, IAwaitable<string> result)
        {
            var choice = await result;
            
            switch (choice)
            {
                case "My account":
                    context.Call(new Account.AccountRootDialog(), this.MessageReceivedAsync);
                    break;
                case "Ask a question":
                    context.Call(new Information.QnADialog(), this.MessageReceivedAsync);
                    break;
                case "Closest banks":
                    context.Call(new Information.ClosestBankDialog(), this.MessageReceivedAsync);
                    break;
                case "Contact an agent":
                    context.Call(new ContactAgentDialog(), this.MessageReceivedAsync);
                    break;
                case "Exchange rates":
                    context.Call(new AdditionalFunctionality.CurrencyDialog(), this.MessageReceivedAsync);
                    break;
                case "Stocks":
                    context.Call(new AdditionalFunctionality.StocksDialog(), this.MessageReceivedAsync);
                    break;
                default:
                    await context.PostAsync("Didn't quite get it. Please try again.");
                    context.Wait(MessageReceivedAsync);
                    break;
            }
            
        }
    }
}