using Microsoft.AspNetCore.Http;

namespace Zora.Tests.Unit
{
    [TestClass]
    public class AuthenticationControllerTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            var controller = new AuthenticationController();

            // Act
            var result = controller.Get();

            // Assert
            Assert.IsAssignable<ObjectResult>(result);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, ((ObjectResult)result).StatusCode);
        }
    }
} 