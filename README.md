# Job Scraper with C# and Playwright

This project uses C# and Microsoft Playwright to scrape job data from Indeed.com.

## Setup

This project requires .NET and the Microsoft Playwright CLI. If you don't have these installed, follow the instructions below.

1. Install .NET from the [.NET download page](https://dotnet.microsoft.com/download).

2. Install the Microsoft Playwright CLI by running the following command in your terminal:

   ```bash
   dotnet tool install --global Microsoft.Playwright.CLI
   ```

3. After installing the Playwright CLI, run the following command to install the necessary browser binaries:

   ```bash
   playwright install
   ```

## Running the Project

1. Open the project in your favorite IDE.

2. Edit the `jobSearchTerm`, `location`, `radius`, and `secondsToWaits` variables in `Program.cs` to customize your job search.

<span style="color: green">Recommended:</span> Be nice to people's servers by not lowering the `secondsToWait` variable too low. <font size="1">(keep yourself from being banned from the site)</font>
3. Run the project. The scraper will open a browser, navigate to Indeed.com with your search parameters, and scrape the job data. for running the project if there are additional setup steps. Similarly, you might need to adjust the Contributing and License sections to match your project's policies.