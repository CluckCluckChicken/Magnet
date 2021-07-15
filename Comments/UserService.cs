using Comments.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comments
{
    class UserService
    {
        private readonly IMongoCollection<User> users;

        public UserService(Settings settings)
        {
            var client = new MongoClient(settings.MongoDBSettings.ConnectionString);
            var database = client.GetDatabase(settings.MongoDBSettings.DatabaseName);

            users = database.GetCollection<User>("users");
        }

        public List<User> Get() =>
            users.Find(user => true).ToList();

        public User Get(int userId) =>
            users.Find<User>(user => user.Id == userId).FirstOrDefault();

        public User Get(string username) =>
            users.Find<User>(user => user.Username == username).FirstOrDefault();

        public List<User> GetMany(List<int> userIds) =>
            users.Find<User>(user => userIds.Contains(user.Id)).ToList();

        public User Create(User user)
        {
            if (Get(user.Username) == null)
            {
                users.InsertOne(user);
            }

            return user;
        }

        public void Update(int userId, User userIn) =>
            users.FindOneAndReplace(user => user.Id == userId, userIn);

        public void Update(string username, User userIn) =>
            users.FindOneAndReplace(user => user.Username == username, userIn);

        public void Remove(User userIn) =>
            users.DeleteOne(user => user.BsonId == userIn.BsonId);

        public void Remove(int userId) =>
            users.DeleteOne(user => user.Id == userId);

        public void Remove(string username) =>
            users.DeleteOne(user => user.Username == username);
    }
}
