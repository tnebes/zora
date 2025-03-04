using zora.Tests.TestFixtures.v1;

namespace zora.Tests.TestFixtures;

[CollectionDefinition("TestCollection")]
public class TestCollection : ICollectionFixture<MockedRepositoryFixture>
{
}
