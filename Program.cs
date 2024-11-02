using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using NotificationSystem.Model;
using NotificationSystem.Service;

namespace NotificationSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Configure Hangfire
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            builder.Services.AddHangfireServer();
            // Register application services
            builder.Services.AddScoped<CampaignEventService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            app.UseHangfireDashboard("/hangfire");
            // Configure recurring job for birthday campaign
            RecurringJob.AddOrUpdate<CampaignEventService>(
                "schedule-birthday-campaign",
                service => service.ScheduleBirthdayCampaignEvent(),
                Cron.Daily);
            app.Run();
        }
    }
}
