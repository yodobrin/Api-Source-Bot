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



using System.Text;

using System.Threading.Tasks;
using System.Configuration;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Collections.Generic;
using System;


namespace SourceBot.Utils
{

    [Serializable]
    public class Utilities
	{
        // messages file
        private static Dictionary<string, string> Sentences;
        private const string NO_SUCH_SENTENCE = "Sorry no such sentence number";
        private const string ISSUE_WITH_SENTENCE = "Sorry I'm having an issue loading the sentences file";

        public const string PERSIST_Q = "PersistQueueName";
        public const string TRANSIENT_Q = "TransientQueueName";


        // Service Bus area
        static string ServiceBusConnString = null;
		static string ServiceBusKey = null;
        // lead creation + survey
		static string QueueNamePersist = null;
        // send mails with results + pdf
        static string QueueNameTransient = null;

        //static string LeadQueueName = null;
        //static string SurveyQueueName = null;
        //static string QueueName = null;

        static IQueueClient queueClientPersist = null;

        static IQueueClient queueClientTransient = null;

        // Azure Searxh Area

        static string SearchIndexName = null;
		static string SearchServiceName = null;
		static string SearchServiceQueryApiKey = null;
		static ISearchIndexClient IndexClient = null;


        // constants
        public static string PRODUCT = "product";


        static void InitSentences()
        {
            // verify we have yet to initilize
            if(Sentences==null || Sentences.Count == 0)
            {
                Sentences = new Dictionary<string, string>();
                LoadSentencesFile( ConfigurationManager.AppSettings["SentencesFile"]);
            }
        }
        public static string GetSentence(string field)
        {
            InitSentences();
            if(Sentences!=null && Sentences.Count>0)
                return (Sentences.ContainsKey(field)) ? (Sentences[field]) : (NO_SUCH_SENTENCE);
            else return ISSUE_WITH_SENTENCE;
        }

        private static void LoadSentencesFile(string filePath)
        {
            foreach (string line in System.IO.File.ReadAllLines(filePath))
            {
                if ((!string.IsNullOrEmpty(line)) &&
                    (!line.StartsWith(";")) &&
                    (!line.StartsWith("#")) &&
                    (!line.StartsWith("'")) &&
                    (!line.StartsWith("/")) &&
                    (line.Contains("=")))
                {
                    int index = line.IndexOf('=');
                    string key = line.Substring(0, index).Trim();
                    string value = line.Substring(index + 1).Trim();

                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    try
                    {
                        //ignore dublicates
                        Sentences.Add(key, value);
                    }
                    catch { }
                }
            }
        }
        /*
		 * Used to initilize the queue client - will be used to send messages to service bus
		 * 
		 **/

        public static async void InitQ()
		{
			// verify global setting were not initilized yet
			if (queueClientPersist == null || QueueNamePersist == null || ServiceBusKey == null || ServiceBusConnString == null)
			{
				// take from bot configuration setting
				ServiceBusConnString = ConfigurationManager.AppSettings["ServiceBusConnString"];
				ServiceBusKey = ConfigurationManager.AppSettings["ServiceBusKey"];
				QueueNamePersist = ConfigurationManager.AppSettings["PersistQueueName"];
                QueueNameTransient = ConfigurationManager.AppSettings["TransientQueueName"];

                // init the queue client with which messages would be sent
                queueClientPersist = new QueueClient(ServiceBusConnString, QueueNamePersist);
                queueClientTransient = new QueueClient(ServiceBusConnString, QueueNameTransient);

            }
			else return;
		}

		/*
		 * Used to initilize the search client
		 * 
		 **/ 

		public static async void InitSearch()
		{
			if (SearchIndexName == null || SearchServiceName == null || SearchServiceQueryApiKey == null || IndexClient == null)
			{
				// take from bot configuration setting
				SearchIndexName = ConfigurationManager.AppSettings["SearchIndexName"];
				SearchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
				SearchServiceQueryApiKey = ConfigurationManager.AppSettings["SearchServiceQueryApiKey"];
				// initiate the index client
				IndexClient = new SearchIndexClient(SearchServiceName, SearchIndexName, new SearchCredentials(SearchServiceQueryApiKey));
			}
			else return;


		}

		/*
		 * used to send messages to the predefined queue (in bot settings)
		 * the message needs to be formatted in json structure as defined below
		 * 
		 **/
         
		//public static async Task AddMessageToQueueAsync(string messageBody)
		//{
		//	InitQ();
		//	var message = new Message(Encoding.UTF8.GetBytes(messageBody));
		//	await queueClientPersist.SendAsync(message);
		//}

        public static async Task AddMessageToQueueAsync(string messageBody, string queue)
        {
            InitQ();
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            switch (queue)
            {
                case PERSIST_Q:
                    await queueClientTransient.SendAsync(message);
                    break;
                case TRANSIENT_Q:
                    await queueClientPersist.SendAsync(message);
                    break;
                default: break;
            }

            await queueClientPersist.SendAsync(message);
        }


        public static DocumentSearchResult Search(string searchText)
		{
			InitSearch();
			// Execute search based on query string
			SearchParameters sp = new SearchParameters() { SearchMode = SearchMode.All };
			return IndexClient.Documents.Search(searchText, sp);
		}

        public static ISearchIndexClient GetSearchClient()
        {
            InitSearch();
            return IndexClient;
        }

    }

    public static class RegexConstants
    {
        public const string Email = @"[a-z0-9!#$%&'*+\/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+\/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

        public const string Phone = @"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$";

        //public const string Country = @"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$";
    }


}