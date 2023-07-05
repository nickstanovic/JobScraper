using Microsoft.Playwright;
using indeed_scraper;

string jobSearchTerm = "C#";
string location = "Cuyahoga Falls, OH";
int radius = 50;
int secondsToWait = 10;
string[] keywords =
{
    "C#", ".net", "sql", "blazor", "razor", "asp.net", "ef core", "typescript", "javascript", "angular", "vue", "svelte",
    "git", "html", "css", "tailwind", "material", "bootstrap"
};
string[] avoidJobKeywords =
{
    "lead", "senior"
};

string encodedJobSearchTerm = System.Web.HttpUtility.UrlEncode(jobSearchTerm);
string encodedLocation = System.Web.HttpUtility.UrlEncode(location);

string indeedUrl = $"https://www.indeed.com/jobs?q={encodedJobSearchTerm}&l={encodedLocation}&radius={radius}";

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Firefox.LaunchAsync();
var context = await browser.NewContextAsync(new BrowserNewContextOptions
{
    UserAgent = "Mozilla/5.0 (Windows NT 10.0; rv:114.0) Gecko/20100101 Firefox/114.0"
});
var openPage = await context.NewPageAsync();
await openPage.GotoAsync(indeedUrl);
await openPage.WaitForTimeoutAsync(secondsToWait * 1000);

// todo: store in sql database w/ ef core
// todo: make web app gui (either angular or blazor haven't decided yet)
// todo: add scraping zip recruiter, linkedin, and monster

List<Job> jobs = new List<Job>();
int pageCount = 0;
bool hasNextPage = true;

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

    for (int i = 0; i < titles.Length; i++)
    {
        await titleElements[i].ClickAsync();
        await openPage.WaitForTimeoutAsync(secondsToWait * 1000);

        var jobDescriptionElement = await openPage.QuerySelectorAsync("#jobDescriptionText");
        string description = await jobDescriptionElement.InnerTextAsync();

        List<string> foundKeywordsForJob = keywords.Where(keyword => description.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

        // Skip this job if it contains undesired keywords in the title
        if (avoidJobKeywords.Any(uk => titles[i].Contains(uk, StringComparison.OrdinalIgnoreCase)))
        {
            continue;
        }

        jobs.Add(new Job
        {
            Title = titles[i],
            CompanyName = companyNames[i],
            Location = locations[i],
            Description = description,
            FoundKeywords = foundKeywordsForJob
        });
    }

    Console.WriteLine($"Page {pageCount} complete.");

    var nextButton = await openPage.QuerySelectorAsync("a[data-testid='pagination-page-next']");
    if (nextButton != null)
    {
        await nextButton.ClickAsync();
        await openPage.WaitForNavigationAsync();
    }
    else
    {
        hasNextPage = false;
    }
}

// added delays for console output readability
Console.WriteLine("Scraping complete.");
await Task.Delay(5000);
Console.WriteLine("Starting sorting process...");
await Task.Delay(5000);
Console.WriteLine("This could take several minutes, depending on the number of pages...");

var sortedJobs = jobs.OrderByDescending(job => job.FoundKeywords.Count);

Console.WriteLine("Sorting complete.");
Console.WriteLine();
await Task.Delay(5000);

foreach (var job in sortedJobs)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("############################################################");
    Console.ResetColor();
    Console.WriteLine($"Job Title: {job.Title}");
    Console.WriteLine($"Company: {job.CompanyName}");
    Console.WriteLine($"Location: {job.Location}");
    if (job.FoundKeywords.Any())
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Number of keywords found: {job.FoundKeywords.Count}");
        Console.WriteLine($"Found keywords: {string.Join(", ", job.FoundKeywords)}");
        Console.ResetColor();
    }
    Console.WriteLine($"Description: {job.Description}");
    Console.WriteLine();
}

await context.CloseAsync();
