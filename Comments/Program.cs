using Comments.Models;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Comments
{
    class Program
    {
        #region Singletons

        public static Settings Settings { get; set; }

        public static CommentService CommentService { get; set; }

        public static UserService UserService { get; set; }

        #endregion

        public static StdSchedulerFactory factory;

        public static IScheduler scheduler;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Configure();

            // construct a scheduler factory
            factory = new StdSchedulerFactory();

            // get a scheduler
            scheduler = await factory.GetScheduler();
            await scheduler.Start();

            await ScheduleAllUsers();

            while (true)
            {
                string input = Console.ReadLine();

                if (input == "die")
                {
                    break;
                }
            }
        }

        private static void Configure()
        {
            Settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")));

            CommentService = new CommentService(Settings);

            UserService = new UserService(Settings);
        }

        public static async Task ScheduleAllUsers()
        {
            foreach (User user in UserService.Get())
            {
                // define the job and tie it to our HelloJob class
                IJobDetail job = JobBuilder.Create<Indexer>()
                    .WithIdentity(user.Username, "usernames")
                    .Build();

                // Trigger the job to run now, and then every 40 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity(user.Username, "usernames")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(30)
                        .RepeatForever())
                .Build();

                await scheduler.ScheduleJob(job, trigger);
            }
        }

        public static async Task ScheduleUser(User user)
        {
            IReadOnlyCollection<JobKey> keys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupContains("usernames"));

            List<JobKey> keysList = new List<JobKey>();

            foreach (JobKey key in keys)
            {
                keysList.Add(key);
            }

            if (keysList.Find(key => key.Name == user.Username) == null)
            {
                // define the job and tie it to our HelloJob class
                IJobDetail job = JobBuilder.Create<Indexer>()
                    .WithIdentity(user.Username, "usernames")
                    .Build();

                // Trigger the job to run now, and then every 40 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity(user.Username, "usernames")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(30)
                        .RepeatForever())
                .Build();

                await scheduler.ScheduleJob(job, trigger);
            }
        }
    }
}
