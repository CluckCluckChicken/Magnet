using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Comments.Models
{
    public enum LocationType
    {
        Project,
        Profile,
        Studio
    }

    record Author
    {
        public Author()
        {
            string[] members = 
            {
                "labdalla",
                //"shaanmasala",
                "ceebee",
                "Zinnea",
                "designerd",
                //"leoburd",
                "FredDog",
                "kittyloaf",
                //"SunnyDay4aBlueJay",
                "Za-Chary",
                "shruti",
                //"paperShannon101",
                "Champ99",
                "dietbacon",
                "lilyland",
                "chrisg",
                "Paddle2See",
                "codubee",
                //"khanning",
                //"pizzafordessert",
                //"theladynico",
                "sgcc_",
                "dsquare",
                "dinopickles",
                "Harakou",
                "mwikali",
                //"me_win",
                "KayOh",
                "scmb1",
                "tarmelop",
                //"mres",
                "mres-admin",
                "ericr",
                "natalie",
                "raimondious",
                "speakvisually",
                "BrycedTea",
                //"jaleesa",
                "cheddargirl",
                "wheelsonfire",
                "achouse",
                "cwillisf",
                "pondermake",
                "originalwow",
                "noncanonical",
                "Class12321",
                "MunchtheCat",
                //"pamimus",
                "floralsunset",
                "ipzy",
                "ScratchCat"
            };

            //Scratchteam = members.Contains(Username);

            //Scratchteam = Username.EndsWith('*');
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        [JsonIgnore]
        public string BsonId { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("scratchteam")]
        public bool Scratchteam { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }
    }

    record Location
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        [JsonIgnore]
        public string BsonId { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public LocationType Type { get; set; }
    }

    record Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        [JsonIgnore]
        public string BsonId { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("parent_id")]
        public int? ParentId { get; set; }

        [JsonPropertyName("commentee_id")]
        public int? CommenteeId { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("datetime_created")]
        public DateTime DatetimeCreated { get; set; }

        [JsonPropertyName("datetime_modified")]
        public DateTime DatetimeModified { get; set; }

        [JsonPropertyName("visibility")]
        public string Visibility { get; set; }

        [JsonPropertyName("author")]
        public Author Author { get; set; }

        [JsonPropertyName("reply_count")]
        public int ReplyCount { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; }
    }
}