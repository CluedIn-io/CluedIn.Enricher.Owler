// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwlerVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the OwlerVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CluedIn.ExternalSearch.Providers.Owler.Vocabularies
{
    public static class OwlerVocabulary
    {        
        static OwlerVocabulary()
        {
            Organization = new OwlerOrganizationVocabulary();
            Person = new OwlerPersonVocabulary();
        }

        public static OwlerOrganizationVocabulary Organization { get; private set; }
        public static OwlerPersonVocabulary Person { get; private set; }
    }
}
