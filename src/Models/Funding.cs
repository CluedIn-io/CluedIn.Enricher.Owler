using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.Owler.Models
{
	public class Funding
	{

		[JsonProperty("date")]
		public DateTimeOffset? Date { get; set; }

		[JsonProperty("amount")]
		public string Amount { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("undisclosed")]
		public string Undisclosed { get; set; }

		[JsonProperty("investor")]
		public List<Investor> Investor { get; set; }
	}
}