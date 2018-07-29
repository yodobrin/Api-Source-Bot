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
		
       

        Lead MyLead; 
        public IList<ProductDocument> tproducts;
        string Action = Lead.SEARCH;

        public RootDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
            tproducts = new List<ProductDocument>();
           
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
              dialog.SetLead(alead);
                await context.PostAsync($"A lead is on the private data{alead.Name}");
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
                MyLead = alead;
                MyLead.SetAction(Action);
                await context.PostAsync($"A lead is on the private data{alead.Name}");
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
            Action = Lead.SEARCH;
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


        //private Attachment GetEndCard()
        //{
        //    var endCard = new HeroCard
        //    {
        //        Title = $"{MyLead.Name} Thank you!",
        //        //Subtitle = "TAPI's Source bots — Welcome tapi your api partner",
        //        Text = "Care to provide us with feedback?",
        //        Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/about-us-new.jpg") },
        //        Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Sure", value: "survey"),
        //            new CardAction(ActionTypes.PostBack, "No", value: "bye"),
        //            new CardAction(ActionTypes.PostBack, "Share in IN", value: "bye") }
        //    };

        //    return endCard.ToAttachment();
        //}

        //private Attachment GetOpenCard(string name, string company)
        //{
        //    var openCard = new HeroCard
        //    {
        //        Title = $"API Source Bot tailored for {name} @ {company}",
        //        Subtitle = "Tapi bots — Welcome tapi your api partner",
        //        Text = "Active Pharmaceutical Ingredients (API) Production and Manufacturing - information and knowledge by TAPI's experts.\n It's all here. You can search by typing sentences as below:!",
        //        Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/about-us-new.jpg") },
        //        Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Find me Aripiprazole", value: "find me Aripiprazole"), new CardAction(ActionTypes.PostBack, "Find me Aztreonam", value: "find me Aztreonam") }
        //    };

        //    return openCard.ToAttachment();
        //}

        //private Attachment GetErrorCard(string code)
        //{
        //    var openCard = new HeroCard
        //    {
        //        Title = Utilities.GetSentence("950"),
        //        Subtitle = string.Format(Utilities.GetSentence("951"),code),
        //        Text = Utilities.GetSentence("952"),
        //        Images = new List<CardImage> { new CardImage("https://cdn.dribbble.com/users/7770/screenshots/3935947/oh_snap_404_1x.jpg") }
        //        //Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Find me Aripiprazole", value: "find me Aripiprazole"), new CardAction(ActionTypes.PostBack, "Find me Aztreonam", value: "find me Aztreonam") }
        //    };

        //    return openCard.ToAttachment();
        //}





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

        //private static Attachment GetNoResults(string query)
        //{
        //    var productCard = new ThumbnailCard
        //    {
        //        Title = Utilities.GetSentence("5"),
        //        Subtitle = string.Format(Utilities.GetSentence("5.1"), query),
        //        Text = Utilities.GetSentence("5.2"),
        //        Images = new List<CardImage> { new CardImage("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQHEl-j7JobwiGjkbpCBVemqrUKp9EQFtPQOyOLXIBsAvycS8Kx") },
        //        Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, Utilities.GetSentence("6"), value: Lead.CONTACT_TAPI),
        //        new CardAction(ActionTypes.PostBack, Utilities.GetSentence("7"), value: Lead.PDF),
        //        new CardAction(ActionTypes.PostBack, Utilities.GetSentence("8"), value: Lead.UPDATE_ONCE_EXIST)
        //        }
        //    };

        //    return productCard.ToAttachment();

        //}

        //private static Attachment GetResultCard(IList<ProductDocument> tproducts)
        //{
        //    string suffix = "";
        //    int count = 0;
        //    if (tproducts.Count == 1) return tproducts[0].GetProductCard(ProductDocument.CONFIRM);
        //    List<CardAction> buttons = new List<CardAction>();
        //    if (tproducts.Count > 0)
        //    {
        //        suffix = (tproducts.Count == 1) ? "" : "s";
                
        //        foreach (ProductDocument prd in tproducts)
        //        {
        //            if (count == ProductDocument.MAX_PROD_IN_RESULT) break;
        //            buttons.Add(new CardAction(ActionTypes.PostBack, $"{prd.MoleculeName}", value: $"find me {prd.MoleculeID}"));
        //            count++;
        //        }
        //    }
        //    var resultCard = new HeroCard
        //    {
        //        Title = $"I found: {tproducts.Count} product{suffix}.",
        //        Subtitle = Utilities.GetSentence("16"),
        //        Text = Utilities.GetSentence("16.1"),
        //        Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/hp-banner_0000_inspections.jpg") },
        //        Buttons = buttons
        //    };

        //    return resultCard.ToAttachment();
        //}

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


            //await context.PostAsync($"Hi { MyLead.Name}! And thank you for using APISourceBot !");
            // echo the current lead details - it will direct to the submit lead intent, in case he clicks on 'Confirm'
            var message = context.MakeMessage();
            message.Attachments.Add(MyLead.GetLeadCard(tproducts));
            await context.PostAsync(message);
        }

        //private Attachment GetLeadCard9()
        //{
        //    if (tproducts != null && tproducts.Count > 0 && MyLead!=null) SetSubject(tproducts);

        //    MyLead = (MyLead != null) ? MyLead : new Lead("dum");
        //    var leadCard = new ThumbnailCard
        //    {
        //        Title = $"Hello {MyLead.Name} @ {MyLead.Company}",
        //        Subtitle = "This is what I know so far about as a lead...",
        //        Text = $"Your Email: {MyLead.Email}\n You were searching for {MyLead.GetSubject()}",
        //        Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/hp-banner_0001_wearetapi.jpg") },
        //        Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Confirm", value: "confirm-lead-creation"), new CardAction(ActionTypes.PostBack, "Revisit Details", value: "i am a dealer") }
        //    };

        //    return leadCard.ToAttachment();
        //}

        

    }
}