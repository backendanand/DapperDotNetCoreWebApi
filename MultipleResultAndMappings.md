# Multiple Results
## ICompanyRespository
```
public Task<Company> GetMultipleResults(int id);
```

## CompanyRepository
```
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
```

## CompaniesController
```
[HttpGet("{id}/WithAllEmployees")]
public async Task<IActionResult> GetMultipleResult(int id)
{
    if (id == 0) return BadRequest();
    var company = await _companyRepository.GetMultipleResults(id);
    if (company is null) return NotFound();
    return Ok(company);
}
```


# Multiple Mappings
## ICompanyRepository
```
public Task<List<Company>> MultipleMappings();
```

## CompanyRepository
```
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
```

## CompaniesController
```
[HttpGet("WithAllEmployees")]
public async Task<IActionResult> MultipleMappings()
{
    var companies = await _companyRepository.MultipleMappings();
    return Ok(companies);
}
```

