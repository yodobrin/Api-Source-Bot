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
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using System.Collections.Generic;
using SourceBot.Utils;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using LuisBot.DataTypes;

using Microsoft.Bot.Builder.Luis.Models;

namespace LuisBot.Dialogs
{
    [Serializable]
    public class SearchDialog : IDialog<object>
    {
        public IList<ProductDocument> products;
        LuisResult LuisResult;

        public SearchDialog(LuisResult luisResult)
        {
            products = new List<ProductDocument>();
            LuisResult = luisResult;
        }
            
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            int count = 0;
            IList<EntityRecommendation> entities = LuisResult.Entities;
            ISearchIndexClient searchClient = Utilities.GetSearchClient();
            // loop over the entities find the "Product" entity
            // todo need to ensure the number of results is accounted for BEFORE displayed
            if (entities != null && entities.Count > 0)
            {
                foreach (EntityRecommendation inst in entities)
                {
                    if (Utilities.PRODUCT.Equals(inst.Type))
                    {
                         count = SearchProduct(context, inst, searchClient);
                    }
                    else continue;
                }
            }
            // in case it is a find intent, but not recognized as a product

            else count = SearchQuery(context, LuisResult.Query, searchClient);
            //context.Wait(MessageReceivedAsync);

            await context.PostAsync($"Your search resulted in: {count} results.");

            context.Done(products);
        }

        private int SearchProduct(IDialogContext context, EntityRecommendation prod, ISearchIndexClient searchClient)
        {
            DocumentSearchResult searchResult = searchClient.Documents.Search(prod.Entity);
            if (searchResult != null)
            {
                //await context.PostAsync($"Searched for {prod.Entity}. \n Result count:{searchResult.Results.Count}");

                foreach (SearchResult temp in searchResult.Results)
                {
                    ProductDocument prodDoc = JsonConvert.DeserializeObject<ProductDocument>((string)temp.Document["content"]);
                    products.Add(prodDoc);
                    //await context.PostAsync($" did u want this param {prodDoc.MoleculeName} ");
                }
                //context.Wait(MessageReceivedAsync);
                //await context.PostAsync($" do you wana to c them? {products.Count}");
                return products.Count;
            }
            else return 0;// await context.PostAsync($" search for {prod.Entity} failed/returned no results");
        }

        private int SearchQuery(IDialogContext context, string query, ISearchIndexClient searchClient)
        {
            DocumentSearchResult searchResult = searchClient.Documents.Search(query);
            if (searchResult != null)
            {
                //await context.PostAsync($"Search for {query}. \n Result count:{searchResult.Results.Count}");

                // TODO - should proceed or not?
                foreach (SearchResult temp in searchResult.Results)
                {
                    ProductDocument prodDoc = JsonConvert.DeserializeObject<ProductDocument>((string)temp.Document["content"]);
                    products.Add(prodDoc);

                }
                return products.Count;
                //await context.PostAsync($" do you wana to c them? {products.Count}");
            }
            else return 0; // await context.PostAsync($" search for {query} failed/returned no results");
        }
    }
}