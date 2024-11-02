# Notification System

A .NET 8 Web API application for managing and scheduling drip and birthday campaign events, powered by Hangfire for background job processing.

## Features

- **Drip Campaign Events**: Schedule and manage drip campaign events for users based on their subscription dates.
- **Birthday Campaign Events**: Schedule and execute birthday campaign events daily for users whose birthdays match the current date.
- **Hangfire Integration**: Use Hangfire for background job processing, including scheduling and executing campaign events.
- **Recurring Jobs**: Configure recurring jobs to run daily for birthday campaign events.
- **Job Management**: Manage and remove jobs related to specific loyalty programs.

## Concepts

### Drip Campaign Events

Drip campaign events are scheduled based on user subscription dates. These events can be configured to send emails or SMS messages to users at specific intervals after they subscribe to a loyalty program.

### Birthday Campaign Events

Birthday campaign events are scheduled to run daily and check for users whose birthdays match the current date. If a match is found, the campaign event is scheduled for those users.

### Hangfire Integration

Hangfire is used for background job processing. It allows for scheduling, executing, and managing background jobs, including campaign events. The Hangfire Dashboard provides a web interface to monitor and manage these jobs.

### Recurring Jobs

Recurring jobs are configured to run at specified intervals. In this project, a recurring job is set up to run daily to schedule birthday campaign events.

### Job Management

Jobs can be managed and removed based on specific criteria, such as loyalty programs. This allows for handling jobs individually and ensuring that all related jobs are managed together.

## How To

### Setting Up the Project

1. **Clone the Repository**: Clone the repository to your local machine.
   ```bash
   git clone https://github.com/objectgyan/NotificationSystem.git
   cd NotificationSystem
   ```

2. **Update Connection Strings**: Update the connection strings in `appsettings.json` to match your database configuration.
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=your_database;Integrated Security=True;TrustServerCertificate=True;",
       "HangfireConnection": "Server=your_server;Database=your_hangfire_database;Integrated Security=True;TrustServerCertificate=True;"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*"
   }
   ```

3. **Create Database Schema and Seed Data**: Use the provided SQL script to create the database schema and seed initial data.

   ```sql
   -- Drop existing tables if they exist
   IF OBJECT_ID('dbo.ProfileTags', 'U') IS NOT NULL DROP TABLE dbo.ProfileTags;
   IF OBJECT_ID('dbo.LoyaltyProfiles', 'U') IS NOT NULL DROP TABLE dbo.LoyaltyProfiles;
   IF OBJECT_ID('dbo.CampaignEvents', 'U') IS NOT NULL DROP TABLE dbo.CampaignEvents;
   IF OBJECT_ID('dbo.Campaigns', 'U') IS NOT NULL DROP TABLE dbo.Campaigns;
   IF OBJECT_ID('dbo.LoyaltyPrograms', 'U') IS NOT NULL DROP TABLE dbo.LoyaltyPrograms;

   -- Create tables
   CREATE TABLE LoyaltyPrograms (
       Id INT PRIMARY KEY IDENTITY,
       Name NVARCHAR(100) NOT NULL
   );

   CREATE TABLE Campaigns (
       Id INT PRIMARY KEY IDENTITY,
       Name NVARCHAR(100) NOT NULL,
       LoyaltyProgramId INT NOT NULL,
       FOREIGN KEY (LoyaltyProgramId) REFERENCES LoyaltyPrograms(Id)
   );

   CREATE TABLE CampaignEvents (
       Id INT PRIMARY KEY IDENTITY,
       Name NVARCHAR(100) NOT NULL,
       CampaignId INT NOT NULL,
       EventType NVARCHAR(50) NOT NULL,
       Message NVARCHAR(500) NOT NULL,
       ParentEventId INT NULL,
       DelayAfterParentInDays INT NULL,
       FOREIGN KEY (CampaignId) REFERENCES Campaigns(Id),
       FOREIGN KEY (ParentEventId) REFERENCES CampaignEvents(Id)
   );

   CREATE TABLE LoyaltyProfiles (
       Id INT PRIMARY KEY IDENTITY,
       Name NVARCHAR(100) NOT NULL,
       LoyaltyProgramId INT NOT NULL,
       SubscriptionDate DATETIME NOT NULL,
       FirstEmailSentDate DATETIME NULL,
       Birthday DATETIME NULL,
       FOREIGN KEY (LoyaltyProgramId) REFERENCES LoyaltyPrograms(Id)
   );

   CREATE TABLE ProfileTags (
       ProfileId INT NOT NULL,
       Tag NVARCHAR(100) NOT NULL,
       FOREIGN KEY (ProfileId) REFERENCES LoyaltyProfiles(Id)
   );

   -- Seed data
   INSERT INTO LoyaltyPrograms (Name) VALUES ('Loyalty Program 1');

   INSERT INTO Campaigns (Name, LoyaltyProgramId) VALUES ('Campaign 1', 1);

   INSERT INTO CampaignEvents (Name, CampaignId, EventType, Message, DelayAfterParentInDays, ParentEventId) VALUES 
   ('Welcome Email', 1, 'Email', 'Welcome to our loyalty program!', 0, NULL),
   ('Follow-Up Email 1', 1, 'Email', 'We hope you are enjoying our program!', 7, 1),
   ('Follow-Up Email 2', 1, 'Email', 'Here is another update!', 14, 2),
   ('Special Offer', 1, 'Email', 'Check out our special offer!', 21, 3),
   ('Reminder Email', 1, 'Email', 'Don\'t forget about our program!', 30, NULL),
   ('Birthday Campaign', 1, 'Email', 'Happy Birthday!', NULL, NULL);

   INSERT INTO LoyaltyProfiles (Name, LoyaltyProgramId, SubscriptionDate, Birthday) VALUES 
   ('User 1', 1, DATEADD(day, -10, GETUTCDATE()), DATEADD(day, 0, GETUTCDATE())),
   ('User 2', 1, DATEADD(day, -5, GETUTCDATE()), DATEADD(day, 1, GETUTCDATE())),
   ('User 3', 1, DATEADD(day, -20, GETUTCDATE()), DATEADD(day, 2, GETUTCDATE())),
   ('User 4', 1, DATEADD(day, -15, GETUTCDATE()), DATEADD(day, 3, GETUTCDATE()));

   INSERT INTO ProfileTags (ProfileId, Tag) VALUES 
   (1, 'VIP'),
   (2, 'Regular'),
   (3, 'New'),
   (4, 'Returning');
   ```

4. **Run the Application**: Start the application.
   ```bash
   dotnet run
   ```

5. **Access the Hangfire Dashboard**: Open your web browser and navigate to `http://localhost:7167/hangfire` to access the Hangfire Dashboard.

### Scheduling Drip Campaign Events

To schedule a drip campaign event, use the `/campaignevent/scheduleDripCampaign` endpoint. This endpoint schedules a drip campaign event for all users based on their subscription dates.

### Scheduling Birthday Campaign Events

A recurring job is configured to run daily and schedule birthday campaign events for users whose birthdays match the current date. This job is set up in `Program.cs` using Hangfire's `RecurringJob.AddOrUpdate` method.

### Managing Jobs

#### Removing a Specific Campaign Event

To remove a specific campaign event, use the `/campaignevent/remove/{id}` endpoint. This endpoint removes the specified campaign event and any associated child events.

#### Removing Jobs by Loyalty Program

To remove all jobs related to a specific loyalty program, use the `/campaignevent/removeByLoyaltyProgram/{loyaltyProgramId}` endpoint. This endpoint removes all jobs related to the specified loyalty program.

## License

This project is licensed under the MIT License.

---
