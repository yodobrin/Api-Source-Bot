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
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using System.Collections.Generic;

using SourceBot.DataTypes;
using SourceBot.Utils;

using System.Threading;

namespace SourceBot.Dialogs
{
    [Serializable]
    public class LeadDialog : IDialog<Lead>
    {
       
        public async Task StartAsync(IDialogContext context)
        {
            var message = context.MakeMessage();
            message.Attachments.Add(AttachmentsUtil.CreateLeadFormCard());
            await context.PostAsync(message);

            context.Wait(this.MessageReceivedAsync);
        }


        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (message.Value != null)
            {
                
                dynamic value = message.Value;
                string submitType = value.Type.ToString();
                string email = value.Email.toString();
                string name = value.Name.toString();
                //string country = value.Country.toString();
                await context.PostAsync($"|{name}-{email}|");
            }else await context.PostAsync("something is wrong - message value is null");
            // pass control back to the calling dialog (root)
            context.Done<object>(null);
        }

      
       
    }
}