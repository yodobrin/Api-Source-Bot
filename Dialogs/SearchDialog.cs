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
using Tapi.Bot.SophiBot.Utils;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using Tapi.Bot.SophiBot.DataTypes;

using Microsoft.Bot.Builder.Luis.Models;

namespace Tapi.Bot.SophiBot.Dialogs
{
    [Serializable]
    public class SearchDialog : IDialog<object>
    {
        public IList<ProductDocument> products;
        

        IList<EntityRecommendation> Entities;
        readonly string Query;

        public SearchDialog(IList<EntityRecommendation> entities,string query)
        {
            products = new List<ProductDocument>();
            Entities = entities;
            Query = query;
        }
            
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            int count = 0;                       
            ISearchIndexClient searchClient = Utilities.GetSearchClient();
            // loop over the entities find the "Product" entity
            if (Entities != null && Entities.Count > 0)
            {
                // do we need to loop, or will just taking the first one be good enough?
                foreach (EntityRecommendation inst in Entities)
                {
                    
                    if (Utilities.PRODUCT.Equals(inst.Type))
                    {                        
                        count = SearchProduct(context, inst, searchClient);
                        // break in case something was found
                        if (count!=0) break;
                    }
                    else if (Utilities.NONPRODUCT.Equals(inst.Type))
                    {
                        
                        count = SearchQuery(context, inst.Entity, searchClient);
                        // break in case something was found
                        if (count != 0) break;
                    }
                    else continue;
                }
            }
            // in case it is a find 'intent', but not recognized as a product
            else
            {                
                count = SearchQuery(context, Query, searchClient);
            }
          
            context.Done(products);
        }

        private int SearchProduct(IDialogContext context, EntityRecommendation prod, ISearchIndexClient searchClient)
        {
            SearchParameters sp = new SearchParameters() { SearchMode = SearchMode.All };
            DocumentSearchResult searchResult = searchClient.Documents.Search(prod.Entity,sp);
            if (searchResult != null)
            {
                foreach (SearchResult temp in searchResult.Results)
                {
                    ProductDocument prodDoc = JsonConvert.DeserializeObject<ProductDocument>((string)temp.Document["content"]);
                    products.Add(prodDoc);                    
                }
                string subject = Lead.GetSubject(products);
                context.ConversationData.SetValue(ProductDocument.USER_QUERY, subject);
                return products.Count;
            }
            else return 0;
        }

        private int SearchQuery(IDialogContext context, string query, ISearchIndexClient searchClient)
        {
            SearchParameters sp = new SearchParameters() { SearchMode = SearchMode.Any};
            DocumentSearchResult searchResult = searchClient.Documents.Search(query,sp);
            if (searchResult != null)
            {
                foreach (SearchResult temp in searchResult.Results)
                {
                    ProductDocument prodDoc = JsonConvert.DeserializeObject<ProductDocument>((string)temp.Document["content"]);
                    products.Add(prodDoc);

                }
                string subject = Lead.GetSubject(products);
                context.ConversationData.SetValue(ProductDocument.USER_QUERY, subject);
                return products.Count;
            }
            else return 0; 
        }
    }
}