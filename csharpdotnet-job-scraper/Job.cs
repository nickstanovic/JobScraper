public class Job
{
    public int Id { get; set; }
    public string SearchTerm { get; set; }
    public string Title { get; set; }
    public string CompanyName { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public string FoundKeywords { get; set; }  // Modified to be storable in DB
    public string? ApplyUrl { get; set; }
    public DateTime ScrapedAt { get; set; }
}