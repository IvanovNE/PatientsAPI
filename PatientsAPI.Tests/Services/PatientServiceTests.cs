using FluentAssertions;
using Moq;
using PatientsAPI.Application.Common;
using PatientsAPI.Application.Common.Interfaces;
using PatientsAPI.Application.Services;
using PatientsAPI.Domain.Entities;
using PatientsAPI.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PatientsAPI.Tests.Services
{
    public class PatientServiceTests
    {
        private readonly Mock<IRepository<Patient>> _repositoryMock;
        private readonly PatientService _service;

        public PatientServiceTests()
        {
            _repositoryMock = new Mock<IRepository<Patient>>();
            _service = new PatientService(_repositoryMock.Object);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ReturnsAllPatients()
        {
            // Arrange
            var patients = new List<Patient>
        {
            TestDataFactory.CreateTestPatient("Иванов"),
            TestDataFactory.CreateTestPatient("Петров")
        };

            _repositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(patients);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Family.Should().Be("Иванов");
            result[1].Name.Family.Should().Be("Петров");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoPatients()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Patient>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsPatient_WhenExists()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = TestDataFactory.CreateTestPatient("Иванов");

            // Используем рефлексию, чтобы установить Id (так как у Patient приватный сеттер)
            typeof(Patient).GetProperty(nameof(Patient.Id))?.SetValue(patient, patientId);

            _repositoryMock
                .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            // Act
            var result = await _service.GetByIdAsync(patientId);

            // Assert
            result.Id.Should().Be(patientId);
            result.Name.Family.Should().Be("Иванов");
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsNotFoundException_WhenPatientDoesNotExist()
        {
            // Arrange
            var patientId = Guid.NewGuid();

            _repositoryMock
                .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            // Act
            var act = () => _service.GetByIdAsync(patientId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Patient with id {patientId} not found");
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_CreatesAndReturnsPatient()
        {
            // Arrange
            var createDto = TestDataFactory.CreateTestCreatePatientDto();

            Patient? capturedPatient = null;

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
                .Callback<Patient, CancellationToken>((p, _) => capturedPatient = p)
                .ReturnsAsync((Patient p, CancellationToken _) => p);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(createDto);

            // Assert
            result.Name.Family.Should().Be("Петров");
            result.Gender.Should().Be("male");
            result.Active.Should().BeTrue();

            capturedPatient.Should().NotBeNull();
            capturedPatient!.Name.Family.Should().Be("Петров");

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_CreatesPatientWithProvidedNameId()
        {
            // Arrange
            var createDto = TestDataFactory.CreateTestCreatePatientDto();
            var providedId = Guid.NewGuid();
            createDto.Name.Id = providedId;

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient p, CancellationToken _) => p);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(createDto);

            // Assert
            result.Name.Id.Should().Be(providedId);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_UpdatesAndReturnsPatient()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var nameId = Guid.NewGuid();
            var patient = TestDataFactory.CreateTestPatient("Иванов");

            typeof(Patient).GetProperty(nameof(Patient.Id))?.SetValue(patient, patientId);

            var updateDto = TestDataFactory.CreateTestUpdatePatientDto(nameId);

            _repositoryMock
                .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateAsync(patientId, updateDto);

            // Assert
            result.Id.Should().Be(patientId);
            result.Name.Family.Should().Be("Сидоров");
            result.Active.Should().BeFalse();

            _repositoryMock.Verify(r => r.Update(It.IsAny<Patient>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsNotFoundException_WhenPatientDoesNotExist()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var updateDto = TestDataFactory.CreateTestUpdatePatientDto(Guid.NewGuid());

            _repositoryMock
                .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            // Act
            var act = () => _service.UpdateAsync(patientId, updateDto);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Patient with id {patientId} not found");

            _repositoryMock.Verify(r => r.Update(It.IsAny<Patient>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_DeletesPatient_WhenExists()
        {
            // Arrange
            var patientId = Guid.NewGuid();
            var patient = TestDataFactory.CreateTestPatient();

            _repositoryMock
                .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var act = () => _service.DeleteAsync(patientId);

            // Assert
            await act.Should().NotThrowAsync();

            _repositoryMock.Verify(r => r.Delete(patient), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsNotFoundException_WhenPatientDoesNotExist()
        {
            // Arrange
            var patientId = Guid.NewGuid();

            _repositoryMock
                .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            // Act
            var act = () => _service.DeleteAsync(patientId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Patient with id {patientId} not found");

            _repositoryMock.Verify(r => r.Delete(It.IsAny<Patient>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region SearchByBirthDateAsync Tests

        [Fact]
        public async Task SearchByBirthDateAsync_ReturnsMatchingPatients()
        {
            // Arrange
            var dateParam = "ge2024-01-01";
            var patients = new List<Patient>
        {
            TestDataFactory.CreateTestPatient("Иванов"),
            TestDataFactory.CreateTestPatient("Петров")
        };

            _repositoryMock
                .Setup(r => r.FindWithDatePrefixAsync(
                    It.IsAny<Expression<Func<Patient, DateTime>>>(),
                    dateParam,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patients);

            // Act
            var result = await _service.SearchByBirthDateAsync(dateParam);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task SearchByBirthDateAsync_ReturnsEmptyList_WhenNoMatches()
        {
            // Arrange
            var dateParam = "lt2020-01-01";

            _repositoryMock
                .Setup(r => r.FindWithDatePrefixAsync(
                    It.IsAny<Expression<Func<Patient, DateTime>>>(),
                    dateParam,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Patient>());

            // Act
            var result = await _service.SearchByBirthDateAsync(dateParam);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("2024-01-01")]
        [InlineData("eq2024-01-01")]
        [InlineData("ne2024-01-01")]
        [InlineData("gt2024-01-01")]
        [InlineData("lt2024-01-01")]
        [InlineData("ge2024-01-01")]
        [InlineData("le2024-01-01")]
        [InlineData("2024-01-01T10:30:00")]
        [InlineData("ge2024-01-01T10:30:00")]
        public async Task SearchByBirthDateAsync_AcceptsAllFhirPrefixes(string dateParam)
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.FindWithDatePrefixAsync(
                    It.IsAny<Expression<Func<Patient, DateTime>>>(),
                    dateParam,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Patient>());

            // Act
            var act = () => _service.SearchByBirthDateAsync(dateParam);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion
    }
}
