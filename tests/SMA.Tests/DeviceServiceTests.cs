using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SMA.Application.DTOs;
using SMA.Application.Interfaces;
using SMA.Application.Services;
using SMA.Domain.Entities;

namespace SMA.Tests
{
    [TestFixture]
    public class DeviceServiceTests
    {
        private Mock<IDeviceRepository> _repo = null!;
        private Mock<IMapper> _mapper = null!;
        private Mock<IIotIntegrationClient> _iot = null!;
        private Mock<ILogger<DeviceService>> _logger = null!;
        private DeviceService _service = null!;

        private DeviceDto _dto = null!;
        private Device _entity = null!;

        [SetUp]
        public void SetUp()
        {
            _repo = new Mock<IDeviceRepository>(MockBehavior.Strict);
            _mapper = new Mock<IMapper>(MockBehavior.Strict);
            _iot = new Mock<IIotIntegrationClient>(MockBehavior.Strict);
            _logger = new Mock<ILogger<DeviceService>>();
            _service = new DeviceService(_repo.Object, _mapper.Object, _iot.Object, _logger.Object);

            _dto = new DeviceDto { Name = "Sensor A", Location = "Sala 1" };
            _entity = new Device { Id = 1, Name = "Sensor A", Location = "Sala 1" };
        }

        [TearDown]
        public void TearDown()
        {
            _repo.Reset();
            _mapper.Reset();
            _iot.Reset();
            _logger.Reset();
        }

        #region Sucesso

        [Test]
        public async Task CreateAsync_Preenche_IntegrationId()
        {
            _mapper.Setup(m => m.Map<Device>(_dto)).Returns(_entity);
            _repo.Setup(r => r.AddAsync(_entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _iot.Setup(i => i.RegisterAsync("Sensor A", "Sala 1", It.IsAny<CancellationToken>())).ReturnsAsync("int-123");
            _repo.Setup(r => r.UpdateAsync(It.Is<Device>(d => d.IntegrationId == "int-123"), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _service.CreateAsync(_dto, CancellationToken.None);

            Assert.That(result.IntegrationId, Is.EqualTo("int-123"));
        }

        [Test]
        public async Task UpdateAsync_AtualizaNome()
        {
            var updateDto = new DeviceDto { Name = "Novo Nome", Location = "Nova Sala" };
            _repo.Setup(r => r.GetByIdForUpdateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(_entity);
            _repo.Setup(r => r.UpdateAsync(_entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var updated = await _service.UpdateAsync(1, updateDto, CancellationToken.None);

            Assert.That(updated!.Name, Is.EqualTo("Novo Nome"));
        }

        [Test]
        public async Task DeleteAsync_QuandoExisteERemove_RetornaTrue()
        {
            var existing = new Device { Id = 5, Name = "X", Location = "L", IntegrationId = "" };
            _repo.Setup(r => r.GetByIdForUpdateAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            _repo.Setup(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var ok = await _service.DeleteAsync(5, CancellationToken.None);

            Assert.That(ok, Is.True);
        }

        #endregion Sucesso

        #region Falha
        [Test]
        public async Task UpdateAsync_QuandoNaoExiste_RetornaNull()
        {
            _repo.Setup(r => r.GetByIdForUpdateAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Device?)null);

            var result = await _service.UpdateAsync(99, _dto, CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_QuandoNaoExiste_RetornaFalse()
        {
            _repo.Setup(r => r.GetByIdForUpdateAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync((Device?)null);

            var result = await _service.DeleteAsync(10, CancellationToken.None);

            Assert.That(result, Is.False);
        }

        #endregion Falha
    }
}
