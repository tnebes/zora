#region

using zora.Tests.TestFixtures.v1;

#endregion

namespace zora.Tests.TestFixtures;

[CollectionDefinition("TestCollection")]
public class TestCollection : ICollectionFixture<MockedRepositoryFixture>
{
}
