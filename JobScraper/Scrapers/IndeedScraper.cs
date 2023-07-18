using JobScraper.Models;
using Microsoft.Playwright;

namespace JobScraper.Scrapers
{
    public class IndeedScraper : JobScraper

    {
        private readonly string _indeedUrl;
        private readonly IBrowserContext _context;

        public IndeedScraper(string jobSearchTerm, int indeedListingAge, IBrowserContext context)
        {
            var encodedJobSearchTerm = System.Web.HttpUtility.UrlEncode(jobSearchTerm);
            var encodedLocation = System.Web.HttpUtility.UrlEncode(Location);
            _indeedUrl =
                $"https://www.indeed.com/jobs?q={encodedJobSearchTerm}&l={encodedLocation}&radius={Radius}&fromage={indeedListingAge}";
            _context = context;
        }

        public override async Task<List<Job>> ScrapeJobsAsync()
        {
            var jobs = new List<Job>();

            var indeedPage = await _context.NewPageAsync();
            await indeedPage.GotoAsync(_indeedUrl);
            await indeedPage.WaitForTimeoutAsync(ScraperConfig.SecondsToWait * 1000);

            var pageCount = 0;
            var hasNextPage = true;

            while (hasNextPage)
            {
                pageCount++;
                Console.WriteLine($"Indeed scraping page {pageCount}...");

                var indeedTitleElements = await indeedPage.QuerySelectorAllAsync("h2.jobTitle");
                var titles = await Task.WhenAll(indeedTitleElements.Select(async t => await t.InnerTextAsync()));

                var indeedCompanyElements = await indeedPage.QuerySelectorAllAsync("span.companyName");
                var companyNames =
                    await Task.WhenAll(indeedCompanyElements.Select(async c => await c.InnerTextAsync()));

                var indeedLocationElements = await indeedPage.QuerySelectorAllAsync("div.companyLocation");
                var locations =
                    await Task.WhenAll(indeedLocationElements.Select(async l => (await l.InnerTextAsync()).Trim()));

                for (var i = 0; i < titles.Length; i++)
                {
                    await indeedTitleElements[i].ClickAsync();
                    await indeedPage.WaitForTimeoutAsync(ScraperConfig.SecondsToWait * 1000);


                    var indeedJobDescriptionElement = await indeedPage.QuerySelectorAsync("#jobDescriptionText");
                    var indeedDescription = await indeedJobDescriptionElement.InnerTextAsync();

                    var indeedApplyUrlElement = await indeedPage.QuerySelectorAsync("span[data-indeed-apply-joburl], " +
                        "button[href*='https://www.indeed.com/applystart?jk=']");
                    string indeedApplyUrl = null;
                    if (indeedApplyUrlElement != null)
                    {
                        var indeedApplyJobUrl =
                            await indeedApplyUrlElement.GetAttributeAsync("data-indeed-apply-joburl");
                        var href = await indeedApplyUrlElement.GetAttributeAsync("href");

                        if (!string.IsNullOrEmpty(indeedApplyJobUrl))
                            indeedApplyUrl = indeedApplyJobUrl;
                        else if (!string.IsNullOrEmpty(href))
                            indeedApplyUrl = href.Split('&')[0];
                    }

                    var foundKeywordsForJob = ScraperConfig.Keywords
                        .Where(keyword => indeedDescription.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (ScraperConfig.AvoidJobKeywords.Any(uk =>
                            titles[i].Contains(uk, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var job = new Job
                    {
                        Origin = "Indeed",
                        SearchTerm = JobSearchTerm,
                        Title = titles[i],
                        CompanyName = companyNames[i],
                        Location = locations[i],
                        Description = indeedDescription,
                        FoundKeywords = string.Join(",", foundKeywordsForJob),
                        ApplyUrl = indeedApplyUrl,
                        ScrapedAt = DateTime.Now,
                    };

                    jobs.Add(job);
                }

                var nextButton = await indeedPage.QuerySelectorAsync("a[data-testid='pagination-page-next']");
                if (nextButton != null)
                {
                    await nextButton.ClickAsync();
                    await indeedPage.WaitForTimeoutAsync(SecondsToWait * 1000);
                }
                else
                {
                    hasNextPage = false;
                }
            }

            Console.WriteLine("Indeed scraping complete.");
            return jobs;
        }
    }

}