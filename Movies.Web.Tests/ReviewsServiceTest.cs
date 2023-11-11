using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Movies.Web.Services;
using Movies.Web.Services.Reviews;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Movies.Web.Tests
{
    [TestClass]
    public class ReviewsServiceTest
    {
        private static
        Mock<HttpMessageHandler> CreateHttpMock(HttpStatusCode expectedCode,
                                                string? expectedJson)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = expectedCode
            };
            if (expectedJson != null)
            {
                response.Content = new StringContent(expectedJson,
                                                     Encoding.UTF8,
                                                     "application/json");
            }
            var mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response)
                .Verifiable();
            return mock;
        }

        private static IReviewsService CreateReviewsService(HttpClient client)
        {
            var mockConfiguration = new Mock<IConfiguration>(MockBehavior.Strict);
            mockConfiguration.Setup(c => c["WebServices:Reviews:BaseURL"])
                             .Returns("http://example.com");
            return new ReviewsService(client, mockConfiguration.Object);
        }

        [TestMethod]
        public async Task GetReviewAsync_WithValid_ShouldOkEntity()
        {
            // Arrange
            var expectedResult = new ReviewDto { Id = 1, AuthorId = "larry", AuthorName = "Larry von Larryington", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Trek", Summary = "Loved it", Text = "Really enjoyed this movie.  Proper good and that.", Rating = 4 };
            var expectedJson = JsonConvert.SerializeObject(expectedResult);
            var expectedUri = new Uri("http://example.com/api/reviews/1");
            var mock = CreateHttpMock(HttpStatusCode.OK, expectedJson);
            var client = new HttpClient(mock.Object);
            var service = CreateReviewsService(client);

            // Act
            var result = await service.GetReviewAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult.Id, result.Id);
            // FIXME: could assert other result property values
            mock.Protected()
                .Verify("SendAsync",
                        Times.Once(),
                        ItExpr.Is<HttpRequestMessage>(
                            req => req.Method == HttpMethod.Get
                                   && req.RequestUri == expectedUri),
                        ItExpr.IsAny<CancellationToken>()
                        );
        }

        [TestMethod]
        public async Task GetReviewAsync_WithInvalid_ShouldReturnNull()
        {
            // Arrange
            var expectedUri = new Uri("http://example.com/api/reviews/100");
            var mock = CreateHttpMock(HttpStatusCode.NotFound, null);
            var client = new HttpClient(mock.Object);
            var service = CreateReviewsService(client);

            // Act
            var result = await service.GetReviewAsync(100);

            // Assert
            Assert.IsNull(result);
            mock.Protected()
                .Verify("SendAsync",
                        Times.Once(),
                        ItExpr.Is<HttpRequestMessage>(
                            req => req.Method == HttpMethod.Get
                                   && req.RequestUri == expectedUri),
                        ItExpr.IsAny<CancellationToken>()
                        );
        }

        [TestMethod]
        public async Task GetReviewAsync_OnHttpBad_ShouldThrow()
        {
            // Arrange
            var expectedUri = new Uri("http://example.com/api/reviews/1");
            var mock = CreateHttpMock(HttpStatusCode.ServiceUnavailable, null);
            var client = new HttpClient(mock.Object);
            var service = CreateReviewsService(client);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => service.GetReviewAsync(1));
        }

        [TestMethod]
        public async Task GetReviewsAsync_WithNull_ShouldReturnAll()
        {
            // Arrange
            var expectedResult = new ReviewDto[]
            {
                new() { Id = 1, AuthorId = "larry", AuthorName = "Larry von Larryington", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Trek", Summary = "Loved it", Text = "Really enjoyed this movie.  Proper good and that.", Rating = 4 },
                new() { Id = 2, AuthorId = "beehive", AuthorName = "Betty Lively", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Trek", Summary = "Entertaining but not worth buying", Text = "Didn't feel like the Star Trek I know and love, but it was still entertaining.  Be warned, there's a bit of an obsession with lens flare!", Rating = 3 },
                new() { Id = 3, AuthorId = "beehive", AuthorName = "Betty Lively", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Wars: The Force Awakens", Summary = "Please sir, I'd like some more", Text = "Sooo excited to see Star Wars back.  Feels like the Star Wars I grew up with, although perhaps a bit too repetitive from the originals.  Just needs more saber battles!", Rating = 5 },
                new() { Id = 4, AuthorId = "larry", AuthorName = "Larry von Larryington", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Santa Claus Conquers the Martians", Summary = "WTF!?", Text = "What the actual chuffing missery did I just watch!", Rating = 1 },
                new() { Id = 5, AuthorId = "bob69", AuthorName = "Robert Bob Robertson", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Trek", Summary = "wheres mr data???", Text = "", Rating = 2 }

            };
            var expectedJson = JsonConvert.SerializeObject(expectedResult);
            var expectedUri = new Uri("http://example.com/api/reviews?category=MOV");
            var mock = CreateHttpMock(HttpStatusCode.OK, expectedJson);
            var client = new HttpClient(mock.Object);
            var service = CreateReviewsService(client);

            // Act
            var result = (await service.GetReviewsAsync(null)).ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult.Length, result.Length);
            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(expectedResult[i].Id, result[i].Id);
                // FIXME: could assert other result property values
            }
            mock.Protected()
                .Verify("SendAsync",
                        Times.Once(),
                        ItExpr.Is<HttpRequestMessage>(
                            req => req.Method == HttpMethod.Get
                                   && req.RequestUri == expectedUri),
                        ItExpr.IsAny<CancellationToken>()
                        );
        }

        [TestMethod]
        public Task GetReviewsAsync_WithValid_ShouldReturnList()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public Task GetReviewsAsync_WithInvalid_ShouldReturnEmpty()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public Task GetReviewsAsync_OnHttpBad_ShouldThrow()
        {
            throw new NotImplementedException();
        }
    }
}
