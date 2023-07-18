using JobScraper.Models;

namespace JobScraper.Scrapers
{
    public abstract class JobScraper
    {
        protected string JobSearchTerm => ScraperConfig.JobSearchTerm;
        protected string Location => ScraperConfig.Location;
        protected int Radius => ScraperConfig.Radius;
        protected int SecondsToWait => ScraperConfig.SecondsToWait;
        protected string[] Keywords => ScraperConfig.Keywords;
        protected string[] AvoidJobKeywords => ScraperConfig.AvoidJobKeywords;

        public abstract Task<List<Job>> ScrapeJobsAsync();
    }
}