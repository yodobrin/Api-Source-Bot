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
using SourceBot.Utils;



using Microsoft.Bot.Connector;

namespace SourceBot.DataTypes
{
    public class SurveyAnswer
    {
        public const string EXT_SAT = "extremely-satisfied";
        public const string VER_SAT = "very-satisfied";
        public const string SAT = "satisfied";
        public const string NOT_SAT = "not-satisfied";
        public const string NOT_AT_SAT = "not-satisfied-at-all";

        public static Attachment GetSurveyCard(string locName)
        {
            
            var leadCard = new HeroCard
            {
                Title = string.Format(Utilities.GetSentence("19"), locName),

                Images = new List<CardImage> { new CardImage("https://www.tapi.com/globalassets/hp-banner_0001_wearetapi.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, Utilities.GetSentence("19.1"), value: string.Format(Utilities.GetSentence("19.20"), NOT_AT_SAT)), // Utilities.GetSentence("19.1"))) ,
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("19.2"), value: string.Format(Utilities.GetSentence("19.20"), NOT_SAT)), //Utilities.GetSentence("19.2"))) ,
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("19.3"), value: string.Format(Utilities.GetSentence("19.20"), SAT)), //Utilities.GetSentence("19.3"))) ,
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("19.4"), value: string.Format(Utilities.GetSentence("19.20"), VER_SAT)), //Utilities.GetSentence("19.4"))) ,
                                                 new CardAction(ActionTypes.PostBack, Utilities.GetSentence("19.5"), value: string.Format(Utilities.GetSentence("19.20"), NOT_AT_SAT)), //Utilities.GetSentence("19.5"))) 
                }

            };

            return leadCard.ToAttachment();
        }

        public static Attachment GetHoreyCard(string locName)
        {

            var leadCard = new HeroCard
            {
                Title = string.Format(Utilities.GetSentence("19.50"), locName),
                Images = new List<CardImage> { new CardImage("http://static.flickr.com/41/124082976_4c0da6dc61_o.jpg") },
                //Media = new List<MediaUrl>  {new MediaUrl() { Url = "https://tenor.com/search/irish-gifs" } }
            };

            return leadCard.ToAttachment();
        }

      
    }
}