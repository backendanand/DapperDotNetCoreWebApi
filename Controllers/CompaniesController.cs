using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiUsingDapper.Contracts;
using WebApiUsingDapper.Dto;

namespace WebApiUsingDapper.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        public CompaniesController(ICompanyRepository companyRepository) => _companyRepository = companyRepository;

        [HttpGet]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies = await _companyRepository.GetCompanies();
            return Ok(companies);
        }

        [HttpGet("{id:int}", Name ="CompanyById")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            if (id == 0) return BadRequest();

            var company = await _companyRepository.GetCompany(id);
            if(company is null) return NotFound();

            return Ok(company);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromForm]CompanyCreateDto company)
        {
            var createdCompany = await _companyRepository.CreateCompany(company);
            return CreatedAtRoute("CompanyById", new { id = createdCompany.Id }, createdCompany);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromForm]CompanyUpdateDto company)
        {
            if (id == 0 || id != company.Id) return BadRequest();

            var companyToUpdate = await _companyRepository.GetCompany(id);
            if(companyToUpdate is null) return NotFound();

            await _companyRepository.UpdateCompany(id, company);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            if(id == 0) return BadRequest();

            var companyToDelete = await _companyRepository.GetCompany(id);
            if (companyToDelete is null) return NotFound();

            await _companyRepository.DeleteCompany(id);
            return NoContent();
        }

        [HttpGet("ByEmployeeId/{id}")]
        public async Task<IActionResult> GetCompanyForEmployee(int id)
        {
            try
            {
                var company = await _companyRepository.GetCompanyByEmployeeId(id);
                if (company is null) return NotFound();
                return Ok(company);
            }
            catch (TaskCanceledException ex)
            {
                // Handle the task canceled exception
                return StatusCode(StatusCodes.Status408RequestTimeout, "Request was canceled.");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            
        }

        [HttpGet("{id}/WithAllEmployees")]
        public async Task<IActionResult> GetMultipleResult(int id)
        {
            if (id == 0) return BadRequest();
            var company = await _companyRepository.GetMultipleResults(id);
            if (company is null) return NotFound();
            return Ok(company);
        }

        [HttpGet("WithAllEmployees")]
        public async Task<IActionResult> MultipleMappings()
        {
            var companies = await _companyRepository.MultipleMappings();
            return Ok(companies);
        }
    }
}
