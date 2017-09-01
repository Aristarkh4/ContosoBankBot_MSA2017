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
                var stateClient = activity.GetStateClient();
                BotData botData = await stateClient.BotState.GetPrivateConversationDataAsync(activity.ChannelId, activity.Conversation.Id, activity.From.Id);

                string accountId = botData.GetProperty<string>("accountId");
                string password = botData.GetProperty<string>("password");

                // Person did not yet log in during this conversation
                if(LogInDialog.isCorrectLogIn(accountId, password))
                {
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
                        await context.PostAsync("You attempted too many times, please type \"menu\" and try again.");
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