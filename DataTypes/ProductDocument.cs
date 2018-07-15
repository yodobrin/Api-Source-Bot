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
using Newtonsoft.Json;

namespace LuisBot.DataTypes
{
    public class ProductDocument
    {
        [JsonProperty("Molecule (Level 1) ID")]
        public string MoleculeID { get; set; }
        [JsonProperty("Molecule Name (Level 1)")]
        public string MoleculeName { get; set; }
        [JsonProperty("Tapi Product Name (Level 2)")]
        public string TapiProductName { get; set; }
        [JsonProperty("Status (Calculated)")]
        public string Status { get; set; }
        [JsonProperty("Sub Status (Calculated)")]
        public string SubStatus { get; set; }
        [JsonProperty("ATC 1")]
        public string ATC { get; set; }

        [JsonConstructor]
        public ProductDocument(string moleculeID, string moleculeName, string tapiProductName, string status, string subStatus, string aTC)
        {
            MoleculeID = moleculeID;
            MoleculeName = moleculeName;
            TapiProductName = tapiProductName;
            Status = status;
            SubStatus = subStatus;
            ATC = aTC;
        }
    }
}