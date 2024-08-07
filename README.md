# Asp.Net Core Web Api with Dapper

## Installation
- Create a new Asp.Net Core Web Api project
- Install the following packages
  - Install-Package `Dapper`
  - Install-Package `Microsoft.Data.SqlClient`
    
## Connection String
- Open `appsettings.json` file and write the connection string
  ```
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=DESKTOP-5EVKAPH\\SQLEXPRESS;Initial Catalog=WebApiDapperDb;TrustServerCertificate=True;Trusted_Connection=True;"
  }
  ```
  
## Dapper Context
- Create a folder `Context` in the root directory
  - Create a class `DapperContext` inside `Context` folder
  - Write the following code
    ```
    private readonly IConfiguration _iconfiguration;
    private readonly string _connectionString;
    public DapperContext(IConfiguration configuration)
    {
        _iconfiguration = configuration;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    ```

## Add Service in `Program.cs`
  ```
  builder.Services.AddSingleton<DapperContext>();
  ```

## Model Classes
- Create a folder `Entities` in the root directory
  - Create a class `Employee` inside `Entities` folder
    - Write the following code inside it
      ```
      public int Id { get; set; }
      public string? Name { get; set; }
      public int Age { get; set; }
      public string? Position { get; set; }
      public int CompanyId { get; set; }
      ```
  - Create a class `Company` inside `Entities` folder
    - Write the following code inside it
      ```
      public int Id { get; set; }
      public string? Name { get; set; }
      public string? Address { get; set; }
      public string? Country { get; set; }
      public List<Employee> Employees { get; set; } = new List<Employee>();
      ```
## Contract Interfaces and Repository Classes
- Create a folder `Contracts` in the root directory
  - Create an interface `ICompanyRepository`
    - Write the following code
      ```
      public Task<IEnumerable<Company>> GetCompanies();
      ```
- Create a folder `Repositories` in the root directory
  - Create a class `CompanyRepository`
    - Write the following code
      ```
      public class CompanyRepository : ICompanyRepository
      {
          private readonly ICompanyRepository _companyRepository;
          private readonly DapperContext _context;
      
          public CompanyRepository(DapperContext context) => _context = context;
      
          public async Task<IEnumerable<Company>> GetCompanies()
          {
              var query = "SELECT * FROM Companies";
              using (var connection = _context.CreateConnection())
              {
                  var companies = await connection.QueryAsync<Company>(query);
                  return companies.ToList();
              }
          }
      }
      ```
## Add Services
- Open `Program.cs` file and add the following code
  ```
  builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
  ```

## Company Controller Action Methods
- Create an API Empty Controller as `CompaniesController`
- Write the following code
  ```
  private readonly ICompanyRepository _companyRepository;
  public CompaniesController(ICompanyRepository companyRepository) => _companyRepository = companyRepository;
  [HttpGet]
  public async Task<IActionResult> GetAllCompanies()
  {
      var companies = await _companyRepository.GetCompanies();
      return Ok(companies);
  }
  ```

## Create Api with POST Method
### Creating Data Transfer Object
- Create a folder `Dto` in the root directory
  - Create a class `CompanyCreateDto` inside `Dto`
    Write the following code
    ```
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    ```
### ICompany Repository
  Write the following code
  ```
  ...
  public Task<Company> CreateCompany(CompanyCreateDto company);
  ```
### Defining in `CompanyRepository`
  Write the following code
  ```
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
  ```
### Defining Controller Function
- Create a `HttpPost` `Async` function inside `CompanyController`
  Write the following code
  ```
  [HttpPost]
  public async Task<IActionResult> CreateCompany([FromForm]CompanyCreateDto company)
  {
      var createdCompany = await _companyRepository.CreateCompany(company);
      return CreatedAtRoute("CompanyById", new { id = createdCompany.Id }, createdCompany);
  }
  ```

## Update api with Update Method
### Create DTO
```
public class CompanyUpdateDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
}
```
### ICompanyRepository
```
public Task UpdateCompany(int id, CompanyUpdateDto company); 
```
### CompanyRepository
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
### CompaniesController
```
[HttpPut("{id:int}")]
public async Task<IActionResult> UpdateCompany(int id, [FromForm]CompanyUpdateDto company)
{
    if (id == 0 || id != company.Id) return BadRequest();

    var companyToUpdate = await _companyRepository.GetCompany(id);
    if(companyToUpdate is null) return NotFound();

    await _companyRepository.UpdateCompany(id, company);
    return NoContent();
}
```

