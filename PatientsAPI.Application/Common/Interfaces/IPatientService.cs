using PatientsAPI.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsAPI.Application.Common.Interfaces
{
    public interface IPatientService
    {
        Task<PatientDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PatientDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PatientDto>> SearchByBirthDateAsync(string dateParameter, CancellationToken cancellationToken = default);
        Task<PatientDto> CreateAsync(CreatePatientDto dto, CancellationToken cancellationToken = default);
        Task<PatientDto> UpdateAsync(Guid id, UpdatePatientDto dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
