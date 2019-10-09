// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwlerExternalSearchProviderBase.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the OwlerExternalSearchProviderBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.ExternalSearch.Providers.Owler.Models;
using CluedIn.ExternalSearch.Providers.Owler.Models.Competitor;
using RestSharp;

namespace CluedIn.ExternalSearch.Providers.Owler
{
    /// <summary>The owler external search provider base.</summary>
    /// <seealso cref="CluedIn.ExternalSearch.ExternalSearchProviderBase" />
    public abstract class OwlerExternalSearchProviderBase : ExternalSearchProviderBase
    {
        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        protected OwlerExternalSearchProviderBase(Guid id, params EntityType[] entityTypes)
            : base(id, entityTypes)
        {
        }

        protected OwlerExternalSearchProviderBase(ExternalSearchProviderPriority priority, Guid id, params EntityType[] entityTypes)
            : base(priority, id, entityTypes)
        {
        }

        protected OwlerExternalSearchProviderBase(int priority, Guid id, params EntityType[] entityTypes)
            : base(priority, id, entityTypes)
        {
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        protected bool CheckPremium(ExecutionContext context, string apiKey)// TODO: This should be executed only once(thread lock)
        {
            return context.ApplicationContext.System.Cache.GetItem<bool>(apiKey, () =>
            {
                var client = new RestClient("https://api.owler.com/v1/");
                var request = new RestRequest("companypremium/url/CluedIn.com", Method.GET);
                request.AddHeader("user_key", apiKey);
                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                    return true;

                return false;
            },
            true,
            policy => policy.WithAbsoluteExpiration(DateTimeOffset.UtcNow.AddHours(4)));
        }

        protected IEnumerable<Competitor> FilterCompetitors(List<Competitor> competitors)
        {
            return competitors.Where(c => c.Score >= 10000)
                .OrderByDescending(c => c.Score)
                .Take(10);

            //return competitors.Where(c => c.Score >= 10000)
            //                  .OrderByDescending(c => c.Score);
        }
    }
}