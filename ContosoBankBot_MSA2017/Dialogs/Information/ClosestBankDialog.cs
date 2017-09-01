using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;

namespace ContosoBankBot_MSA2017.Dialogs.Information
{
    [Serializable]
    public class ClosestBankDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("What is the name of your city?");
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if(activity == null)
            {
                await context.PostAsync("What is the name of your city?");
                context.Wait(MessageReceivedAsync);
            }
            else {
                await context.PostAsync("Got it. Please wait a second, I'll look for the branch adresses in your city.");
                string cityName = activity.Text;

                List<string> branchAddresses = RequestBranchAddressesAsync(cityName);
                await context.PostAsync("Here is the list of Contoso Bank branches in your city:");
                foreach (string branchAddress in branchAddresses)
                {
                    await context.PostAsync(branchAddress);
                }

                try
                {
                    PromptDialog.Choice(
                        context,
                        AfterAddressesFoundAsync,
                        (new string[] { "Search for branches in another city", "Go back" })
    
                        "Did you find what you were looking for?");
                }
                catch(TooManyAttemptsException e)
                {
                    await context.PostAsync("Didn't quite catch that. Please try again.");
                    context.Wait(MessageReceivedAsync);
                }
            }
        }

        private async Task AfterAddressesFoundAsync(IDialogContext context, IAwaitable<string> result)
        {
            var choice = await result;

            switch (choice)
            {
                case "Search for branches in another city":
                    await context.PostAsync("What is the name of your city?");
                    context.Wait(MessageReceivedAsync);
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

        private List<string> RequestBranchAddressesAsync(string cityName)
        {
            List<string> branchAddresses = new List<string>();

            return branchAddresses;
        }
    }
}