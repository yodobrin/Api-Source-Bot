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

using LuisBot.DataTypes;
using LuisBot.Dialogs;
using System.Threading;


using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
		
        public const string emailOption = "email";
        public const string botOption = "bot";

       // public string UserName = "";
        Lead MyLead = new Lead(); 


        public IList<ProductDocument> products;
        public RootDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
            products = new List<ProductDocument>();
        }
        

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
            context.Call(new GenericDetailDialog("Name"), this.ResumeAfterGreating);
            //await this.ShowLuisResult(context, result);
        }

        
        [LuisIntent("Catalog.Fetch")]
        public async Task CatalogFetchIntent(IDialogContext context, LuisResult result)
        {

            switch (result.Query)
            {
                case "flush":
                    await FlushProducts(context);
                    break;
                case "bymail":
                    await context.PostAsync($"so be it, but i will need the mail");
                    break;


            }
            context.Wait(this.MessageReceived);
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
            
            await context.PostAsync($"Ok, let me find relevant information...");
           
            await context.Forward(new SearchDialog(result.Entities, result.Query), this.ResumeAfterSearchDialog, context.Activity, CancellationToken.None);

		}

        private  Attachment GetOpenCard(string name, string company)
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

        private static Attachment GetResultCard(IList<ProductDocument> products)
        {
            string suffix = "";
            if (products.Count > 0)
            {
                suffix = (products.Count == 1) ? "" : "s";
                // yes we have
               // await context.PostAsync($"I found: {products.Count} product{suffix}.");
            }
            var resultCard = new HeroCard
            {
                Title = $"I found: {products.Count} product{suffix}.",
                Subtitle = "Matching your search criteria",
                Text = "How would you like the information be provided?",
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/image-for-laszlo-article-june-2018.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Send me an email", value: "bymail"), new CardAction(ActionTypes.PostBack, "Flush it here please", value: "flush") }
            };

            return resultCard.ToAttachment();
        }
       
        private async Task ResumeAfterSearchDialog(IDialogContext context, IAwaitable<object> result)
        {
            products =(IList<ProductDocument>) await result;
            //if(products.Count >0)
            //{
            //    string suffix = (products.Count == 1) ? "" : "s";
            //    // yes we have
            //    await context.PostAsync($"I found: {products.Count} product{suffix}.");
            //}
            //await context.PostAsync($"Result count: {products.Count} ");
            var message = context.MakeMessage();

            message.Attachments.Add(GetResultCard(products));

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);

        }

        /**
        * Spits out the products found
        * 
        */
        private async Task FlushProducts(IDialogContext context)
        {
            foreach (ProductDocument prd in products)
            {
                await context.PostAsync($"I got {prd.MoleculeID} -- {prd.MoleculeName} -- {prd.TapiProductName} ");
            }
        }

        private async Task ResumeAfterGreating(IDialogContext context, IAwaitable<string> result)
        {
            MyLead.Name = await result;
            await context.PostAsync($"Hi { MyLead.Name}! And thank you for using APISourceBot !");            
            context.Call(new GenericDetailDialog("Company"), this.ResumeAfterCompany);            
        }
        private async Task ResumeAfterCompany(IDialogContext context, IAwaitable<string> result)
        {
            MyLead.Company = await result;
            await context.PostAsync($"Glad to see you work for {MyLead.Company}");

            var message = context.MakeMessage();

            message.Attachments.Add(GetOpenCard(MyLead.Name,MyLead.Company));

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }



        [LuisIntent("CRM.LeadCreation")]
		public async Task CRMLeadCreationIntent(IDialogContext context, LuisResult result)
		{
			//await this.ShowLuisExtendedt(context, result);
			await Utilities.AddMessageToQueueAsync("wow");
			await this.ShowLuisResult(context, result);
		}

		[LuisIntent("Product Name")]
		public async Task ProductNameIntent(IDialogContext context, LuisResult result)
		{
			//await this.ShowLuisExtendedt(context, result);
			
			await this.ShowLuisResult(context, result);
		}
		private async Task ShowLuisResult(IDialogContext context, LuisResult result) 
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Wait(MessageReceived);
        }

               

        
    }
}