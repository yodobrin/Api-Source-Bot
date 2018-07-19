/*
Sample Code is provided for the purpose of illustration only and is not intended to be used in a production environment.
THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
We grant You a nonexclusive, royalty-free right to use and modify the Sample Code and to reproduce and distribute the object code form of the Sample Code, provided that. 
You agree: 
	(i) to not use Our name, logo, or trademarks to market Your software product in which the Sample Code is embedded;
    (ii) to include a valid copyright notice on Your software product in which the Sample Code is embedded; and
	(iii) to indemnify, hold harmless, and defend Us and Our suppliers from and against any claims or lawsuits, including attorneys’ fees, that arise or result from the use or distribution of the Sample Code
**/

// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)

using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;

using Microsoft.Bot.Builder.Luis.Models;


using System.Collections.Generic;
using SourceBot.Utils;

using SourceBot.DataTypes;
using SourceBot.Dialogs;
using System.Threading;


using Microsoft.Bot.Connector;

namespace SourceBot.Dialogs
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
		
        //public const string emailOption = "email";
        //public const string botOption = "bot";

        Lead MyLead; 
        public IList<ProductDocument> tproducts;
        string Action = Lead.SEARCH;

        public RootDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
            tproducts = new List<ProductDocument>();
            //MyLead = new Lead();
        }
        
        /**
         * Intents Section
         *
         */ 

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(Utilities.GetSentence("1"));
            context.Call(new DetailsDialog(), this.ResumeAfterForm);
            //context.Wait(this.MessageReceived);
        }

        [LuisIntent("CRM.SendCatalog")]
        public async Task SendCatalogIntent(IDialogContext context, LuisResult result)
        {
            Action = Lead.PDF;
            if(MyLead==null) context.Call(new DetailsDialog(), this.ResumeAfterForm);
            else MyLead.SetAction(Action);

            //context.Call(new SendCatalogDialog(MyLead), this.ResumeAfterSend);
            //context.Wait(this.MessageReceived);
        }


        [LuisIntent("Catalog.Fetch")]
        public async Task CatalogFetchIntent(IDialogContext context, LuisResult result)
        {
            switch (result.Query)
            {
                case "flush":
                    await FlushProducts(context);
                    context.Wait(this.MessageReceived);
                    break;
                case "bymail":
                    if(MyLead!=null && MyLead.IsLead())
                    {                        
                        await Utilities.AddMessageToQueueAsync(MyLead.ToMessage());
                        await context.PostAsync($"A request was sent to our communication auto-broker to the address:{MyLead.Email} provided.");
                        
                    }
                    else
                    {
                        Action = Lead.SEARCH;
                        context.Call(new DetailsDialog(), this.ResumeAfterForm);
                    }
                        
                    // await context.PostAsync($"so be it, but i will need the mail");
                    //await context.Forward(new GenericDetailDialog("Email"), this.ResumeAfterEmail,context.Activity, CancellationToken.None);
                    break;
                default: break;                 
            }
            

        }



        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        /**
         * 
         * In case of a find item intent, the context is forwaded to the search dialog. 
         * The search dialog will return a list of products it retrived from the azure search
         * 
         */ 
		[LuisIntent("Catalog.FindItem")]
		public async Task CatalogFindItemIntent(IDialogContext context, LuisResult result)
		{
            //await context.Forward(new SearchDialog(result.Entities, result.Query), this.ResumeAfterSearchDialog, context.Activity, CancellationToken.None);
            await context.PostAsync($"Searching for:{result.Query}");
            // setting the action to search
            Action = Lead.SEARCH;
            await context.Forward(new SearchDialog(result.Entities, result.Query), this.ResumeAfterSearchDialog, context.Activity, CancellationToken.None);
            //context.Call(new SearchDialog(result.Entities, result.Query), this.ResumeAfterSearchDialog);
            //context.Wait(this.MessageReceived);
        }


        [LuisIntent("CRM.Lead")]
        public async Task CRMLeadIntent(IDialogContext context, LuisResult result)
        {
            //await context.PostAsync($"You are in CRMLeadIntent");            
            if (MyLead==null || !MyLead.IsLead())
            {
                MyLead = new Lead();
                //setting the action to lead creation
                Action = Lead.LEADCREATE;
                // await context.PostAsync($"You asked to be contacted via email, however I have yet to capture valid contact details");
                context.Call(new DetailsDialog(), this.ResumeAfterForm);//, context.Activity, CancellationToken.None);
            }                  
            //context.Wait(this.MessageReceived);
        }

        [LuisIntent("CRM.SubmitLead")]
        public async Task CRMSubmitLeadIntent(IDialogContext context, LuisResult result)
        {
            
            await Utilities.AddMessageToQueueAsync(MyLead.ToMessage());               
            await context.PostAsync($"A request was sent to our communication auto-broker with the {MyLead.Email} provided.");
            context.Wait(this.MessageReceived);
        }


        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Wait(MessageReceived);
        }


        private Attachment GetOpenCard(string name, string company)
        {
            var openCard = new HeroCard
            {
                Title = $"API Source Bot tailored for {name} @ {company}",
                Subtitle = "Tapi bots — Welcome tapi your api partner",
                Text = "Active Pharmaceutical Ingredients (API) Production and Manufacturing - information and knowledge by TAPI's experts.\n It's all here. You can search by typing sentences as below:!",
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/about-us-new.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Find me Aripiprazole", value: "find me Aripiprazole"), new CardAction(ActionTypes.PostBack, "Find me Aztreonam", value: "find me Aztreonam") }
            };

            return openCard.ToAttachment();
        }

       

        
       
        

        /**
        * Spits out the products found
        */
        private async Task FlushProducts(IDialogContext context)
        {
            foreach (ProductDocument prd in tproducts)
            {
                await context.PostAsync($"I got {prd.MoleculeID} -- {prd.MoleculeName} -- {prd.TapiProductName} ");
            }
        }

        

        /*
         * Resume After section, all the methods are called once another dialog is done
         * 
         */

        private async Task ResumeAfterSearchDialog(IDialogContext context, IAwaitable<object> result)
        {
            tproducts = (IList<ProductDocument>)await result;
            if (tproducts != null && tproducts.Count > 0)
            {
               // await context.PostAsync($"after search {tproducts.Count}");
               // SetSubject(tproducts);
                var message = context.MakeMessage();
                message.Attachments.Add(GetResultCard(tproducts));
                await context.PostAsync(message);
            }
            else await context.PostAsync("No results");
           // context.Wait(this.MessageReceived);
        }

        private static Attachment GetResultCard(IList<ProductDocument> tproducts)
        {
            string suffix = "";
            if (tproducts.Count > 0)
            {
                suffix = (tproducts.Count == 1) ? "" : "s";
            }
            var resultCard = new HeroCard
            {
                Title = $"I found: {tproducts.Count} product{suffix}.",
                Subtitle = "Matching your search criteria",
                Text = "How would you like the information be provided?",
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/image-for-laszlo-article-june-2018.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Send me an email", value: "bymail"), new CardAction(ActionTypes.PostBack, "Flush it here please", value: "flush") }
            };

            return resultCard.ToAttachment();
        }

        private void SetSubject(IList<ProductDocument> tproducts)
        {
            string result = "";
            if (tproducts != null)
            {
                foreach (ProductDocument prd in tproducts)
                {
                    string.Concat(result, ",", prd.MoleculeName);
                }
                MyLead.Subject = result;
            }
        }


        private async Task ResumeAfterForm(IDialogContext context, IAwaitable<Lead> result)
        {
            MyLead = await result;
            MyLead.SetAction(Action);


            //await context.PostAsync($"Hi { MyLead.Name}! And thank you for using APISourceBot !");
            // echo the current lead details - it will direct to the submit lead intent, in case he clicks on 'Confirm'
            var message = context.MakeMessage();
            message.Attachments.Add(MyLead.GetLeadCard(tproducts));
            await context.PostAsync(message);
        }

        private async Task ResumeAfterSend(IDialogContext context, IAwaitable<object> result)
        {
            object obj = await result;
            MyLead.SetAction(Action);


            //await context.PostAsync($"Hi { MyLead.Name}! And thank you for using APISourceBot !");
            // echo the current lead details - it will direct to the submit lead intent, in case he clicks on 'Confirm'
            var message = context.MakeMessage();
            message.Attachments.Add(MyLead.GetLeadCard(tproducts));
            await context.PostAsync(message);
        }

        private Attachment GetLeadCard9()
        {
            if (tproducts != null && tproducts.Count > 0 && MyLead!=null) SetSubject(tproducts);

            MyLead = (MyLead != null) ? MyLead : new Lead("dum");
            var leadCard = new ThumbnailCard
            {
                Title = $"Hello {MyLead.Name} @ {MyLead.Company}",
                Subtitle = "This is what I know so far about as a lead...",
                Text = $"Your Email: {MyLead.Email}\n You were searching for {MyLead.Subject}",
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/hp-banner_0001_wearetapi.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Confirm", value: "confirm-lead-creation"), new CardAction(ActionTypes.PostBack, "Revisit Details", value: "i am a dealer") }
            };

            return leadCard.ToAttachment();
        }




    }
}