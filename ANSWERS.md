# Aihrly Assessment Answers

## 1. Schema Question

### Table Sketches (Plain Text / SQL-like)

```sql
-- Applications
CREATE TABLE Applications (
    Id INT PRIMARY KEY,
    JobId INT REFERENCES Jobs(Id),
    CandidateName TEXT NOT NULL,
    CandidateEmail TEXT NOT NULL,
    CurrentStage INT NOT NULL, -- Enum: Applied, Screening, etc.
    CoverLetter TEXT,
    -- Unique constraint for duplicate application rule
    CONSTRAINT UQ_Job_Email UNIQUE (JobId, CandidateEmail)
);

-- ApplicationNotes
CREATE TABLE ApplicationNotes (
    Id INT PRIMARY KEY,
    ApplicationId INT REFERENCES Applications(Id),
    Type INT NOT NULL,
    Description TEXT NOT NULL,
    CreatedById INT REFERENCES TeamMembers(Id),
    CreatedAt TIMESTAMP DEFAULT UTC_NOW
);

-- StageHistory
CREATE TABLE StageHistories (
    Id INT PRIMARY KEY,
    ApplicationId INT REFERENCES Applications(Id),
    FromStage INT NOT NULL,
    ToStage INT NOT NULL,
    ChangedById INT REFERENCES TeamMembers(Id),
    ChangedAt TIMESTAMP DEFAULT UTC_NOW,
    Comment TEXT
);

-- Scores (Separate tables as per ERD/Requirement for individual tracking)
CREATE TABLE CultureFitScores (
    Id INT PRIMARY KEY,
    ApplicationId INT UNIQUE REFERENCES Applications(Id),
    UpdatedById INT REFERENCES TeamMembers(Id),
    Score INT CHECK (Score >= 1 AND Score <= 5),
    Comment TEXT,
    UpdatedAt TIMESTAMP
);
-- (Similar tables for InterviewScores and AssessmentScores)
```

### Indexes
- `IX_Applications_JobId`: Speeds up listing applications for a specific job.
- `UQ_Job_Email`: Enforces the business rule that a candidate can't apply to the same job twice and provides fast lookup for duplicate checks.
- `IX_ApplicationNotes_ApplicationId`: Speeds up retrieving all notes for an applicant profile.
- `IX_StageHistories_ApplicationId`: Speeds up retrieving the audit trail for an applicant.

### GET /api/applications/{id} Query
The query uses `LEFT JOIN`s (or `Include` in EF Core) to fetch the profile in one go.
```sql
SELECT a.*, cf.*, is.*, as.*, n.*, h.*
FROM Applications a
LEFT JOIN CultureFitScores cf ON a.Id = cf.ApplicationId
LEFT JOIN InterviewScores is ON a.Id = is.ApplicationId
LEFT JOIN AssessmentScores as ON a.Id = as.ApplicationId
LEFT JOIN ApplicationNotes n ON a.Id = n.ApplicationId
LEFT JOIN StageHistories h ON a.Id = h.ApplicationId
WHERE a.Id = @id;
```
**Round-trips:** In my implementation using EF Core's `Include`, it can be done in **1 round-trip** (if using a single query) or a few (if split queries are used for performance on large collections). For a "tiny slice", a single round-trip is efficient.

## 2. Scoring Design Trade-offs

### (a) Three Separate Endpoints vs One Generic PUT
- **Better for Granularity:** Recruiters might only be responsible for one dimension (e.g., Culture Fit). Separate endpoints allow for cleaner permissioning and front-end state management.
- **Conflict Reduction:** If two people are scoring different dimensions simultaneously, they won't overwrite each other's work if they only send the specific field they changed.
- **Audit Clarity:** It's easier to see exactly *what* changed and *who* did it when the intent is specific to one dimension.

**Opposite (Generic PUT) is better when:**
- The UI presents all scores in a single modal/form and expects to save them all at once.
- Reducing API surface area is a priority.

### (b) History of Every Score Change
I would change the Score tables to a "many-to-one" relationship instead of "one-to-one".
- Remove the `UNIQUE` constraint on `ApplicationId`.
- Add a `Version` or just rely on `UpdatedAt` to find the "Current" score (highest ID or latest timestamp).
- The endpoints would now always `INSERT` a new row instead of `UPDATE`.

## 3. Debugging Question: The "Stuck in Screening" Bug

1.  **Check the audit trail:** Query the `StageHistory` for that specific application. Did a transition to Interview actually happen?
2.  **Verify the API response:** Reproduce the action in a test environment. Does the `PATCH /stage` endpoint return 204/200 but fail to commit?
3.  **Check logs:** Look for database exceptions or validation errors during the time the recruiter claims to have moved the candidate.
4.  **Concurrency check:** Did someone else move them back to Screening? (The history would show this).
5.  **Caching issues:** (If Part 2 Option B was implemented) Is the Redis cache stale?
6.  **Frontend/Client:** Is the UI showing a cached state or failing to refresh after a successful API call?

## 4. Honest Self-Assessment

- **C#:** 5/5 (Used net9 features, clean async/await, DI, and middleware).
- **SQL:** 4/5 (Comfortable with EF Core mappings and relational constraints).
- **Git:** 4/5 (Meaningful commits and standard workflow).
- **REST API design:** 5/5 (Followed requested paths, used DTOs, consistent error handling).
- **Writing tests:** 5/5 (Unit + Integration tests with in-memory DB and test server).
