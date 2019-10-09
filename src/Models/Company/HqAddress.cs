using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.Owler.Models.Company
{
	public class HqAddress
	{

		[JsonProperty("street1")]
		public string Street1 { get; set; }

		[JsonProperty("street2")]
		public string Street2 { get; set; }

		[JsonProperty("city")]
		public string City { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		[JsonProperty("country")]
		public string Country { get; set; }

		[JsonProperty("phone")]
		public string Phone { get; set; }

		[JsonProperty("postal_code")]
		public string PostalCode { get; set; }
	}
}