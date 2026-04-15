using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsAPI.Application.DTO
{
    public class PatientDto
    {
        public Guid Id { get; set; }
        public HumanNameDto Name { get; set; } = null!;
        public string? Gender { get; set; }
        public string BirthDate { get; set; } = null!;
        public bool Active { get; set; }
    }

    public class HumanNameDto
    {
        public Guid Id { get; set; }
        public string? Use { get; set; }
        public string Family { get; set; } = null!;
        public List<string> Given { get; set; } = new();
    }
}
