using System.Web;
using Microsoft.Playwright;

string jobSearchTerm = "C#";
string location = "Cuyahoga Falls, OH";
int radius = 50;
int secondsToWait = 7;

string encodedJobSearchTerm = HttpUtility.UrlEncode(jobSearchTerm);
string encodedLocation = HttpUtility.UrlEncode(location);

string indeedUrl = $"https://www.indeed.com/jobs?q={encodedJobSearchTerm}&l={encodedLocation}&radius={radius}";

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
var context = await browser.NewContextAsync();
var openPage = await context.NewPageAsync();
await openPage.GotoAsync(indeedUrl);
await openPage.WaitForTimeoutAsync(secondsToWait * 1000);

// todo: add click behavior to get each job description, currently only first job description is retrieved
// todo: properly organize scraping loop to mimic listings
// todo: add pagination to scrape all listings
// todo: may think of more things at a later time

// get job titles
var titleElements = await openPage.QuerySelectorAllAsync("span");
var titles = await Task.WhenAll(titleElements.Select(async t => await t.GetAttributeAsync("title")));
foreach (var title in titles)
{
    Console.WriteLine(title);
}

// get company names
var companyElements = await openPage.QuerySelectorAllAsync("span.companyName");
var companyNames = await Task.WhenAll(companyElements.Select(async c => await c.InnerTextAsync()));
foreach (var name in companyNames)
{
    Console.WriteLine(name);
}

// get locations
var locationElements = await openPage.QuerySelectorAllAsync("div.companyLocation");
var locations = await Task.WhenAll(locationElements.Select(async l => (await l.InnerTextAsync()).Trim()));
foreach (var loc in locations)
{
    Console.WriteLine(loc);
}

// get job description
var jobDescriptionElement = await openPage.QuerySelectorAsync("#jobDescriptionText");
if (jobDescriptionElement != null)
{
    var description = await jobDescriptionElement.InnerTextAsync();
    Console.WriteLine(description);
}

await context.CloseAsync();