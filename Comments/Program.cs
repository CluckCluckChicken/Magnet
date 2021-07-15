using Comments.Models;
using Quartz;
using Quartz.Impl;
using System;
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

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Configure();

            // construct a scheduler factory
            StdSchedulerFactory factory = new StdSchedulerFactory();

            // get a scheduler
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();

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
                        .WithIntervalInSeconds(40)
                        .RepeatForever())
                .Build();

                await scheduler.ScheduleJob(job, trigger);
            }

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
    }
}
