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
    public class ReviewsControllerTest
    {
        private ReviewDto[] GetTestReviews() => new ReviewDto[] {
            new ReviewDto { Id = 1, AuthorId = "larry", AuthorName = "Larry von Larryington", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Trek", Summary = "Loved it", Text = "Really enjoyed this movie.  Proper good and that.", Rating = 4 },
            new ReviewDto { Id = 2, AuthorId = "beehive", AuthorName = "Betty Lively", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Trek", Summary = "Entertaining but not worth buying", Text = "Didn't feel like the Star Trek I know and love, but it was still entertaining.  Be warned, there's a bit of an obsession with lens flare!", Rating = 3 },
            new ReviewDto { Id = 3, AuthorId = "beehive", AuthorName = "Betty Lively", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Wars: The Force Awakens", Summary = "Please sir, I'd like some more", Text = "Sooo excited to see Star Wars back.  Feels like the Star Wars I grew up with, although perhaps a bit too repetitive from the originals.  Just needs more saber battles!", Rating = 5 },
            new ReviewDto { Id = 4, AuthorId = "larry", AuthorName = "Larry von Larryington", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Santa Claus Conquers the Martians", Summary = "What!?", Text = "What the actual chuffing missery did I just watch!", Rating = 1 },
            new ReviewDto { Id = 5, AuthorId = "bob69", AuthorName = "Robert Bob Robertson", CategoryId = "MOV", CategoryTitle = "Movie", Subject = "Star Trek", Summary = "wheres mr data???", Text = null, Rating = 2 }
        };

        [Fact]
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
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetIndex_WithNullSubject_ShouldViewServiceEnumerable()
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
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(
                viewResult.ViewData.Model);
            Assert.Equal(expected.Length, model.Count());
            // FIXME: could assert other result property values here

            mockReviews.Verify(r => r.GetReviewsAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetIndex_WithSubject_ShouldViewServiceEnumerable()
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
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(
                viewResult.ViewData.Model);
            Assert.Equal(expected.Length, model.Count());
            // FIXME: could assert other result property values here

            mockReviews.Verify(r => r.GetReviewsAsync("test subject"), Times.Once);
        }

        [Fact]
        public async Task GetIndex_WhenBadServiceCall_ShouldViewEmptyEnumerable()
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
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(
                viewResult.ViewData.Model);
            Assert.Empty(model);
            mockReviews.Verify(r => r.GetReviewsAsync(null), Times.Once);
        }

        [Fact]
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
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
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
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
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
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable,
                         statusCodeResult.StatusCode);
            mockReviews.Verify(r => r.GetReviewAsync(3), Times.Once);
        }

        [Fact]
        public async Task GetDetails_WithUnknownId_ShouldNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReviewsController>>();
            var mockReviews = new Mock<IReviewsService>(MockBehavior.Strict);
            mockReviews.Setup(r => r.GetReviewAsync(13))
                       .ReturnsAsync((ReviewDto)null)
                       .Verifiable();
            var controller = new ReviewsController(mockLogger.Object,
                                                   mockReviews.Object);

            // Act
            var result = await controller.Details(13);

            // Assert
            var statusCodeResult = Assert.IsType<NotFoundResult>(result);
            mockReviews.Verify(r => r.GetReviewAsync(13), Times.Once);
        }

        [Fact]
        public async Task GetDetails_WithId_ShouldViewServiceObject()
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
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ReviewDto>(viewResult.ViewData.Model);
            Assert.Equal(expected.Id, model.Id);
            // FIXME: could assert other result property values here

            mockReviews.Verify(r => r.GetReviewAsync(expected.Id), Times.Once);
        }
    }
}
