# Case: Claims API poc

## How to Run
- Requires Docker
- Swagger UI available at https://localhost:7052/swagger

## Architecture Overview

The original codebase had controllers doing everything.
database access, validation, auditing, premium calculation. I restructured it into clean layers:

- Controllers accept DTOs, delegate to services, return responses. 
- Services owns the business logic
- Validators FluentValidation
- Auditing Consumer-producer pattern for async handling

Two databases by design: 
- MongoDB for Claims & Covers
- SQL Server for audit logs

## Tasks

### Task 1: Refactoring & cleanup

Extracted models, DTOs, services, and interfaces from the controllers. Also fixed compilation issues, including migration desync and the DateOnly BsonAttr.

- Trusted DB over code and added removed "nullable" from ID's & requestTypes and set props as required.
- Services own the business logic
- Auditer writes to a queue. No longer blocks HTTP requests.
- GlobalExceptionHandler returns ProblemDetails and suppresses stack traces in production
- Cleaned up warnings and added cancellationtokens.
- Moved to DateTime instead of DateOnly for consistency across the code and to avoid the serialization issue with MongoDB.
- Added AsNoTracking & indexes for audits in DB.

### Task 2: Validation

Used FluentValidation with a global filter to keep controllers thin. Validation failures return early with a descriptive message.

All date comparisons use .Date consistently across validators and the premium calculator. There was a bug caught by copilot code review where TimeSpan.TotalDays returned fractional hours which truncated differently than calendar day subtraction. Leading to a off by 1 bug. Where the validator accepted any input where the endDate.DateTime > startDate.DateTime (even just 1 minute later). Which would throw in the calculator

### Task 3: Async Auditing

Replaced the synchronous database writes with System.Threading.Channels for in-memory queuing:

1. Auditer writes an AuditMessage to the channel/queue
2. AuditBackgroundService reads from the channel and persists to SQL Server

This decouples HTTP response time from audit persistence. I chose Channel because it was quick and easy. A proper implementation should use Azure Service Bus. The code has TODOs documenting what a proper implementation would need: Service Bus for durability, Polly for retry, dead-letter queue for failed writes.

### Task 4: Tests

Testing strategy for PremiumCalculator: Expected value was calculated from the business rules without using the calculator. This means the tests should be a known good quantity. If someone accidentally changes a discount rate, the test catches it. It requires tests to be updated if the premium rates change.

Integration tests use test end-to-end.

### Task 5: PremiumCalculator

The original implementation had three independent if blocks inside a for-loop. This caused days in the lower tiers to be counted multiple times: 

Replaced with a tier-based calculation using Math.Min & Math.Max.

Guard clause throws ArgumentOutOfRangeException for unexpected or invalid data. Fail fast instead of silently misbehaving

## Things I would do with more time:

- Set up servicebus for auditing instead of in-memory channel with:
  - dead-letter queue
  - Polly for retries
  - Rate-limiting on the Api
  - peeklock on service bus
  - outbox pattern for placing items into service bus (write to db to avoid dualwrite problems)
- Add terraform scripts for deploying to azure
- CI/CD pipeline with test gating
- Code scanning in pipeline with trivy or similar for security vulnerabilities in dependencies & use SNYK or GHAS.
- Health check endpoints 
- Do a thorough investigation of DateTime and timezones. I expect this to be a rabbit hole of bugs and edge cases.
- Authentication and Authorization ofcourse, the API is wide open 
