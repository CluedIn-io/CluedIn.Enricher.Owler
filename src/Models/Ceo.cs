using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.Owler.Models
{
	public class Ceo
	{

		[JsonProperty("first_name")]
		public string FirstName { get; set; }

		[JsonProperty("last_name")]
		public string LastName { get; set; }

		[JsonProperty("image_url")]
		public string ImageUrl { get; set; }

		[JsonProperty("ceo_rating")]
		public string CeoRating { get; set; }
	}
}