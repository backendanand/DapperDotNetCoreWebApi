# API using Stored Procedure

## Create Stored Procedures
Open SQL Server, Inside your database, navigate to StoredProcedures.
- Right click on `Stored Procedures`, then click on `Stored Procedure`
- Write the following query
  ```
  CREATE PROCEDURE [dbo].[ShowCompanyByEmployeeId]
  	@Id int
  AS
  	SELECT c.Id, c.Name, c.Address, c.Country
  	FROM Companies c INNER JOIN Employees e 
  	ON c.Id = e.CompanyId
  	WHERE e.Id = @Id
  ```
- Execute the query

## ICompanyRepository
```
public Task<Company> GetCompanyByEmployeeId(int employeeId);
```

## CompanyRepository
```
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
```

## Companies Controller
```
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
```
