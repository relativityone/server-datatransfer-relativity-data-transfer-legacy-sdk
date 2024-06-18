using Moq;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Tests
{
	[TestFixture]
	public class UserServiceTests
	{
		private Mock<IServiceContextFactory> _mockServiceContextFactory;

		[SetUp]
		public void SetUp()
		{
			_mockServiceContextFactory = new Mock<IServiceContextFactory>();
		}

		[Test]
		public void LogoutAsync_ShouldThrowNotSupportedException()
		{
			// Arrange
			var userService = new UserService(_mockServiceContextFactory.Object);

			// Act & Assert
			Assert.ThrowsAsync<NotSupportedException>(() => userService.LogoutAsync("test"));
		}

		[Test]
		public void ClearCookiesBeforeLoginAsync_ShouldThrowNotSupportedException()
		{
			// Arrange
			var userService = new UserService(_mockServiceContextFactory.Object);

			// Act & Assert
			Assert.ThrowsAsync<NotSupportedException>(() => userService.ClearCookiesBeforeLoginAsync("test"));
		}

		[Test]
		public void LoggedInAsync_ShouldThrowNotSupportedException()
		{
			// Arrange
			var userService = new UserService(_mockServiceContextFactory.Object);

			// Act & Assert
			Assert.ThrowsAsync<NotSupportedException>(() => userService.LoggedInAsync("test"));
		}

		[Test]
		public void LoginAsync_ShouldThrowNotSupportedException()
		{
			// Arrange
			var userService = new UserService(_mockServiceContextFactory.Object);

			// Act & Assert
			Assert.ThrowsAsync<NotSupportedException>(() => userService.LoginAsync("test@test.com", "password", "test"));
		}

		[Test]
		public void GenerateDistributedAuthenticationTokenAsync_ShouldThrowNotSupportedException()
		{
			// Arrange
			var userService = new UserService(_mockServiceContextFactory.Object);

			// Act & Assert
			Assert.ThrowsAsync<NotSupportedException>(() => userService.GenerateDistributedAuthenticationTokenAsync("test"));
		}
	}
}

