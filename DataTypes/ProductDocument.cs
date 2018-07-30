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
        public const string NO_SUCH_CAT = "Missing information for: {0}";
        public const string NO_RESULT = "no-results";

        public const string FETCH_BY_MAIL = "by-mail";
        public const string SHOW_ME_MORE = "detail-product";
        public const string FLUSH = "flush";
        public const string HELP = "help";
        public const string CONFIRM = "confirm";


        public const string USER_QUERY = "user-query";


        public const int MAX_PROD_IN_RESULT = 5;

        //private Dictionary<string, string> Data;

        //1
        [JsonProperty("Molecule (Level 1) ID")]
        public string MoleculeID { get; set; }
        //2
        [JsonProperty("Molecule + Salt IMS Name")]
        public string MoleculeSaltName { get; set; }
        //3
        [JsonProperty("Tapi Product Name (Level 2)")]
        public string TapiProductName { get; set; }
        //4
        [JsonProperty("Innovator/Marketer")]
        public string InnovatorMarketer { get; set; }
        //5
        [JsonProperty("CAS Number")]
        public string CASNumber { get; set; }
        //6
        [JsonProperty("Sub Status (Calculated)")]
        public string SubStatus { get; set; }
        //7
        [JsonProperty("DMF Availability")]
        public string DMFAvailability { get; set; }
        //8
        [JsonProperty("Dosage Form")]
        public string DosageForm { get; set; }
        //9
        [JsonProperty("Number of available samples")]
        public string NumOfAvailSamples { get; set; }
        //10
        [JsonProperty("Packaging PIC")]
        public string PackagingPIC { get; set; }
        //11
        [JsonProperty("LOA indication (Y/N)")]
        public string LOAInd { get; set; }
        //12
        [JsonProperty("COA (Y/N)")]
        public string COAInd { get; set; }

        public string ToMessage()
        {           
           return JsonConvert.SerializeObject(this);
        }

        public string GetCategory(string cat)
        {
            //string res;
            switch(cat)
            {
                case "COA": return COAInd;
                case "LOA indication": return LOAInd;
                case "Packaging PIC": return PackagingPIC;
                case "Number of available samples": return NumOfAvailSamples;
                case "Dosage Form": return GetFormated(DosageForm,';');
                case "DMF Availability": return GetFormated(DMFAvailability, ';');
                case "Sub Status (Calculated)": return SubStatus;
                case "CAS Number": return CASNumber;
                case "Innovator/Marketer": return InnovatorMarketer;
                case "Tapi Product Name(Level 2)": return TapiProductName;
                case "Molecule + Salt IMS Name": return MoleculeSaltName;
                case "Molecule (Level 1) ID": return MoleculeID;
                default: return string.Format(NO_SUCH_CAT, cat);
            }

         
        }

        private string GetFormated(string value, char delim)
        {
            string message = "";
            string[] splits = value.Split(delim);
            foreach(string split in splits)
            {
                message += "* " + split + "\n\n";
            }
            return message;
        }

        [JsonConstructor]
        public ProductDocument(string moleculeID, string moleculeSaltName, string tapiProductName, 
            string innovatorMarketer, string cASNumber, string subStatus, string dMFAvailability,
            string dosageForm, string numOfAvailSamples , string packagingPIC, string lOAInd, string cOAInd)
        {
            MoleculeID = moleculeID;
            MoleculeSaltName = moleculeSaltName;
            TapiProductName = tapiProductName;
            InnovatorMarketer = innovatorMarketer;
            CASNumber = cASNumber;
            SubStatus = subStatus;
            DMFAvailability = dMFAvailability;
            DosageForm = dosageForm;
            NumOfAvailSamples = numOfAvailSamples;
            PackagingPIC = packagingPIC;
            LOAInd = lOAInd;
            COAInd = cOAInd;
            
        }

        public Attachment GetProductCard(string option)
        {
            
            switch (option)
            {
                case FULL:
                    return GetFull();
                case HIGHLIGHT:
                    return GetHighligh();
                case CONFIRM:
                    return GetProductConfirm();
                //case NO_RESULT:
                //    return GetNoResults();
                    
                default: return GetHighligh();
            }
            
        }

        public Attachment GetProductCat(string category)
        {
            if ("Packaging PIC".Equals(category)) return GetProductPic();
            var productCard = new HeroCard
            {
                Title = string.Format(Utilities.GetSentence("12.40"), category),
                Subtitle = Utilities.GetSentence("12.41"),
                Text = GetCategory(category),
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/1-png.png") } ,
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.41"), value: SHOW_ME_MORE),
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.42"), value: FETCH_BY_MAIL) }
            };

            return productCard.ToAttachment();

        }

        private Attachment GetProductPic()
        {
            string picURI = $"https://tapistore.blob.core.windows.net/packingpics/{PackagingPIC}";
            var productCard = new HeroCard
            {
                Title = $"Packing pic for {TapiProductName}",
                //Subtitle = Utilities.GetSentence("12.41"),
                //Text = GetCategory(category),
                Images = new List<CardImage> { new CardImage(picURI) },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.41"), value: SHOW_ME_MORE),
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.42"), value: FETCH_BY_MAIL) }
            };

            return productCard.ToAttachment();

        }




        private Attachment GetProductConfirm()
        {
            var productCard = new ThumbnailCard
            {
                Title = string.Format(Utilities.GetSentence("12"), TapiProductName),
                Subtitle = Utilities.GetSentence("12.1"),
                Text = Utilities.GetSentence("12.2"),
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/hp-banner_0004_catalog.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.5"), value: HIGHLIGHT), new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.6"), value: HELP) }
            };

            return productCard.ToAttachment();
        }

        private Attachment GetHighligh()
        {
            var productCard = new ThumbnailCard
            {
                Title = string.Format(Utilities.GetSentence("12.0"), TapiProductName) ,
                Subtitle = Utilities.GetSentence("12.01"),
                Text = Utilities.GetSentence("12.02"),
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/laszlo-article-for-hp-june-2018.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.3"), value: FETCH_BY_MAIL), new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.4"), value: SHOW_ME_MORE) }
            };

            return productCard.ToAttachment();
        }


        // TODO show only categories with values 
        private Attachment GetFull()
        {
            var productCard = new HeroCard
            {
                Title = Utilities.GetSentence("12.10") +$" : {TapiProductName} " ,
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
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.27"), value: Utilities.GetSentence("12.27")) }
                                                 //new CardAction(ActionTypes.PostBack, Utilities.GetSentence("12.28"), value: Utilities.GetSentence("12.28")) }            
            };

            return productCard.ToAttachment();
        }
    }
}