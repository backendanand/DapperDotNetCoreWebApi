namespace WebApiUsingDapper.Dto
{
    public class CompanyUpdateDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Country { get; set; }
    }
}
