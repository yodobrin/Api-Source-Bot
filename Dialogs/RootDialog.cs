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

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
		
        public const string emailOption = "email";
        public const string botOption = "bot";

        public string UserName = "";
        Lead Lead = new Lead(); 


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

        private static Attachment GetOpenCard()
        {
            var heroCard = new HeroCard
            {
                Title = "API Source Bot",
                Subtitle = "Tapi bots — Welcome tapi your api partner",
                Text = "Active Pharmaceutical Ingredients (API) Production and Manufacturing - information and knowledge by TAPI's experts. It's all here!",
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/about-us-new.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework"), new CardAction(ActionTypes.PostBack, "WTF", value: "find me Aztreonam") }
            };

            return heroCard.ToAttachment();
        }

        /**
         * Spits out the products found
         * 
         */
        private async Task ResumeAfterSearchDialog(IDialogContext context, IAwaitable<object> result)
        {
            products =(IList<ProductDocument>) await result;
            await context.PostAsync($"Result count: {products.Count} ");
            foreach (ProductDocument prd in products)
            {
                await context.PostAsync($"I got {prd.MoleculeID} -- {prd.MoleculeName} -- {prd.TapiProductName} ");
            }
            
            context.Wait(this.MessageReceived);
        }

        private async Task ResumeAfterGreating(IDialogContext context, IAwaitable<string> result)
        {
            Lead.Name = await result;
            await context.PostAsync($"Hi { Lead.Name}! \n Thank you for using APISourceBot.");
            
            context.Call(new GenericDetailDialog("Company"), this.ResumeAfterCompany);
            //context.Wait(this.MessageReceived);
        }
        private async Task ResumeAfterCompany(IDialogContext context, IAwaitable<string> result)
        {
            Lead.Company = await result;
            await context.PostAsync($"Glad to see you work for {Lead.Company}");

            var message = context.MakeMessage();

            message.Attachments.Add(GetOpenCard());

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