using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Text;

namespace ContosoBankBot_MSA2017.Dialogs.Account
{
    [Serializable]
    public class TransferFundsDialog : IDialog<object>
    {
        private string senderAccountId;
        private string receiverAccountId;
        private double transferSum;

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Please enter somehting.");
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity == null)
            {
                await context.PostAsync("Please enter somehting.");
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                await context.PostAsync("Checking the authorisation.");

                senderAccountId = context.UserData.GetValueOrDefault<string>("accountId", null);
                string password = context.UserData.GetValueOrDefault<string>("password", null);

                // Person did not yet log in during this conversation
                if ((senderAccountId == null) || (password == null) || (!await LogInDialog.IsCorrectLogInAsync(context, senderAccountId, password)))
                {
                    await context.PostAsync("Please log in.");
                    context.Call(new LogInDialog(), this.MessageReceivedAsync);
                }
                else
                {
                    await RequestTransferAsync(context);
                }
            }
        }

        private async Task RequestTransferAsync(IDialogContext context)
        {
            try
            {
                PromptDialog.Text(
                    context,
                    AfterSelectingReceiverAsync,
                    "Please enter the reciever's Contoso Bank Account ID.");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("You attempted too many times, please try again.");
                await AfterFinishingTransferAsync(context);
            }
        }

        private async Task AfterSelectingReceiverAsync(IDialogContext context, IAwaitable<string> result)
        {
            receiverAccountId = await result;

            try
            {
                PromptDialog.Text(
                    context,
                    AfterSelectingTransferSumAsync,
                    "Please enter the sum you want to transfer.");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("You attempted too many times, please try again.");
                await AfterFinishingTransferAsync(context);
            }
        }

        private async Task AfterSelectingTransferSumAsync(IDialogContext context, IAwaitable<string> result)
        {
            transferSum = double.Parse(await result);
            if(transferSum <= 0) { 
                await context.PostAsync("Entered transfer sum is incorrect, please try again.");
                await AfterFinishingTransferAsync(context);
            }
            else
            {
                await context.PostAsync("Got it. Now transferring " + transferSum + " from " + senderAccountId + " to " + receiverAccountId + ".");

                Models.Account senderAccount = null;
                Models.Account receiverAccount = null;
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
                        if (account.AccountId == senderAccountId)
                        {
                            senderAccount = account;
                        }

                        if (account.AccountId == receiverAccountId)
                        {
                            receiverAccount = account;
                        }
                    }

                    if (senderAccount == null || receiverAccount == null)
                    {
                        throw new ArgumentException();
                    }
                }
                catch
                {
                    await context.PostAsync("Somehting went wrong, please check that the provided account IDs are correct.");
                    await AfterFinishingTransferAsync(context);
                }

                if(senderAccount.Balance < transferSum)
                {
                    await context.PostAsync("You don't have enough money for this transfer.");
                    await AfterFinishingTransferAsync(context);
                } else
                {
                    senderAccount.Balance = senderAccount.Balance - transferSum;
                    receiverAccount.Balance = receiverAccount.Balance + transferSum;

                    var senderAccountString = new JavaScriptSerializer().Serialize(senderAccount);
                    var receiverAccountString = new JavaScriptSerializer().Serialize(receiverAccount);
                    HttpContent httpContentSender = new StringContent(senderAccountString, Encoding.UTF8, "application/json");
                    HttpContent httpContentReceiver = new StringContent(receiverAccountString, Encoding.UTF8, "application/json");
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var senderResponse = await client.PostAsync(new Uri(url), httpContentSender);
                            var receiverResponse = await client.PostAsync(new Uri(url), httpContentReceiver);
                            senderResponse.EnsureSuccessStatusCode();
                            receiverResponse.EnsureSuccessStatusCode();
                            await context.PostAsync("Transfer was succesful.");
                        }
                    }
                    catch
                    {
                        await context.PostAsync("Something went wrong, please try again.");
                    }

                    await AfterFinishingTransferAsync(context);
                }
            }
        }

        private async Task AfterFinishingTransferAsync(IDialogContext context)
        {
            try
            {
                PromptDialog.Choice(
                    context,
                    AfterChoosingNextAcitonAsync,
                    (new string[] { "Make another transfer", "See my balance", "Go back" }),
                    "What do you want to do now?");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("Didn't quite catch that. Please try again.");
                await AfterFinishingTransferAsync(context);
            }
        }

        private async Task AfterChoosingNextAcitonAsync(IDialogContext context, IAwaitable<string> result)
        {
            var choice = await result;

            switch (choice)
            {
                case "Make another transfer":
                    context.Wait(MessageReceivedAsync);
                    break;
                case "See my balance":
                    context.Call(new BalanceDialog(), this.MessageReceivedAsync);
                    break;
                case "Go back":
                    context.Done<object>(null);
                    break;
                default:
                    await context.PostAsync("Didn't quite get it. Please try again.");
                    await AfterFinishingTransferAsync(context);
                    break;
            }
        }
    }
}