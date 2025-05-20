using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AzureLabb3.Models
{
    [BsonIgnoreExtraElements]
    public class Pokemon : IHasPokemonId
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("pokemonId")]
        public int pokemonId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("types")]
        public List<Types> Types { get; set; }

        [BsonElement("abilities")]
        public List<Abilities> Abilities { get; set; }

        [BsonElement("height")]
        public string Height { get; set; }

        [BsonElement("weight")]
        public string Weight { get; set; }

        [BsonElement("stats")]
        public List<Stats> Stats { get; set; }

        [BsonElement("Genus")]
        public string Genus { get; set; }
    }

    public class Stats
    {
        [BsonElement]
        public int BaseStat { get; set; }
        public Stat Stat { get; set; }
    }

    public class Abilities
    {
        public Ability Ability { get; set; }
    }

    public class Ability
    {
        public string Name { get; set; }
    }

    public class Types
    {
        public Type Type { get; set; }
    }

    public class Type
    {
        public string Name { get; set; }
    }

    public class Stat
    {
        public string Name { get; set; }
    }
}
