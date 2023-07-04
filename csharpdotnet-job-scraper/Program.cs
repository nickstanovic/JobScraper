using Microsoft.Playwright;


string jobSearchTerm = "C#";
string location = "Cuyahoga Falls, OH";
int radius = 50;
int secondsToWait = 10;
string[] keywords =
{
    "C#", ".net", "sql", "angular", "javascript", "git", "html", "css", "tailwind", "typescript", "node", "python",
    "material ui", "bootstrap"
};

string encodedJobSearchTerm = System.Web.HttpUtility.UrlEncode(jobSearchTerm);
string encodedLocation = System.Web.HttpUtility.UrlEncode(location);

string indeedUrl = $"https://www.indeed.com/jobs?q={encodedJobSearchTerm}&l={encodedLocation}&radius={radius}";

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
var context = await browser.NewContextAsync();
var openPage = await context.NewPageAsync();
await openPage.GotoAsync(indeedUrl);
await openPage.WaitForTimeoutAsync(secondsToWait * 1000);

// Get job titles
var titleElements = await openPage.QuerySelectorAllAsync("h2.jobTitle");
var titles = await Task.WhenAll(titleElements.Select(async t => await t.InnerTextAsync()));

// Get company names
var companyElements = await openPage.QuerySelectorAllAsync("span.companyName");
var companyNames = await Task.WhenAll(companyElements.Select(async c => await c.InnerTextAsync()));

// Get locations
var locationElements = await openPage.QuerySelectorAllAsync("div.companyLocation");
var locations = await Task.WhenAll(locationElements.Select(async l => (await l.InnerTextAsync()).Trim()));

List<Job> jobs = new List<Job>();

for (int i = 0; i < titles.Length; i++)
{
    // Click the job title to load the description
    await titleElements[i].ClickAsync();
    await openPage.WaitForTimeoutAsync(secondsToWait * 1000);

    // Get the job description
    var jobDescriptionElement = await openPage.QuerySelectorAsync("#jobDescriptionText");
    string description = await jobDescriptionElement.InnerTextAsync();

    List<string> foundKeywordsForJob = keywords.Where(keyword => description.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

    jobs.Add(new Job
    {
        Title = titles[i],
        CompanyName = companyNames[i],
        Location = locations[i],
        Description = description,
        FoundKeywords = foundKeywordsForJob
    });
}

var sortedJobs = jobs.OrderByDescending(job => job.FoundKeywords.Count);

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

internal class Job
{
    public string Title { get; init; }
    public string CompanyName { get; init; }
    public string Location { get; init; }
    public string Description { get; init; }
    public List<string> FoundKeywords { get; init; }
}