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

using AdaptiveCards;
using SourceBot.DataTypes;
using SourceBot.Dialogs;
using System.Threading;


using Microsoft.Bot.Connector;

namespace SourceBot.Utils
{
    [Serializable]
    public class AttachmentsUtil
    {

        public const string FULL = "full-lead";
        public const string MINIMAL = "minimal-lead";
        public const string REVISIT = "revisit-lead";

        public static Attachment GetEndCard(string name)
        {
            var endCard = new HeroCard
            {
                Title = $"{name}  - Thank you!",
                Text = Utilities.GetSentence("19.60"),
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/about-us-new.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Sure", value: "survey"),
                    new CardAction(ActionTypes.PostBack, "No", value: "bye") }
            };

            return endCard.ToAttachment();
        }

        public static Attachment GetConversationEndCard(string safeword)
        {
            var endCard = new HeroCard
            {
                Title = string.Format(Utilities.GetSentence("1.1"), safeword),
                Text = Utilities.GetSentence("1.11"),
                //Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/about-us-new.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Sure, I'm done", value: "wipe-clean") }
            };

            return endCard.ToAttachment();
        }

        public static Attachment GetConversationEndCard1(string safeword)
        {
           
            AdaptiveCard card = new AdaptiveCard()
            {
                Body = new List<CardElement>()
                { 
                    new Container()
                    {
                        Items = new List<CardElement>  { new TextBlock  { Text = string.Format(Utilities.GetSentence("1.1"),safeword), Wrap = true  }   }
                    }
                },
                Actions = new List<ActionBase>() { new SubmitAction() { Data = "wipe-clean" , Title = "Wipe all data" } }
            };

            Attachment attachment = new Attachment() { ContentType = AdaptiveCard.ContentType,  Content = card  };

            return attachment;

        }

        public static Attachment GetConversationStartCard()
        {

            AdaptiveCard card = new AdaptiveCard()
            {
                Body = new List<CardElement>()
                {
                    new Container()
                    {
                        Items = new List<CardElement>
                        {
                            new TextBlock  { Text = Utilities.GetSentence("0"), Wrap = true  },
                            new TextBlock  { Text = Utilities.GetSentence("0.01"), Wrap = true  }
                        }
                    }
                }
            };

            Attachment attachment = new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };

            return attachment;

        }

        public static Attachment GetSpellSuggestCard(string original, string altered)
        {
            var spellCard = new HeroCard
            {
                Title = $"Spell Suggestion",
                //Text = $"Do you want to continue and search for {original} or  {altered}",
                Images = new List<CardImage> { new CardImage("https://www.webdevelopersnotes.com/wp-content/uploads/change-spell-check-dictionary-french-english-outlook-express.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Original-"+original, value: original),
                    new CardAction(ActionTypes.PostBack, "Suggested-"+altered, value: altered) }
            };

            return spellCard.ToAttachment();
        }



        //public static Attachment GetOpenCard(string name, string company)
        //{
        //    var openCard = new HeroCard
        //    {
        //        Title = string.Format(Utilities.GetSentence("19.70"), name, company), // $"API Source Bot tailored for {name} @ {company}",
        //        Subtitle = Utilities.GetSentence("19.71"),
        //        Text = Utilities.GetSentence("19.72"),
        //        Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/about-us-new.jpg") },
        //        Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Find me Aripiprazole", value: "find me Aripiprazole"), new CardAction(ActionTypes.PostBack, "Find me Aztreonam", value: "find me Aztreonam") }
        //    };

        //    return openCard.ToAttachment();
        //}

        public static Attachment GetErrorCard(string code)
        {
            var openCard = new HeroCard
            {
                Title = Utilities.GetSentence("950"),
                Subtitle = string.Format(Utilities.GetSentence("951"), code),
                Text = Utilities.GetSentence("952"),
                Images = new List<CardImage> { new CardImage("https://cdn.dribbble.com/users/7770/screenshots/3935947/oh_snap_404_1x.jpg") }                
            };

            return openCard.ToAttachment();
        }


        public static Attachment GetNoResults(string query)
        {
            var productCard = new ThumbnailCard
            {
                Title = Utilities.GetSentence("5"),
                Subtitle = string.Format(Utilities.GetSentence("5.1"), query),
                Text = Utilities.GetSentence("5.2"),
                Images = new List<CardImage> { new CardImage("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQHEl-j7JobwiGjkbpCBVemqrUKp9EQFtPQOyOLXIBsAvycS8Kx") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, Utilities.GetSentence("6"), value: Lead.CONTACT_TAPI),
                new CardAction(ActionTypes.PostBack, Utilities.GetSentence("7"), value: Lead.SEND_PDF),
                new CardAction(ActionTypes.PostBack, Utilities.GetSentence("8"), value: Lead.UPDATE_ONCE_EXIST)
                }
            };

            return productCard.ToAttachment();

        }


        public static Attachment GetResultCard(IList<ProductDocument> tproducts)
        {
            string suffix = "";
            int count = 0;
            if (tproducts.Count == 1) return tproducts[0].GetProductCard(ProductDocument.CONFIRM);
            List<CardAction> buttons = new List<CardAction>();
            if (tproducts.Count > 0)
            {
                suffix = (tproducts.Count == 1) ? "" : "s";

                foreach (ProductDocument prd in tproducts)
                {
                    if (count == ProductDocument.MAX_PROD_IN_RESULT) break;
                    buttons.Add(new CardAction(ActionTypes.PostBack, $"{prd.TapiProductName}", value: $"find me {prd.MoleculeID}"));
                    count++;
                }
            }
            var resultCard = new HeroCard
            {
                Title = $"I found: {tproducts.Count} product{suffix}.",
                Subtitle = Utilities.GetSentence("16"),
                Text = Utilities.GetSentence("16.1"),
                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/hp-banner_0000_inspections.jpg") },
                Buttons = buttons
            };

            return resultCard.ToAttachment();
        }

        //private List<CardElement> GetFormFields()
        //{
        //    List<CardElement> list = new List<CardElement>();
        //    foreach(string field in)
        //}

        public static Attachment CreateFullLeadFormCard(Lead lead)
        {
            var card = new AdaptiveCard();
            string subject = lead.GetSubject();
            var columnsBlock = new ColumnSet()
            {
                Separation = SeparationStyle.None,
                Columns = new List<Column>
                {
                        new Column
                        {
                            Size = "2",
                            Items = new List<CardElement>
                            {  new TextBlock  {  Text = "Please revisit details...", Weight = TextWeight.Bolder,  Size = TextSize.Large, },
                               //new TextBlock  {  Text = "We just need a few more details to get you TAPI's Information", IsSubtle = false,  Wrap = true, },

                               new TextBlock  {  Text = "Your name", Wrap = true, },
                               new TextInput  {  Id = "Name", Placeholder = lead.Name, Style = TextInputStyle.Text, },

                               new TextBlock  {  Text = "Your email", Wrap = true, },
                               new TextInput  {  Id = "Email", Placeholder = lead.Email, Style = TextInputStyle.Email, },

                               new TextBlock  {  Text = "Phone Number", Wrap = true, Color = TextColor.Attention },
                               new TextInput  {  Id = "PhoneNumber", Placeholder = lead.Phone, Style = TextInputStyle.Tel, },

                               new TextBlock  {  Text = "Country", Wrap = true, Color = TextColor.Attention },                               
                               new ChoiceSet(){  Id = "Country",  Style = ChoiceInputStyle.Compact, Choices = GetCountries() },

                               new TextBlock  {  Text = "Company", Wrap = true, Color = TextColor.Attention },
                               new TextInput  {  Id = "Company",  Placeholder = lead.Company, Style = TextInputStyle.Text, },

                               new TextBlock  {  Text = "Commments?", Wrap = true, Color = TextColor.Attention },
                               new TextInput  {  Id = "Comments",  Placeholder = lead.Company, Style = TextInputStyle.Text, IsMultiline = true, },

                               new TextBlock  {  Text = $"Subject:{subject}", Wrap = true, Color = TextColor.Attention },                               

                               
                            },

                        }
                }
            };
            card.Body.Add(columnsBlock);

            card.Actions = new List<ActionBase>()
                {
                    new SubmitAction
                    {
                        Title = "Submit",
                        DataJson = "{ \"Type\": \"CreateLeadCard\" }",
                    }
                };

            Attachment attachment = new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };

            return attachment;
        }

        public static Attachment CreateFullLeadFormCard()
        {
            var card = new AdaptiveCard();

            var columnsBlock = new ColumnSet()
            {
                Separation = SeparationStyle.None,
                Columns = new List<Column>
                {
                        new Column
                        {
                            Size = "2",
                            Items = new List<CardElement>
                            {  new TextBlock  {  Text = "Tell us about yourself...", Weight = TextWeight.Bolder,  Size = TextSize.Large, },
                               new TextBlock  {  Text = "We just need a few more details to get you TAPI's Information", IsSubtle = false,  Wrap = true, },

                               new TextBlock  {  Text = "Your name", Wrap = true, },
                               new TextInput  {  Id = "Name", Placeholder = "Last, First",  },

                               new TextBlock  {  Text = "Your email", Wrap = true, },
                               new TextInput  {  Id = "Email", Placeholder = "youremail@example.com", Style = TextInputStyle.Email, },

                               new TextBlock  {  Text = "Phone Number", Wrap = true, Color = TextColor.Attention },
                               new TextInput  {  Id = "PhoneNumber", Placeholder = "optional", Style = TextInputStyle.Tel, },

                               new TextBlock  {  Text = "Country", Wrap = true, Color = TextColor.Attention },
                               new ChoiceSet(){  Id = "Country",  Style = ChoiceInputStyle.Compact, Choices = GetCountries() },                               

                               new TextBlock  {  Text = "Company", Wrap = true, Color = TextColor.Attention },
                               new TextInput  {  Id = "Company",  Style = TextInputStyle.Tel, },

                               new TextBlock  {  Text = "Commments?", Wrap = true, Color = TextColor.Attention },
                               new TextInput  {  Id = "Comments",  Style = TextInputStyle.Tel, IsMultiline = true, },
                            },
                        }
                }
            };
            card.Body.Add(columnsBlock);

            card.Actions = new List<ActionBase>()
                {
                    new SubmitAction
                    {
                        Title = "Submit",
                        DataJson = "{ \"Type\": \"CreateLeadCard\" }",
                    }
                };

            Attachment attachment = new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };

            return attachment;
            
        }


        public static Attachment CreateMinimalLeadFormCard()
        {
            var card = new AdaptiveCard();

            var columnsBlock = new ColumnSet()
            {
                Separation = SeparationStyle.None,
                Columns = new List<Column>
                {
                        new Column
                        {
                            Size = "2",
                            Items = new List<CardElement>
                            {  
                               new TextBlock  {  Text = "We just need an email address to get you TAPI's Information", IsSubtle = false,  Wrap = true, },

                               new TextBlock  {  Text = "Your email", Wrap = true, },
                               new TextInput  {  Id = "Email", Placeholder = "youremail@example.com", Style = TextInputStyle.Email, },
                            },

                        }
                }
            };
            card.Body.Add(columnsBlock);

            card.Actions = new List<ActionBase>()
                {
                    new SubmitAction
                    {
                        Title = "Submit",
                        DataJson = "{ \"Type\": \"CreateMinimalLeadCard\" }",
                    }
                };

            Attachment attachment = new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };

            return attachment;

        }

        public static Attachment getAdaptiveFull()
        {
            AdaptiveCard card = new AdaptiveCard()
            {
                Body = new List<CardElement>()
                {
                    new Container()
                    {
                        
                        Items = new List<CardElement>()
                        {
                            new ColumnSet()
                            {
                                Columns = new List<Column>()
                                {
                                    new Column()
                                    {
                                        Size = ColumnSize.Auto,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text =  "my first topic!",
                                                Weight = TextWeight.Normal,
                                                Color = TextColor.Good,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {
                                                Text =  "my second topic:",
                                                Weight = TextWeight.Normal,
                                                Color = TextColor.Dark,
                                                IsSubtle = true
                                            }
                                        }
                                    },
                                    new Column()
                                    {
                                        Size = ColumnSize.Stretch,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text =  "a topic value!",
                                                Weight = TextWeight.Bolder,
                                                Wrap = true,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {
                                                Text =  "a second topic value!",
                                                Weight = TextWeight.Bolder,
                                                Color = TextColor.Dark,
                                                Wrap = true,
                                                IsSubtle = true
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                // Buttons
                Actions = new List<ActionBase>() {

                    new ShowCardAction()
                    {
                        Title = "Flights",
                        Speak = "<s>Flights</s>",
                        Card = new AdaptiveCard()
                        {
                            Body = new List<CardElement>()
                            {
                                new TextBlock()
                                {
                                    Text = "Flights is not implemented =(",
                                    Speak = "<s>Flights is not implemented</s>",
                                    Weight = TextWeight.Bolder
                                }
                            }
                        }
                    }
                }
            };

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
            return attachment;
        }

        private static List<Choice> GetCountries()
        {
            List<Choice> list = new List<Choice>()
            {
                new Choice() { Title = "Afghanistan", Value = "Afghanistan" },
                new Choice() { Title = "Albania", Value = "Albania" },
                new Choice() { Title = "Algeria", Value = "Algeria" },
                new Choice() { Title = "Andorra", Value = "Andorra" },
                new Choice() { Title = "Angola", Value = "Angola" },
                new Choice() { Title = "Anguilla", Value = "Anguilla" },
                new Choice() { Title = "Antigua & Barbuda", Value = "Antigua & Barbuda" },
                new Choice() { Title = "Argentina", Value = "Argentina" },
                new Choice() { Title = "Armenia", Value = "Armenia" },
                new Choice() { Title = "Australia", Value = "Australia" },
                new Choice() { Title = "Austria", Value = "Austria" },
                new Choice() { Title = "Azerbaijan", Value = "Azerbaijan" },
                new Choice() { Title = "Bahamas", Value = "Bahamas" },
                new Choice() { Title = "Bahrain", Value = "Bahrain" },
                new Choice() { Title = "Bangladesh", Value = "Bangladesh" },
                new Choice() { Title = "Barbados", Value = "Barbados" },
                new Choice() { Title = "Belarus", Value = "Belarus" },
                new Choice() { Title = "Belgium", Value = "Belgium" },
                new Choice() { Title = "Belize", Value = "Belize" },
                new Choice() { Title = "Benin", Value = "Benin" },
                new Choice() { Title = "Bermuda", Value = "Bermuda" },
                new Choice() { Title = "Bhutan", Value = "Bhutan" },
                new Choice() { Title = "Bolivia", Value = "Bolivia" },
                new Choice() { Title = "Bosnia & Herzegovina", Value = "Bosnia & Herzegovina" },
                new Choice() { Title = "Botswana", Value = "Botswana" },
                new Choice() { Title = "Brazil", Value = "Brazil" },
                new Choice() { Title = "Brunei Darussalam", Value = "Brunei Darussalam" },
                new Choice() { Title = "Bulgaria", Value = "Bulgaria" },
                new Choice() { Title = "Burkina Faso", Value = "Burkina Faso" },
                new Choice() { Title = "Burundi", Value = "Burundi" },
                new Choice() { Title = "Cambodia", Value = "Cambodia" },
                new Choice() { Title = "Cameroon", Value = "Cameroon" },
                new Choice() { Title = "Canada", Value = "Canada" },
                new Choice() { Title = "Cape Verde", Value = "Cape Verde" },
                new Choice() { Title = "Cayman Islands", Value = "Cayman Islands" },
                new Choice() { Title = "Central African Republic", Value = "Central African Republic" },
                new Choice() { Title = "Chad", Value = "Chad" },
                new Choice() { Title = "Chile", Value = "Chile" },
                new Choice() { Title = "China", Value = "China" },
                new Choice() { Title = "Colombia", Value = "Colombia" },
                new Choice() { Title = "Comoros", Value = "Comoros" },
                new Choice() { Title = "Congo", Value = "Congo" },
                new Choice() { Title = "Congo", Value = "Congo" },
                new Choice() { Title = "Costa Rica", Value = "Costa Rica" },
                new Choice() { Title = "Croatia", Value = "Croatia" },
                new Choice() { Title = "Cuba", Value = "Cuba" },
                new Choice() { Title = "Cyprus", Value = "Cyprus" },
                new Choice() { Title = "Czech Republic", Value = "Czech Republic" },
                new Choice() { Title = "Denmark", Value = "Denmark" },
                new Choice() { Title = "Djibouti", Value = "Djibouti" },
                new Choice() { Title = "Dominica", Value = "Dominica" },
                new Choice() { Title = "Dominican Republic", Value = "Dominican Republic" },
                new Choice() { Title = "Ecuador", Value = "Ecuador" },
                new Choice() { Title = "Egypt", Value = "Egypt" },
                new Choice() { Title = "El Salvador", Value = "El Salvador" },
                new Choice() { Title = "Equatorial Guinea", Value = "Equatorial Guinea" },
                new Choice() { Title = "Eritrea", Value = "Eritrea" },
                new Choice() { Title = "Estonia", Value = "Estonia" },
                new Choice() { Title = "Ethiopia", Value = "Ethiopia" },
                new Choice() { Title = "Fiji", Value = "Fiji" },
                new Choice() { Title = "Finland", Value = "Finland" },
                new Choice() { Title = "France", Value = "France" },
                new Choice() { Title = "French Guiana", Value = "French Guiana" },
                new Choice() { Title = "Gabon", Value = "Gabon" },
                new Choice() { Title = "Gambia", Value = "Gambia" },
                new Choice() { Title = "Georgia", Value = "Georgia" },
                new Choice() { Title = "Germany", Value = "Germany" },
                new Choice() { Title = "Ghana", Value = "Ghana" },
                new Choice() { Title = "Greece", Value = "Greece" },
                new Choice() { Title = "Grenada", Value = "Grenada" },
                new Choice() { Title = "Guadeloupe", Value = "Guadeloupe" },
                new Choice() { Title = "Guatemala", Value = "Guatemala" },
                new Choice() { Title = "Guinea", Value = "Guinea" },
                new Choice() { Title = "Guinea-Bissau", Value = "Guinea-Bissau" },
                new Choice() { Title = "Guyana", Value = "Guyana" },
                new Choice() { Title = "Haiti", Value = "Haiti" },
                new Choice() { Title = "Honduras", Value = "Honduras" },
                new Choice() { Title = "Hungary", Value = "Hungary" },
                new Choice() { Title = "Iceland", Value = "Iceland" },
                new Choice() { Title = "India", Value = "India" },
                new Choice() { Title = "Indonesia", Value = "Indonesia" },
                new Choice() { Title = "Iran", Value = "Iran" },
                new Choice() { Title = "Iraq", Value = "Iraq" },
                new Choice() { Title = "Israel", Value = "Israel" },
                new Choice() { Title = "Italy", Value = "Italy" },
                new Choice() { Title = "Ivory Coast (Cote d'Ivoire)", Value = "Ivory Coast (Cote d'Ivoire)" },
                new Choice() { Title = "Jamaica", Value = "Jamaica" },
                new Choice() { Title = "Japan", Value = "Japan" },
                new Choice() { Title = "Jordan", Value = "Jordan" },
                new Choice() { Title = "Kazakhstan", Value = "Kazakhstan" },
                new Choice() { Title = "Kenya", Value = "Kenya" },
                new Choice() { Title = "Kosovo", Value = "Kosovo" },
                new Choice() { Title = "Kuwait", Value = "Kuwait" },
                new Choice() { Title = "Kyrgyzstan", Value = "Kyrgyzstan" },
                new Choice() { Title = "Laos", Value = "Laos" },
                new Choice() { Title = "Latvia", Value = "Latvia" },
                new Choice() { Title = "Lebanon", Value = "Lebanon" },
                new Choice() { Title = "Lesotho", Value = "Lesotho" },
                new Choice() { Title = "Liberia", Value = "Liberia" },
                new Choice() { Title = "Libya", Value = "Libya" },
                new Choice() { Title = "Liechtenstein", Value = "Liechtenstein" },
                new Choice() { Title = "Lithuania", Value = "Lithuania" },
                new Choice() { Title = "Luxembourg", Value = "Luxembourg" },
                new Choice() { Title = "Macedonia", Value = "Macedonia" },
                new Choice() { Title = "Madagascar", Value = "Madagascar" },
                new Choice() { Title = "Malawi", Value = "Malawi" },
                new Choice() { Title = "Malaysia", Value = "Malaysia" },
                new Choice() { Title = "Maldives", Value = "Maldives" },
                new Choice() { Title = "Mali", Value = "Mali" },
                new Choice() { Title = "Malta", Value = "Malta" },
                new Choice() { Title = "Martinique", Value = "Martinique" },
                new Choice() { Title = "Mauritania", Value = "Mauritania" },
                new Choice() { Title = "Mauritius", Value = "Mauritius" },
                new Choice() { Title = "Mayotte", Value = "Mayotte" },
                new Choice() { Title = "Mexico", Value = "Mexico" },
                new Choice() { Title = "Moldova, Republic of", Value = "Moldova, Republic of" },
                new Choice() { Title = "Monaco", Value = "Monaco" },
                new Choice() { Title = "Mongolia", Value = "Mongolia" },
                new Choice() { Title = "Montenegro", Value = "Montenegro" },
                new Choice() { Title = "Montserrat", Value = "Montserrat" },
                new Choice() { Title = "Morocco", Value = "Morocco" },
                new Choice() { Title = "Mozambique", Value = "Mozambique" },
                new Choice() { Title = "Myanmar/Burma", Value = "Myanmar/Burma" },
                new Choice() { Title = "Namibia", Value = "Namibia" },
                new Choice() { Title = "Nepal", Value = "Nepal" },
                new Choice() { Title = "Netherlands", Value = "Netherlands" },
                new Choice() { Title = "New Zealand", Value = "New Zealand" },
                new Choice() { Title = "Nicaragua", Value = "Nicaragua" },
                new Choice() { Title = "Niger", Value = "Niger" },
                new Choice() { Title = "Nigeria", Value = "Nigeria" },
                new Choice() { Title = "North Korea", Value = "North Korea" },
                new Choice() { Title = "Norway", Value = "Norway" },
                new Choice() { Title = "Oman", Value = "Oman" },
                new Choice() { Title = "Pacific Islands", Value = "Pacific Islands" },
                new Choice() { Title = "Pakistan", Value = "Pakistan" },
                new Choice() { Title = "Panama", Value = "Panama" },
                new Choice() { Title = "Papua New Guinea", Value = "Papua New Guinea" },
                new Choice() { Title = "Paraguay", Value = "Paraguay" },
                new Choice() { Title = "Peru", Value = "Peru" },
                new Choice() { Title = "Philippines", Value = "Philippines" },
                new Choice() { Title = "Poland", Value = "Poland" },
                new Choice() { Title = "Portugal", Value = "Portugal" },
                new Choice() { Title = "Puerto Rico", Value = "Puerto Rico" },
                new Choice() { Title = "Qatar", Value = "Qatar" },
                new Choice() { Title = "Reunion", Value = "Reunion" },
                new Choice() { Title = "Romania", Value = "Romania" },
                new Choice() { Title = "Russian Federation", Value = "Russian Federation" },
                new Choice() { Title = "Rwanda", Value = "Rwanda" },
                new Choice() { Title = "Saint Kitts and Nevis", Value = "Saint Kitts and Nevis" },
                new Choice() { Title = "Saint Lucia", Value = "Saint Lucia" },
                new Choice() { Title = "Saint Vincent's & Grenadines", Value = "Saint Vincent's & Grenadines" },
                new Choice() { Title = "Samoa", Value = "Samoa" },
                new Choice() { Title = "Sao Tome and Principe", Value = "Sao Tome and Principe" },
                new Choice() { Title = "Saudi Arabia", Value = "Saudi Arabia" },
                new Choice() { Title = "Senegal", Value = "Senegal" },
                new Choice() { Title = "Serbia", Value = "Serbia" },
                new Choice() { Title = "Seychelles", Value = "Seychelles" },
                new Choice() { Title = "Sierra Leone", Value = "Sierra Leone" },
                new Choice() { Title = "Singapore", Value = "Singapore" },
                new Choice() { Title = "Slovak Republic (Slovakia)", Value = "Slovak Republic (Slovakia)" },
                new Choice() { Title = "Slovenia", Value = "Slovenia" },
                new Choice() { Title = "Solomon Islands", Value = "Solomon Islands" },
                new Choice() { Title = "Somalia", Value = "Somalia" },
                new Choice() { Title = "South Africa", Value = "South Africa" },
                new Choice() { Title = "South Korea", Value = "South Korea" },
                new Choice() { Title = "South Sudan", Value = "South Sudan" },
                new Choice() { Title = "Spain", Value = "Spain" },
                new Choice() { Title = "Sri Lanka", Value = "Sri Lanka" },
                new Choice() { Title = "Sudan", Value = "Sudan" },
                new Choice() { Title = "Suriname", Value = "Suriname" },
                new Choice() { Title = "Swaziland", Value = "Swaziland" },
                new Choice() { Title = "Sweden", Value = "Sweden" },
                new Choice() { Title = "Switzerland", Value = "Switzerland" },
                new Choice() { Title = "Syria", Value = "Syria" },
                new Choice() { Title = "Tajikistan", Value = "Tajikistan" },
                new Choice() { Title = "Tanzania", Value = "Tanzania" },
                new Choice() { Title = "Thailand", Value = "Thailand" },
                new Choice() { Title = "Timor Leste", Value = "Timor Leste" },
                new Choice() { Title = "Togo", Value = "Togo" },
                new Choice() { Title = "Trinidad & Tobago", Value = "Trinidad & Tobago" },
                new Choice() { Title = "Tunisia", Value = "Tunisia" },
                new Choice() { Title = "Turkey", Value = "Turkey" },
                new Choice() { Title = "Turkmenistan", Value = "Turkmenistan" },
                new Choice() { Title = "Turks & Caicos Islands", Value = "Turks & Caicos Islands" },
                new Choice() { Title = "Uganda", Value = "Uganda" },
                new Choice() { Title = "Ukraine", Value = "Ukraine" },
                new Choice() { Title = "United Arab Emirates", Value = "United Arab Emirates" },
                new Choice() { Title = "United Kingdom", Value = "United Kingdom" },
                new Choice() { Title = "United States of America (USA)", Value = "United States of America (USA)", IsSelected = true},
                new Choice() { Title = "Uruguay", Value = "Uruguay" },
                new Choice() { Title = "Uzbekistan", Value = "Uzbekistan" },
                new Choice() { Title = "Venezuela", Value = "Venezuela" },
                new Choice() { Title = "Vietnam", Value = "Vietnam" },
                new Choice() { Title = "Virgin Islands (UK)", Value = "Virgin Islands (UK)" },
                new Choice() { Title = "Virgin Islands (US)", Value = "Virgin Islands (US)" },
                new Choice() { Title = "Yemen", Value = "Yemen" },
                new Choice() { Title = "Zambia", Value = "Zambia" },
                new Choice() { Title = "Zimbabwe", Value = "Zimbabwe" }

            };
            return list;
        }


    }



    
}