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
using Tapi.Bot.SophiBot.DataTypes;

using Newtonsoft.Json;


namespace Tapi.Bot.SophiBot.Utils
{

    [Serializable]
    public class Utilities
	{
        // messages file
        private static Dictionary<string, string> Sentences;
        private const string NO_SUCH_SENTENCE = "Sorry no such sentence number";
        private const string ISSUE_WITH_SENTENCE = "Sorry I'm having an issue loading the sentences file";

        public const string LEAD_Q = "PersistQueueName";
        public const string TRANSIENT_Q = "TransientQueueName";
        public const string SURVEY_Q = "SurveyQueueName";


        // Service Bus area
        static string ServiceBusConnString = null;
		static string ServiceBusKey = null;
        // lead creation 
		static string QueueNameLeadPersist = null;
        // send mails with results + pdf
        static string QueueNameTransient = null;

        // survey creation 
        static string QueueNameSurveyPersist = null;



        static IQueueClient queueClientLeadPersist = null;

        static IQueueClient queueClientSurveyPersist = null;

        static IQueueClient queueClientTransient = null;

        // Azure Searxh Area

        static string SearchIndexName = null;
		static string SearchServiceName = null;
		static string SearchServiceQueryApiKey = null;
		static ISearchIndexClient IndexClient = null;

        // sql area

        //static System.Data.SqlClient.SqlConnection SqlConnection = null;            
        
        

        // constants
        public static string PRODUCT = "product";
        public static string NONPRODUCT = "UnknownProduct";

        //static void InitSQL()
        //{
        //    if(SqlConnection==null)
        //    {
        //        SqlConnection = new System.Data.SqlClient.SqlConnection(ConfigurationManager.AppSettings["SQLConnectionString"]);
        //    }
        //}

       

        //public static void WriteToDB(BotLog message)
        //{
        //    InitSQL();
        //    System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
        //    cmd.CommandType = System.Data.CommandType.Text;
        //    cmd.CommandText = message.GetWriteSql();
        //    cmd.Connection = SqlConnection;

        //    SqlConnection.Open();
        //    cmd.ExecuteNonQuery();
        //    SqlConnection.Close();
        //}

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
			if (queueClientLeadPersist == null || QueueNameLeadPersist == null || ServiceBusKey == null || ServiceBusConnString == null)
			{
				// take from bot configuration setting
				ServiceBusConnString = ConfigurationManager.AppSettings["ServiceBusConnString"];
				ServiceBusKey = ConfigurationManager.AppSettings["ServiceBusKey"];
				QueueNameLeadPersist = ConfigurationManager.AppSettings[LEAD_Q];
                QueueNameTransient = ConfigurationManager.AppSettings[TRANSIENT_Q];
                QueueNameSurveyPersist = ConfigurationManager.AppSettings[SURVEY_Q];

                // init the queue client with which messages would be sent
                queueClientLeadPersist = new QueueClient(ServiceBusConnString, QueueNameLeadPersist);
                queueClientTransient = new QueueClient(ServiceBusConnString, QueueNameTransient);
                queueClientSurveyPersist = new QueueClient(ServiceBusConnString, QueueNameSurveyPersist);
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


        public static async Task AddMessageToQueueAsync(string messageBody, string queue)
        {
            InitQ();
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            
            switch (queue)
            {
                case LEAD_Q:
                    await queueClientTransient.SendAsync(message);
                    break;
                case TRANSIENT_Q:
                    await queueClientTransient.SendAsync(message);
                    break;
                case SURVEY_Q:
                    await queueClientSurveyPersist.SendAsync(message);
                    break;
                default: break;
            }

            await queueClientLeadPersist.SendAsync(message);
        }


        public static DocumentSearchResult Search(string searchText)
		{
			InitSearch();
			// Execute search based on query string
			SearchParameters sp = new SearchParameters() { SearchMode = SearchMode.All };
			return IndexClient.Documents.Search(searchText, sp);
		}

        public static ProductDocument SearchQuery(string searchText, int feelinglucky)
        {
            IList<ProductDocument> products = new List<ProductDocument>();
            DocumentSearchResult searchResult = Search(searchText);
            if (searchResult != null)
            {
                foreach (SearchResult temp in searchResult.Results)
                {
                    ProductDocument prodDoc = JsonConvert.DeserializeObject<ProductDocument>((string)temp.Document["content"]);
                    products.Add(prodDoc);

                }
                return products[0];
            }
            else return null;
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
    
    }


}