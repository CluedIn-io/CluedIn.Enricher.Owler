// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwlerCompetitorsExternalSearchProvider.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the OwlerCompetitorsExternalSearchProvider type.
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
using CluedIn.ExternalSearch.Providers.Owler.Models.Competitor;
using CluedIn.ExternalSearch.Providers.Owler.Vocabularies;

using RestSharp;

namespace CluedIn.ExternalSearch.Providers.Owler
{
    /// <summary>The owler competitors external search provider.</summary>
    /// <seealso cref="CluedIn.ExternalSearch.Providers.Owler.OwlerExternalSearchProviderBase" />
    /// <seealso cref="CluedIn.ExternalSearch.ExternalSearchProviderBase" />
    public class OwlerCompetitorsExternalSearchProvider : OwlerExternalSearchProviderBase
    {
        /**********************************************************************************************************
         * FIELDS
         **********************************************************************************************************/

        public string ApiToken = "13d3b4e999bdb9937936251cc345d443"; // TODO: Get this from configuration.

        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="OwlerCompetitorsExternalSearchProvider"/> class.
        /// </summary>
        public OwlerCompetitorsExternalSearchProvider()
            : base(new Guid("BD313DD0-B7EB-407E-8E45-2A1C06EEEAAC"), EntityType.Organization)
        {
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

            if (request.NoRecursion.HasValue && request.NoRecursion.Value)
                yield break;

            var existingResults = request.GetQueryResults<OwlerCompetitorResult>(this).ToList();

            Func<int, bool> idFilter = value => existingResults.Any(r => r.Data.CompanyId == value);

            var entityType = request.EntityMetaData.EntityType;
            var id      = request.QueryParameters.GetValue(OwlerVocabulary.Organization.Id); 

            if (id != null && id.Any())
            {
                int dummy;

                var values = id.Where(v => v != null && int.TryParse(v, out dummy)).Select(int.Parse);

                foreach (var value in values.Where(v => !idFilter(v)))
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Identifier, value.ToString());
            }
        }

        /// <summary>Executes the search.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <returns>The results.</returns>
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
        {
            var id = query.QueryParameters.GetValue<string, HashSet<string>>(ExternalSearchQueryParameter.Identifier.ToString(), new HashSet<string>()).FirstOrDefault();

            if (string.IsNullOrEmpty(id))
                yield break;

            var isPremium = true; //  this.CheckPremium(context, ApiToken);

            var header = isPremium ? "competitorpremium/id/" : "competitor/id/";
            var client  = new RestClient("https://api.owler.com/v1/company/");
            var request = new RestRequest(header + id + "?format=json", Method.GET);
            request.AddHeader("user_key", ApiToken);
            request.AddHeader("Content-Type", "application/json");

            var response = client.ExecuteTaskAsync<CompetitorResponse>(request).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var competitors = response.Data;

                if (competitors.Competitors == null)
                    throw new ApplicationException("Could not execute external search query - Competitor list empty.");

                yield return new ExternalSearchQueryResult<OwlerCompetitorResult>(query, new OwlerCompetitorResult(id, competitors));
            }
            else if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
                yield break;
            else if (response.StatusCode == (HttpStatusCode)429)
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; Content: " + response.Content);
            else if (response.ErrorException != null)
                throw new AggregateException(response.ErrorException.Message, response.ErrorException);
            else
                throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode);
                //throw new ApplicationException("Could not execute external search query - StatusCode:" + response.StatusCode + "; Content: " + response.Content);
    }

        /// <summary>Builds the clues.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The clues.</returns>
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var processingContext   = (ProcessingContext)context;
            var recurse             = !request.NoRecursion.HasValue || !request.NoRecursion.Value;

            if (!recurse)
                return new Clue[0];

            var resultItem = result.As<OwlerCompetitorResult>();

            var competitors = this.FilterCompetitors(resultItem.Data.Competitors.Competitors);

            foreach (var competitor in competitors)
            {
                var metadata = new EntityMetadataPart();

                this.PopulateSearchCompetitorMetadata(metadata, resultItem.Data.CompanyId, competitor);

                processingContext.Workflow.StartSubWorkflow(() => new ExternalSearchCommand(processingContext, Guid.NewGuid(), metadata) { CustomQueryInput = competitor, NoRecursion = true });
            }

            var code = this.GetOriginEntityCode(resultItem);
            var clue = new Clue(code, context.Organization);

            this.PopulateMetadata(clue.Data.EntityData, resultItem);

            return new Clue[] { clue };
        }

        /// <summary>Gets the primary entity metadata.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The primary entity metadata.</returns>
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem  = result.As<OwlerCompetitorResult>();
            var metadata    = new EntityMetadataPart();

            this.PopulateMetadata(metadata, resultItem);

            return metadata;
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
            return null;
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The origin entity code.</returns>
        private IEntityCode GetOriginEntityCode(IExternalSearchQueryResult<OwlerCompetitorResult> resultItem)
        {
            return new EntityCode(EntityType.Organization, this.GetCodeOrigin(), resultItem.Data.CompanyId.ToString());
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(Competitor resultItem)
        {
            return new EntityCode(EntityType.Organization, this.GetCodeOrigin(), resultItem.CompanyId.ToString());
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
        private void PopulateMetadata(IEntityMetadataPart metadata, IExternalSearchQueryResult<OwlerCompetitorResult> resultItem)
        {
            var code = this.GetOriginEntityCode(resultItem);

            metadata.OriginEntityCode = code;
            metadata.EntityType       = EntityType.Organization;

            metadata.Codes.Add(code);

            metadata.Properties[OwlerVocabulary.Organization.Id] = resultItem.Data.CompanyId.PrintIfAvailable();

            if (resultItem.Data.Competitors != null && resultItem.Data.Competitors.Competitors != null)
            {
                foreach (var competitor in this.FilterCompetitors(resultItem.Data.Competitors.Competitors))
                {
                    var from    = new EntityReference(code);
                    var to      = new EntityReference(this.GetOriginEntityCode(competitor), competitor.Name ?? competitor.ShortName);
                    var edge    = new EntityEdge(from, to, EntityEdgeType.Competitor);
                    metadata.OutgoingEdges.Add(edge);
                }
            }
        }

        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="competitor">The competitor.</param>
        private void PopulateSearchCompetitorMetadata(IEntityMetadata metadata, int companyId, Competitor competitor)
        {
            var code = this.GetOriginEntityCode(competitor);
            metadata.Codes.Add(code);

            metadata.EntityType         = EntityType.Organization;
            metadata.Name               = competitor.Name;
            metadata.OriginEntityCode   = code;

            if (competitor.ProfileUrl != null && UriUtility.IsValid(competitor.ProfileUrl))
                metadata.Uri = new Uri(competitor.ProfileUrl);

            if (competitor.ShortName != null)
                metadata.Aliases.Add(competitor.ShortName);

            metadata.Properties[OwlerVocabulary.Organization.Id]            = competitor.CompanyId.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.Name]          = competitor.Name.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.ShortName]     = competitor.ShortName.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.LogoUrl]       = competitor.LogoUrl.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.ProfileUrl]    = competitor.ProfileUrl.PrintIfAvailable();
            metadata.Properties[OwlerVocabulary.Organization.Website]       = competitor.Website.PrintIfAvailable();
        }
    }
}