using Microsoft.AspNetCore.Mvc;
using PatientsAPI.Application.Common.Interfaces;
using PatientsAPI.Application.DTO;

namespace PatientsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        /// <summary>
        /// Get all patients
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all patients</returns>
        /// <response code="200">Returns the list of patients</response>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<PatientDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<PatientDto>>> GetAll(CancellationToken cancellationToken)
        {
            var patients = await _patientService.GetAllAsync(cancellationToken);
            return Ok(patients);
        }

        /// <summary>
        /// Get patient by ID
        /// </summary>
        /// <param name="id">Patient unique identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Patient details</returns>
        /// <response code="200">Returns the patient</response>
        /// <response code="404">Patient not found</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientDto>> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var patient = await _patientService.GetByIdAsync(id, cancellationToken);
            return Ok(patient);
        }

        /// <summary>
        /// Search patients by birth date (FHIR compliant)
        /// </summary>
        /// <param name="birthDate">
        /// Date parameter with optional FHIR prefix.
        /// Examples: 
        /// - "2024-01-01" (exact date)
        /// - "gt2024-01-01" (greater than)
        /// - "ge2024-01-01" (greater or equal)
        /// - "lt2024-01-01" (less than)
        /// - "le2024-01-01" (less or equal)
        /// - "eq2024-01-01" (equal, default)
        /// - "ne2024-01-01" (not equal)
        /// </param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of matching patients</returns>
        /// <response code="200">Returns the matching patients</response>
        /// <response code="400">Invalid date format</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IReadOnlyList<PatientDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IReadOnlyList<PatientDto>>> SearchByBirthDate(
            [FromQuery(Name = "birthDate")] string birthDate,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(birthDate))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = "birthDate parameter is required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var patients = await _patientService.SearchByBirthDateAsync(birthDate, cancellationToken);
            return Ok(patients);
        }

        /// <summary>
        /// Create a new patient
        /// </summary>
        /// <param name="dto">Patient data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created patient</returns>
        /// <response code="201">Patient created successfully</response>
        /// <response code="400">Validation error</response>
        [HttpPost]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PatientDto>> Create(
            [FromBody] CreatePatientDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var patient = await _patientService.CreateAsync(dto, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = patient.Id },
                patient);
        }

        /// <summary>
        /// Update an existing patient
        /// </summary>
        /// <param name="id">Patient unique identifier</param>
        /// <param name="dto">Updated patient data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated patient</returns>
        /// <response code="200">Patient updated successfully</response>
        /// <response code="400">Validation error</response>
        /// <response code="404">Patient not found</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientDto>> Update(
            [FromRoute] Guid id,
            [FromBody] UpdatePatientDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var patient = await _patientService.UpdateAsync(id, dto, cancellationToken);
            return Ok(patient);
        }

        /// <summary>
        /// Delete a patient
        /// </summary>
        /// <param name="id">Patient unique identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content</returns>
        /// <response code="204">Patient deleted successfully</response>
        /// <response code="404">Patient not found</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            await _patientService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
