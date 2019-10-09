// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwlerCompanyResponse.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the Ceo type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.Owler.Models.Company
{
    public class Stock
    {

        [JsonProperty("ticker")]
        public string Ticker { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }
    }
}
