using Microsoft.Playwright;
using indeed_scraper;

const string jobSearchTerm = "C#";
const string location = "Cuyahoga Falls, OH";
const int radius = 50;
const int secondsToWait = 10;
string[] keywords =
{
    "C#", ".net", "sql", "blazor", "razor", "asp.net", "ef core", "typescript", "javascript", "angular", "git", "html", 
    "css", "tailwind", "material", "bootstrap"
};
string[] avoidJobKeywords =
{
    "lead", "senior"
};

var encodedJobSearchTerm = System.Web.HttpUtility.UrlEncode(jobSearchTerm);
var encodedLocation = System.Web.HttpUtility.UrlEncode(location);

var indeedUrl = $"https://www.indeed.com/jobs?q={encodedJobSearchTerm}&l={encodedLocation}&radius={radius}";

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Firefox.LaunchAsync();
var context = await browser.NewContextAsync(new BrowserNewContextOptions
{
    UserAgent = "Mozilla/5.0 (Windows NT 10.0; rv:114.0) Gecko/20100101 Firefox/114.0"
});
var openPage = await context.NewPageAsync();
await openPage.GotoAsync(indeedUrl);
await openPage.WaitForTimeoutAsync(secondsToWait * 1000);

// todo: add scraping zip recruiter, linkedin, and monster
// todo: make web app gui (either angular or blazor haven't decided yet)

using (var db = new JobDbContext())
{
    db.Database.EnsureCreated(); 

    var pageCount = 0;
    var hasNextPage = true;

    while (hasNextPage)
    {
        pageCount++;
        Console.WriteLine($"Scraping page {pageCount}...");

        var titleElements = await openPage.QuerySelectorAllAsync("h2.jobTitle");
        var titles = await Task.WhenAll(titleElements.Select(async t => await t.InnerTextAsync()));

        var companyElements = await openPage.QuerySelectorAllAsync("span.companyName");
        var companyNames = await Task.WhenAll(companyElements.Select(async c => await c.InnerTextAsync()));

        var locationElements = await openPage.QuerySelectorAllAsync("div.companyLocation");
        var locations = await Task.WhenAll(locationElements.Select(async l => (await l.InnerTextAsync()).Trim()));

        for (var i = 0; i < titles.Length; i++)
        {
            await titleElements[i].ClickAsync();
            await openPage.WaitForTimeoutAsync(secondsToWait * 1000);

            var jobDescriptionElement = await openPage.QuerySelectorAsync("#jobDescriptionText");
            var description = await jobDescriptionElement.InnerTextAsync();

            var foundKeywordsForJob = keywords
                .Where(keyword => description.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

            if (avoidJobKeywords.Any(uk => titles[i].Contains(uk, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var job = new Job
            {
                Title = titles[i],
                CompanyName = companyNames[i],
                Location = locations[i],
                Description = description,
                FoundKeywords = string.Join(",", foundKeywordsForJob)
            };

            db.Jobs.Add(job);
        }
        
        await db.SaveChangesAsync();

        Console.WriteLine($"Page {pageCount} complete.");

        var nextButton = await openPage.QuerySelectorAsync("a[data-testid='pagination-page-next']");
        if (nextButton != null)
        {
            await nextButton.ClickAsync();
        }
        else
        {
            hasNextPage = false;
        }
    }

    Console.WriteLine("Scraping complete.");


    await context.CloseAsync();
}
