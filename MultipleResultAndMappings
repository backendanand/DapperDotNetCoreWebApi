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
[HttpGet("{id}/MultipleResult")]
public async Task<IActionResult> GetMultipleResult(int id)
{
    if (id == 0) return BadRequest();
    var company = await _companyRepository.GetMultipleResults(id);
    if (company is null) return NotFound();
    return Ok(company);
}
```
