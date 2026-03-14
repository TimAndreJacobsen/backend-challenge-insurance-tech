using Xunit;

namespace Claims.Tests.Fixtures;

[CollectionDefinition("ClaimsIntegration")]
public class ClaimsIntegrationCollection : ICollectionFixture<ClaimsApiFactory>;

[CollectionDefinition("CoversIntegration")]
public class CoversIntegrationCollection : ICollectionFixture<ClaimsApiFactory>;