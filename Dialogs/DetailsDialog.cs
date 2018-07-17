using System;
using System.Collections.Generic;

using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using LuisBot.DataTypes;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class DetailsDialog : IDialog<Lead>
    {
        public async Task StartAsync(IDialogContext context)
    {
        await context.PostAsync("Lets start ...");

        var leadFormDialog = FormDialog.FromForm(this.BuildLeadForm, FormOptions.PromptInStart);

        context.Call(leadFormDialog, this.ResumeAfterLeadFormDialog);
    }

    private IForm<Lead> BuildLeadForm()
    {
        OnCompletionAsyncDelegate<Lead> processLead = async (context, state) =>
        {
            await context.PostAsync($"Ok. got the info {state.Name}");
        };

        return new FormBuilder<Lead>()
            .AddRemainingFields()
            .OnCompletion(processLead)
            .Build();
    }

        private Attachment GetLeadCard(Lead MyLead)
        {
            var leadCard = new HeroCard
            {
                Title = $"Hello {MyLead.Name} @ {MyLead.Company}",
                Subtitle = "This is what I know so far about as a lead...",
                Text = $"Your Email: {MyLead.Email}\n You were searching for {MyLead.Subject}",
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/hp-banner_0001_wearetapi.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Confirm", value: "confirm"), new CardAction(ActionTypes.PostBack, "Revisit Details", value: "hi") }
            };

            return leadCard.ToAttachment();
        }

        private async Task ResumeAfterLeadFormDialog(IDialogContext context, IAwaitable<Lead> result)
    {
            Lead lead = null;
        try
        {
            lead = await result;

            var resultMessage = context.MakeMessage();
            resultMessage.Attachments.Add(GetLeadCard(lead));

            await context.PostAsync(resultMessage);
        }
        catch (FormCanceledException ex)
        {
            string reply;

            if (ex.InnerException == null)
            {
                reply = "You have canceled the operation. Quitting from the LeadDialog";
            }
            else
            {
                reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
            }

            await context.PostAsync(reply);
        }
        finally
        {
            context.Done<Lead>(lead);
        }
    }

   
}
}