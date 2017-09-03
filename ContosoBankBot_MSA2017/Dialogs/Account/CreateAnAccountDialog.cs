using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace ContosoBankBot_MSA2017.Dialogs.Account
{
    [Serializable]
    public class CreateAnAccountDialog : IDialog<object>
    {
        private string fullName;
        private string email;
        private string accountId;
        private string password;

        public async Task StartAsync(IDialogContext context)
        {
            try
            {
                PromptDialog.Text(
                    context,
                    AfterGettingFullNameAsync,
                    "What is your full name?");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("You attempted too many times, please try again.");
                context.Wait(MessageReceivedAsync);
            }
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
                try
                {
                    PromptDialog.Text(
                        context,
                        AfterGettingFullNameAsync,
                        "What is your full name?");
                }
                catch (TooManyAttemptsException e)
                {
                    await context.PostAsync("You attempted too many times, please try again.");
                    context.Wait(MessageReceivedAsync);
                }

            }
        }

        private async Task AfterGettingFullNameAsync(IDialogContext context, IAwaitable<string> result)
        {
            fullName = await result;

            try
            {
                PromptDialog.Text(
                    context,
                    AfterGettingEmailAsync,
                    "What is your email?");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("You attempted too many times, please try again.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterGettingEmailAsync(IDialogContext context, IAwaitable<string> result)
        {
            email = await result;

            try
            {
                PromptDialog.Text(
                    context,
                    AfterGettingAccountIdAsync,
                    "What is your Contoso Bank Account Id? This should be provided to you by bank when regestering for an account.");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("You attempted too many times, please try again.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterGettingAccountIdAsync(IDialogContext context, IAwaitable<string> result)
        {
            accountId = await result;

            try
            {
                PromptDialog.Text(
                    context,
                    AfterGettingPasswordAsync,
                    "Please enter a password for your account.");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("You attempted too many times, please try again.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterGettingPasswordAsync(IDialogContext context, IAwaitable<string> result)
        {
            password = await result;

            var newAccount = new Models.Account()
            {
                FullName = fullName,
                Email = email,
                AccountId = accountId,
                Password = password,
                Balance = 0
            };
            var newAccountString = new JavaScriptSerializer().Serialize(newAccount);
            HttpContent httpContent = new StringContent(newAccountString, Encoding.UTF8, "application/json");
            await context.PostAsync(newAccountString);
            string url = "http://contosobankbotmsa2017dataapp.azurewebsites.net/tables/accounts?zumo-api-version=2.0.0";
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(new Uri(url), httpContent);
                    response.EnsureSuccessStatusCode();
                }

                await context.PostAsync("Creating an acount was succesful.");
                context.UserData.SetValue("accountId", accountId);
                context.UserData.SetValue("password", password);
                context.UserData.SetValue("fullName", fullName);
                context.UserData.SetValue("email", email);
                context.Done<object>(null);
            }
            catch
            {
                await context.PostAsync("Something went wrong, please try again.");
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}