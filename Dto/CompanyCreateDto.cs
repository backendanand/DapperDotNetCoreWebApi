using WebApiUsingDapper.Entities;

namespace WebApiUsingDapper.Dto
{
    public class CompanyCreateDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Country { get; set; }
    }
}
