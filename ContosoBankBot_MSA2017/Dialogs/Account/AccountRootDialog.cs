using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ContosoBankBot_MSA2017.Dialogs.Account
{
    [Serializable]
    public class AccountRootDialog : IDialog<object>
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
                context.Wait(MessageReceivedAsync);
            } else
            {
                await context.PostAsync("Checking the authorisation.");

                string accountId = context.UserData.GetValueOrDefault<string>("accountId", null);
                string password = context.UserData.GetValueOrDefault<string>("password", null);

                // Person did not yet log in during this conversation
                if ((accountId == null) || (password == null) || (!await LogInDialog.IsCorrectLogInAsync(context, accountId, password))) 
                {
                    await context.PostAsync("Please log in.");
                    context.Call(new LogInDialog(), this.MessageReceivedAsync);
                }
                else
                {
                    // Show the account menu
                    try
                    {
                        PromptDialog.Choice(
                            context,
                            AfterMenuChoiceAsync,
                            (new string[] { "See the balance", "Transfer funds", "Log out", "Go back" }),
                            "How can I help you?");
                    }
                    catch (TooManyAttemptsException e)
                    {
                        await context.PostAsync("You attempted too many times, please try again.");
                        context.Wait(MessageReceivedAsync);
                    }
                }
            }

        }

        private async Task AfterMenuChoiceAsync(IDialogContext context, IAwaitable<string> result)
        {
            var choice = await result;

            switch (choice)
            {
                case "See the balance":
                    context.Call(new BalanceDialog(), this.MessageReceivedAsync);
                    break;
                case "Transfer funds":
                    context.Call(new TransferFundsDialog(), this.MessageReceivedAsync);
                    break;
                case "Log out":
                    context.Call(new LogOutDialog(), this.MessageReceivedAsync);
                    break;
                case "Go back":
                    context.Done<object>(null);
                    break;
                default:
                    await context.PostAsync("Didn't quite get it. Please try again.");
                    context.Wait(MessageReceivedAsync);
                    break;
            }
        }
    }
}