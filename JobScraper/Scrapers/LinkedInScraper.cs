using JobScraper.Models;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace JobScraper.Scrapers
{
    public class LinkedInScraper : JobScraper
    {
        private readonly string _linkedinUrl;
        private readonly IBrowserContext _context;

        public LinkedInScraper(string jobSearchTerm, int linkedInListingAge, IBrowserContext context)
        {
            JobSearchTerm = jobSearchTerm;
            var encodedJobSearchTerm = System.Web.HttpUtility.UrlEncode(jobSearchTerm);
            var encodedLocation = System.Web.HttpUtility.UrlEncode(Location);
            _linkedinUrl = $"https://www.linkedin.com/jobs/search/?distance={Radius}&f_TPR=r{linkedInListingAge * 86400}&keywords={encodedJobSearchTerm}&location={encodedLocation}";
            _context = context;
        }

        public override async Task<List<Job>> ScrapeJobsAsync()
        {
            var jobs = new List<Job>();

            var linkedinPage = await _context.NewPageAsync();
            await linkedinPage.GotoAsync(_linkedinUrl);
            await linkedinPage.WaitForTimeoutAsync(SecondsToWait * 1000);
            Console.WriteLine($"LinkedIn begin scraping...");
            var linkedinTitleElements = await linkedinPage.QuerySelectorAllAsync("h3.base-search-card__title");
            var linkedinTitles = await Task.WhenAll(linkedinTitleElements.Select(async t => await t.InnerTextAsync()));

            var linkedinCompanyElements = await linkedinPage.QuerySelectorAllAsync("a.hidden-nested-link");
            var linkedinCompanyNames =
                await Task.WhenAll(linkedinCompanyElements.Select(async c => await c.InnerTextAsync()));

            var linkedinLocationElements = await linkedinPage.QuerySelectorAllAsync("span.job-search-card__location");
            var linkedinLocations =
                await Task.WhenAll(linkedinLocationElements.Select(async l => (await l.InnerTextAsync()).Trim()));

            var linkedinApplyElements = await linkedinPage.QuerySelectorAllAsync("a.base-card__full-link");
            var linkedinApplyUrls =
                await Task.WhenAll(linkedinApplyElements.Select(async l => await l.GetAttributeAsync("href")));

            for (var i = 0; i < linkedinTitles.Length; i++)
            {
                await linkedinApplyElements[i].ClickAsync();
                await linkedinPage.WaitForTimeoutAsync(SecondsToWait * 1000);

                var linkedinJobDescriptionElement = await linkedinPage.QuerySelectorAsync(".description__text");
                var linkedinDescription = linkedinJobDescriptionElement != null
                    ? Regex.Replace(await linkedinJobDescriptionElement.InnerTextAsync(), @"\s{2,}", " ")
                    : "";
                // Regex Explanation: Cleans up description significantly by matching any sequence of whitespace characters
                // (spaces, tabs, line breaks etc.) which are repeated 2 or more times. It is replaced with a single space,
                // effectively condensing all instances of multiple whitespaces down to a single space.

                var foundKeywordsForJob = Keywords
                    .Where(keyword => linkedinDescription.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (AvoidJobKeywords.Any(uk => linkedinTitles[i].Contains(uk, StringComparison.OrdinalIgnoreCase)) || 
                    !foundKeywordsForJob.Any())
                {
                    continue;
                }

                var job = new Job
                {
                    Origin = "LinkedIn",
                    SearchTerm = JobSearchTerm,
                    Title = linkedinTitles[i],
                    CompanyName = linkedinCompanyNames[i],
                    Location = linkedinLocations[i],
                    Description = linkedinDescription,
                    FoundKeywords = string.Join(",", foundKeywordsForJob),
                    ApplyUrl = linkedinApplyUrls[i],
                    ScrapedAt = DateTime.Now
                };

                jobs.Add(job);
            }

            Console.WriteLine("LinkedIn scraping complete.");
            return jobs;
        }
    }
}