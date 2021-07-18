using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comments
{
    class JobRunner
    {
        public List<string> CurrentJobs { get; set; }

        public JobRunner()
        {
            CurrentJobs = new List<string>();
        }

        public void Go()
        {
            while (true)
            {
                if (Program.indexingEnabled)
                {
                    while (CurrentJobs.Count >= 10 || Program.Queue.Count == 0) { }

                    //Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(Program.Queue));

                    string name = Program.Queue.First();

                    CurrentJobs.Add(name);
                    Program.Queue.Remove(name);

                    //Console.WriteLine($"starting job on {name}");

                    Task.Run(async () =>
                    {
                        await Program.Indexer.IndexProfileComments(name);

                        CurrentJobs.Remove(name);
                    });
                }
            }
        }
    }
}
