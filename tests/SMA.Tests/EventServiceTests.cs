using Microsoft.Extensions.Logging;
using Moq;
using SMA.Application.DTOs;
using SMA.Application.Interfaces;
using SMA.Application.Services;
using SMA.Domain.Entities;

namespace SMA.Tests
{
    [TestFixture]
    public class EventServiceTests
    {
        private Mock<IDeviceRepository> _deviceRepo = null!;
        private Mock<IEventRepository> _eventRepo = null!;
        private Mock<ILogger<EventService>> _logger = null!;
        private EventService _service = null!;

        private Device _device = null!;
        private EventDto _dto = null!;
        private Event _eventEntity = null!;

        [SetUp]
        public void SetUp()
        {
            _deviceRepo = new Mock<IDeviceRepository>(MockBehavior.Strict);
            _eventRepo = new Mock<IEventRepository>(MockBehavior.Strict);
            _logger = new Mock<ILogger<EventService>>();
            _service = new EventService(_deviceRepo.Object, _eventRepo.Object, _logger.Object);

            _device = new Device { Id = 1, Name = "Sensor X", Location = "Lab", IntegrationId = "int-123" };
            _dto = new EventDto { IntegrationId = "int-123", Temperature = 22, Humidity = 60, IsAlarm = false, OccurredAt = DateTime.UtcNow };
            _eventEntity = new Event { Id = 100, DeviceId = 1, Temperature = 22m, Humidity = 60m, IsAlarm = false, OccurredAt = _dto.OccurredAt };
        }

        [TearDown]
        public void TearDown()
        {
            _deviceRepo.Reset();
            _eventRepo.Reset();
            _logger.Reset();
        }

        #region Sucesso

        [Test]
        public async Task IngestAsync_DefineDeviceId()
        {
            _deviceRepo.Setup(r => r.GetByIntegrationIdAsync("int-123", It.IsAny<CancellationToken>()))
                       .ReturnsAsync(_device);
            _eventRepo.Setup(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var ev = await _service.IngestAsync(_dto, CancellationToken.None);

            Assert.That(ev.DeviceId, Is.EqualTo(1));
        }

        [Test]
        public async Task GetLatestAsync_PopulaDeviceNoResultado()
        {
            _eventRepo.Setup(r => r.GetLatestAsync(5, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Event> { _eventEntity });
            _deviceRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(_device);

            var result = await _service.GetLatestAsync(5, CancellationToken.None);

            Assert.That(result[0].Device!.Name, Is.EqualTo("Sensor X"));
        }

        [Test]
        public async Task GetByDeviceAsync_PopulaDeviceNoResultado()
        {
            _eventRepo.Setup(r => r.GetByDeviceAsync(1, 5, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Event> { _eventEntity });
            _deviceRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(_device);

            var result = await _service.GetByDeviceAsync(1, 5, CancellationToken.None);

            Assert.That(result[0].Device!.Name, Is.EqualTo("Sensor X"));
        }

        #endregion Sucesso

        #region Falha

        [Test]
        public void IngestAsync_IntegrationIdInexistente_LancaKeyNotFound()
        {
            _deviceRepo.Setup(r => r.GetByIntegrationIdAsync("int-123", It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Device?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.IngestAsync(_dto, CancellationToken.None));
        }

        [Test]
        public void GetLatestAsync_RepositorioLancaExcecao_Propaga()
        {
            _eventRepo.Setup(r => r.GetLatestAsync(20, It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("DB down"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetLatestAsync(20, CancellationToken.None));
        }

        [Test]
        public void GetByDeviceAsync_RepositorioLancaExcecao_Propaga()
        {
            _eventRepo.Setup(r => r.GetByDeviceAsync(99, 10, It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("DB down"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetByDeviceAsync(99, 10, CancellationToken.None));
        }

        #endregion Falha
    }
}
