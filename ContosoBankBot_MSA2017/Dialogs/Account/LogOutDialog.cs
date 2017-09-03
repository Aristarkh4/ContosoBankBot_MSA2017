using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ContosoBankBot_MSA2017.Dialogs.Account
{
    [Serializable]
    public class LogOutDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.UserData.RemoveValue("accountId");
            context.UserData.RemoveValue("password");
            context.UserData.RemoveValue("email");

            await context.PostAsync("Log out successful.");
            context.Done<object>(null);
        }
    }
}