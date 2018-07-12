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

using System.Collections;
using System.Collections.Generic;
using SourceBot.Utils;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
		public static string PRODUCT = "product";
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
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
            
            IList<EntityRecommendation> entities = result.Entities;
            ISearchIndexClient searchClient = Utilities.GetSearchClient();

            DocumentSearchResult  searchResult;
			long resultsCount = 0;
			// loop over the entities find the "Product" entity
			if (entities != null && entities.Count>0)
			{
				foreach (EntityRecommendation inst in entities)
				{
                    if (PRODUCT.Equals(inst.Type))
                    {
                        await context.PostAsync($"in find item u said: {result.Query} ");
                        //searchResult = Utilities.Search(inst.Entity);
                        searchResult = searchClient.Documents.Search(inst.Entity);
                        if (searchResult != null)
                        {
                            await context.PostAsync($"after search for {inst.Entity}");
                            await context.PostAsync($"cnt =  {searchResult.Results.Count}");
                            foreach (SearchResult temp in searchResult.Results)
                            {
                                
                                await context.PostAsync($" did u want this {extractFromDict(temp.Document)} ");
                                await context.PostAsync($" did u want this {temp.Document["metadata_storage_path"]} ");
                                await context.PostAsync($" did u want this {temp.Document["content"]} ");
                                
                            }
                            resultsCount = (long)searchResult.Results.Count;
                        }
                        else await context.PostAsync($" search for {inst.Entity} failed/returned no results");

                    }
					else continue;
				}
			}
			await context.PostAsync($"The number of  results {resultsCount} ");
			//await this.ShowLuisExtendedt(context, result);
			context.Wait(MessageReceived);
		}

        private string extractFromDict(Document document)
        {
            string result="";
            foreach (KeyValuePair<string, object> kvp in document)
            {
                result = string.Concat( kvp.Key, "::", kvp.Value, "\n",result);
            }

            return result;
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