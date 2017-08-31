using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using Newtonsoft.Json;
using System.Net;

namespace ContosoBankBot_MSA2017.Dialogs.Information
{
    [Serializable]
    public class QnADialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("What is your question?");
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            await AfterQuestionRecievedAsync(context, activity.Text);
        }

        private async Task AfterQuestionRecievedAsync(IDialogContext context, string result)
        {
            var question = result;

            await context.PostAsync("Your question is : \"" + question + "\". Let me see if we have an answer.");

            try
            {
                // Get a response
                string response = RequestAnswerFromQnAMaker(question);
                // Check whether answer was found or not
                if(response == "No good match found in the KB")
                {
                    await context.PostAsync("Sorry, I couldn't find anything.");
                } else
                {
                    await context.PostAsync("The answer for your question is:\n" + response);
                }
                // Ask what to do next
                PromptDialog.Choice(
                    context,
                    AfterQuestionAnsweredAsync,
                    (new string[] { "Ask another question", "Contact an agent", "Go back" }),
                    "Did this answer your question?");
            }
            catch
            {
                await context.PostAsync("Didn't quite catch that. Please try again.");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterQuestionAnsweredAsync(IDialogContext context, IAwaitable<string> result)
        {
            var choice = await result;

            switch (choice)
            {
                case "Ask another question":
                    await context.PostAsync("What is your question?");
                    context.Wait(MessageReceivedAsync);
                    break;
                case "Contact an agent":
                    context.Call(new ContactAgentDialog(), this.MessageReceivedAsync);
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

        private string RequestAnswerFromQnAMaker(string query)
        {
            string responseString = string.Empty;

            var knowledgebaseId = "0a4a012c-e0e2-49b6-8913-f49c6aa1f14d"; // Use knowledge base id created.
            var qnamakerSubscriptionKey = "228aaf337deb4bdb9b5d9340ec78358a"; //Use subscription key assigned to you.

            //Build the URI
            Uri qnamakerUriBase = new Uri("https://westus.api.cognitive.microsoft.com/qnamaker/v1.0");
            var builder = new UriBuilder($"{qnamakerUriBase}/knowledgebases/{knowledgebaseId}/generateAnswer");

            //Add the question as part of the body
            var postBody = $"{{\"question\": \"{query}\"}}";

            //Send the POST request
            using (WebClient client = new WebClient())
            {
                //Set the encoding to UTF8
                client.Encoding = System.Text.Encoding.UTF8;

                //Add the subscription key header
                client.Headers.Add("Ocp-Apim-Subscription-Key", qnamakerSubscriptionKey);
                client.Headers.Add("Content-Type", "application/json");
                responseString = client.UploadString(builder.Uri, postBody);
            }

            try
            {
                var response = JsonConvert.DeserializeObject<Models.QnAMakerResult>(responseString);
                return response.Answer;
            }
            catch
            {
                throw new Exception("Unable to deserialize QnA Maker response string.");
            }
        }
    }
}