using FluentAssertions;
using Moq;
using UrbaPF.Domain.Entities;
using UrbaPF.Domain.Services;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.Services;
using UrbaPF.Infrastructure.Repositories;

namespace UrbaPF.Tests.Services;

public class IncidentServiceTests
{
    private readonly Mock<IIncidentRepository> _incidentRepositoryMock;
    private readonly IncidentDomainService _domainService;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly IncidentService _incidentService;

    public IncidentServiceTests()
    {
        _incidentRepositoryMock = new Mock<IIncidentRepository>();
        _domainService = new IncidentDomainService();
        _auditServiceMock = new Mock<IAuditService>();
        _incidentService = new IncidentService(
            _incidentRepositoryMock.Object,
            _domainService,
            _auditServiceMock.Object
        );
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateAndLogEvent()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var incident = new Incident { Id = incidentId, ReporterId = userId, Status = 1, CondominiumId = Guid.NewGuid() };
        var request = new UpdateIncidentRequest("New Title", "New Description", 1, "NewLocation", "NewAddress", null);

        _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incidentId)).ReturnsAsync(incident);
        _incidentRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Incident>())).ReturnsAsync(true);

        // Act
        var result = await _incidentService.UpdateAsync(incidentId, userId, 2, request);

        // Assert
        result.Should().BeTrue();
        _incidentRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Incident>(i => i.Title == "New Title")), Times.Once);
        _auditServiceMock.Verify(a => a.LogEventAsync(userId, incident.CondominiumId, "INCIDENT_UPDATED", incidentId, request), Times.Once);
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldUpdateAndLogEvent()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var incident = new Incident { Id = incidentId, Status = 1, CondominiumId = Guid.NewGuid() };
        var newStatus = 2;

        _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incidentId)).ReturnsAsync(incident);
        _incidentRepositoryMock.Setup(r => r.UpdateStatusAsync(incidentId, newStatus, null)).ReturnsAsync(true);

        // Act
        var result = await _incidentService.UpdateStatusAsync(incidentId, 3, false, newStatus, null);

        // Assert
        result.Should().BeTrue();
        _incidentRepositoryMock.Verify(r => r.UpdateStatusAsync(incidentId, newStatus, null), Times.Once);
        _auditServiceMock.Verify(a => a.LogEventAsync(It.IsAny<Guid>(), incident.CondominiumId, "INCIDENT_STATUS_UPDATED", incidentId, It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_ShouldDeleteAndLogEvent()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var incident = new Incident { Id = incidentId, ReporterId = userId, Status = 1, CondominiumId = Guid.NewGuid() };

        _incidentRepositoryMock.Setup(r => r.GetByIdAsync(incidentId)).ReturnsAsync(incident);
        _incidentRepositoryMock.Setup(r => r.DeleteAsync(incidentId)).ReturnsAsync(true);

        // Act
        var result = await _incidentService.DeleteAsync(incidentId, userId, 2);

        // Assert
        result.Should().BeTrue();
        _incidentRepositoryMock.Verify(r => r.DeleteAsync(incidentId), Times.Once);
        _auditServiceMock.Verify(a => a.LogEventAsync(userId, incident.CondominiumId, "INCIDENT_DELETED", incidentId, It.IsAny<object>()), Times.Once);
    }
}
