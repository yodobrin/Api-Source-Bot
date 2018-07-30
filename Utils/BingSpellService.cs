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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using Newtonsoft.Json;
using System.Configuration;
using System;
namespace SourceBot.Utils
{
    [Serializable]
    public class BingSpellService
    {
        /// <summary>
        /// The Bing Spell Check Api Url.
        /// </summary>
        private static readonly string SpellCheckApiUrl = ConfigurationManager.AppSettings["BingSpellCheckApiEndpoint"];

        /// <summary>
        /// Microsoft Bing Spell Check Api Key.
        /// </summary>
        private static readonly string ApiKey = ConfigurationManager.AppSettings["BingSpellcheckKey"];

        /// <summary>
        /// Gets the correct spelling for the given text
        /// </summary>
        /// <param name="text">The text to be corrected</param>
        /// <returns>string with corrected text</returns>
        public async Task<string> GetCorrectedTextAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiKey);

                var values = new Dictionary<string, string>
                {
                    { "text", text }
                };

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync(SpellCheckApiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                var spellCheckResponse = JsonConvert.DeserializeObject<BingSpellCheckResponse>(responseString);

                StringBuilder sb = new StringBuilder();
                int previousOffset = 0;

                foreach (var flaggedToken in spellCheckResponse.FlaggedTokens)
                {
                    // Append the text from the previous offset to the current misspelled word offset
                    sb.Append(text.Substring(previousOffset, flaggedToken.Offset - previousOffset));

                    // Append the corrected word instead of the misspelled word
                    sb.Append(flaggedToken.Suggestions.First().Suggestion);

                    // Increment the offset by the length of the misspelled word
                    previousOffset = flaggedToken.Offset + flaggedToken.Token.Length;
                }

                // Append the text after the last misspelled word.
                if (previousOffset < text.Length)
                {
                    sb.Append(text.Substring(previousOffset));
                }

                return sb.ToString();
            }
        }
    }

    [Serializable]
    public class BingSpellCheckResponse
    {
        [JsonProperty("_type")]
        public string Type { get; set; }

        public BingSpellCheckFlaggedToken[] FlaggedTokens { get; set; }

        public BingSpellCheckError Error { get; set; }
    }

    [Serializable]
    public class BingSpellCheckFlaggedToken
    {
        public int Offset { get; set; }

        public string Token { get; set; }

        public string Type { get; set; }

        public BingSpellCheckSuggestion[] Suggestions { get; set; }
    }

    [Serializable]
    public class BingSpellCheckSuggestion
    {
        public string Suggestion { get; set; }

        public double Score { get; set; }
    }
    [Serializable]
    public class BingSpellCheckError
    {
        public int StatusCode { get; set; }

        public string Message { get; set; }
    }
}
