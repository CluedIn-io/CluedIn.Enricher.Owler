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
    public class OwlerPersonVocabulary : SimpleVocabulary
    {
        public OwlerPersonVocabulary()
        {
            this.VocabularyName = "Owler Person";
            this.KeyPrefix      = "owler.person";
            this.KeySeparator   = ".";
            this.Grouping       = EntityType.Person;

            this.AddGroup("Owler Metadata", group =>
            {
                this.FirstName = group.Add(new VocabularyKey("firstName"));
                this.LastName  = group.Add(new VocabularyKey("lastName"));
                this.Rating    = group.Add(new VocabularyKey("rating"));
                this.ImageUrl  = group.Add(new VocabularyKey("imageUrl", VocabularyKeyDataType.Uri, VocabularyKeyVisibility.Hidden));
            });

            this.AddMapping(this.FirstName, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.FirstName);
            this.AddMapping(this.LastName, CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.LastName);
        }

        public VocabularyKey FirstName { get; set; }

        public VocabularyKey LastName { get; set; }

        public VocabularyKey Rating { get; set; }

        public VocabularyKey ImageUrl { get; set; }
    }
}