using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureLabb3.Models
{
    [BsonIgnoreExtraElements]
    public class Pokemon
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
        
        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("level")]
        public  int Level { get; set; }

    }
}
