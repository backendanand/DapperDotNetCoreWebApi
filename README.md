# Asp.Net Core Web Api with Dapper

## Installation
- Create a new Asp.Net Core Web Api project
- Install the following packages
  - Install-Package `Dapper`
  - Install-Package `Microsoft.Data.SqlClient`
  - 
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
