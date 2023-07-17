-- Easiest way to clean uo duplicate jobs
WITH DuplicateJobs AS (
    SELECT *,
           ROW_NUMBER() OVER (
            PARTITION BY Title, CompanyName, Location
            ORDER BY ScrapedAt DESC
        ) AS RowNumber
    FROM Jobs
)
DELETE FROM DuplicateJobs
WHERE RowNumber > 1;
