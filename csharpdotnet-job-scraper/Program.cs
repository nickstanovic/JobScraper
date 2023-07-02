using Microsoft.Playwright;

string jobSearchTerm = "C#";
string location = "Cuyahoga Falls, OH";
int radius = 50;
int secondsToWait = 7;

string encodedJobSearchTerm = System.Web.HttpUtility.UrlEncode(jobSearchTerm);
string encodedLocation = System.Web.HttpUtility.UrlEncode(location);

string indeedUrl = $"https://www.indeed.com/jobs?q={encodedJobSearchTerm}&l={encodedLocation}&radius={radius}";

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
var context = await browser.NewContextAsync();
var openPage = await context.NewPageAsync();
await openPage.GotoAsync(indeedUrl);
await openPage.WaitForTimeoutAsync(secondsToWait * 1000);


// todo: add click behavior to get each job description, currently only first job description is retrieved
// todo: add pagination to scrape all listings

// Get job titles
var titleElements = await openPage.QuerySelectorAllAsync("h2.jobTitle");
var titles = await Task.WhenAll(titleElements.Select(async t => await t.InnerTextAsync()));

// Get company names
var companyElements = await openPage.QuerySelectorAllAsync("span.companyName");
var companyNames = await Task.WhenAll(companyElements.Select(async c => await c.InnerTextAsync()));

// Get locations
var locationElements = await openPage.QuerySelectorAllAsync("div.companyLocation");
var locations = await Task.WhenAll(locationElements.Select(async l => (await l.InnerTextAsync()).Trim()));

for (int i = 0; i < titles.Length; i++)
{
    Console.WriteLine(titles[i]);
    Console.WriteLine(companyNames[i]);
    Console.WriteLine(locations[i]);
    Console.WriteLine();
}

await context.CloseAsync();