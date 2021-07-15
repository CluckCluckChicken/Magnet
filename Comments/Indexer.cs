using Comments.Models;
using HtmlAgilityPack;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Comments
{
    class Indexer : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine(context.JobDetail.Key.Name);
            await IndexProfileComments(context.JobDetail.Key.Name);
        }

        // Indexer's gotta index
        public async Task Index()
        {
            await IndexProfileComments("potatophant");
        }

        private async Task IndexProfileComments(string username)
        {
            HttpClient client = new HttpClient();

            string response = await client.GetStringAsync($"https://scratch.mit.edu/site-api/comments/user/{username}");

            HtmlDocument html = new HtmlDocument();

            html.LoadHtml(response);

            HtmlNodeCollection commentNodes = html.DocumentNode.SelectNodes("//div[@class=\"comment \"]");

            List<Comment> comments = new List<Comment>();

            if (commentNodes != null)
            {
                foreach (HtmlNode node in commentNodes)
                {
                    HtmlNode info = node.SelectSingleNode(".//div[@class=\"info\"]");
                    HtmlNode user = node.SelectSingleNode(".//a[@id=\"comment-user\"]");
                    string authorName = info.SelectSingleNode(".//div[@class=\"name\"]").InnerText.Trim();
                    User authorUser = Program.UserService.Get(authorName);

                    int authorId = 0;

                    if (authorUser != null)
                    {
                        authorId = authorUser.Id;
                    }

                    Author author = new Author()
                    {
                        Id = authorId,
                        Scratchteam = authorName.EndsWith('*'),
                        Username = authorName,
                        Image = user.SelectSingleNode(".//img[@class=\"avatar\"]").Attributes["src"].Value
                    };

                    await RegisterUserFromAuthor(author);

                    Comment comment = new Comment()
                    {
                        Id = int.Parse(node.Attributes["data-comment-id"].Value),
                        ParentId = null,
                        CommenteeId = null,
                        Content = info.SelectSingleNode(".//div[@class=\"content\"]").InnerText.Trim().Replace("\n      ", ""),
                        DatetimeCreated = DateTime.Parse(info.SelectSingleNode(".//span[@class=\"time\"]").Attributes["title"].Value),
                        DatetimeModified = DateTime.Parse(info.SelectSingleNode(".//span[@class=\"time\"]").Attributes["title"].Value),
                        Visibility = "visible",
                        Author = author,
                        ReplyCount = 0
                    };

                    List<Comment> replies = new List<Comment>();

                    if (!node.ParentNode.HasClass("reply"))
                    {
                        foreach (HtmlNode replyContainer in node.ParentNode.SelectSingleNode(".//ul[@class=\"replies\"]").ChildNodes)
                        {
                            if (replyContainer.SelectSingleNode(".//div[@class=\"comment \"]") != null)
                            {
                                HtmlNode replyInfo = replyContainer.SelectSingleNode(".//div[@class=\"info\"]");
                                HtmlNode replyUser = replyContainer.SelectSingleNode(".//a[@id=\"comment-user\"]");
                                string replyAuthorName = replyInfo.SelectSingleNode(".//div[@class=\"name\"]").InnerText.Trim();
                                User replyAuthorUser = Program.UserService.Get(replyAuthorName);

                                int replyAuthorId = 0;

                                if (replyAuthorUser != null)
                                {
                                    replyAuthorId = replyAuthorUser.Id;
                                }

                                Author replyAuthor = new Author()
                                {
                                    Id = replyAuthorId,
                                    Username = replyAuthorName,
                                    Image = replyUser.SelectSingleNode(".//img[@class=\"avatar\"]").Attributes["src"].Value
                                };

                                await RegisterUserFromAuthor(replyAuthor);

                                Comment reply = new Comment()
                                {
                                    Id = int.Parse(replyContainer.SelectSingleNode(".//div[@class=\"comment \"]").Attributes["data-comment-id"].Value),
                                    ParentId = comment.Id,
                                    CommenteeId = null,
                                    Content = replyInfo.SelectSingleNode(".//div[@class=\"content\"]").InnerText.Trim().Replace("\n      ", ""),
                                    DatetimeCreated = DateTime.Parse(replyInfo.SelectSingleNode(".//span[@class=\"time\"]").Attributes["title"].Value),
                                    DatetimeModified = DateTime.Parse(replyInfo.SelectSingleNode(".//span[@class=\"time\"]").Attributes["title"].Value),
                                    Visibility = "visible",
                                    Author = replyAuthor,
                                    ReplyCount = 0
                                };

                                replies.Add(reply);

                                comments.Add(reply);

                                ++comment.ReplyCount;
                            }
                        }

                        comments.Add(comment);
                    }
                }
            }

            Program.CommentService.Create(comments);
        }

        public async Task RegisterUserFromAuthor(Author author)
        {
            User user = Program.UserService.Create(new User()
            {
                Id = author.Id,
                Username = author.Username,
                LastIndexed = null,
                CanIndex = true
            });

            await Program.ScheduleUser(user);
        }
    }
}
