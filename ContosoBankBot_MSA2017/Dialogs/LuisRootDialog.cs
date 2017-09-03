using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ContosoBankBot_MSA2017.Dialogs
{
    [LuisModel(, , domain: , staging: true)]
    [Serializable]
    public class LuisRootDialog : LuisDialog<object>
    {
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            await this.MessageReceived(context, (IAwaitable<IMessageActivity>)result);
        }

        [LuisIntent("Hi")]
        private async Task HiAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hello.");
        }

        [LuisIntent("Menu")]
        [LuisIntent("")]
        private async Task OpenMenuAsync(IDialogContext context, LuisResult result)
        {
            context.Call(new RootDialog(), this.MessageReceivedAsync);   
        }

        [LuisIntent("ContactAgent")]
        private async Task ContactAgentAsync(IDialogContext context, LuisResult result)
        {
            context.Call(new ContactAgentDialog(), this.MessageReceivedAsync);
        }

        [LuisIntent("Information.FindBank")]
        private async Task FindBankAsync(IDialogContext context, LuisResult result)
        {
            context.Call(new Information.ClosestBankDialog(), this.MessageReceivedAsync);
        }

        [LuisIntent("Information.AskQuestion")]
        private async Task AskQuestionAsync(IDialogContext context, LuisResult result)
        {
            context.Call(new Information.QnADialog(), this.MessageReceivedAsync);
        }

        [LuisIntent("AdditionalFunctionality.Currency")]
        private async Task CurrencyAsync(IDialogContext context, LuisResult result)
        {
            context.Call(new AdditionalFunctionality.CurrencyDialog(), this.MessageReceivedAsync);
        }

        [LuisIntent("Account.AccessAccount")]
        private async Task AccessAccountAsync(IDialogContext context, LuisResult result)
        {
            context.Call(new Account.AccountRootDialog(), this.MessageReceivedAsync);
        }

        [LuisIntent("Account.LogIn")]
        private async Task LogInAsync(IDialogContext context, LuisResult result)
        {
            context.Call(new Account.LogInDialog(), this.MessageReceivedAsync);
        }

        [LuisIntent("Account.LogOut")]
        private async Task LogOutAsync(IDialogContext context, LuisResult result)
        {
            context.Call(new Account.LogOutDialog(), this.MessageReceivedAsync);
        }

        [LuisIntent("Account.GetBalance")]
        private async Task GetBalanceAsync(IDialogContext context, LuisResult result)
        {
            context.Call(new Account.BalanceDialog(), this.MessageReceivedAsync);
        }

        [LuisIntent("Account.TransferFunds")]
        private async Task TransferBalanceAsync(IDialogContext context, LuisResult result)
        {
            context.Call(new Account.TransferFundsDialog(), this.MessageReceivedAsync);
        }
    }
}