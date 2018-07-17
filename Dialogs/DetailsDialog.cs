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

using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using SourceBot.DataTypes;

namespace SourceBot.Dialogs
{
    [Serializable]
    public class DetailsDialog : IDialog<Lead>
    {
        public async Task StartAsync(IDialogContext context)
    {
        await context.PostAsync("I would need some basic details from you ...");

        var leadFormDialog = FormDialog.FromForm(this.BuildLeadForm, FormOptions.PromptInStart);

        context.Call(leadFormDialog, this.ResumeAfterLeadFormDialog);
    }

    private IForm<Lead> BuildLeadForm()
    {
        OnCompletionAsyncDelegate<Lead> processLead = async (context, state) =>
        {
            await context.PostAsync($"Ok. I got the information I needed ! ");
        };

        return new FormBuilder<Lead>()
            .AddRemainingFields()
            .OnCompletion(processLead)
            .Build();
    }

       

        private async Task ResumeAfterLeadFormDialog(IDialogContext context, IAwaitable<Lead> result)
    {
            Lead lead = null;
        try
        {
            lead = await result;
        }
        catch (FormCanceledException ex)
        {
            string reply;

            if (ex.InnerException == null)
            {
                reply = "You have canceled the operation. Quitting from the LeadDialog";
            }
            else
            {
                reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
            }

            await context.PostAsync(reply);
        }
        finally
        {
            context.Done<Lead>(lead);
        }
    }

   
}
}