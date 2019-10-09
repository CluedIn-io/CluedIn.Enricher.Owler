using System.Collections.Generic;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.Owler.Models.Company
{
	public class Company
	{
		[JsonProperty("company_id")]
		public int? CompanyId { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("short_name")]
		public string ShortName { get; set; }

		[JsonProperty("website")]
		public string Website { get; set; }

		[JsonProperty("logo_url")]
		public string LogoUrl { get; set; }

		[JsonProperty("profile_url")]
		public string ProfileUrl { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("company_type")]
		public string CompanyType { get; set; }

		[JsonProperty("stock")]
		public Stock Stock { get; set; }

		[JsonProperty("perm_id")]
		public string PermId { get; set; }

		[JsonProperty("hq_address")]
		public HqAddress HqAddress { get; set; }

		[JsonProperty("founded_date")]
		public string FoundedDate { get; set; }

		[JsonProperty("revenue")]
		public string Revenue { get; set; }

		[JsonProperty("employee_count")]
		public string EmployeeCount { get; set; }

		[JsonProperty("industries")]
		public List<string> Industries { get; set; }

		[JsonProperty("ceo")]
		public Ceo Ceo { get; set; }

		[JsonProperty("funding")]
		public List<Funding> Funding { get; set; }

		[JsonProperty("acquisition")]
		public List<Acquisition> Acquisition { get; set; }

		[JsonProperty("facebook_link")]
		public string FacebookLink { get; set; }

		[JsonProperty("twitter_link")]
		public string TwitterLink { get; set; }

		[JsonProperty("youtube_link")]
		public string YoutubeLink { get; set; }

		[JsonProperty("linkedin_link")]
		public string LinkedinLink { get; set; }

		[JsonProperty("portfolio_company_ids")]
		public List<string> PortfolioCompanyIds { get; set; }

	}
}