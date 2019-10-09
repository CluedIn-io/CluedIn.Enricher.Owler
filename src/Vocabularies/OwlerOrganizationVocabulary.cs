// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwlerOrganizationVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the OwlerOrganizationVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.Owler.Vocabularies
{
    public class OwlerOrganizationVocabulary : SimpleVocabulary
    {
        public OwlerOrganizationVocabulary()
        {
            this.VocabularyName = "Owler Organization";
            this.KeyPrefix      = "owler.organization";
            this.KeySeparator   = ".";
            this.Grouping       = EntityType.Organization;

            this.AddGroup("Owler Metadata", group =>
            {
                this.Id             = group.Add(new VocabularyKey("id",                                                                                 VocabularyKeyVisibility.Hidden));
                this.Name           = group.Add(new VocabularyKey("name"));
                this.ShortName      = group.Add(new VocabularyKey("shortName"));
                this.CompanyType    = group.Add(new VocabularyKey("companyType"));
                this.Description    = group.Add(new VocabularyKey("description"));
                this.EmployeeCount  = group.Add(new VocabularyKey("employeeCount"));
                this.FoundedDate    = group.Add(new VocabularyKey("foundedDate"));
                this.Revenue        = group.Add(new VocabularyKey("revenue"));
                this.Industries     = group.Add(new VocabularyKey("industries"));
                this.Website        = group.Add(new VocabularyKey("website",                                VocabularyKeyDataType.Uri));
                this.ProfileUrl     = group.Add(new VocabularyKey("profileUrl",                             VocabularyKeyDataType.Uri));
                this.FacebookLink   = group.Add(new VocabularyKey("facebookLink",                           VocabularyKeyDataType.Uri));
                this.LinkedinLink   = group.Add(new VocabularyKey("linkedinLink",                           VocabularyKeyDataType.Uri));
                this.TwitterLink    = group.Add(new VocabularyKey("twitterLink",                            VocabularyKeyDataType.Uri));
                this.YoutubeLink    = group.Add(new VocabularyKey("youtubeLink",                            VocabularyKeyDataType.Uri));
                this.LogoUrl        = group.Add(new VocabularyKey("logoUrl",                                VocabularyKeyDataType.Uri,                  VocabularyKeyVisibility.Hidden));
            });

            this.AddGroup("Owler Address", group =>
            {
                this.Street     = group.Add(new VocabularyKey("street"));
                this.Street1    = group.Add(new VocabularyKey("street1",                                                                                VocabularyKeyVisibility.Hidden));
                this.Street2    = group.Add(new VocabularyKey("street2",                                                                                VocabularyKeyVisibility.Hidden));
                this.City       = group.Add(new VocabularyKey("city",                                       VocabularyKeyDataType.GeographyCity));
                this.State      = group.Add(new VocabularyKey("state"));
                this.PostalCode = group.Add(new VocabularyKey("postalCode"));
                this.Country    = group.Add(new VocabularyKey("country",                                    VocabularyKeyDataType.GeographyCountry));
                this.Phone      = group.Add(new VocabularyKey("phone",                                      VocabularyKeyDataType.PhoneNumber));
            });

            this.AddGroup("Owler Latest Funding", group =>
            {
                this.LastestFundingDate        = group.Add(new VocabularyKey("lastestFundingDate",          VocabularyKeyDataType.DateTime));
                this.LastestFundingAmount      = group.Add(new VocabularyKey("lastestFundingAmount",        VocabularyKeyDataType.Money));
                this.LastestFundingType        = group.Add(new VocabularyKey("lastestFundingType"));
                this.LastestFundingUndisclosed = group.Add(new VocabularyKey("lastestFundingUndisclosed"));
            });

            this.AddGroup("Owler Acquisition", group =>
            {
                this.AcquisitionAmount      = group.Add(new VocabularyKey("acquisitionAmount",              VocabularyKeyDataType.Money));
                this.AcquisitionUndisclosed = group.Add(new VocabularyKey("acquisitionUndisclosed",         VocabularyKeyDataType.Boolean));
                this.AcquisitionStatus      = group.Add(new VocabularyKey("acquisitionStatus"));
                this.AcquisitionDate        = group.Add(new VocabularyKey("acquisitionDate",                VocabularyKeyDataType.DateTime));
            });

            this.AddGroup("Owler Stock", group =>
            {
                this.Ticker   = group.Add(new VocabularyKey("ticker"));
                this.Exchange = group.Add(new VocabularyKey("exchange"));
            });

            this.AddMapping(this.Name, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.OrganizationName);
            this.AddMapping(this.Website, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website);
            this.AddMapping(this.EmployeeCount, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.EmployeeCount);
            this.AddMapping(this.FoundedDate, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.FoundingDate);
            this.AddMapping(this.Revenue, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.AnnualRevenue);
            this.AddMapping(this.Industries, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Industry);

            this.AddMapping(this.FacebookLink, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Social.Facebook);
            this.AddMapping(this.LinkedinLink, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Social.LinkedIn);
            this.AddMapping(this.TwitterLink, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Social.Twitter);
            this.AddMapping(this.YoutubeLink, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Social.YouTube);

            this.AddMapping(this.Phone, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.PhoneNumber);
            this.AddMapping(this.PostalCode, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.AddressZipCode);
            this.AddMapping(this.Street, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.AddressStreetName);
            this.AddMapping(this.Country, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.AddressCountryCode);
            this.AddMapping(this.State, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.AddressState);

            this.AddMapping(this.Ticker, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.TickerSymbol);
        }

        public VocabularyKey Id { get; set; }

        public VocabularyKey Industries { get; set; }

        public VocabularyKey TwitterLink { get; set; }

        public VocabularyKey Revenue { get; set; }

        public VocabularyKey LogoUrl { get; set; }

        public VocabularyKey LinkedinLink { get; set; }

        public VocabularyKey FoundedDate { get; set; }

        public VocabularyKey FacebookLink { get; set; }

        public VocabularyKey YoutubeLink { get; set; }

        public VocabularyKey EmployeeCount { get; set; }

        public VocabularyKey Description { get; set; }

        public VocabularyKey CompanyType { get; set; }

        public VocabularyKey Name { get; set; }
        public VocabularyKey ShortName { get; set; }

        public VocabularyKey Website { get; set; }
        public VocabularyKey ProfileUrl { get; set; }

        public VocabularyKey Street { get; set; }
        public VocabularyKey Street1 { get; set; }
        public VocabularyKey Street2 { get; set; }
        public VocabularyKey City { get; set; }
        public VocabularyKey State { get; set; }
        public VocabularyKey Country { get; set; }
        public VocabularyKey Phone { get; set; }
        public VocabularyKey PostalCode { get; set; }

        public VocabularyKey AcquisitionAmount { get; set; }
        public VocabularyKey AcquisitionUndisclosed { get; set; }
        public VocabularyKey AcquisitionStatus { get; set; }
        public VocabularyKey AcquisitionDate { get; set; }

        public VocabularyKey Ticker { get; set; }
        public VocabularyKey Exchange { get; set; }

        public VocabularyKey LastestFundingAmount { get; set; }
        public VocabularyKey LastestFundingDate { get; set; }
        public VocabularyKey LastestFundingType { get; set; }
        public VocabularyKey LastestFundingUndisclosed { get; set; }
    }
}