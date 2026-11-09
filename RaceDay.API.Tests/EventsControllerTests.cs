using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests
{
    public class EventsControllerTests
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
        public async Task GetEvents_ReturnsEmptyList_WhenNoEventsExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new EventsController(context);

            // Act
            var result = await controller.GetEvents();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Event>>>(result);
            var events = Assert.IsAssignableFrom<IEnumerable<Event>>(actionResult.Value);
            Assert.Empty(events);
        }

        [Fact]
        public async Task PostEvent_CreatesAndReturnsEvent()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new EventsController(context);

            var newEvent = new Event
            {
                Name = "Soweto Marathon",
                Description = "Annual road race",
                EventDate = DateTime.Now.AddDays(30),
                Location = "Soweto",
                Distance = 42.2m,
                EventTypeID = 1,
                OrganiserID = 1
            };

            // Act
            var result = await controller.PostEvent(newEvent);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedEvent = Assert.IsType<Event>(createdAtActionResult.Value);
            Assert.Equal("Soweto Marathon", returnedEvent.Name);
        }
    }
}