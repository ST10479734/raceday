using System;
using Microsoft.AspNetCore.Mvc;
using Moq;
using raceday.Controllers;
using raceday.database;
using Xunit;
namespace raceday_unittest
{
    public class LoginControllerTests
    {
        private readonly Mock<StoreUserInfo> _mockStoreUserInfo;
        private readonly loginController _controller;

        public LoginControllerTests()
        {
            // Note: For Moq to mock a concrete class, 
            // the StoreUserInfo class and its ValidateUser method must be marked as 'virtual'.
            _mockStoreUserInfo = new Mock<StoreUserInfo>();
            _controller = new loginController(_mockStoreUserInfo.Object);
        }

        [Theory]
        [InlineData("", "password123")]
        [InlineData("test@example.com", "")]
        [InlineData(null, "password123")]
        [InlineData("test@example.com", null)]
        [InlineData("   ", "password123")]
        public void Login_MissingEmailOrPassword_ReturnsBadRequest(string email, string password)
        {
            // Act
            var result = _controller.Login(email, password);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);

            // Verify database method was never called
            _mockStoreUserInfo.Verify(
                s => s.ValidateUser(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    out It.Ref<string>.IsAny,
                    out It.Ref<string>.IsAny),
                Times.Never
            );
        
        }
        [Fact]
        public void Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            string email = "wrong@example.com";
            string password = "wrongpassword";

            // Using OutArgs to properly handle 'out' parameters in Moq setups
            string dummyFullName = string.Empty;
            string dummyRole = string.Empty;

            _mockStoreUserInfo
                .Setup(s => s.ValidateUser(
                    email,
                    password,
                    out dummyFullName,
                    out dummyRole))
                .Returns(false);

            // Act
            var result = _controller.Login(email, password);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsOkWithUserDetails()
        {
            // Arrange
            string email = "john@example.com";
            string password = "securePassword123";
            string expectedFullName = "John Doe";
            string expectedRole = "Participant";

            string fullNameOut = expectedFullName;
            string roleOut = expectedRole;

            _mockStoreUserInfo
                 .Setup(s => s.ValidateUser(
                     email,
                     password,
                     out It.Ref<string>.Is(IsMatcher(expectedFullName)),
                     out It.Ref<string>.Is(IsMatcher(expectedRole))))
                 .Returns(true);

            // Act
            var result = _controller.Login(email, password);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        // Helper expression matcher to assign values to 'out' parameters during Moq setup
        private static System.Linq.Expressions.Expression<Func<string, bool>> IsMatcher(string value)
        {
            return x => true; // Matches any string and allows Moq to pass the out value back
        }
    }
}