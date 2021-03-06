﻿using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ContosoBankBot_MSA2017.Dialogs.AdditionalFunctionality
{
    [Serializable]
    public class CurrencyDialog : IDialog<object>
    {
        private string  baseCurrency = "EUR";
        private string goalCurrency = "EUR";

        public async Task StartAsync(IDialogContext context)
        {
            baseCurrency = context.UserData.GetValueOrDefault<string>("baseCurrency", "EUR");
            await context.PostAsync("What currency rate do you want to know? (USD, GBP, etc.)");
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {

            var activity = await result as Activity;

            if (activity == null)
            {
                await context.PostAsync("What currency rate do you want to know? (USD, GBP, etc.)");
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                goalCurrency = activity.Text;
                await AfterGoalCurrencyReceivedAsync(context);
            }    
        }

        private async Task AfterGoalCurrencyReceivedAsync(IDialogContext context)
        {
            try
            {
                PromptDialog.Confirm(
                    context,
                    AfterAcceptingChangeOfBaseCurrencyAsync,
                    "Current base currency is " + baseCurrency + ". Do you want to change base currency ?");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("You attempted too many times, please enter the goal currency again.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterAcceptingChangeOfBaseCurrencyAsync(IDialogContext context, IAwaitable<bool> result)
        {
            var isBaseCurrencyChanging = await result;
            if (isBaseCurrencyChanging)
            {
                try
                {
                    PromptDialog.Text(
                        context,
                        AfterReceivingNewBaseCurrencyAsync,
                        "Enter the new base currency.");
                }
                catch (TooManyAttemptsException e)
                {
                    await context.PostAsync("You attempted too many times, please enter the goal currency again.");
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                await GetGoalCurrencyRateAsync(context);
            }
        }

        private async Task AfterReceivingNewBaseCurrencyAsync(IDialogContext context, IAwaitable<string> result)
        {
            baseCurrency = await result;
            context.UserData.SetValue<string>("BaseCurrency", baseCurrency);
            await GetGoalCurrencyRateAsync(context);
        }

        private async Task GetGoalCurrencyRateAsync(IDialogContext context)
        {
            await context.PostAsync("Got it. Give me a second to get the latest exchange rate.");

            string url = "http://api.fixer.io/latest?base=" + baseCurrency + "&symbols=" + goalCurrency;
            try
            {
                string responseString = null;
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    responseString = await response.Content.ReadAsStringAsync();
                }

                var ratesList = JObject.Parse(responseString);
                var rates = ratesList.Value<JObject>("rates");
                var goalCurrencyRate = rates.Value<string>(goalCurrency);
                await context.PostAsync("The exhange rate of " + goalCurrency + " with base " + baseCurrency + " is: " + goalCurrencyRate);
            }
             catch
            {
                await context.PostAsync("Sorry, something went wrong. Please try again. Ensure that the currencies you select are not misspelt.");
            }

            // Ask what to do next
            PromptDialog.Choice(
                context,
                AfterCurrancyRateGivenAsync,
                (new string[] { "Get another currency exchange rate", "Go back" }),
                "What do you want to do now?");
        }

        private async Task AfterCurrancyRateGivenAsync(IDialogContext context, IAwaitable<string> result)
        {
            var choice = await result;

            switch (choice)
            {
                case "Get another currency exchange rate":
                    await context.PostAsync("What currency rate do you want to know? (USD, GBP, etc.)");
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