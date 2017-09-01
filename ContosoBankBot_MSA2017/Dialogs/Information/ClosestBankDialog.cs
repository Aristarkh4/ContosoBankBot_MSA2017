using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

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

                List<string> branchAddresses = await RequestBranchAddressesAsync(cityName);
                if(branchAddresses.Count == 0)
                {
                    await context.PostAsync("Sorry, no branches were found.");
                }
                else
                {
                    await context.PostAsync("Here is the list of Contoso Bank branches in your city:");
                    foreach (string branchAddress in branchAddresses)
                    {
                        await context.PostAsync(branchAddress);
                    }
                }
                

                try
                {
                    PromptDialog.Choice(
                        context,
                        AfterAddressesFoundAsync,
                        (new string[] { "Search for branches in another city", "Go back" }),
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

        private async Task<List<string>> RequestBranchAddressesAsync(string cityName)
        {
            List<string> branchAddresses = new List<string>();

            string url = "http://contosobankbotmsa2017dataapp.azurewebsites.net/tables/branches?zumo-api-version=2.0.0";

            try
            {
                Models.BankBranch[] branches;
                using (var client = new HttpClient())
                {
                    string responseString = await client.GetStringAsync(new Uri(url));
                    branches = JsonConvert.DeserializeObject<Models.BankBranch[]>(responseString);
                }

                foreach (Models.BankBranch branch in branches)
                {
                    if (branch.CityName == cityName)
                    {
                        branchAddresses.Add(branch.BranchAddress);
                    }
                }
            }
            catch { }

            return branchAddresses;
        }
    }
}