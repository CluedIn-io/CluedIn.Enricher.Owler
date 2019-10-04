using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.Owler.Models
{
	public class Acquisition
	{

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("acquirer_company_id")]
		public string AcquirerCompanyId { get; set; }

		[JsonProperty("company_id")]
		public string CompanyId { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("date")]
		public string Date { get; set; }

		[JsonProperty("website")]
		public string Website { get; set; }

		[JsonProperty("amount")]
		public string Amount { get; set; }

		[JsonProperty("undisclosed")]
		public string Undisclosed { get; set; }
	}
}