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

using System.Threading;
using Newtonsoft.Json;


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
            var message = context.MakeMessage();
            message.Attachments.Add(AttachmentsUtil.GetSpellSuggestCard(result.Query,altered));
            await context.PostAsync(message);
        }

        

      

        [LuisIntent("Greeting")]
        public async Task GreetingInten(IDialogContext context, LuisResult result)
        {
            var message = context.MakeMessage();
            message.Text = Utilities.GetSentence("0.1");
            await context.PostAsync(message);
        }


        [LuisIntent("CRM.SendCatalog")]
        public async Task SendCatalogIntent(IDialogContext context, LuisResult result)
        {
            Action = Lead.PDF;
            Lead alead;
            //DetailsDialog dialog = new DetailsDialog();
            LeadDialog diag = new LeadDialog();
            diag.LeadType = AttachmentsUtil.MINIMAL;

            if (context.PrivateConversationData.TryGetValue("bot-lead", out alead))
            {
                alead.SetMessageType(Lead.PDF);
                alead.SetSubject("A lead is interested in the catalog pdf.");
                await Utilities.AddMessageToQueueAsync(alead.ToMessage(),Utilities.TRANSIENT_Q);
                var message = context.MakeMessage();
                message.Text = string.Format(Utilities.GetSentence("19.80"),alead.Email);
                await context.PostAsync(message);
            }

            else context.Call(diag, this.ResumeAfterLeadForm);

           
        }

        [LuisIntent("Catalog.GetCategory")]
        public async Task CatalogGetCategoryIntent(IDialogContext context, LuisResult result)
        {
            var message = context.MakeMessage();
            if (tproducts!=null && tproducts[0]!=null)
            {
                if ("Packaging PIC".Equals(result.Query))
                {                    
                    message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    message.Attachments = tproducts[0].GetProductPicCarousel();
                }else  message.Attachments.Add(tproducts[0].GetProductCat(result.Query));
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
                    //DetailsDialog dialog = new DetailsDialog();
                    LeadDialog diag = new LeadDialog();
                    diag.LeadType = AttachmentsUtil.FULL;
                    if (context.PrivateConversationData.TryGetValue("bot-lead", out alead))
                    {
                        alead.SetMessageType(ProductDocument.FETCH_BY_MAIL);
                        alead.SetSubject(tproducts[0].MoleculeID);
                        alead.SetProduct(tproducts[0]);
                        // need to create a lead and send the search results
                        await Utilities.AddMessageToQueueAsync(alead.ToMessage(),Utilities.TRANSIENT_Q);
                        await Utilities.AddMessageToQueueAsync(alead.ToMessage(), Utilities.PERSIST_Q);
                        await context.PostAsync(string.Format(Utilities.GetSentence("22"), alead.Email));
                        //await context.PostAsync($"A request was sent to our communication auto-broker to the address:{alead.Email} provided.");
                    }
                    else
                    {
                        Action = Lead.SEARCH;
                        context.Call(diag, this.ResumeAfterLeadForm);
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
            // this intent, remove any and all conversation left overs. similar to closing the browser.
            var message = context.MakeMessage();
            message.Attachments.Add(AttachmentsUtil.GetConversationEndCard(result.Query));
            await context.PostAsync(message);            
            
        }

        [LuisIntent("Conversation.Terminate")]
        public async Task ConversationTerminateIntent(IDialogContext context, LuisResult result)
        {
            // this intent, remove any and all conversation left overs. similar to closing the browser.
            context.EndConversation(ActivityTypes.EndOfConversation);
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {

            //LeadDialog diag = new LeadDialog();
            //context.Call(diag, ResumeAfterLeadForm);

            await context.PostAsync(Utilities.GetSentence("911.0"));

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

                EntityRecommendation inst = entities[0];
                // send the result to the persist queue
                string surveyMessage = $"Answer:{inst.Entity}, time stamp{DateTime.Now}";
               // await context.PostAsync(surveyMessage);
                
                await Utilities.AddMessageToQueueAsync(surveyMessage,Utilities.PERSIST_Q);

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
            //DetailsDialog dialog = new DetailsDialog();
            Action = Lead.LEADCREATE;
            if (context.PrivateConversationData.TryGetValue("bot-lead", out alead))
            {
                MyLead = alead;
            }else
            {
                LeadDialog diag = new LeadDialog();
                diag.LeadType = AttachmentsUtil.FULL;
                context.Call(diag, ResumeAfterLeadForm);
            }
            //if (MyLead==null || !MyLead.IsLead()) MyLead = new Lead();
            ////setting the action to lead creation
            //dialog.SetLead(MyLead);
            //context.Call(dialog, this.ResumeAfterForm);

        }

        [LuisIntent("CRM.SubmitLead")]
        // TODO
        // 1. check what is the action, based on the action, determine what info is required to continue
        // 2. in the case it is only pdf, just email. the other, need full lead
        

        public async Task CRMSubmitLeadIntent(IDialogContext context, LuisResult result)
        {
            // check if lead exist
            Lead alead;
            if (context.PrivateConversationData.TryGetValue("bot-lead", out alead))
            {
                MyLead = alead;
            }

            MyLead.SetMessageType(Action);
            string dispName = (!string.IsNullOrEmpty(alead.Name)) ? alead.Name : alead.Email;
            IList<EntityRecommendation> entities = result.Entities;
            if (entities != null && entities.Count > 0)
            {

                EntityRecommendation inst = entities[0];
                switch (inst.Entity)
                {
                    case "confirm-lead-send-catalog":
                        MyLead.SetSubject("A contact with these details expressed interest");
                        await Utilities.AddMessageToQueueAsync(MyLead.ToMessage(), Utilities.TRANSIENT_Q);
                        break;
                    case "confirm-lead-creation":
                        if (tproducts != null && tproducts[0] != null) MyLead.SetProduct(tproducts[0]);
                        await Utilities.AddMessageToQueueAsync(MyLead.ToMessage(), Utilities.PERSIST_Q);
                        break;
                }
            }      
                           
            // Infor the lead process ended
            await context.PostAsync(string.Format(Utilities.GetSentence("22"), MyLead.Email));
            // post a nice end message with an option to provide feedback (and share - not functional)
            var message = context.MakeMessage();
            message.Attachments.Add(AttachmentsUtil.GetEndCard(dispName));
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

        

        //private void SetSubject(IList<ProductDocument> tproducts)
        //{
        //    string result = "";
        //    if (tproducts != null)
        //    {
        //        foreach (ProductDocument prd in tproducts)
        //        {
        //            string.Concat(result, ",", prd.TapiProductName);
        //        }
        //        MyLead.SetSubject( result);
        //    }
        //}


        //private async Task ResumeAfterForm(IDialogContext context, IAwaitable<Lead> result)
        //{
        //    // remove the verbiage
        //    MyLead = await result;
        //    if(MyLead!=null)
        //    {
        //        MyLead.SetAction(Action);
        //        var message = context.MakeMessage();
        //        message.Attachments.Add(MyLead.GetLeadCard(tproducts));
        //        await context.PostAsync(message);
        //    } //else await context.PostAsync(" Lead process ended without a lead");

        //}

        private async Task ResumeAfterLeadForm(IDialogContext context, IAwaitable<object> result)
        {
            var tempMess = await result;
            // just for test - would remove >>
            MyLead = JsonConvert.DeserializeObject<Lead>(tempMess.ToString());
            if (MyLead != null)
            {
                MyLead.SetAction(Action);
                var message = context.MakeMessage();
                message.Attachments.Add(MyLead.GetLeadCard(tproducts));
                await context.PostAsync(message);
            }
            //await context.PostAsync($"back to root from lead dialog: {lead.Name}");
            // <<
            //context.Wait(MessageReceived);
        }

        //private async Task ResumeAfterSend(IDialogContext context, IAwaitable<object> result)
        //{
        //    object obj = await result;
        //    MyLead.SetAction(Action);
        //    // echo the current lead details - it will direct to the submit lead intent, in case he clicks on 'Confirm'
        //    var message = context.MakeMessage();
        //    message.Attachments.Add(MyLead.GetLeadCard(tproducts));
        //    await context.PostAsync(message);
        //}

        
        

    }
}