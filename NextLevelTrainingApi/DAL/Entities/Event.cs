using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;
using NextLevelTrainingApi.Models;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("Events")]

    public class Event : IDocument
    {
        [BsonId]
        public Guid Id { get; set; }

        public string Name { get; set; }
        
        public bool IsActive { get; set; }

        public EventType Type { get; set; }

        
    }
}