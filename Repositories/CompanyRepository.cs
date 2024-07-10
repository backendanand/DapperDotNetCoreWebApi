using Dapper;
using WebApiUsingDapper.Context;
using WebApiUsingDapper.Contracts;
using WebApiUsingDapper.Dto;
using WebApiUsingDapper.Entities;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace WebApiUsingDapper.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly DapperContext _context;
        public CompanyRepository(DapperContext context) => _context = context;

        public async Task<Company> CreateCompany(CompanyCreateDto company)
        {
            var query = "INSERT INTO Companies (Name, Address, Country) VALUES (@Name, @Address, @Country);" + 
                "SELECT CAST(SCOPE_IDENTITY() AS int)";

            var parameters = new DynamicParameters();
            parameters.Add("Name", company.Name, DbType.String);
            parameters.Add("Address", company.Address, DbType.String);
            parameters.Add("Country", company.Country, DbType.String);

            using (var connection = _context.CreateConnection())
            {
                var id = await connection.QuerySingleAsync<int>(query, parameters);
                var createdCompany = new Company
                {
                    Id = id,
                    Name = company.Name,
                    Address = company.Address,
                    Country = company.Country,
                };
                return createdCompany;
            }
        }

        public async Task DeleteCompany(int id)
        {
            var query = "DELETE FROM Companies WHERE Id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("Id", id, DbType.Int32);
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
            }
        }

        public async Task<IEnumerable<Company>> GetCompanies()
        {
            var query = "SELECT Id, Name, Address, Country FROM Companies";
            using (var connection = _context.CreateConnection())
            {
                var companies = await connection.QueryAsync<Company>(query);
                return companies.ToList();
            }
        }

        public async Task<Company> GetCompany(int id)
        {
            var query = "SELECT Id, Name, Address, Country FROM Companies WHERE Id=@Id";
            using (var connection = _context.CreateConnection())
            {
                var company = await connection.QuerySingleOrDefaultAsync<Company>(query, new {id});
                return company;
            }
        }

        public async Task<Company> GetCompanyByEmployeeId(int id)
        {
            var procedureName = "ShowCompanyByEmployeeId";
            var parameters = new DynamicParameters();
            parameters.Add("Id", id, DbType.Int32, ParameterDirection.Input);

            using (var connection = _context.CreateConnection())
            {
                var company = await connection.QueryFirstOrDefaultAsync<Company>(procedureName, parameters, commandType : CommandType.StoredProcedure, commandTimeout: 60);
                return company;
            }
        }

        public async Task<Company> GetMultipleResults(int id)
        {
            var query = "SELECT * FROM Companies WHERE Id=@Id;" +
                "SELECT * FROM Employees WHERE CompanyId=@Id";

            using (var connection = _context.CreateConnection())
                using (var multi = await connection.QueryMultipleAsync(query, new { id }))
            {
                var company = await multi.ReadSingleOrDefaultAsync<Company>();
                if (company is not null)
                {
                    company.Employees = (await multi.ReadAsync<Employee>()).ToList();
                }
                return company;
            }
        }

        public async Task<List<Company>> MultipleMappings()
        {
            var query = "SELECT * FROM Companies c JOIN Employees e ON c.Id = e.CompanyId";
            using (var connection = _context.CreateConnection())
            {
                var companyDict = new Dictionary<int, Company>();
                var companies = await connection.QueryAsync<Company, Employee, Company>(query, (company, employee) =>
                {
                    if(!companyDict.TryGetValue(company.Id, out var currentCompany))
                    {
                        currentCompany = company;
                        companyDict.Add(company.Id, currentCompany);
                    }
                    currentCompany.Employees.Add(employee);
                    return currentCompany;
                });
                return companies.ToList();
            }
        }

        public async Task UpdateCompany(int id, CompanyUpdateDto company)
        {
            var query = "UPDATE Companies SET Name=@Name, Address=@Address, Country=@Country WHERE Id=@Id";

            var parameters = new DynamicParameters();
            parameters.Add("Id", id, DbType.Int32);
            parameters.Add("Name", company.Name, DbType.String);
            parameters.Add("Address", company.Address, DbType.String);
            parameters.Add("Country", company.Country, DbType.String);

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);

            }
        }
    }
}
