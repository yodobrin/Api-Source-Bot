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
using Microsoft.Bot.Builder.FormFlow;
using SourceBot.Utils;

using System.Collections.Generic;


using Microsoft.Bot.Connector;

namespace SourceBot.DataTypes
{
    [Serializable]
    public class Lead
    {
        public const string PDF = "lead-send-catalog";
        public const string SEND_PDF = "send-catalog";
        public const string SEARCH = "lead-search";
        public const string LEADCREATE = "lead-create";
        public const string UPDATE_ONCE_EXIST = "update-me-once-api";
        public const string CONTACT_TAPI = "contact-tapi";

        public const string NO_SUCH_FIELD = "No Such Field";
        public const string FIELD_NOT_SET = "Field not set";

        public const int ALL = 0;
        public const int UNFILLED = 1;

        public readonly string[] Fields = { "Email", "Name", "PhoneNumber","Country","Company","Comments" };
        public Dictionary<string, LineItem> properties;

        //[JsonProperty("MessageType")]        
        //public string MessageType { get; set; }
        // defaulting to search action
        private string Action = SEARCH;
        public void SetAction(string action)
        {
            Action = action;
        }


        public Lead(int dum)
        {
            InitProperties();
        }

        private void InitProperties()
        {
            properties = new Dictionary<string, LineItem>();
            foreach (string line in Fields)
            {
                LineItem itm = new LineItem();
                itm.Type = line;
                properties.Add(line, itm);

            }

        }

        public string GetValueBYType(string type)
        {
            LineItem itm;
            if (properties.ContainsKey(type))
            {
                properties.TryGetValue(type, out itm);
                return (itm.IsFill()) ? itm.Value : FIELD_NOT_SET;
            }
            else return NO_SUCH_FIELD;
        }

      

        //[Prompt("Please enter your full name")]
        [JsonProperty("Name")]
        public string Name { get; set; }

        //[Prompt("What is your last name?")]
        //[JsonProperty("Last Name")]
        //public string LastName { get; set; }

        //[Prompt("Please enter your email address")]
        //[Pattern(RegexConstants.Email)]
        [JsonProperty("Email")]
        public string Email { get; set; }

        // TODO must select from a list of countries
        //[Prompt("Great, in order to provide the most relavant information please provide your target market")]
        //[Pattern(RegexConstants.Email)]
        [JsonProperty("Country")]
        public string Country { get; set; }

        //[Prompt("Excelent, please provide your company name, so we can forward your request to the relavant person, in case you already a TAPI customer")]
        [JsonProperty("Company")]
        public string Company { get; set; }

        // add a validation on the format and contact
        //[Prompt("Thanks, please provide your phone number ")]
        //[Pattern(RegexConstants.Phone)]
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


        //[Prompt("Any Comments? Information you provide will help us to provide information in the most accurate way, for example which documents are required? What is the quanitity of the API you need? Do you need price quatation? etc.")]
        [JsonProperty("Comments")]
        public string Comments { get; set; }

        

        public string ToMessage()
        {
            //    return "wtf";
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

        public void SetProduct(ProductDocument product)
        {
            Product = product.ToMessage();
        }


        private void SetTimeStamp()
        {
            TimeStamp = DateTime.Now.ToLongDateString();
        }

        public string GetSubject()
        {
            return Subject;
        }
        public bool IsLead()
        {
            if (this.Name != null && this.Name != "N/A" && this.Email != null && this.Email.Length > 3 && this.Subject != null && this.Subject.Length > 4) return true;
            else return false;
        }

        public Lead(string dummy)
        {
            Email = "dummy@mail.com";
            Name = "Dummi";
            //LastName = "DoDo";
            Company = "essence";
            Subject = "search dummy subject";
            Comments = "just dummy comments";
        }

        public Lead() { }


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
                //Subtitle = "This is what I know so far about as a lead...",
                Text = message,
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/hp-banner_0001_wearetapi.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Confirm", value: $"confirm-{Action}"), new CardAction(ActionTypes.PostBack, "Revisit my details", value: ProductDocument.FETCH_BY_MAIL) }
            };

            return leadCard.ToAttachment();
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
                Subject = result;
            }
        }
    }
}