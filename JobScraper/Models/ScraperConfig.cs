namespace JobScraper.Models
{
    public static class ScraperConfig
    {
        public static readonly string Location = "Cuyahoga Falls, OH";
        public static readonly int Radius = 50;
        public static readonly int SecondsToWait = 10;
        public static readonly string[] Keywords = { "c#", ".net", "sql", "blazor", "razor", "asp.net", "ef core", "entity framework", "typescript", "javascript", "angular", "git", "html", "css", "tailwind", "material", "bootstrap" };
        public static readonly string[] AvoidJobKeywords = { "lead", "senior" };
    }
}