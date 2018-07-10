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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

using Newtonsoft.Json;

namespace SourceBot.Utils
{
	

	public class Utilities
	{
		// Service Bus area
		static string ServiceBusConnString = null;
		static string ServiceBusKey = null;
		static string QueueName = null;
		static IQueueClient queueClient = null;

		// Azure Searxh Area

		static string SearchIndexName = null;
		static string SearchServiceName = null;
		static string SearchServiceQueryApiKey = null;
		static SearchIndexClient IndexClient = null;

		/*
		 * Used to initilize the queue client - will be used to send messages to service bus
		 * 
		 **/

		public static async void InitQ()
		{
			// verify global setting were not initilized yet
			if (queueClient == null || QueueName == null || ServiceBusKey == null || ServiceBusConnString == null)
			{
				// take from bot configuration setting
				ServiceBusConnString = ConfigurationManager.AppSettings["ServiceBusConnString"];
				ServiceBusKey = ConfigurationManager.AppSettings["ServiceBusKey"];
				QueueName = ConfigurationManager.AppSettings["QueueName"];
				// init the queue client with which messages would be sent
				queueClient = new QueueClient(ServiceBusConnString, QueueName);
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

		public static async Task AddMessageToQueueAsync(string messageBody)
		{
			InitQ();
			var message = new Message(Encoding.UTF8.GetBytes(messageBody));
			await queueClient.SendAsync(message);
		}


		public static DocumentSearchResult Search(string searchText)
		{
			InitSearch();
			// Execute search based on query string
			SearchParameters sp = new SearchParameters() { SearchMode = SearchMode.All };
			return IndexClient.Documents.Search(searchText, sp);
		}

	}

	public class CommonMessage
	{
		[JsonProperty("MessageType")]
		public string MessageType { get; set; }
		[JsonProperty("Email")]
		public string Email { get; set; }
		[JsonProperty("APIID")]
		public string APIID { get; set; }
	}
}