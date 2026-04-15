using PatientsAPI.Domain.Common;
using PatientsAPI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsAPI.Domain.ValueObjects
{

    public class HumanName : IEquatable<HumanName>
    {
        public Guid Id { get; private set; }
        public HumanNameUse? Use { get; private set; }
        public string Family { get; private set; }
        public IReadOnlyList<string> Given { get; private set; }

        private HumanName()
        {
            Given = new List<string>();
        }

        public static HumanName Create(string family, IEnumerable<string>? given = null, HumanNameUse? use = HumanNameUse.Official)
        {
            if (string.IsNullOrWhiteSpace(family))
                throw new DomainException("Family name is required");

            return new HumanName
            {
                Id = Guid.NewGuid(),
                Family = family,
                Given = given?.ToList() ?? new List<string>(),
                Use = use
            };
        }

        public static HumanName CreateWithId(Guid id, string family, IEnumerable<string>? given = null, HumanNameUse? use = HumanNameUse.Official)
        {
            var name = Create(family, given, use);
            name.Id = id;
            return name;
        }

        public HumanName Update(string? family = null, IEnumerable<string>? given = null, HumanNameUse? use = null)
        {
            return new HumanName
            {
                Id = this.Id,
                Family = family ?? this.Family,
                Given = given?.ToList() ?? this.Given.ToList(),
                Use = use ?? this.Use
            };
        }

        public bool Equals(HumanName? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Id == other.Id &&
                   Use == other.Use &&
                   Family == other.Family &&
                   Given.SequenceEqual(other.Given);
        }

        public override bool Equals(object? obj) => Equals(obj as HumanName);

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Id);
            hashCode.Add(Use);
            hashCode.Add(Family);
            foreach (var g in Given) hashCode.Add(g);
            return hashCode.ToHashCode();
        }
    }
}
