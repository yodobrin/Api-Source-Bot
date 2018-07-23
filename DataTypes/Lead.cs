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
        public const string PDF = "send-catalog";
        public const string SEARCH = "search";
        public const string LEADCREATE = "leadcreate";
        public const string UPDATE_ONCE_EXIST = "update-me-once-api";
        public const string CONTACT_TAPI = "contact-tapi";

        public const string NO_SUCH_FIELD = "No Such Field";
        public const string FIELD_NOT_SET = "Field not set";

        public const int ALL = 0;
        public const int UNFILLED = 1;

        readonly string[] Fields = { "Email", "Fisrt Name", "Last Name" };
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

        public Dictionary<string, LineItem> GetValues(int option)
        {
            if(properties == null)
            {
                InitProperties();
            }
            switch (option)
            {
                case ALL:
                    return properties;

                case UNFILLED:
                    return FilterUnFilled();

                default: return properties;
            }
        }

        private Dictionary<string, LineItem> FilterUnFilled()
        {
            Dictionary<string, LineItem> tmp = new Dictionary<string, LineItem>();
            foreach (LineItem itm in properties.Values)
            {
                if (!itm.IsFill()) tmp.Add(itm.Type, itm);
            }
            return tmp;
        }

        public string flush()
        {
            string result = "{ ";
            string pat = "\"{0}\":\"{1}\"";
            foreach (LineItem itm in properties.Values)
            {
                result += string.Format(pat, itm.Type, itm.Value);
            }
            return result+" }";
        }


        [Pattern(RegexConstants.Email)]
        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Company")]
        public string Company { get; set; }

        
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("First Name")]
        public string FirstName { get; set; }

        [JsonProperty("Last Name")]
        public string LastName { get; set; }

        // will be taken from an app setting        
        //[JsonProperty("Lead Source")]
        //public string LeadSource { get; set; }

        [JsonProperty("Subject")]
        public string Subject { get; set; }

        [JsonProperty("Comments")]
        public string Comments { get; set; }

        
        //[JsonProperty("Creation Time")]
        //public string CreationTime { get; set; }

        public string ToMessage()
        {
            return "wtf";
           // return JsonConvert.SerializeObject(this);
        }

        public bool IsLead()
        {
            if (this.Name != null && this.Name != "N/A" && this.Email != null && this.Email.Length > 3 && this.Subject != null && this.Subject.Length > 4) return true;
            else return false;
        }

        public Lead(string dummy)
        {
            Email = "dummy@mail.com";
            FirstName = "Dummi";
            LastName = "DoDo";
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
            
            var leadCard = new ThumbnailCard
            {
                Title = $"Hello {FirstName} {LastName} @ {Company}",
                Subtitle = "This is what I know so far about as a lead...",
                Text = message,
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/hp-banner_0001_wearetapi.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Confirm", value: $"{Action}"), new CardAction(ActionTypes.PostBack, "Revisit Details", value: "i am a dealer") }
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
                    string.Concat(result, ",", prd.MoleculeName);
                }
                Subject = result;
            }
        }
    }
}