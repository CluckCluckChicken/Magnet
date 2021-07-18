using Comments.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comments
{
    class CommentService
    {
        private readonly IMongoCollection<Comment> comments;

        public CommentService(Settings settings)
        {
            var client = new MongoClient(settings.MongoDBSettings.ConnectionString);
            var database = client.GetDatabase(settings.MongoDBSettings.DatabaseName);

            comments = database.GetCollection<Comment>("comments");
        }

        public List<Comment> Get() =>
            comments.Find(comment => true).ToList();

        public Comment Get(int commentId) =>
            comments.Find<Comment>(comment => comment.Id == commentId).FirstOrDefault();

        public List<Comment> GetMany(List<int> commentIds) =>
            comments.Find<Comment>(comment => commentIds.Contains(comment.Id)).ToList();

        public Comment Create(Comment comment)
        {
            if (Get(comment.Id) == null)
            {
                comments.InsertOne(comment);
            }
            else
            {
                Update(comment.Id, comment);
            }

            return comment;
        }

        public List<Comment> Create(List<Comment> commentsIn)
        {
            commentsIn.RemoveAll(comment => Get().Find(c => c.Id == comment.Id) != null);

            if (commentsIn.Count > 0)
            {
                comments.InsertMany(commentsIn);
            }

            return commentsIn;
        }

        public void Update(int commentId, Comment commentIn) =>
            comments.FindOneAndReplace(comment => comment.Id == commentId, commentIn);

        public void Remove(Comment commentIn) =>
            comments.DeleteOne(comment => comment.BsonId == commentIn.BsonId);

        public void Remove(string id) =>
            comments.DeleteOne(comment => comment.BsonId == id);
    }
}
