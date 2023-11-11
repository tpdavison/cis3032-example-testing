using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.Web.Controllers;
using Movies.Web.Services;
using Movies.Web.Services.Reviews;

namespace Movies.Web.Tests
{
    [TestClass]
    public class ReviewsControllerTest
    {
        private static ReviewDto[] GetTestReviews() => new ReviewDto[] {
            new() { Id = 1, AuthorId = "larry", AuthorName = "Larry von Larryington", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Trek", Summary = "Loved it", Text = "Really enjoyed this movie.  Proper good and that.", Rating = 4 },
            new() { Id = 2, AuthorId = "beehive", AuthorName = "Betty Lively", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Trek", Summary = "Entertaining but not worth buying", Text = "Didn't feel like the Star Trek I know and love, but it was still entertaining.  Be warned, there's a bit of an obsession with lens flare!", Rating = 3 },
            new() { Id = 3, AuthorId = "beehive", AuthorName = "Betty Lively", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Wars: The Force Awakens", Summary = "Please sir, I'd like some more", Text = "Sooo excited to see Star Wars back.  Feels like the Star Wars I grew up with, although perhaps a bit too repetitive from the originals.  Just needs more saber battles!", Rating = 5 },
            new() { Id = 4, AuthorId = "larry", AuthorName = "Larry von Larryington", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Santa Claus Conquers the Martians", Summary = "What!?", Text = "What the actual chuffing missery did I just watch!", Rating = 1 },
            new() { Id = 5, AuthorId = "bob69", AuthorName = "Robert Bob Robertson", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Trek", Summary = "wheres mr data???", Text = "", Rating = 2 }
        };

        [TestMethod]
        public async Task GetIndex_WithInvalidModelState_ShouldBadResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReviewsController>>();
            var mockReviews = new Mock<IReviewsService>();
            var controller = new ReviewsController(mockLogger.Object,
                                                   mockReviews.Object);
            controller.ModelState.AddModelError("Something", "Something");

            // Act
            var result = await controller.Index(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task GetIndex_WithNullSubject_ShouldViewWithEnumerable()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReviewsController>>();
            var mockReviews = new Mock<IReviewsService>(MockBehavior.Strict);
            var expected = GetTestReviews();
            mockReviews.Setup(r => r.GetReviewsAsync(null))
                       .ReturnsAsync(expected)
                       .Verifiable();
            var controller = new ReviewsController(mockLogger.Object,
                                                   mockReviews.Object);

            // Act
            var result = await controller.Index(null);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.ViewData.Model as IEnumerable<ReviewDto>;
            Assert.IsNotNull(model);
            Assert.AreEqual(expected.Length, model.Count());
            // FIXME: could assert other result property values here

            mockReviews.Verify(r => r.GetReviewsAsync(null), Times.Once);
        }

        [TestMethod]
        public async Task GetIndex_WithSubject_ShouldViewWithEnumerable()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReviewsController>>();
            var mockReviews = new Mock<IReviewsService>(MockBehavior.Strict);
            var expected = GetTestReviews();
            mockReviews.Setup(r => r.GetReviewsAsync("test subject"))
                       .ReturnsAsync(expected)
                       .Verifiable();
            var controller = new ReviewsController(mockLogger.Object,
                                                   mockReviews.Object);

            // Act
            var result = await controller.Index("test subject");

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.ViewData.Model as IEnumerable<ReviewDto>;
            Assert.IsNotNull(model);
            Assert.AreEqual(expected.Length, model.Count());
            // FIXME: could assert other result property values here

            mockReviews.Verify(r => r.GetReviewsAsync("test subject"), Times.Once);
        }

        [TestMethod]
        public async Task GetIndex_WhenBadServiceCall_ShouldViewWithEmptyEnumerable()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReviewsController>>();
            var mockReviews = new Mock<IReviewsService>(MockBehavior.Strict);
            mockReviews.Setup(r => r.GetReviewsAsync(null))
                       .ThrowsAsync(new Exception())
                       .Verifiable();
            var controller = new ReviewsController(mockLogger.Object,
                                                   mockReviews.Object);

            // Act
            var result = await controller.Index(null);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.ViewData.Model as IEnumerable<ReviewDto>;
            Assert.IsNotNull(model);
            Assert.IsFalse(model.Any()); // assert model should be empty
            mockReviews.Verify(r => r.GetReviewsAsync(null), Times.Once);
        }

        [TestMethod]
        public async Task GetDetails_WithInvalidModelState_ShouldBadResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReviewsController>>();
            var mockReviews = new Mock<IReviewsService>();
            var controller = new ReviewsController(mockLogger.Object,
                                                   mockReviews.Object);
            controller.ModelState.AddModelError("Something", "Something");

            // Act
            var result = await controller.Details(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task GetDetails_WithNullId_ShouldBadResult()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReviewsController>>();
            var mockReviews = new Mock<IReviewsService>();
            var controller = new ReviewsController(mockLogger.Object,
                                                   mockReviews.Object);

            // Act
            var result = await controller.Details(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task GetDetails_WhenBadServiceCall_ShouldInternalError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReviewsController>>();
            var mockReviews = new Mock<IReviewsService>(MockBehavior.Strict);
            mockReviews.Setup(r => r.GetReviewAsync(3))
                       .ThrowsAsync(new Exception())
                       .Verifiable();
            var controller = new ReviewsController(mockLogger.Object,
                                                   mockReviews.Object);

            // Act
            var result = await controller.Details(3);

            // Assert
            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(StatusCodes.Status503ServiceUnavailable,
                            statusCodeResult.StatusCode);
            mockReviews.Verify(r => r.GetReviewAsync(3), Times.Once);
        }

        [TestMethod]
        public async Task GetDetails_WithUnknownId_ShouldNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReviewsController>>();
            var mockReviews = new Mock<IReviewsService>(MockBehavior.Strict);
            mockReviews.Setup(r => r.GetReviewAsync(13))
                       .ReturnsAsync((ReviewDto?)null)
                       .Verifiable();
            var controller = new ReviewsController(mockLogger.Object,
                                                   mockReviews.Object);

            // Act
            var result = await controller.Details(13);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
            mockReviews.Verify(r => r.GetReviewAsync(13), Times.Once);
        }

        [TestMethod]
        public async Task GetDetails_WithId_ShouldViewWithObject()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReviewsController>>();
            var mockReviews = new Mock<IReviewsService>(MockBehavior.Strict);
            var expected = GetTestReviews().First();
            mockReviews.Setup(r => r.GetReviewAsync(expected.Id))
                       .ReturnsAsync(expected)
                       .Verifiable();
            var controller = new ReviewsController(mockLogger.Object,
                                                   mockReviews.Object);

            // Act
            var result = await controller.Details(expected.Id);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.ViewData.Model as ReviewDto;
            Assert.IsNotNull(model);
            Assert.AreEqual(expected.Id, model.Id);
            // FIXME: could assert other result property values here

            mockReviews.Verify(r => r.GetReviewAsync(expected.Id), Times.Once);
        }
    }
}
