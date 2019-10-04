namespace CluedIn.ExternalSearch.Providers.Owler.Models
{
	public class OwlerCompetitorResult
	{
		public OwlerCompetitorResult()
		{
		}

		public OwlerCompetitorResult(string id, CompetitorResponse competitors)
		{
			this.CompanyId      = int.Parse(id);
			this.Competitors    = competitors;
		}

		public int CompanyId { get; set; }

		public CompetitorResponse Competitors { get; set; }
	}
}