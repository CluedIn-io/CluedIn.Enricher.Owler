using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.Owler.Models.Company
{
	public class Investor
	{

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("company_id")]
		public string CompanyId { get; set; }

		[JsonProperty("website")]
		public string Website { get; set; }

		[JsonProperty("source_company_id")]
		public int? SourceCompanyId { get; set; }
	}
}