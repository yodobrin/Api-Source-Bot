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
using SourceBot.Utils;
using System.Text.RegularExpressions;

namespace SourceBot.DataTypes
{
    [Serializable]
    public class LineItem
    {
        public const string EMAIL = "email";
        public const string TEXT = "text";

        private readonly string [] NON_VALID_SUFF = {"gmail", "yahoo", "walla"};

        //private const string 
        public string Type { get; set; }
        public string Value { get; set; }

        public LineItem(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public bool IsFill()
        {
            return ( ! string.IsNullOrEmpty(Value));
        }

        public bool IsValid()
        {
            switch(Type)
            {
                case EMAIL:
                    return validateEmail(Value);
                    
                case TEXT:
                    return IsFill();
                    
                default: return true;
            }
        }
        private bool validateEmail(string email)
        {
            var match = Regex.Match(email, RegexConstants.Email, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                // check further on corporate mail
                foreach(string suff in NON_VALID_SUFF)
                {
                    if (email.Contains(suff)) return false;
                }
                return true;
            }
            else return false;
        }

    }
}