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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SourceBot.Utils;




using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace SourceBot.DataTypes
{
    [Serializable]
    public class ProductDocument
    {
        public const string FULL = "full-product";
        public const string HIGHLIGHT = "product-highlight";
        public const string NO_SUCH_CAT = "Missing information for ";

        public const string FETCH_BY_MAIL = "bymail";
        public const string SHOW_ME_MORE = "detail-product";
        public const string FLUSH = "flush";

        public const int MAX_PROD_IN_RESULT = 5;

        private Dictionary<string, string> Data;


        [JsonProperty("Molecule (Level 1) ID")]
        public string MoleculeID { get; set; }
        [JsonProperty("Molecule Name (Level 1)")]
        public string MoleculeName { get; set; }
        [JsonProperty("Tapi Product Name (Level 2)")]
        public string TapiProductName { get; set; }
        [JsonProperty("Status (Calculated)")]
        public string Status { get; set; }
        [JsonProperty("Sub Status (Calculated)")]
        public string SubStatus { get; set; }
        [JsonProperty("ATC 1")]
        public string ATC { get; set; }



        public string GetCategory(string cat)
        {
            string res;
            if (Data==null)
            {
                string json = JsonConvert.SerializeObject(this);
                Data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            if (Data.TryGetValue(cat, out res)) return res;
            else return NO_SUCH_CAT;
        }

        [JsonConstructor]
        public ProductDocument(string moleculeID, string moleculeName, string tapiProductName, string status, string subStatus, string aTC)
        {
            MoleculeID = moleculeID;
            MoleculeName = moleculeName;
            TapiProductName = tapiProductName;
            Status = status;
            SubStatus = subStatus;
            ATC = aTC;
        }

        public Attachment GetProductCard(string option)
        {
            
            switch (option)
            {
                case FULL:
                    return GetFull();
                case HIGHLIGHT:
                    return GetHighligh();
                    
                default: return GetHighligh();
            }
            
        }

        public Attachment GetProductCat(string category)
        {
            var productCard = new ThumbnailCard
            {
                Title = string.Format(Utilities.GetSentence("16.40"), category),
                Subtitle = Utilities.GetSentence("12.41"),
                Text = GetCategory(category),
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/1-png.png") } ,
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.4"), value: SHOW_ME_MORE) }
            };

            return productCard.ToAttachment();

        }

        private Attachment GetHighligh()
        {
            var productCard = new ThumbnailCard
            {
                Title = string.Format(Utilities.GetSentence("12"),MoleculeName ) ,
                Subtitle = Utilities.GetSentence("12.1"),
                Text = Utilities.GetSentence("12.2"),
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/hp-banner_0004_catalog.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.3"), value: FETCH_BY_MAIL), new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.4"), value: SHOW_ME_MORE) }
            };

            return productCard.ToAttachment();
        }

        private Attachment GetFull()
        {
            var productCard = new HeroCard
            {
                Title = Utilities.GetSentence("12.10") +$" : {MoleculeName} " ,
                Subtitle = Utilities.GetSentence("12.11"),
                Text = Utilities.GetSentence("12.12"),
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/safety-by-design-1.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.20"), value: Utilities.GetSentence("12.20")),
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.21"), value: Utilities.GetSentence("12.21")) ,
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.22"), value: Utilities.GetSentence("12.22")) ,
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.23"), value: Utilities.GetSentence("12.23")) ,
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.24"), value: Utilities.GetSentence("12.24")) ,
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.25"), value: Utilities.GetSentence("12.25")) ,
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.26"), value: Utilities.GetSentence("12.26")) ,
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.27"), value: Utilities.GetSentence("12.27")) ,
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.28"), value: Utilities.GetSentence("12.28")) }            
            };

            return productCard.ToAttachment();
        }
    }
}