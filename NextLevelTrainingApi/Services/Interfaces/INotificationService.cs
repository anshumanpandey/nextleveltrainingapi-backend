using System;
using System.Threading.Tasks;
using NextLevelTrainingApi.DAL.Entities;
using NextLevelTrainingApi.Models;

namespace NextLevelTrainingApi.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendEventNotification(Guid userId, EventType eventType);

        Task SendPureNotification(Guid userId, string content, Users u = null);
    }
}
