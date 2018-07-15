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
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis.Models;

using System.Collections;
using System.Collections.Generic;
using SourceBot.Utils;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using LuisBot.DataTypes;
using LuisBot.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
		
        public const string emailOption = "email";
        public const string botOption = "bot";

        public string UserName = "";

        public IList<ProductDocument> products;
        public RootDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
            products = new List<ProductDocument>();
        }

        
        //public async override Task StartAsync(IDialogContext context)
        //{
        //    await context.PostAsync("Welcome to APISource Bot, I will help in finding information");
        //    await StartAsync(context);
        //}

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
            await this.ShowLuisResult(context, result);
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

		[LuisIntent("Catalog.FindItem")]
		public async Task CatalogFindItemIntent(IDialogContext context, LuisResult result)
		{
            await context.PostAsync($"Ok, let me find relevant information...");
            //var message = await result;
            await context.Forward(new SearchDialog(result.Entities, result.Query), this.ResumeAfterSearchDialog,result, CancellationToken.None);
            //IList<EntityRecommendation> entities = result.Entities;
            //ISearchIndexClient searchClient = Utilities.GetSearchClient();

            ////DocumentSearchResult  searchResult;
            
            //// loop over the entities find the "Product" entity
            //// todo need to ensure the number of results is accounted for BEFORE displayed
            //if (entities != null && entities.Count > 0)
            //{
            //    foreach (EntityRecommendation inst in entities)
            //    {
            //        if (Utilities.PRODUCT.Equals(inst.Type))
            //        {
            //            await SearchProduct(context, inst, searchClient);                    
            //        }
            //        else continue;
            //    }
            //}
            //// in case it is a find intent, but not recognized as a product

            //else await SearchQuery(context, result.Query, searchClient);
            //context.Wait(MessageReceivedAsync);
		}
        private async Task ResumeAfterSearchDialog(IDialogContext context, IAwaitable<object> result)
        {
            products =(IList<ProductDocument>) await result;
            string message = "";
            foreach (ProductDocument prd in products)
            {
                string.Concat(message, prd.MoleculeName," - ");
            }
            await context.PostAsync($"I got results: {message} ");
            context.Wait(this.MessageReceived);
        }

        

        

        //private async Task SearchProduct(IDialogContext context, EntityRecommendation prod, ISearchIndexClient searchClient)
        //{
        //    DocumentSearchResult searchResult = searchClient.Documents.Search(prod.Entity);
        //    if (searchResult != null)
        //    {
        //        await context.PostAsync($"Searched for {prod.Entity}. \n Result count:{searchResult.Results.Count}");
                
        //        foreach (SearchResult temp in searchResult.Results)
        //        {
        //            ProductDocument prodDoc = JsonConvert.DeserializeObject<ProductDocument>((string)temp.Document["content"]);
        //            products.Add(prodDoc);
        //            //await context.PostAsync($" did u want this param {prodDoc.MoleculeName} ");
        //        }
        //        context.Wait(MessageReceivedAsync);
        //        await context.PostAsync($" do you wana to c them? {products.Count}");
        //    }
        //    else await context.PostAsync($" search for {prod.Entity} failed/returned no results");
        //}

        //private async Task SearchQuery(IDialogContext context, string query, ISearchIndexClient searchClient)
        //{
        //    DocumentSearchResult searchResult = searchClient.Documents.Search(query);
        //    if (searchResult != null)
        //    {
        //        await context.PostAsync($"Search for {query}. \n Result count:{searchResult.Results.Count}");
                
        //        // TODO - should proceed or not?
        //        foreach (SearchResult temp in searchResult.Results)
        //        {
        //            ProductDocument prodDoc = JsonConvert.DeserializeObject<ProductDocument>((string)temp.Document["content"]);
        //            products.Add(prodDoc);

        //        }
        //        await context.PostAsync($" do you wana to c them? {products.Count}");
        //    }
        //    else await context.PostAsync($" search for {query} failed/returned no results");
        //}

        //private string extractFromDict(Document document)
        //{
        //    string result="";
        //    foreach (KeyValuePair<string, object> kvp in document)
        //    {
        //        result = string.Concat( kvp.Key, "::", kvp.Value, "\n",result);
        //    }

        //    return result;
        //}

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


        // copied from carosel git
       

        
    }
}