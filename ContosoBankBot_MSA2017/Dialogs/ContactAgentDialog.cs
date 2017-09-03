using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ContosoBankBot_MSA2017.Dialogs
{
    [Serializable]
    public class ContactAgentDialog : IDialog<object>
    {
        private string userFullname;
        private string userEmail;
        private string enquiryTopic;
        private string enquiry;

        public async Task StartAsync(IDialogContext context)
        {
            try
            {
                PromptDialog.Confirm(
                        context,
                        ContactAnAgentAsync,
                        "Do you want to contact an agent?");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("You attempted too many times, please start over again.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                PromptDialog.Confirm(
                        context,
                        ContactAnAgentAsync,
                        "Do you want to contact an agent?");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("You attempted too many times, please start over again.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task ContactAnAgentAsync(IDialogContext context, IAwaitable<bool> result)
        {
            bool contactAnAgent = await result;

            if(contactAnAgent)
            {
                if (context.UserData.ContainsKey("fullName")) {
                    // Name is already in the save data
                    await AfterGetUserFullnameAsync(context, new AwaitableFromItem<string>(context.UserData.GetValue<string>("fullName")));
                } else
                {
                    try
                    {
                        PromptDialog.Text(
                        context,
                        AfterGetUserFullnameAsync,
                        "Please give us your full name.");
                    }
                    catch (TooManyAttemptsException e)
                    {
                        await context.PostAsync("You attempted too many times, please start over again.");
                        context.Wait(MessageReceivedAsync);
                    }
                }
            } else
            {
                // Exit the dialog
                context.Done<object>(null);
            }
        }

        private async Task AfterGetUserFullnameAsync(IDialogContext context, IAwaitable<string> result)
        {
            userFullname = await result;

            if (context.UserData.ContainsKey("email"))
            {
                // Email is already in the save data
                await AfterGetUserFullnameAsync(context, new AwaitableFromItem<string>(context.UserData.GetValue<string>("email")));
            }
            else
            {
                try
                {
                    PromptDialog.Text(
                    context,
                    AfterGetUserEmailAsync,
                    "Please give us your email adress.");
                }
                catch (TooManyAttemptsException e)
                {
                    await context.PostAsync("You attempted too many times, please start over again.");
                    context.Wait(MessageReceivedAsync);
                }
            }
        }

        private async Task AfterGetUserEmailAsync(IDialogContext context, IAwaitable<string> result)
        {
            userEmail = await result;

            try
            {
                PromptDialog.Text(
                context,
                AfterGetEnquiryTopicAsync,
                "What is the topic of your enquiry?");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("You attempted too many times, please start over again.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterGetEnquiryTopicAsync(IDialogContext context, IAwaitable<string> result)
        {
            enquiryTopic = await result;

            try
            {
                PromptDialog.Text(
                context,
                AfterGetEnquiryAsync,
                "What is your enquiry?");
            }
            catch (TooManyAttemptsException e)
            {
                await context.PostAsync("You attempted too many times, please start over again.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterGetEnquiryAsync(IDialogContext context, IAwaitable<string> result)
        {
            enquiry = await result;

            await context.PostAsync("Contacting an agent is not available in this version. Sorry for inconvenience.");
            context.Done<object>(null);
        }
    }
}