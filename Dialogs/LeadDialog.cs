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
        Lead thisLead;
        //string tempLine;
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("I would need some basic details from you ...");
            if (!context.PrivateConversationData.TryGetValue("bot-lead", out thisLead))
            {
                thisLead = new Lead(1);
                await context.PostAsync($"i know nothing:{thisLead.flush()}");
                context.PrivateConversationData.SetValue("bot-lead", thisLead);
            }
            Dictionary<string, LineItem> fields = thisLead.GetValues(Lead.UNFILLED);
            //string val;
            await context.PostAsync($"I would need some basic details from you ...{fields.Count}");
            foreach (LineItem itm in fields.Values)
            {
                //await context.Forward(new LineDialog(itm.Type),this.ResumeAfterLine, context.Activity, CancellationToken.None);
                // await context.PostAsync($"trying to initiate line dialog for {itm.Type}");
                //await context.Forward(new LineDialog(itm.Type), this.ResumeAfterLine, context.Activity, CancellationToken.None);
                //context.Call(new LineDialog(itm.Type), this.ResumeAfterLine);
                //await context.PostAsync($"I got: {tempLine} for the filed {itm.Type}");
                context.PrivateConversationData.SetValue("curent-field", itm.Type);
                PromptDialog.Text(context, this.ResumeAfterPrompt, Utilities.GetSentence(itm.Type));
                string temp;
                context.PrivateConversationData.TryGetValue(itm.Type, out temp);
                itm.Value = temp;
                await context.PostAsync($"got {itm.Value} for {itm.Type}");
                //thisLead.properties[]
            }

            context.Wait(this.MessageReceivedAsync);
        }


        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {            
            context.Done(thisLead);
        }

        private async Task ResumeAfterPrompt(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                string line;
                context.PrivateConversationData.TryGetValue("current-field", out line);

                string value = (string)await result;
                context.PrivateConversationData.SetValue(line, value);
            }
            catch (TooManyAttemptsException)
            {
            }

            context.Wait(this.MessageReceivedAsync);
        }

       
    }
}