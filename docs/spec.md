Project Specification: "Respect Ledger"

1\. Project Overview



Respect Ledger is a gamified social application for a closed circle of friends. It serves as a "digital chronicle of friendship." The core purpose is not to exchange points for goods, but to track social status, gratitude, and history through a mix of Social Feed, RPG Mechanics, and Competitive Seasons.



Core Philosophy:



&nbsp;   The Feed: Records history and context (Why was the respect given?).



&nbsp;   RPG System: Long-term accumulation of status (Levels, Classes, Titles).



&nbsp;   Seasons: Short-term competition (Monthly leaderboards) to keep engagement high.



2\. Core Concepts \& Terminology



&nbsp;   Respect: The fundamental unit of currency. It acts as both a transaction of gratitude and Experience Points (XP).



&nbsp;   Mana (Daily Allowance): A limited resource that restricts how many respects a user can give per day.



&nbsp;   XP (Experience Points): Lifetime counter of received respects. Determines User Level.



&nbsp;   Season Score: Monthly counter of received respects. Determines Leaderboard position. Resets every month.



&nbsp;   Class: A dynamic title assigned to a user based on the most frequent "Tags" in their received respects (e.g., "Driver", "Techie", "Jester").



3\. Functional Modules \& User Stories

Module A: Authentication \& User Profile



User Stories:



&nbsp;   As a Guest, I want to register with an email, nickname, and password so that I can join the system.



&nbsp;   As a User, I want to log in to access the functionality.



&nbsp;   As a User, I want to upload an avatar and update my bio.



&nbsp;   As an Admin, I want to approve new registrations so that only trusted friends can join.



Business Rules:



&nbsp;   Invite-Only Logic: New accounts are created with a status of Pending. They cannot perform actions until an Admin sets status to Active.



&nbsp;   Uniqueness: Nicknames and Emails must be unique.



Module B: Core Interaction (Giving Respect)



User Stories:



&nbsp;   As a User, I want to select a friend from a list to give them respect.



&nbsp;   As a User, I must provide a text reason (comment) and an optional tag (e.g., #help, #fun) so the history has context.



&nbsp;   As a User, I want to attach a photo to the transaction as proof (optional).



&nbsp;   As a User, I want to see my current "Mana" balance to know how many respects I can still give today.



Business Rules:



&nbsp;   Daily Mana Limit: Every user has a hard limit of 3 Respects to give per day.



&nbsp;   Mana Reset: The limit resets automatically at 00:00 Server Time. Unused Mana does not roll over to the next day.



&nbsp;   Self-Respect Block: A user cannot send respect to themselves (SenderId != ReceiverId).



&nbsp;   Anti-Spam Cooldown: A user cannot send respect to the same receiver more than once per 1 hour.



&nbsp;   Immutability: Once sent, a respect transaction cannot be edited or deleted by the user (only by Admins).



Module C: The Feed \& History



User Stories:



&nbsp;   As a User, I want to view a global feed of all respect transactions in chronological order.



&nbsp;   As a User, I want to "Like" (validate) a transaction in the feed (this is purely visual/social and adds no points).



&nbsp;   As a User, I want to visit a friend's profile to see their specific history of received respects.



Business Rules:



&nbsp;   Public Visibility: All transactions are public to all active users.



Module D: RPG Progression (Long-Term)



User Stories:



&nbsp;   As a User, I want to see my current Level and a progress bar to the next level.



&nbsp;   As a User, I want to see my "Class" (e.g., "Paladin", "Bard") based on my history.



&nbsp;   As a User, I want to earn Achievements (Badges) for specific milestones (e.g., "Received 50 respects").



Business Rules:



&nbsp;   XP Calculation: 1 Received Respect = +1 Global XP.



&nbsp;   Leveling Formula: Non-linear progression.



&nbsp;       Formula Example: Level = Floor(Constant \* Sqrt(TotalXP)).



&nbsp;   Dynamic Class System:



&nbsp;       The system calculates the most frequent tag in the user's received history.



&nbsp;       If the top tag constitutes >30% of total respects, the user is assigned a specific Class (e.g., Tag #car -> Class Driver).



Module E: Seasons \& Leaderboard (Short-Term)



User Stories:



&nbsp;   As a User, I want to see a Leaderboard ranking users by "Season Score".



&nbsp;   As a User, I want to see a timer indicating when the current season ends.



&nbsp;   As a User, I want to browse past seasons to see who won previously.



Business Rules:



&nbsp;   Season Cycle: A season lasts exactly one calendar month.



&nbsp;   Dual Scoring: When a respect is received:



&nbsp;       GlobalXP increases by 1 (Never resets).



&nbsp;       SeasonScore increases by 1 (Resets monthly).



&nbsp;   Season Reset Logic:



&nbsp;       At 23:59:59 on the last day of the month:



&nbsp;           Snapshot top 3 users into SeasonWinners table.



&nbsp;           Reset SeasonScore to 0 for all users.



&nbsp;           Start new season.



4\. Technical Architecture \& Stack Requirements

Backend Structure



&nbsp;   Framework: .NET 10 (Web API).



&nbsp;   Authentication: Implement JWT Authentication using standard ASP.NET Core Identity.



&nbsp;   Architecture Pattern: Clean Architecture.



&nbsp;       Layer 1: Domain (Entities, Enums, Exceptions). No dependencies.



&nbsp;       Layer 2: Application (Interfaces, CQRS Handlers, DTOs, Validators). Depends on Domain.



&nbsp;       Layer 3: Infrastructure (EF Core, Cloudinary Implementation, Email Service). Depends on Application.



&nbsp;       Layer 4: API (Controllers/Endpoints). Depends on Application.



&nbsp;   Key Patterns:



&nbsp;       CQRS: Use MediatR library. Separate commands (write) and queries (read).



&nbsp;       Repository Pattern: Use generic IRepository<T> and specific repositories where needed (e.g., IRespectRepository). Use Unit of Work pattern for transaction management.



&nbsp;       Dependency Injection: Use standard .NET DI container.



&nbsp;   Libraries:



&nbsp;       FluentValidation For Input/DTO validation in the Application Layer (Pipeilne Behavior).



&nbsp;	Ardalis.GuardClauses: For enforcing Domain Invariants and validations inside Entities/Value Objects (Domain Layer).



&nbsp;       Mapster or AutoMapper for object mapping.



&nbsp;       CloudinaryDotNet for image storage.



&nbsp;       Serilog for structured logging.



&nbsp;       Database Provider: Microsoft.EntityFrameworkCore.SqlServer.



Frontend Structure



&nbsp;   Framework: React 19+ (TypeScript) built with Vite.



&nbsp;   Styling: Tailwind CSS.



&nbsp;   Component Library: shadcn/ui (Radix UI based).



&nbsp;   Animations: Framer Motion (essential for "gamified" feel - XP bar filling, level up modals).



&nbsp;   State/Data Fetching: TanStack Query (React Query) v5.



&nbsp;   Routing: React Router DOM v7.



Infrastructure \& Deployment Strategy (Constraint: $0 Cost)



&nbsp;   Database: Microsoft SQL Server. Target host: Azure SQL Database (Free Tier).



&nbsp;   Backend Hosting: Dockerized application deployed to Render.com (Free Tier).



&nbsp;   Frontend Hosting: Static export deployed to Vercel.



&nbsp;   Image Storage: Cloudinary (Free Tier).



Specific Implementation Notes for AI



&nbsp;   EF Core: Configure strictly for Microsoft SQL Server. Ensure usage of nvarchar for strings and datetime2 for dates.



&nbsp;   Images: When a user uploads a photo, the backend should handle the upload to Cloudinary and store only the URL in the database.



&nbsp;   Clean Code: Ensure strict separation of concerns. The Domain layer must remain pure.

