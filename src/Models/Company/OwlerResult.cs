using CluedIn.ExternalSearch.Providers.Owler.Models.Competitor;

namespace CluedIn.ExternalSearch.Providers.Owler.Models.Company
{
    public class OwlerResult
    {
        public OwlerResult()
        {
        }

        public OwlerResult(Company company)
        {
            this.Company = company;
        }

        public OwlerResult(Company company, CompetitorResponse competitors)
        {
            this.Company = company;
            this.Competitors = competitors;
        }

        public Company Company { get; set; }

        public CompetitorResponse Competitors { get; set; }
    }
}
