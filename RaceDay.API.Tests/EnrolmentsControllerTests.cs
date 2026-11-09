using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests
{
    public class EnrolmentsControllerTests
    {
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new RaceDayDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetEnrolments_ReturnsEmptyList_WhenNoEnrolmentsExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new EnrolmentsController(context);

            // Act
            var result = await controller.GetEnrolments();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Enrolment>>>(result);
            var enrolments = Assert.IsAssignableFrom<IEnumerable<Enrolment>>(actionResult.Value);
            Assert.Empty(enrolments);
        }

        [Fact]
        public async Task PostEnrolment_CreatesAndReturnsEnrolment()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new EnrolmentsController(context);

            var newEnrolment = new Enrolment
            {
                ParticipantID = 1,
                EventID = 1
            };

            // Act
            var result = await controller.PostEnrolment(newEnrolment);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedEnrolment = Assert.IsType<Enrolment>(createdAtActionResult.Value);
            Assert.Equal(1, returnedEnrolment.ParticipantID);
            Assert.Equal(1, returnedEnrolment.EventID);
        }
    }
}