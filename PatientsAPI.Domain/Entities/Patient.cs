using PatientsAPI.Domain.Common;
using PatientsAPI.Domain.Enums;
using PatientsAPI.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsAPI.Domain.Entities
{
    public class Patient : BaseEntity, IAggregateRoot
    {
        public HumanName Name { get; private set; }
        public Gender? Gender { get; private set; }
        public DateTime BirthDate { get; private set; }
        public bool Active { get; private set; }

        private Patient() { }

        public static Patient Create(
            HumanName name,
            Gender? gender,
            DateTime birthDate,
            bool active = true)
        {
            if (name == null)
                throw new DomainException("Patient name is required");

            if (birthDate > DateTime.UtcNow)
                throw new DomainException("Birth date cannot be in the future");

            if (birthDate.Year < 1900)
                throw new DomainException("Birth date cannot be before 1900");

            return new Patient
            {
                Name = name,
                Gender = gender,
                BirthDate = birthDate,
                Active = active
            };
        }

        public void Update(
            HumanName name,
            Gender? gender,
            DateTime birthDate,
            bool active)
        {
            if (name == null)
                throw new DomainException("Patient name is required");

            if (birthDate > DateTime.UtcNow)
                throw new DomainException("Birth date cannot be in the future");

            if (birthDate.Year < 1900)
                throw new DomainException("Birth date cannot be before 1900");

            Name = name;
            Gender = gender;
            BirthDate = birthDate;
            Active = active;

            SetUpdated();
        }
    }

    public interface IAggregateRoot { }
}
