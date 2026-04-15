using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PatientsAPI.Application.Common;
using PatientsAPI.Application.Common.Interfaces;
using PatientsAPI.Application.DTO;
using PatientsAPI.Controllers;
using PatientsAPI.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PatientsAPI.Tests.Controllers
{
    public class PatientsControllerTests
    {
        private readonly Mock<IPatientService> _patientServiceMock;
        private readonly Mock<ILogger<PatientsController>> _loggerMock;
        private readonly PatientsController _controller;

        public PatientsControllerTests()
        {
            _patientServiceMock = new Mock<IPatientService>();
            _loggerMock = new Mock<ILogger<PatientsController>>();
            _controller = new PatientsController(_patientServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfPatients()
        {
            // Arrange
            var patients = new List<PatientDto>
        {
            TestDataFactory.CreateTestPatientDto(Guid.NewGuid(), "Иванов"),
            TestDataFactory.CreateTestPatientDto(Guid.NewGuid(), "Петров")
        };

            _patientServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(patients);

            // Act
            var result = await _controller.GetAll(CancellationToken.None);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPatients = okResult.Value.Should().BeAssignableTo<IReadOnlyList<PatientDto>>().Subject;
            returnedPatients.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList_WhenNoPatients()
        {
            // Arrange
            _patientServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PatientDto>());

            // Act
            var result = await _controller.GetAll(CancellationToken.None);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPatients = okResult.Value.Should().BeAssignableTo<IReadOnlyList<PatientDto>>().Subject;
            returnedPatients.Should().BeEmpty();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenPatientExists()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = TestDataFactory.CreateTestPatientDto(patientId);

            _patientServiceMock
                .Setup(s => s.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            // Act
            var result = await _controller.GetById(patientId, CancellationToken.None);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPatient = okResult.Value.Should().BeAssignableTo<PatientDto>().Subject;
            returnedPatient.Id.Should().Be(patientId);
        }

        [Fact]
        public async Task GetById_ThrowsNotFoundException_WhenPatientDoesNotExist()
        {
            // Arrange
            var patientId = Guid.NewGuid();

            _patientServiceMock
                .Setup(s => s.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException($"Patient with id {patientId} not found"));

            // Act
            var act = () => _controller.GetById(patientId, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenValidDto()
        {
            // Arrange
            var createDto = TestDataFactory.CreateTestCreatePatientDto();
            var createdPatient = TestDataFactory.CreateTestPatientDto(Guid.NewGuid(), "Петров");

            _patientServiceMock
                .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdPatient);

            // Act
            var result = await _controller.Create(createDto, CancellationToken.None);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(PatientsController.GetById));
            createdAtActionResult.RouteValues!["id"].Should().Be(createdPatient.Id);

            var returnedPatient = createdAtActionResult.Value.Should().BeAssignableTo<PatientDto>().Subject;
            returnedPatient.Id.Should().Be(createdPatient.Id);
        }

        [Fact]
        public async Task Update_ReturnsOkResult_WhenPatientExists()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var nameId = Guid.NewGuid();
            var updateDto = TestDataFactory.CreateTestUpdatePatientDto(nameId);
            var updatedPatient = TestDataFactory.CreateTestPatientDto(patientId, "Сидоров");

            _patientServiceMock
                .Setup(s => s.UpdateAsync(patientId, updateDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedPatient);

            // Act
            var result = await _controller.Update(patientId, updateDto, CancellationToken.None);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPatient = okResult.Value.Should().BeAssignableTo<PatientDto>().Subject;
            returnedPatient.Id.Should().Be(patientId);
            returnedPatient.Name.Family.Should().Be("Сидоров");
        }

        [Fact]
        public async Task Update_ThrowsNotFoundException_WhenPatientDoesNotExist()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var updateDto = TestDataFactory.CreateTestUpdatePatientDto(Guid.NewGuid());

            _patientServiceMock
                .Setup(s => s.UpdateAsync(patientId, updateDto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException($"Patient with id {patientId} not found"));

            // Act
            var act = () => _controller.Update(patientId, updateDto, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenPatientExists()
        {
            // Arrange
            var patientId = Guid.NewGuid();

            _patientServiceMock
                .Setup(s => s.DeleteAsync(patientId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(patientId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_ThrowsNotFoundException_WhenPatientDoesNotExist()
        {
            // Arrange
            var patientId = Guid.NewGuid();

            _patientServiceMock
                .Setup(s => s.DeleteAsync(patientId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException($"Patient with id {patientId} not found"));

            // Act
            var act = () => _controller.Delete(patientId, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task SearchByBirthDate_ReturnsOkResult_WithMatchingPatients()
        {
            // Arrange
            var birthDateParam = "ge2024-01-01";
            var patients = new List<PatientDto>
        {
            TestDataFactory.CreateTestPatientDto(Guid.NewGuid(), "Иванов")
        };

            _patientServiceMock
                .Setup(s => s.SearchByBirthDateAsync(birthDateParam, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patients);

            // Act
            var result = await _controller.SearchByBirthDate(birthDateParam, CancellationToken.None);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedPatients = okResult.Value.Should().BeAssignableTo<IReadOnlyList<PatientDto>>().Subject;
            returnedPatients.Should().HaveCount(1);
        }

        [Fact]
        public async Task SearchByBirthDate_ReturnsBadRequest_WhenParameterIsEmpty()
        {
            // Act
            var result = await _controller.SearchByBirthDate("", CancellationToken.None);

            // Assert
            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var problemDetails = badRequestResult.Value.Should().BeAssignableTo<ProblemDetails>().Subject;
            problemDetails.Detail.Should().Be("birthDate parameter is required");
        }
    }
}
