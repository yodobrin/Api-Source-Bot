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
    
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {


        private readonly BingSpellService spellService = new BingSpellService();
        Lead MyLead; 
        public IList<ProductDocument> tproducts;
        string Action = Lead.SEARCH;

        public RootDialog() : base(new LuisService(GetLuisModelAttribute()))
        {
            tproducts = new List<ProductDocument>();
           
        }

        //public RootDialog() : base(new LuisService(new LuisModelAttribute(
        //   ConfigurationManager.AppSettings["LuisAppId"],
        //   ConfigurationManager.AppSettings["LuisAPIKey"],
        //   domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        //{
        //    tproducts = new List<ProductDocument>();

        //}

        private static LuisModelAttribute GetLuisModelAttribute()
        {
            var attribute = new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"]);
            attribute.BingSpellCheckSubscriptionKey = ConfigurationManager.AppSettings["BingSpellcheckKey"];
            return attribute;
        }

        /**
         * Intents Section
         *
         */

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            Action = Lead.SEARCH;
            string altered = await this.spellService.GetCorrectedTextAsync(result.Query);
            await context.PostAsync($"query:{result.Query} --- alterquery {altered}");
            await context.Forward(new SearchDialog(result.Entities, result.Query), this.ResumeAfterSearchDialog, context.Activity, CancellationToken.None);
            // await this.ShowLuisResult(context, result);
        }

       // TODO
       // Survey - test (+ feedback in case it was good)
       // Share - potential
       // message about other searches


        [LuisIntent("Greeting")]
        public async Task GreetingInten(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(Utilities.GetSentence("1"));
            Action = Lead.LEADCREATE;
            Lead alead;
            DetailsDialog dialog = new DetailsDialog();
            
            if ( context.PrivateConversationData.TryGetValue("bot-lead", out alead) )
            {
              //dialog.SetLead(alead);
                MyLead = alead;
                //await context.PostAsync($"A lead is on the private data{alead.Name}");
            }
            
            else context.Call(dialog, this.ResumeAfterForm);           
        }


        [LuisIntent("CRM.SendCatalog")]
        public async Task SendCatalogIntent(IDialogContext context, LuisResult result)
        {
            Action = Lead.PDF;
            Lead alead;
            DetailsDialog dialog = new DetailsDialog();

            if (context.PrivateConversationData.TryGetValue("bot-lead", out alead))
            {
                alead.SetMessageType(Lead.PDF);
                alead.SetSubject("A lead is interested in the catalog pdf.");
                await Utilities.AddMessageToQueueAsync(alead.ToMessage());
                await context.PostAsync($"A request was sent to our communication auto-broker to the address:{alead.Email} provided.");
                //MyLead = alead;
                //MyLead.SetAction(Action);
                //// need to add a message to a queue
                //await context.PostAsync($"A lead is on the private data{alead.Name}");
            }

            else context.Call(dialog, this.ResumeAfterForm);

           
        }

        [LuisIntent("Catalog.GetCategory")]
        public async Task CatalogGetCategoryIntent(IDialogContext context, LuisResult result)
        {
            var message = context.MakeMessage();
            if (tproducts!=null && tproducts[0]!=null)
            {
                message.Attachments.Add(tproducts[0].GetProductCat(result.Query));
            }else message.Attachments.Add(AttachmentsUtil.GetErrorCard("No products in search results"));


            await context.PostAsync(message);
        }

        [LuisIntent("Catalog.Fetch")]
        public async Task CatalogFetchIntent(IDialogContext context, LuisResult result)
        {
            switch (result.Query)
            {
                case ProductDocument.SHOW_ME_MORE:
                    var message = context.MakeMessage();
                    message.Attachments.Add(tproducts[0].GetProductCard(ProductDocument.FULL));
                    await context.PostAsync(message);
                    break;
                case ProductDocument.FLUSH:
                    await FlushProducts(context);
                    break;
                case ProductDocument.FETCH_BY_MAIL:
                    Lead alead;
                    DetailsDialog dialog = new DetailsDialog();
                    if (context.PrivateConversationData.TryGetValue("bot-lead", out alead))
                    {
                        alead.SetMessageType(ProductDocument.FETCH_BY_MAIL);
                        alead.SetSubject(tproducts[0].MoleculeID);
                        alead.SetProduct(tproducts[0]);
                        
                        await Utilities.AddMessageToQueueAsync(alead.ToMessage());
                        await context.PostAsync($"A request was sent to our communication auto-broker to the address:{alead.Email} provided.");
                    }
                    else
                    {
                        Action = Lead.SEARCH;
                        context.Call(dialog, this.ResumeAfterForm);
                    }
                    break;
                case ProductDocument.HIGHLIGHT:
                    var message1 = context.MakeMessage();
                    message1.Attachments.Add(tproducts[0].GetProductCard(ProductDocument.HIGHLIGHT));
                    await context.PostAsync(message1);
                    break;

                default: break;                 
            }
            

        }

        
        [LuisIntent("Conversation.End")]
        public async Task ConversationEndIntent(IDialogContext context, LuisResult result)
        {
            //context.Reset();
            context.EndConversation(ActivityTypes.EndOfConversation);
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
            await context.PostAsync(Utilities.GetSentence("911.0"));
            //await this.ShowLuisResult(context, result);
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
            // setting the action to search
            Action = ProductDocument.FETCH_BY_MAIL;// Lead.SEARCH;
            await context.Forward(new SearchDialog(result.Entities, result.Query), this.ResumeAfterSearchDialog, context.Activity, CancellationToken.None);

        }

        [LuisIntent("CRM.Survey")]
        public async Task CRMSurveyIntent(IDialogContext context, LuisResult result)
        {
            // need to check the entities, if exist act according to them (send a message to a q)
            var message = context.MakeMessage();
            string locName = (MyLead != null && MyLead.Name != null) ? MyLead.Name : "Guest";
            IList<EntityRecommendation> entities = result.Entities;
            if (entities != null && entities.Count > 0)
            {
                // ONLY take the first entity
                // TODO - why is the not at all, and not sat are not showing any message
                EntityRecommendation inst = entities[0];
                //await context.PostAsync($"the entity is {inst.Entity}");
                switch (inst.Entity)
                {
                    case SurveyAnswer.NOT_AT_SAT:
                        await context.PostAsync(Utilities.GetSentence("19.30"));
                        break;
                    case SurveyAnswer.NOT_SAT:
                        await context.PostAsync(Utilities.GetSentence("19.30"));
                        break;
                    case SurveyAnswer.SAT:
                        await context.PostAsync(Utilities.GetSentence("19.31"));
                        break;
                    case SurveyAnswer.VER_SAT:
                        //show somehting nice
                        message.Attachments.Add(SurveyAnswer.GetHoreyCard(locName));
                        await context.PostAsync(message);
                        break;
                    case SurveyAnswer.EXT_SAT:
                        //show somehting nice
                        message.Attachments.Add(SurveyAnswer.GetHoreyCard(locName));
                        await context.PostAsync(message);
                        break;
                    default: break;
                }
            }else
            {
                
                message.Attachments.Add(SurveyAnswer.GetSurveyCard(locName));
                await context.PostAsync(message);

            }


        }


        [LuisIntent("CRM.Lead")]
        public async Task CRMLeadIntent(IDialogContext context, LuisResult result)
        {
            Lead alead;
            DetailsDialog dialog = new DetailsDialog();
            Action = Lead.LEADCREATE;
            if (context.PrivateConversationData.TryGetValue("bot-lead", out alead))
            {
                MyLead = alead;
            }
            if (MyLead==null || !MyLead.IsLead()) MyLead = new Lead();
            //setting the action to lead creation
            dialog.SetLead(MyLead);
            context.Call(dialog, this.ResumeAfterForm);

        }

        [LuisIntent("CRM.SubmitLead")]
        public async Task CRMSubmitLeadIntent(IDialogContext context, LuisResult result)
        {

            MyLead.SetMessageType(Action);
            //MyLead.SetSubject("A contact with these details expressed interest");
            MyLead.SetProduct(tproducts[0]);
            await Utilities.AddMessageToQueueAsync(MyLead.ToMessage());               
            // Infor the lead process ended
            await context.PostAsync(string.Format(Utilities.GetSentence("22"), MyLead.Email));
            // post a nice end message with an option to provide feedback (and share - not functional)
            var message = context.MakeMessage();
            message.Attachments.Add(AttachmentsUtil.GetEndCard(MyLead.Name));
            await context.PostAsync(message);

        }


        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Wait(MessageReceived);
        }


        /**
        * Spits out the products found
        */
        private async Task FlushProducts(IDialogContext context)
        {
            foreach (ProductDocument prd in tproducts)
            {
                await context.PostAsync($"I got {prd.MoleculeID} -- {prd.InnovatorMarketer} -- {prd.TapiProductName} ");
            }
        }

        

        /*
         * Resume After section, all the methods are called once another dialog is done
         * 
         */

        private async Task ResumeAfterSearchDialog(IDialogContext context, IAwaitable<object> result)
        {
            tproducts = (IList<ProductDocument>)await result;
            var message = context.MakeMessage();
            if (tproducts != null && tproducts.Count > 0)
            {
                // await context.PostAsync($"after search {tproducts.Count}");
                // SetSubject(tproducts);

                message.Attachments.Add(AttachmentsUtil.GetResultCard(tproducts));
                await context.PostAsync(message);
            }
            else
            {
                // no results
                
                string output;
                context.ConversationData.TryGetValue(ProductDocument.USER_QUERY, out output);
                message.Attachments.Add(AttachmentsUtil.GetNoResults(output));
                await context.PostAsync(message);
               
            }
           
        }

        

        private void SetSubject(IList<ProductDocument> tproducts)
        {
            string result = "";
            if (tproducts != null)
            {
                foreach (ProductDocument prd in tproducts)
                {
                    string.Concat(result, ",", prd.TapiProductName);
                }
                MyLead.SetSubject( result);
            }
        }


        private async Task ResumeAfterForm(IDialogContext context, IAwaitable<Lead> result)
        {
            MyLead = await result;
            if(MyLead!=null)
            {
                MyLead.SetAction(Action);
                var message = context.MakeMessage();
                message.Attachments.Add(MyLead.GetLeadCard(tproducts));
                await context.PostAsync(message);
            } else await context.PostAsync(" Lead process ended without a lead");

        }

        private async Task ResumeAfterSend(IDialogContext context, IAwaitable<object> result)
        {
            object obj = await result;
            MyLead.SetAction(Action);
            // echo the current lead details - it will direct to the submit lead intent, in case he clicks on 'Confirm'
            var message = context.MakeMessage();
            message.Attachments.Add(MyLead.GetLeadCard(tproducts));
            await context.PostAsync(message);
        }

        
        

    }
}