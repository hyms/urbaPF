using FluentAssertions;
using Moq;
using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.Services;
using UrbaPF.Infrastructure.Repositories;

namespace UrbaPF.Tests.Services;

public class AlertServiceTests
{
    private readonly Mock<IAlertRepository> _alertRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly AlertDomainService _domainService;
    private readonly AlertService _alertService;

    public AlertServiceTests()
    {
        _alertRepositoryMock = new Mock<IAlertRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _auditServiceMock = new Mock<IAuditService>();
        _domainService = new AlertDomainService();
        _alertService = new AlertService(
            _alertRepositoryMock.Object,
            _userRepositoryMock.Object,
            _domainService,
            _auditServiceMock.Object
        );
    }

    [Test]
    public async Task CreateAsync_ShouldCreateAlertAndLogEvent()
    {
        // Arrange
        var condominiumId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var alertId = Guid.NewGuid();
        var request = new CreateAlertRequest(1, "Test Alert", "Description", "Location");

        _alertRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Alert>()))
            .ReturnsAsync(alertId);

        // Act
        var result = await _alertService.CreateAsync(condominiumId, creatorId, 5, request);

        // Assert
        result.Should().Be(alertId);
        _alertRepositoryMock.Verify(r => r.CreateAsync(It.Is<Alert>(a => a.Title == request.Title)), Times.Once);
        _auditServiceMock.Verify(a => a.LogEventAsync(creatorId, condominiumId, "ALERT_CREATED", alertId, request), Times.Once);
    }

    [Test]
    public async Task ApproveAsync_WithValidAlert_ShouldApproveAndLogEvent()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var alert = new Alert { Id = alertId, Status = 1, CondominiumId = Guid.NewGuid() };

        _alertRepositoryMock.Setup(r => r.GetByIdAsync(alertId)).ReturnsAsync(alert);
        _alertRepositoryMock.Setup(r => r.ApproveAsync(alertId, approverId)).ReturnsAsync(true);
        _alertRepositoryMock.Setup(r => r.MarkNotifiedAsync(alertId)).ReturnsAsync(true);

        // Act
        var result = await _alertService.ApproveAsync(alertId, approverId);

        // Assert
        result.Should().BeTrue();
        _alertRepositoryMock.Verify(r => r.ApproveAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        _auditServiceMock.Verify(a => a.LogEventAsync(approverId, alert.CondominiumId, "ALERT_APPROVED", alertId, It.IsAny<object>()), Times.Once);
    }
}
