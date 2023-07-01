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

await context.CloseAsync();