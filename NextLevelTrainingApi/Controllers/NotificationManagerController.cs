using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NextLevelTrainingApi.DAL.Entities;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;
using NextLevelTrainingApi.Services.Interfaces;

namespace NextLevelTrainingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationManagerController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;


        public NotificationManagerController(IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }


        [HttpGet]
        [Route("GetEvents")]
        public IActionResult GetEvents()
        {
            var events = _unitOfWork.EventRepository.AsQueryable().ToList();

            return Ok(events.ToList());
        }


        [HttpPost]
        [Route("CreateNotification")]
        public IActionResult CreateNotification(DynamicNotification dynamicNotificationModel)
        {
            _unitOfWork.DynamicNotificationRepository.InsertOne(dynamicNotificationModel);

            return Ok();
        }

        [HttpGet]
        [Route("GetNotifications")]
        public IActionResult GetDynamicNotifications()
        {
            var notifications = _unitOfWork.DynamicNotificationRepository.AsQueryable().ToList();

            return Ok(notifications);
        }



        [HttpPost]
        [Route("UpdateNotification")]
        public IActionResult UpdateNotification(DynamicNotification dynamicNotificationModel)
        {
            var notification = _unitOfWork.DynamicNotificationRepository.FindById(dynamicNotificationModel.Id);
            if (notification == null)
                return NotFound();

            notification.Name = dynamicNotificationModel.Name;
            notification.Content = dynamicNotificationModel.Content;
            notification.FirstTriggerDate = dynamicNotificationModel.FirstTriggerDate;
            notification.TriggerTime = dynamicNotificationModel.TriggerTime;
            notification.Type = dynamicNotificationModel.Type;
            notification.IsActive = dynamicNotificationModel.IsActive;
            notification.ScheduleType = dynamicNotificationModel.ScheduleType;
            notification.UserType = dynamicNotificationModel.UserType;
            notification.EventType = dynamicNotificationModel.EventType;

            _unitOfWork.DynamicNotificationRepository.ReplaceOne(notification);
            return Ok();
        }


        [HttpGet]
        [Route("CheckNotificationJob")]
        public async Task<IActionResult> CheckNotificationJob()
        {
            var now = DateTime.Now;
            var scheduledNotifications = _unitOfWork.DynamicNotificationRepository.AsQueryable().Where(x => x.Type == Models.NotificationType.Scheduled && x.IsActive).ToList();
            foreach(var notification in scheduledNotifications)
            {
                if(notification.FirstTriggerDate.Date == now.Date)
                {
                    if(notification.TriggerTime.Hours == now.Hour && notification.TriggerTime.Minutes == now.Minute)
                    {
                        await SendNotification(notification);
                        notification.LastTriggeredDate = now;
                    }
                }else if(notification.FirstTriggerDate.Date < now.Date)
                {
                    var addedDayCount = -1;
                    if (notification.ScheduleType == Models.ScheduleType.Daily)
                        addedDayCount = 1;
                    else if (notification.ScheduleType == Models.ScheduleType.Weekly)
                        addedDayCount = 7;
                    else if (notification.ScheduleType == Models.ScheduleType.Monthly)
                        addedDayCount = 30;
                 


                    var triggerDate = notification.LastTriggeredDate.AddDays(addedDayCount);
                    if(triggerDate.Date == now.Date)
                    {
                        if (notification.TriggerTime.Hours == now.Hour && notification.TriggerTime.Minutes == now.Minute)
                        {
                            await SendNotification(notification);
                            notification.LastTriggeredDate = now;
                        }
                    }
                }
                await _unitOfWork.DynamicNotificationRepository.ReplaceOneAsync(notification);
            }

            return Ok();
        }



        private async Task SendNotification(DynamicNotification notification)
        {
            var users = new List<Users>();
            if(notification.UserType == Models.UserType.All)
            {
                users = _unitOfWork.UserRepository.AsQueryable().ToList();
            }
            else if(notification.UserType == Models.UserType.Coach)
            {
                users = _unitOfWork.UserRepository.AsQueryable().Where(x => x.Role == Constants.COACH).ToList();
            }
            else if(notification.UserType == Models.UserType.Player)
            {
                users = _unitOfWork.UserRepository.AsQueryable().Where(x => x.Role == Constants.PLAYER).ToList();
            }


            foreach(var u in users)
            {
                await _notificationService.SendPureNotification(u.Id, notification.Content, u);
            }

            return;
        }

    }
}