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

/*
 
-----
Indeed.com retrieve all job titles, company names, locations, days ago posted, and job description in vanilla javascript
-----
job titles:

const spanElements = document.querySelectorAll('span');

spanElements.forEach((element) => {
  const titleValue = element.title;
  console.log(titleValue);
});

---
company names:

const companyNames = document.querySelectorAll('span.companyName');

companyNames.forEach(function(element) {
  console.log(element.innerText);
});

---
locations:

const companyLocations = document.querySelectorAll('div.companyLocation');

companyLocations.forEach((element) => {
  const textContent = element.textContent.trim();
  console.log(textContent);
});

---
descriptions:

const parentElement = document.getElementById('jobDescriptionText');

function extractTextContent(element) {
  let text = '';

// Loop through each child node of the current element
  for (let i = 0; i < element.childNodes.length; i++) {
    const childNode = element.childNodes[i];

// Check if the child node is a text node
    if (childNode.nodeType === Node.TEXT_NODE) {
      // Extract the text content and append it to the result
      text += childNode.textContent.trim();
    } else if (childNode.nodeType === Node.ELEMENT_NODE) {
      // Recursively call the function for child elements
      text += extractTextContent(childNode);
    }
  }

  return text;
}

const extractedText = extractTextContent(parentElement);

console.log(extractedText)
---

*/