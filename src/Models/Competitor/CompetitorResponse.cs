using System.Collections.Generic;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.Owler.Models.Competitor
{
	public class CompetitorResponse
	{

		[JsonProperty("competitor")]
		public List<Competitor> Competitors { get; set; }

		[JsonProperty("pagination_id")]
		public string PaginationId { get; set; }
	}
}