using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using Newtonsoft.Json;

namespace ContosoBankBot_MSA2017.Dialogs.Account
{
    [Serializable]
    public class BalanceDialog : IDialog<object>
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
            }
            else
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
                    await GetTheBalanceAsync(context, accountId);
                }
            }

        }

        private async Task GetTheBalanceAsync(IDialogContext context, string accountId)
        {
            double balance;

            string url = "http://contosobankbotmsa2017dataapp.azurewebsites.net/tables/accounts?zumo-api-version=2.0.0";
            try
            {
                Models.Account[] accounts;
                using (var client = new HttpClient())
                {
                    string responseString = await client.GetStringAsync(new Uri(url));
                    accounts = JsonConvert.DeserializeObject<Models.Account[]>(responseString);
                }

                foreach (Models.Account account in accounts)
                {
                    if (account.AccountId == accountId)
                    {
                        balance = account.Balance;
                        await context.PostAsync("Your account balance is " + balance);
                        break;
                    }
                }
            }
            catch { }

            try
            {
                PromptDialog.Choice(
                    context,
                    AfterGettingBalanceAsync,
                    (new string[] { "Get the balance", "Go back" }),
                    "What do you want to do?");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("Didn't quite catch that. Please try again.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterGettingBalanceAsync(IDialogContext context, IAwaitable<string> result)
        {
            var choice = await result;

            switch (choice)
            {
                case "Get the balance":
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
    }
}