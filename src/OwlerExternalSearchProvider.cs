// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwlerExternalSearchProvider.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the OwlerExternalSearchProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Processing;
using CluedIn.Crawling.Helpers;
using CluedIn.ExternalSearch.Providers.Owler.Models;
using CluedIn.ExternalSearch.Providers.Owler.Vocabularies;
using CluedIn.Core.ExternalSearch;
using CluedIn.ExternalSearch.Providers.Owler.Models.Company;
using CluedIn.ExternalSearch.Providers.Owler.Models.Competitor;
using DomainNameParser;
using RestSharp;

namespace CluedIn.ExternalSearch.Providers.Owler
{
    /// <summary>The owler external search provider.</summary>
    /// <seealso cref="CluedIn.ExternalSearch.ExternalSearchProviderBase" />
    public class OwlerExternalSearchProvider : OwlerExternalSearchProviderBase
    {
        /**********************************************************************************************************
         * FIELDS
         **********************************************************************************************************/

        private class TemporaryTokenProvider : IExternalSearchTokenProvider
        {
            public string ApiToken { get; private set; }

            public TemporaryTokenProvider(string token)
            {
                ApiToken = token;
            }
        }

        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="OwlerExternalSearchProvider" /> class.
        /// </summary>
        public OwlerExternalSearchProvider()
            : base(new Guid("8543ADF6-F397-4D37-AEA2-DD4CD953CEC8"), EntityType.Organization)
        {
            // TODO
            // Should use nameBasedTokenProvider, when token has been add to container.config
            TokenProvider           = new TemporaryTokenProvider("13d3b4e999bdb9937936251cc345d443");
            TokenProviderIsRequired = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OwlerExternalSearchProvider" /> class.
        /// </summary>
        public OwlerExternalSearchProvider([NotNull] Core.ExternalSearch.IExternalSearchTokenProvider tokenProvider)
            : base(new Guid("8543ADF6-F397-4D37-AEA2-DD4CD953CEC8"), EntityType.Organization)
        {
            TokenProvider           = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            TokenProviderIsRequired = true;
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        /// <summary>Builds the queries.</summary>
        /// <param name="context">The context.</param>
        /// <param name="request">The request.</param>
        /// <returns>The search queries.</returns>
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request)
        {
            if (!this.Accepts(request.EntityMetaData.EntityType))
                yield break;

            var existingResults = request.GetQueryResults<OwlerResult>(this).ToList();

            Func<int, bool>     idFilter     = value => existingResults.Any(r => r.Data.Company.CompanyId.HasValue && r.Data.Company.CompanyId.Value == value);
            Func<string, bool>  websiteFilter   = value => existingResults.Any(r => string.Equals(r.Data.Company.Website, value, StringComparison.InvariantCultureIgnoreCase));

            var entityType  = request.EntityMetaData.EntityType;
            var id          = request.QueryParameters.GetValue(OwlerVocabulary.Organization.Id); 
            var website     = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website, new HashSet<string>());

            if (id != null && id.Any())
            {
                int dummy;

                var values = id.Where(v => v != null && int.TryParse(v, out dummy)).Select(int.Parse);

                foreach (var value in values.Where(v => !idFilter(v)))
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Identifier, value.ToString());
            }
            else if (website != null)
            {
                var values = website.Where(UriUtility.IsValid)
                                    .Select(v =>
                                    {
                                        DomainName domain;
                                        if (DomainName.TryParse(new Uri(v).Host, out domain))
                                            return domain.Domain + "." + domain.TLD;

                                        return null;
                                    })
                                    .Where(v => v != null);

                foreach (var value in values.Where(v => !websiteFilter(v)))
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Uri, value);
            }
        }

        /// <summary>Executes the search.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <returns>The results.</returns>
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
        {
            var website = query.QueryParameters.GetValue<string, HashSet<string>>(ExternalSearchQueryParameter.Uri.ToString(), new HashSet<string>()).FirstOrDefault();
            var id      = query.QueryParameters.GetValue<string, HashSet<string>>(ExternalSearchQueryParameter.Identifier.ToString(), new HashSet<string>()).FirstOrDefault();

            if (string.IsNullOrEmpty(website) && string.IsNullOrEmpty(id))
                yield break;

            // HACK!!!
            System.Threading.Thread.Sleep(5 * 1000);

            var isPremium   = true; //  this.CheckPremium(context, ApiToken);
            var baseUrl     = isPremium ? "https://api.owler.com/v1/companypremium/" : "https://api.owler.com/v1/company/";

            var client = new RestClient(baseUrl);

            IRestResponse<Company> response = null;

            if (!string.IsNullOrEmpty(website))
            {
                var request = new RestRequest("url/" + website + "?format=json", Method.GET);
                request.AddHeader("user_key", TokenProvider.ApiToken);
                request.AddHeader("Content-Type", "application/json");

                response = client.ExecuteTaskAsync<Company>(request).Result;
            }

            if (!string.IsNullOrEmpty(id))
            {
                var request = new RestRequest("id/" + id + "?format=json", Method.GET);
                request.AddHeader("user_key", TokenProvider.ApiToken);
                request.AddHeader("Content-Type", "application/json");

                response = client.ExecuteTaskAsync<Company>(request).Result;
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var company = response.Data;
                yield return new ExternalSearchQueryResult<OwlerResult>(query, new OwlerResult(company));
            }
            else if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                yield break;
            else if (response.StatusCode == (HttpStatusCode)429)
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; Content: " + response.Content);
            else if (response.ErrorException != null)
                throw new AggregateException(response.ErrorException.Message, response.ErrorException);
            else
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; Content: " + response.Content);
        }

        /// <summary>Builds the clues.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The clues.</returns>
        [CanBeNull]
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<OwlerResult>();

            if (resultItem.Data.Company.CompanyId == null)
                yield break;

            var recurse             = !request.NoRecursion.HasValue || !request.NoRecursion.Value;
            var processingContext   = (ProcessingContext)context;

            // Organization
            {
                var code = this.GetOriginEntityCode(resultItem);

                var clue = new Clue(code, context.Organization);

                this.PopulateMetadata(clue.Data.EntityData, resultItem, recurse, request.CustomQueryInput);

                if (resultItem.Data.Company.LogoUrl != null)
                {
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    headers.Add("user_key", TokenProvider.ApiToken);

                    this.DownloadPreviewImage(context, resultItem.Data.Company.LogoUrl, clue, headers);
                }

                yield return clue;
            }

            // Ceo
            if (resultItem.Data.Company.Ceo != null)
            {
                var code = this.GetOriginEntityCode(resultItem.Data.Company.Ceo);

                var clue = new Clue(code, context.Organization);

                this.PopulateCeoMetadata(clue.Data.EntityData, resultItem.Data.Company.Ceo, resultItem.Data.Company);

                if (resultItem.Data.Company.Ceo.ImageUrl != null)
                {
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    headers.Add("user_key", TokenProvider.ApiToken);

                    this.DownloadPreviewImage(context, resultItem.Data.Company.Ceo.ImageUrl, clue, headers);
                }

                yield return clue;
            }

            if (recurse)
            {
                // Competitors
                {
                    var metadata = new EntityMetadataPart();
                    this.PopulateSearchCompetitorMetadata(metadata, resultItem.Data.Company);

                    processingContext.Workflow.StartSubWorkflow(() => new ExternalSearchCommand(processingContext, Guid.NewGuid(), metadata) { CustomQueryInput = resultItem.Data.Company, NoRecursion = false, ProviderIds = new List<Guid>() { new Guid("BD313DD0-B7EB-407E-8E45-2A1C06EEEAAC") } });
                }

                // Acquisitions
                if (resultItem.Data.Company.Acquisition != null)
                {
                    foreach (var acquisition in resultItem.Data.Company.Acquisition.Where(a => a.CompanyId != null))
                    {
                        var metadata = new EntityMetadataPart();

                        this.PopulareSearchAcquisitionMetadata(metadata, acquisition, resultItem.Data.Company);

                        processingContext.Workflow.StartSubWorkflow(() => new ExternalSearchCommand(processingContext, Guid.NewGuid(), metadata) { CustomQueryInput = acquisition, NoRecursion = true });
                    }
                }

                // Investors
                if (resultItem.Data.Company.Funding != null)
                {
                    var investorOrganizations = resultItem.Data.Company.Funding
                                                    .Where(f => f.Investor != null)
                                                    .SelectMany(f => f.Investor)
                                                    .Where(i => !string.IsNullOrEmpty(i.CompanyId));

                    foreach (var investor in investorOrganizations)
                    {
                        var metadata = new EntityMetadataPart();

                        this.PopulateSearchInvestorMetadata(metadata, investor, resultItem.Data.Company);

                        investor.SourceCompanyId = resultItem.Data.Company.CompanyId;

                        processingContext.Workflow.StartSubWorkflow(() => new ExternalSearchCommand(processingContext, Guid.NewGuid(), metadata) { CustomQueryInput = investor, NoRecursion = true });
                    }
                }
            }
        }

        /// <summary>Gets the primary entity metadata.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The primary entity metadata.</returns>
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<OwlerResult>();
            return this.CreateMetadata(resultItem);
        }

        /// <summary>Gets the preview image.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The preview image.</returns>
        public override IPreviewImage GetPrimaryEntityPreviewImage(
            ExecutionContext context,
            IExternalSearchQueryResult result,
            IExternalSearchRequest request)
        {
            var data = result.As<OwlerResult>().Data;

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("user_key", TokenProvider.ApiToken);

            return this.DownloadPreviewImageBlob(context, data.Company.LogoUrl, headers);
        }

        /// <summary>Creates the metadata.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The metadata.</returns>
        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<OwlerResult> resultItem)
        {
            var metadata = new EntityMetadataPart();

            this.PopulateMetadata(metadata, resultItem, false, null);

            return metadata;
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(IExternalSearchQueryResult<OwlerResult> resultItem)
        {
            return this.GetOriginEntityCode(resultItem.Data.Company);
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="company">The company.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(Company company)
        {
            return new EntityCode(EntityType.Organization, this.GetCodeOrigin(), company.CompanyId.ToString());
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="competitor">The competitor.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(Competitor competitor)
        {
            return new EntityCode(EntityType.Organization, this.GetCodeOrigin(), competitor.CompanyId.ToString());
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="acquisition">The acquisition.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(Acquisition acquisition)
        {
            return new EntityCode(EntityType.Organization, this.GetCodeOrigin(), acquisition.CompanyId.ToString());
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="investor">The investor.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(Investor investor)
        {
            return new EntityCode(EntityType.Organization, this.GetCodeOrigin(), investor.CompanyId.ToString());
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="ceo">The ceo.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(Ceo ceo)
        {
            return new EntityCode(EntityType.Person, this.GetCodeOrigin(), string.Format("{0} {1}", ceo.FirstName, ceo.LastName));
        }

        /// <summary>Gets the code origin.</summary>
        /// <returns>The code origin</returns>
        private CodeOrigin GetCodeOrigin()
        {
            return CodeOrigin.CluedIn.CreateSpecific("owler");
        }

        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="resultItem">The result item.</param>
        /// <param name="recurse">if set to <c>true</c> then create edges.</param>
        /// <param name="customQueryInput">The custom query input.</param>
        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<OwlerResult> resultItem, bool recurse, object customQueryInput)
        {
            var code = this.GetOriginEntityCode(resultItem);

            metadata.OriginEntityCode = code;
            metadata.EntityType       = EntityType.Organization;
            metadata.Name             = resultItem.Data.Company.Name;
            metadata.Description      = resultItem.Data.Company.Description;

            if (resultItem.Data.Company.ProfileUrl != null && UriUtility.IsValid(resultItem.Data.Company.ProfileUrl))
                metadata.Uri = new Uri(resultItem.Data.Company.ProfileUrl);

            if (resultItem.Data.Company.ShortName != null)
                metadata.Aliases.Add(resultItem.Data.Company.ShortName);

            metadata.Codes.Add(code);

            var street = resultItem.Data.Company.HqAddress != null ? string.Join(Environment.NewLine, new string[] { resultItem.Data.Company.HqAddress.Street1 ?? string.Empty, resultItem.Data.Company.HqAddress.Street2 ?? string.Empty }).Trim() : null;

            metadata.Properties[OwlerVocabulary.Organization.Id]            = resultItem.Data.Company.CompanyId.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.CompanyType]   = resultItem.Data.Company.CompanyType.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.Description]   = resultItem.Data.Company.Description.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.EmployeeCount] = resultItem.Data.Company.EmployeeCount.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.FacebookLink]  = resultItem.Data.Company.FacebookLink.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.FoundedDate]   = resultItem.Data.Company.FoundedDate.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.LinkedinLink]  = resultItem.Data.Company.LinkedinLink.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.LogoUrl]       = resultItem.Data.Company.LogoUrl.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.ProfileUrl]    = resultItem.Data.Company.ProfileUrl.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.Revenue]       = resultItem.Data.Company.Revenue.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.ShortName]     = resultItem.Data.Company.ShortName.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.TwitterLink]   = resultItem.Data.Company.TwitterLink.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.Industries]    = resultItem.Data.Company.Industries.PrintIfAvailable(v => v, v => string.Join(";", v));
            metadata.Properties[OwlerVocabulary.Organization.Website]       = resultItem.Data.Company.Website.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.Street]        = street;
            metadata.Properties[OwlerVocabulary.Organization.Street1]       = resultItem.Data.Company.HqAddress.PrintIfAvailable(v => v.Street1);
            metadata.Properties[OwlerVocabulary.Organization.Street2]       = resultItem.Data.Company.HqAddress.PrintIfAvailable(v => v.Street2);
            metadata.Properties[OwlerVocabulary.Organization.City]          = resultItem.Data.Company.HqAddress.PrintIfAvailable(v => v.City);
            metadata.Properties[OwlerVocabulary.Organization.Country]       = resultItem.Data.Company.HqAddress.PrintIfAvailable(v => v.Country);
            metadata.Properties[OwlerVocabulary.Organization.Phone]         = resultItem.Data.Company.HqAddress.PrintIfAvailable(v => v.Phone);
            metadata.Properties[OwlerVocabulary.Organization.PostalCode]    = resultItem.Data.Company.HqAddress.PrintIfAvailable(v => v.PostalCode);
            metadata.Properties[OwlerVocabulary.Organization.Ticker]        = resultItem.Data.Company.Stock.PrintIfAvailable(s => s.Ticker);
            metadata.Properties[OwlerVocabulary.Organization.Exchange]      = resultItem.Data.Company.Stock.PrintIfAvailable(s => s.Exchange);

            // Latest Funding Round
            if (resultItem.Data.Company.Funding != null)
            {
                var lastestFundingRound = resultItem.Data.Company.Funding
                                            .Where(f => f.Date.HasValue)
                                            .OrderByDescending(f => f.Date.Value)
                                            .FirstOrDefault();

                if (lastestFundingRound != null)
                {
                    metadata.Properties[OwlerVocabulary.Organization.LastestFundingAmount]      = lastestFundingRound.Amount.PrintIfAvailable();
                    metadata.Properties[OwlerVocabulary.Organization.LastestFundingDate]        = lastestFundingRound.Date.PrintIfAvailable();
                    metadata.Properties[OwlerVocabulary.Organization.LastestFundingType]        = lastestFundingRound.Type.PrintIfAvailable();
                    metadata.Properties[OwlerVocabulary.Organization.LastestFundingUndisclosed] = lastestFundingRound.Undisclosed.PrintIfAvailable();
                    
                }
            }

            // Acquisition
            {
                var acquisition = customQueryInput as Acquisition;
                if (acquisition != null)
                {
                    metadata.Properties[OwlerVocabulary.Organization.AcquisitionAmount]         = acquisition.Amount.PrintIfAvailable();
                    metadata.Properties[OwlerVocabulary.Organization.AcquisitionUndisclosed]    = acquisition.Undisclosed.PrintIfAvailable();
                    metadata.Properties[OwlerVocabulary.Organization.AcquisitionStatus]         = acquisition.Status.PrintIfAvailable();
                    metadata.Properties[OwlerVocabulary.Organization.AcquisitionDate]           = acquisition.Date.PrintIfAvailable();
                }
            }

            // Investor
            {
                var investor = customQueryInput as Investor;
                if (investor != null)
                {
                    var from    = new EntityReference(code); 
                    var to      = new EntityReference(new EntityCode(EntityType.Organization, this.GetCodeOrigin(), investor.SourceCompanyId.ToString()));
                    var edge    = new EntityEdge(from, to, EntityEdgeType.Investor);
                    metadata.OutgoingEdges.Add(edge);
                }
            }

            if (recurse)
            {
                if (resultItem.Data.Company.Acquisition != null && resultItem.Data.Company.Acquisition != null)
                {
                    foreach (var acquisition in resultItem.Data.Company.Acquisition.Where(a => a.CompanyId != null))
                    {
                        var from    = new EntityReference(code);
                        var to      = new EntityReference(this.GetOriginEntityCode(acquisition), acquisition.Name);
                        var edge    = new EntityEdge(from, to, EntityEdgeType.Owns);
                        metadata.OutgoingEdges.Add(edge);
                    }
                }
            }
        }

        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="company">The company.</param>
        private void PopulateSearchCompetitorMetadata(IEntityMetadata metadata, Company company)
        {
            var code = this.GetOriginEntityCode(company);
            metadata.Codes.Add(code);

            metadata.EntityType         = EntityType.Organization;
            metadata.Name               = company.Name;
            metadata.OriginEntityCode   = code;

            metadata.Properties[OwlerVocabulary.Organization.Id]            = company.CompanyId.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.Name]          = company.Name.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.ShortName]     = company.ShortName.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.Website]       = company.Website.PrintIfAvailable();
        }

        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="company">The company.</param>
        /// <param name="acquisition">The acquirer.</param>
        private void PopulareSearchAcquisitionMetadata(IEntityMetadata metadata, Acquisition acquisition, Company company)
        {
            var code = this.GetOriginEntityCode(acquisition);

            metadata.EntityType       = EntityType.Organization;
            metadata.Name             = acquisition.Name;
            metadata.OriginEntityCode = code;

            metadata.Codes.Add(code);

            metadata.Properties[OwlerVocabulary.Organization.Id]                        = acquisition.CompanyId.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.Name]                      = acquisition.Name.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.Website]                   = acquisition.Website.PrintIfAvailable();

            metadata.Properties[OwlerVocabulary.Organization.AcquisitionAmount]         = acquisition.Amount.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.AcquisitionUndisclosed]    = acquisition.Undisclosed.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.AcquisitionStatus]         = acquisition.Status.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.AcquisitionDate]           = acquisition.Date.PrintIfAvailable();
        }

        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="company">The company.</param>
        /// <param name="investor">The investor.</param>
        private void PopulateSearchInvestorMetadata(IEntityMetadata metadata, Investor investor, Company company)
        {
            var code = this.GetOriginEntityCode(investor);

            metadata.EntityType       = EntityType.Organization;
            metadata.OriginEntityCode = code;
            metadata.Name             = investor.Name;

            metadata.Codes.Add(code);

            metadata.Properties[OwlerVocabulary.Organization.Id]        = investor.CompanyId.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.Name]      = investor.Name.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.Website]   = investor.Website.PrintIfAvailable();
        }

        private void PopulateCeoMetadata(IEntityMetadata metadata, Ceo ceo, Company company)
        {
            var code = this.GetOriginEntityCode(ceo);

            metadata.EntityType       = EntityType.Person;
            metadata.OriginEntityCode = code;
            metadata.Name             = string.Format("{0} {1}", ceo.FirstName, ceo.LastName);

            metadata.Codes.Add(code);

            metadata.Properties[OwlerVocabulary.Person.FirstName]   = ceo.FirstName.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Person.LastName]    = ceo.LastName.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Person.Rating]      = ceo.CeoRating.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Person.ImageUrl]    = ceo.ImageUrl.PrintIfAvailable();

            var from   = new EntityReference(code);
            var to     = new EntityReference(this.GetOriginEntityCode(company));
            var edge   = new EntityEdge(from, to, EntityEdgeType.WorksFor);
            metadata.OutgoingEdges.Add(edge);
        }
    }
}