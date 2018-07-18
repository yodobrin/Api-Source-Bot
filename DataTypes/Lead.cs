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

namespace SourceBot.DataTypes
{
    [Serializable]
    public class Lead
    {
        
        //[JsonProperty("MessageType")]        
        //public string MessageType { get; set; }

        
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
            return JsonConvert.SerializeObject(this);
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

    }
}