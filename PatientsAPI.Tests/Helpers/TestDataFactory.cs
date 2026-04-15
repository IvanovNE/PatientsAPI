using PatientsAPI.Application.DTO;
using PatientsAPI.Domain.Entities;
using PatientsAPI.Domain.Enums;
using PatientsAPI.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PatientsAPI.Tests.Helpers
{
    public static class TestDataFactory
    {
        public static Patient CreateTestPatient(string family = "Иванов", string given1 = "Иван", string given2 = "Иванович")
        {
            var name = HumanName.Create(family, new[] { given1, given2 }, HumanNameUse.Official);
            return Patient.Create(name, Gender.Male, new DateTime(2024, 1, 13, 18, 25, 43), true);
        }

        public static PatientDto CreateTestPatientDto(Guid id, string family = "Иванов")
        {
            return new PatientDto
            {
                Id = id,
                Name = new HumanNameDto
                {
                    Id = Guid.NewGuid(),
                    Use = "official",
                    Family = family,
                    Given = new List<string> { "Иван", "Иванович" }
                },
                Gender = "male",
                BirthDate = "2024-01-13T18:25:43",
                Active = true
            };
        }

        public static CreatePatientDto CreateTestCreatePatientDto()
        {
            return new CreatePatientDto
            {
                Name = new CreateHumanNameDto
                {
                    Family = "Петров",
                    Given = new List<string> { "Петр", "Петрович" },
                    Use = "official"
                },
                Gender = "male",
                BirthDate = "2024-02-20T10:30:00",
                Active = true
            };
        }

        public static UpdatePatientDto CreateTestUpdatePatientDto(Guid nameId)
        {
            return new UpdatePatientDto
            {
                Name = new UpdateHumanNameDto
                {
                    Id = nameId,
                    Family = "Сидоров",
                    Given = new List<string> { "Сидор", "Сидорович" },
                    Use = "official"
                },
                Gender = "male",
                BirthDate = "2024-03-15T14:00:00",
                Active = false
            };
        }
    }
}
