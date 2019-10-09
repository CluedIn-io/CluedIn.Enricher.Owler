// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwlerTests.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the OwlerTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Messages.Processing;
using CluedIn.ExternalSearch.Providers.Owler;
using CluedIn.Testing.Base.ExternalSearch;
using Moq;
using Xunit;

namespace ExternalSearch.Owler.Integration.Tests
{
    public class OwlerTests : BaseExternalSearchTest<OwlerExternalSearchProvider>
    {
        // TODO Issue 170 - Test Failures
        //[Fact]
        //public void Test()
        //{
        //    // Arrange
        //    this._testContext = new TestContext();
        //    var properties = new EntityMetadataPart();
        //    properties.Properties.Add(Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website, "http://cluedin.com");
        //    properties.Properties.Add("Website", "http://cluedin.com");

        //    IEntityMetadata entityMetadata = new EntityMetadataPart()
        //    {
        //        Name = "CluedIn",
        //        EntityType = EntityType.Organization,
        //        Properties = properties.Properties
        //    };

        //    var externalSearchProvider = new Mock<OwlerExternalSearchProvider>(MockBehavior.Loose);
        //    var clues = new List<CompressedClue>();

        //    externalSearchProvider.CallBase = true;

        //    this._testContext.ProcessingHub.Setup(h => h.SendCommand(It.IsAny<ProcessClueCommand>())).Callback<IProcessingCommand>(c => clues.Add(((ProcessClueCommand)c).Clue));

        //    this._testContext.Container.Register(Component.For<IExternalSearchProvider>().UsingFactoryMethod(() => externalSearchProvider.Object));

        //    var context = this._testContext.Context.ToProcessingContext();
        //    var command = new ExternalSearchCommand();
        //    var actor = new ExternalSearchProcessing(context.ApplicationContext);
        //    var workflow = new Mock<Workflow>(MockBehavior.Loose, context, new EmptyWorkflowTemplate<ExternalSearchCommand>());

        //    workflow.CallBase = true;

        //    command.With(context);
        //    command.OrganizationId = context.Organization.Id;
        //    command.EntityMetaData = entityMetadata;
        //    command.Workflow = workflow.Object;
        //    context.Workflow = command.Workflow;

        //    // Act
        //    var result = actor.ProcessWorkflowStep(context, command);
        //    Assert.Equal(WorkflowStepResult.Repeat.SaveResult, result.SaveResult);

        //    result = actor.ProcessWorkflowStep(context, command);
        //    Assert.Equal(WorkflowStepResult.Success.SaveResult, result.SaveResult);
        //    context.Workflow.AddStepResult(result);

        //    context.Workflow.ProcessStepResult(context, command);

        //    // Assert
        //    this._testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.AtLeastOnce);

        //    Assert.True(clues.Count > 0);
        //}

        //[Theory]
        //[InlineData("CluedIn", "http://cluedin.com")]
        //[InlineData("Sitecore corporation", "http://sitecore.net")]
        //[InlineData("Pfa pension", "http://pfa.dk")]
        //public void TestClueProduction(string name, string website)
        //{
        //    var dummy = new DummyTokenProvider("13d3b4e999bdb9937936251cc345d443");
        //    object[] parameters = { dummy };

        //    var properties = new EntityMetadataPart();
        //    properties.Properties.Add(Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website, website);
        //    properties.Properties.Add("Website", website);

        //    IEntityMetadata entityMetadata = new EntityMetadataPart()
        //    {
        //        Name = name,
        //        EntityType = EntityType.Organization,
        //        Properties = properties.Properties
        //    };

        //    Setup(parameters, entityMetadata);

        //    testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.AtLeastOnce);
        //    Assert.True(clues.Count > 0);
        //}

        [Theory]
        [InlineData("asdasdasd", "http://asdasdasd.com")]
        [InlineData("", "http://asdasdasd.com")]
        [InlineData("asdasdasd", "")]
        [InlineData("", "")]
        [InlineData(null, "http://asdasdasd.com")]
        [InlineData("asdasdasd", null)]
        [InlineData(null, null)]
        [Trait("Category", "slow")]
        public void TestNoClueProduction(string name, string website)
        {
            var dummy = new DummyTokenProvider("13d3b4e999bdb9937936251cc345d443");
            object[] parameters = { dummy };

            var properties = new EntityMetadataPart();
            properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website, website);
            properties.Properties.Add("Website", website);

            IEntityMetadata entityMetadata = new EntityMetadataPart()
            {
                Name = name,
                EntityType = EntityType.Organization,
                Properties = properties.Properties
            };

            Setup(parameters, entityMetadata);

            testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.Never);
            Assert.True(clues.Count == 0);
        }

        [Theory]
        [InlineData("null")]
        [InlineData("empty")]
        [InlineData("nonWorking")]
        [Trait("Category", "slow")]
        public void TestInvalidApiToken(string provider)
        {
            var tokenProvider = GetProviderByName(provider);
            object[] parameters = { tokenProvider };

            var properties = new EntityMetadataPart();
            properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website, "http://cluedin.com");
            properties.Properties.Add("Website", "http://cluedin.com");

            IEntityMetadata entityMetadata = new EntityMetadataPart()
            {
                Name = "CluedIn",
                EntityType = EntityType.Organization,
                Properties = properties.Properties
            };

            Setup(parameters, entityMetadata);
            // Assert
            this.testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.Never);
            Assert.True(clues.Count == 0);
        }
    }
}