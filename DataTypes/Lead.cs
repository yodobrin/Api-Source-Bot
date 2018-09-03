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
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Connector;

namespace Tapi.Bot.SophiBot.DataTypes
{
    [Serializable]
    public class Lead
    {
        public const string PDF = "lead-send-catalog";
        public const string SEND_PDF = "send-catalog";
        public const string REVISIT_DETAILS = "revisit-lead-details";
        public const string SEARCH = "lead-search";
        public const string LEADCREATE = "lead-create";
        public const string UPDATE_ONCE_EXIST = "update-me-once-api";
        public const string CONTACT_TAPI = "contact-tapi";

        public const string NO_SUCH_FIELD = "No Such Field";
        public const string FIELD_NOT_SET = "Field not set";

        public const int ALL = 0;
        public const int UNFILLED = 1;

       


        private string Action = SEARCH;
        public void SetAction(string action)
        {
            Action = action;
        }


          


        [JsonProperty("Name")]
        public string Name { get; set; }


        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }


        [JsonProperty("Company")]
        public string Company { get; set; }


        [JsonProperty("Phone")]
        public string Phone { get; set; }

       

        [JsonProperty("Subject")]
        private string Subject { get; set; }

        [JsonProperty("Message-Type")]
        private string MessageType { get; set; }

        [JsonProperty("Time-Stamp")]
        private string TimeStamp { get; set; }

        [JsonProperty("Product")]
        private string Product { get; set; }


        
        [JsonProperty("Comments")]
        public string Comments { get; set; }

        public Dictionary<string,LineItem>Validate()
        {
            Dictionary<string,LineItem> validation = new Dictionary<string, LineItem>();
            validation.Add("Name", new LineItem(LineItem.TEXT, Name));
            validation.Add("Email", new LineItem(LineItem.EMAIL, Email));
            //validation.Add("Phone", new LineItem(LineItem.TEXT, Phone));
            validation.Add("Company", new LineItem(LineItem.TEXT, Company));            
            return validation;
        }

        public bool IsValid(Dictionary<string, LineItem> validation)
        {                         
            foreach(KeyValuePair<string,LineItem> itm in validation)
            {
                if (!itm.Value.IsValid()) return false;
                else continue;
            }
            return true;
        }

        public string GetValidationResult(Dictionary<string, LineItem> validation)
        {
            string result = "";
            foreach (KeyValuePair<string, LineItem> itm in validation)
            {
                result += $"For the field:{itm.Key} you entered:{itm.Value.Value} and isValid? {itm.Value.IsValid()} \n";
            }
            return result;
        }
        

        public string ToMessage()
        {      
           SetTimeStamp();
            
           return JsonConvert.SerializeObject(this, Formatting.None);
        }

        public void SetSubject(string subject)
        {
            Subject = subject;
        }

        public void SetMessageType(string type)
        {
            MessageType = type;
        }

        //public void SetProduct(ProductDocument product)
        //{
        //    Product = product.ToMessage();
        //}

        public void SetProduct(string product)
        {
            Product = product;
        }

        

        private void SetTimeStamp()
        {
            TimeStamp = DateTime.Now.ToLongDateString();
        }

        public string GetSubject()
        {
            return Subject;
        }
       


        public Attachment GetLeadCard(IList<ProductDocument> tproducts)
        {
            if (tproducts != null && tproducts.Count > 0 ) SetSubject(tproducts);
            string message = "";
            if(Action!=null && Action.Length>0)
            {
                switch (Action)
                {
                    case PDF: message = $"I will send a copy of our product catalog to your email address:{Email}";
                        break;
                    case SEARCH: message = $"I will send the search results for {Subject} to your email address:{Email}";
                        break;
                    case LEADCREATE: message = $"I will submit a Lead creation request with the details provided, thank you!";
                        break;
                    default: break;
                }
            }
            string dispName = (!string.IsNullOrEmpty(Name)) ? Name : Email;
            string dispComp = (!string.IsNullOrEmpty(Company)) ? $"@ {Company}" : "";
            var leadCard = new ThumbnailCard
            {
                Title = $"Hello {dispName} {dispComp}",
                
                Text = message,
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/hp-banner_0001_wearetapi.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Confirm", value: $"confirm-{Action}"), new CardAction(ActionTypes.PostBack, "Revisit my details", value: REVISIT_DETAILS) }
            };

            return leadCard.ToAttachment();
        }

        public Attachment GetInvalidLeadCard()
        {
            var leadCard = new ThumbnailCard
            {
                Title = $"Invalid information Provided",

                Text = "Please revisit the details",
                Images = new List<CardImage> { new CardImage("https://safetymanagementgroup.com/wp-content/uploads/2017/07/Oopsbutton.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Revisit my details", value: REVISIT_DETAILS) }
            };

            return leadCard.ToAttachment();
        }

        private void SetSubject(IList<ProductDocument> tproducts)
        {
            Subject = GetSubject(tproducts);
        }

        public static string GetSubject(IList<ProductDocument> tproducts)
        {
            string result = "";
            if (tproducts != null)
            {
                foreach (ProductDocument prd in tproducts)
                {                    
                    if ("".Equals(result)) result = string.Concat(result, prd.TapiProductName);
                    else result = string.Concat(result, ", ", prd.TapiProductName);
                }
               
            }
            return result;
        }

    }
}