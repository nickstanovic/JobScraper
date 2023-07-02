using Microsoft.Playwright;

string jobSearchTerm = "C#";
string location = "Cuyahoga Falls, OH";
int radius = 50;
int secondsToWait = 10;

string encodedJobSearchTerm = System.Web.HttpUtility.UrlEncode(jobSearchTerm);
string encodedLocation = System.Web.HttpUtility.UrlEncode(location);

string indeedUrl = $"https://www.indeed.com/jobs?q={encodedJobSearchTerm}&l={encodedLocation}&radius={radius}";

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
var context = await browser.NewContextAsync();
var openPage = await context.NewPageAsync();
await openPage.GotoAsync(indeedUrl);
await openPage.WaitForTimeoutAsync(secondsToWait * 1000);


// todo: add pagination to scrape all listings
// todo: add key terms filter to list the highest amount of matches first

// Get job titles
var titleElements = await openPage.QuerySelectorAllAsync("h2.jobTitle");
var titles = await Task.WhenAll(titleElements.Select(async t => await t.InnerTextAsync()));

// Get company names
var companyElements = await openPage.QuerySelectorAllAsync("span.companyName");
var companyNames = await Task.WhenAll(companyElements.Select(async c => await c.InnerTextAsync()));

// Get locations
var locationElements = await openPage.QuerySelectorAllAsync("div.companyLocation");
var locations = await Task.WhenAll(locationElements.Select(async l => (await l.InnerTextAsync()).Trim()));

// Prepare a list for descriptions
var descriptions = new List<string>();

for (int i = 0; i < titles.Length; i++)
{
    // Click the job title to load the description
    await titleElements[i].ClickAsync();

    // Wait for the description to load
    await openPage.WaitForTimeoutAsync(secondsToWait * 1000);

    // Get the job description
    var jobDescriptionElement = await openPage.QuerySelectorAsync("#jobDescriptionText");
    string description = await jobDescriptionElement.InnerTextAsync();

    descriptions.Add(description);
}

for (int i = 0; i < titles.Length; i++)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("############################################################");
    Console.ResetColor();
    Console.WriteLine($"Job Title: {titles[i]}");
    Console.WriteLine($"Company {companyNames[i]}");
    Console.WriteLine($"Location: {locations[i]}");
    Console.WriteLine($"Description: {descriptions[i]}");
    Console.WriteLine();
}

await context.CloseAsync();
