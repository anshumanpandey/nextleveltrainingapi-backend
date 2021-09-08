using System;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;
using NextLevelTrainingApi.Models;

namespace NextLevelTrainingApi.DAL.Entities
{
    [BsonCollection("DynamicNotification")]
    public class DynamicNotification : IDocument
    {

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Content { get; set; }

        public NotificationType Type { get; set; }

        public EventType EventType { get; set; }

        public UserType UserType { get; set; }

        public ScheduleType ScheduleType { get; set; }

        public DateTime FirstTriggerDate { get; set; }

        public TimeSpan TriggerTime { get; set; }

        public DateTime LastTriggeredDate { get; set; }

        public bool IsActive { get; set; }

    }
}
