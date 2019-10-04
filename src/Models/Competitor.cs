// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwlerCompetitorResponse.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the Competitor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.Owler.Models
{
    public class Competitor
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("score")]
        public int? Score { get; set; }

        [JsonProperty("company_id")]
        public int? CompanyId { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("logo_url")]
        public string LogoUrl { get; set; }

        [JsonProperty("profile_url")]
        public string ProfileUrl { get; set; }
    }
}
