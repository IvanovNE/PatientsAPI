using PatientsAPI.Application.Common;
using PatientsAPI.Application.Common.Interfaces;
using PatientsAPI.Application.DTO;
using PatientsAPI.Domain.Entities;
using PatientsAPI.Domain.Enums;
using PatientsAPI.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PatientsAPI.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly IRepository<Patient> _repository;

        public PatientService(IRepository<Patient> repository)
        {
            _repository = repository;
        }

        public async Task<PatientDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var patient = await _repository.GetByIdAsync(id, cancellationToken);

            if (patient == null)
                throw new NotFoundException($"Patient with id {id} not found");

            return MapToDto(patient);
        }

        public async Task<IReadOnlyList<PatientDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var patients = await _repository.GetAllAsync(cancellationToken);
            return patients.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<PatientDto>> SearchByBirthDateAsync(
            string dateParameter,
            CancellationToken cancellationToken = default)
        {
            var patients = await _repository.FindWithDatePrefixAsync(
                p => p.BirthDate,
                dateParameter,
                cancellationToken);

            return patients.Select(MapToDto).ToList();
        }

        public async Task<PatientDto> CreateAsync(CreatePatientDto dto, CancellationToken cancellationToken = default)
        {
            var patient = MapToEntity(dto);

            await _repository.AddAsync(patient, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(patient);
        }

        public async Task<PatientDto> UpdateAsync(Guid id, UpdatePatientDto dto, CancellationToken cancellationToken = default)
        {
            var patient = await _repository.GetByIdAsync(id, cancellationToken);

            if (patient == null)
                throw new NotFoundException($"Patient with id {id} not found");

            var name = HumanName.CreateWithId(
                dto.Name.Id,
                dto.Name.Family,
                dto.Name.Given,
                ParseUse(dto.Name.Use));

            var gender = ParseGender(dto.Gender);
            var birthDate = DateTime.Parse(dto.BirthDate);

            patient.Update(name, gender, birthDate, dto.Active);

            _repository.Update(patient);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(patient);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var patient = await _repository.GetByIdAsync(id, cancellationToken);

            if (patient == null)
                throw new NotFoundException($"Patient with id {id} not found");

            _repository.Delete(patient);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        private PatientDto MapToDto(Patient patient)
        {
            return new PatientDto
            {
                Id = patient.Id,
                Name = new HumanNameDto
                {
                    Id = patient.Name.Id,
                    Use = patient.Name.Use?.ToString().ToLower(),
                    Family = patient.Name.Family,
                    Given = patient.Name.Given.ToList()
                },
                Gender = patient.Gender?.ToString().ToLower(),
                BirthDate = patient.BirthDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                Active = patient.Active
            };
        }

        private Patient MapToEntity(CreatePatientDto dto)
        {
            HumanName name;

            if (dto.Name.Id.HasValue)
            {
                name = HumanName.CreateWithId(
                    dto.Name.Id.Value,
                    dto.Name.Family,
                    dto.Name.Given,
                    ParseUse(dto.Name.Use));
            }
            else
            {
                name = HumanName.Create(
                    dto.Name.Family,
                    dto.Name.Given,
                    ParseUse(dto.Name.Use));
            }

            Gender? gender = ParseGender(dto.Gender);
            DateTime birthDate = DateTime.Parse(dto.BirthDate);

            return Patient.Create(name, gender, birthDate, dto.Active);
        }

        private HumanNameUse? ParseUse(string? use) => use?.ToLower() switch
        {
            "usual" => HumanNameUse.Usual,
            "official" => HumanNameUse.Official,
            "temp" => HumanNameUse.Temp,
            "nickname" => HumanNameUse.Nickname,
            "anonymous" => HumanNameUse.Anonymous,
            "old" => HumanNameUse.Old,
            "maiden" => HumanNameUse.Maiden,
            _ => null
        };

        private Gender? ParseGender(string? gender) => gender?.ToLower() switch
        {
            "male" => Gender.Male,
            "female" => Gender.Female,
            "other" => Gender.Other,
            "unknown" => Gender.Unknown,
            _ => null
        };

    }
}
