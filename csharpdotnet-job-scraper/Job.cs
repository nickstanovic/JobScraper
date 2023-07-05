namespace indeed_scraper
{
    public class Job
    {
        public string Title { get; init; }
        public string CompanyName { get; init; }
        public string Location { get; init; }
        public string Description { get; init; }
        public List<string> FoundKeywords { get; init; }
    }
}