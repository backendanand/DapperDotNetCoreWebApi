using WebApiUsingDapper.Dto;
using WebApiUsingDapper.Entities;

namespace WebApiUsingDapper.Contracts
{
    public interface ICompanyRepository
    {
        public Task<IEnumerable<Company>> GetCompanies();
        public Task<Company> GetCompany(int id);
        public Task<Company> CreateCompany(CompanyCreateDto company);
        public Task UpdateCompany(int id, CompanyUpdateDto company); 
        public Task DeleteCompany(int id);
        public Task<Company> GetCompanyByEmployeeId(int employeeId);
        public Task<Company> GetMultipleResults(int id);
        public Task<List<Company>> MultipleMappings();
    }
}
