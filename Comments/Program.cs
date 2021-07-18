using Comments.Models;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static Indexer Indexer { get; set; }

        public static JobRunner JobRunner { get; set; }

        #endregion

        public static StdSchedulerFactory factory;

        public static IScheduler scheduler;

        public static List<string> Queue { get; set; }

        public static bool indexingEnabled { get; set; }

        static async Task Main(string[] args)
        {
            Console.WriteLine("commencing gaming. prepare for house fire.");

            Configure();

            // construct a scheduler factory
            factory = new StdSchedulerFactory();

            // get a scheduler
            scheduler = await factory.GetScheduler();
            await scheduler.Start();

            QueueAllUsers();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() =>
            {
                JobRunner.Go();
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            while (true)
            {
                string input = Console.ReadLine();

                string command = input.Split(' ')[0];

                string[] arguments = input.Split(' ').Skip(1).ToArray();

                switch (command)
                {
                    case "die":
                        Environment.Exit(0);
                        break;
                    case "pause":
                        indexingEnabled = false;
                        break;
                    case "resume":
                        indexingEnabled = true;
                        break;
                    case "index":
                        if (arguments.Length > 0)
                        {
                            string username = arguments[0];

                            RegisterUser(new User()
                            {
                                Id = 0,
                                Username = username,
                                LastIndexed = null,
                                CanIndex = true
                            });

                            Console.WriteLine($"queued {username} for index and added them to db");
                        }
                        else
                        {
                            Console.WriteLine("step on legos");
                        }
                        break;
                    case "indexonce":
                        if (arguments.Length > 0)
                        {
                            string username = arguments[0];

                            await Indexer.IndexProfileComments(username);

                            Console.WriteLine($"indexed {username} once");
                        }
                        else
                        {
                            Console.WriteLine("use gimp");
                        }
                        break;
                    case "jobcount":
                        Console.WriteLine($"current jobcount: {JobRunner.CurrentJobs.Count}");
                        break;
                    case "jobs":
                        Console.WriteLine($"current jobs: {string.Join(", ", JobRunner.CurrentJobs)}");
                        break;
                    case "placeinqueue":
                        if (arguments.Length > 0)
                        {
                            string username = arguments[0];

                            if (Queue.Contains(username))
                            {
                                Console.WriteLine($"{username} is place {Queue.IndexOf(username) + 1} in the queue");
                            }
                            else
                            {
                                Console.WriteLine("this user isn't queued. use command \"index\" to add them to the queue");
                            }
                        }
                        else
                        {
                            Console.WriteLine("you parse html with regex");
                        }
                        break;
                }
            }
        }

        private static void Configure()
        {
            Settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")));

            CommentService = new CommentService(Settings);

            UserService = new UserService(Settings);

            Indexer = new Indexer();

            JobRunner = new JobRunner();

            Queue = new List<string>();

            indexingEnabled = true;
        }

        public static void QueueAllUsers()
        {
            foreach (User user in UserService.Get())
            {
                if (user.LastIndexed == null ? true : (user.LastIndexed.Value.AddDays(7) - DateTime.Now).Ticks <= 0)
                {
                    QueueUser(user);
                }
            }
        }

        public static void QueueUser(User user)
        {
            if (!Queue.Contains(user.Username) && !JobRunner.CurrentJobs.Contains(user.Username))
            {
                //Console.WriteLine($"queueing {user.Username}");

                Queue.Add(user.Username);
            }
        }

        public static void RegisterUser(User user)
        {
            UserService.Create(user);

            QueueUser(user);
        }

        public static void RegisterUserFromAuthor(Author author)
        {
            User user = UserService.Get(author.Username);

            if (user == null)
            {
                user = UserService.Create(new User()
                {
                    Id = author.Id,
                    Username = author.Username,
                    LastIndexed = null,
                    CanIndex = true
                });
            }

            if (user.LastIndexed == null ? true : (user.LastIndexed.Value.AddDays(7) - DateTime.Now).Ticks <= 0)
            {
                QueueUser(user);
            }
        }
    }
}
