﻿using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using Newtonsoft.Json;

namespace ContosoBankBot_MSA2017.Dialogs.Account
{
    [Serializable]
    public class LogInDialog : IDialog<object>
    {
        private BotData privateConversationData;
        private string accountId;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if(activity == null)
            {
                context.Wait(MessageReceivedAsync);
            } else
            {
                privateConversationData = activity.GetStateClient().BotState.GetPrivateConversationData(activity.ChannelId, activity.Conversation.Id, activity.From.Id);
                try
                {
                    PromptDialog.Choice(
                        context,
                        AfterSelectingOptionAsync,
                        (new string[] { "Log in", "Create new account", "Cancel"}),
                        "Do you have an account?");
                }
                catch (TooManyAttemptsException e)
                {
                    await context.PostAsync("You attempted too many times, please try again.");
                    context.Wait(MessageReceivedAsync);
                }
            }
        }

        private async Task AfterSelectingOptionAsync(IDialogContext context, IAwaitable<string> result)
        {
            var choice = await result;

            switch(choice)
            {
                case ("Log in"):
                    await LogInAsync(context);
                    break;
                case ("Create new account"):
                    context.Call(new CreateAnAccountDialog(), MessageReceivedAsync);
                    break;
                case ("Cancel"):
                    context.Call(new RootDialog(), MessageReceivedAsync);
                    break;
                default:
                    await context.PostAsync("Didn't quite get it. Please try again.");
                    context.Wait(MessageReceivedAsync);
                    break;
            }
        }

        private async Task LogInAsync(IDialogContext context)
        {
            try
            {
                PromptDialog.Text(
                    context,
                    AfterGettingAccountIdAsync,
                    "What is your Contoso Bank Account ID number?");
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
                    "What is your Contoso Bank Account password?");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("You attempted too many times, please try again.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterGettingPasswordAsync(IDialogContext context, IAwaitable<string> result)
        {
            string password = await result;

            if(await IsCorrectLogInAsync(accountId, password))
            {
                privateConversationData.SetProperty<string>("accountId", accountId);
                privateConversationData.SetProperty<string>("password", password);
                context.Done<object>(null);
            } else {
                await context.PostAsync("The account ID and/or password you entered are incorrect. Please try again.");
                context.Wait(MessageReceivedAsync);
            }
        }

        public static async Task<bool> IsCorrectLogInAsync(string accountId, string password)
        {

            bool isCorrectLogIn = false;

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
                        if(account.Password == password)
                        {
                            isCorrectLogIn = true;
                            break;
                        } 
                    }
                }
            }
            catch { }

            return isCorrectLogIn;
        }
    }
}