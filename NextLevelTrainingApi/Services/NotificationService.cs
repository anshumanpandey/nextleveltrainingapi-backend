using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CorePush.Apple;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NextLevelTrainingApi.DAL.Entities;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.Helper;
using NextLevelTrainingApi.Models;
using NextLevelTrainingApi.Services.Interfaces;
using NextLevelTrainingApi.ViewModels;

namespace NextLevelTrainingApi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FCMSettings _fcmSettings;

        public NotificationService(IUnitOfWork unitOfWork,
            IOptions<FCMSettings> fcmSettings)
        {
            _unitOfWork = unitOfWork;
            _fcmSettings = fcmSettings.Value;

        }

        public async Task SendEventNotification(Guid userId, EventType eventType)
        {
            var relatedEvent = _unitOfWork.DynamicNotificationRepository.AsQueryable().FirstOrDefault(x => x.EventType == eventType && x.IsActive);
            if (relatedEvent == null)
                return;

            await SendPureNotification(userId, relatedEvent.Content);

        }




        public async Task SendPureNotification(Guid userId,string content,Users u = null)
        {
            var user = new Users();
            if(u == null)
            {
                 user = _unitOfWork.UserRepository.FindById(userId);
                if (user == null)
                    return;
            }
            else
            {
                user = u;
            }
            

            Notification notification = new Notification();
            notification.Id = Guid.NewGuid();
            notification.Text = content;
            notification.CreatedDate = DateTime.Now;
            notification.UserId = userId;
            _unitOfWork.NotificationRepository.InsertOne(notification);
            if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.ANDRIOD_DEVICE)
            {
                await AndriodPushNotification(user.DeviceToken, notification);
            }
            else if (user.DeviceType != null && Convert.ToString(user.DeviceType).ToLower() == Constants.APPLE_DEVICE)
            {
                await ApplePushNotification(user.DeviceToken, notification);
            }
        }



        private async Task AndriodPushNotification(string deviceToken, Notification notification)
        {

            GoogleNotification googleNotification = new GoogleNotification
            {
                To = deviceToken.Trim(),
                Collapse_Key = "type_a",
                Data = new DataNotification
                {
                    Notification = notification
                },
                Notification = new NotificationModel
                {
                    Title = notification.Text,
                    Text = notification.Text,
                    Icon = !string.IsNullOrEmpty(notification.Image) ? notification.Image : "https://www.nextlevelfootballacademy.co.uk/wp-content/uploads/2019/06/logo.png"
                }
            };
            using (var httpClient = new HttpClient())
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");
                httpRequest.Headers.Add("Authorization", $"key = {_fcmSettings.ServerKey}");
                httpRequest.Headers.Add("Sender", $"id = {_fcmSettings.SenderId}");
                var http = new HttpClient();
                var json = JsonConvert.SerializeObject(googleNotification);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.SendAsync(httpRequest);
                if (!response.IsSuccessStatusCode)
                {
                    var notificationError = response.Content.ReadAsStringAsync().Result;
                    var device_Token = deviceToken;
                    var user = _unitOfWork.UserRepository.FindOne(x => x.DeviceToken.ToLower() == device_Token.ToLower());
                    string path = Directory.GetCurrentDirectory();
                    var num = string.IsNullOrEmpty(user.MobileNo) ? "" : user.MobileNo.ToString();
                    var email = string.IsNullOrEmpty(user.EmailID) ? "" : user.EmailID.ToString();
                    string newPath = path + "\\wwwroot\\ErrorLogFile\\AndroidErrorDevices.txt";
                    using (StreamWriter writer = new StreamWriter(newPath, true))
                    {
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine("Email : " + email.ToString());
                        writer.WriteLine("Mobile_No : " + num);
                        writer.WriteLine("Device_Token : " + device_Token.ToString());
                        writer.WriteLine("Error : " + notificationError.ToString());
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine();
                    }
                }
                else
                {
                    var device_Token = deviceToken;
                    var user = _unitOfWork.UserRepository.FindOne(x => x.DeviceToken.ToLower() == device_Token.ToLower());
                    string path = Directory.GetCurrentDirectory();
                    var num = string.IsNullOrEmpty(user.MobileNo) ? "" : user.MobileNo.ToString();
                    var email = string.IsNullOrEmpty(user.EmailID) ? "" : user.EmailID.ToString();
                    string newPath = path + "\\wwwroot\\ErrorLogFile\\AndroidWorkingDevices.txt";
                    using (StreamWriter writer = new StreamWriter(newPath, true))
                    {
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine("Email : " + email);
                        writer.WriteLine("Mobile_No : " + num);
                        writer.WriteLine("Device_Token : " + device_Token.ToString());
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine();
                    }
                }
            }
        }

        //[HttpGet]
        //[Route("AppleNotificaion")]
        private async Task ApplePushNotification(string deviceToken, Notification notification)
        {
            try
            {

                HttpClient httpClient = new HttpClient();
                ApnSettings apnSettings = new ApnSettings() { AppBundleIdentifier = "com.nextleveltraining", P8PrivateKey = "MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQgZ1ugPXE4Hhh3L1embZmjfUdYBij8HbsrolZnzfR49X6gCgYIKoZIzj0DAQehRANCAARbCwj0VnMCOzw/Tyx4GsS4W+QN4LLCe6RRgIR/LZBJQqKi0q4XWg/p4Qa6JQAdKOZziemK4/dJZaqH/EFijM1S", P8PrivateKeyId = "FQ6ZXC7U8L", ServerType = ApnServerType.Production, TeamId = "Y77A2C426U" };
                AppleNotification appleNotification = new AppleNotification();
                appleNotification.Aps.AlertBody = notification.Text;
                appleNotification.Notification = JsonConvert.SerializeObject(notification);
                var apn = new ApnSender(apnSettings, httpClient);
                var result = await apn.SendAsync(appleNotification, deviceToken.Trim());
                if (!result.IsSuccess)
                {
                    ApnSettings devApnSettings = new ApnSettings() { AppBundleIdentifier = "com.nextleveltraining", P8PrivateKey = "MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQgZ1ugPXE4Hhh3L1embZmjfUdYBij8HbsrolZnzfR49X6gCgYIKoZIzj0DAQehRANCAARbCwj0VnMCOzw/Tyx4GsS4W+QN4LLCe6RRgIR/LZBJQqKi0q4XWg/p4Qa6JQAdKOZziemK4/dJZaqH/EFijM1S", P8PrivateKeyId = "FQ6ZXC7U8L", ServerType = ApnServerType.Development, TeamId = "Y77A2C426U" };
                    AppleNotification appleNotificationDev = new AppleNotification();
                    appleNotificationDev.Aps.AlertBody = notification.Text;
                    appleNotificationDev.Notification = JsonConvert.SerializeObject(notification);
                    var apnDev = new ApnSender(devApnSettings, httpClient);
                    var resultDev = await apnDev.SendAsync(appleNotificationDev, deviceToken.Trim());
                    if (!resultDev.IsSuccess)
                    {
                        var notificationError = result.Error.Reason;
                        var device_Token = deviceToken;
                        var user = _unitOfWork.UserRepository.FindOne(x => x.DeviceToken.ToLower() == device_Token.ToLower());
                        string path = Directory.GetCurrentDirectory();
                        var num = string.IsNullOrEmpty(user.MobileNo) ? "" : user.MobileNo.ToString();
                        var email = string.IsNullOrEmpty(user.EmailID) ? "" : user.EmailID.ToString();
                        string newPath = path + "\\wwwroot\\ErrorLogFile\\AppleErrorDevices.txt";
                        using (StreamWriter writer = new StreamWriter(newPath, true))
                        {
                            writer.WriteLine("-----------------------------------------------------------------------------");
                            writer.WriteLine("Email : " + email.ToString());
                            writer.WriteLine("Mobile_No : " + num);
                            writer.WriteLine("Device_Token : " + device_Token.ToString());
                            writer.WriteLine("Error : " + notificationError.ToString());
                            writer.WriteLine("Mode : Development");
                            writer.WriteLine("-----------------------------------------------------------------------------");
                            writer.WriteLine();
                        }
                    }
                    else
                    {
                        var device_Token = deviceToken;
                        var user = _unitOfWork.UserRepository.FindOne(x => x.DeviceToken.ToLower() == device_Token.ToLower());
                        string path = Directory.GetCurrentDirectory();
                        var num = string.IsNullOrEmpty(user.MobileNo) ? "" : user.MobileNo.ToString();
                        var email = string.IsNullOrEmpty(user.EmailID) ? "" : user.EmailID.ToString();
                        string newPath = path + "\\wwwroot\\ErrorLogFile\\AppleWorkingDevices.txt";
                        using (StreamWriter writer = new StreamWriter(newPath, true))
                        {
                            writer.WriteLine("-----------------------------------------------------------------------------");
                            writer.WriteLine("Email : " + email.ToString());
                            writer.WriteLine("Mobile_No : " + num);
                            writer.WriteLine("Device_Token : " + device_Token.ToString());
                            writer.WriteLine("Mode : Development");
                            writer.WriteLine("-----------------------------------------------------------------------------");
                            writer.WriteLine();
                        }
                    }


                }
                else
                {
                    var device_Token = deviceToken;
                    var user = _unitOfWork.UserRepository.FindOne(x => x.DeviceToken.ToLower() == device_Token.ToLower());
                    string path = Directory.GetCurrentDirectory();
                    var num = string.IsNullOrEmpty(user.MobileNo) ? "" : user.MobileNo.ToString();
                    var email = string.IsNullOrEmpty(user.EmailID) ? "" : user.EmailID.ToString();
                    string newPath = path + "\\wwwroot\\ErrorLogFile\\AppleWorkingDevices.txt";
                    using (StreamWriter writer = new StreamWriter(newPath, true))
                    {
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine("Email : " + email);
                        writer.WriteLine("Mobile_No : " + num);
                        writer.WriteLine("Device_Token : " + device_Token.ToString());
                        writer.WriteLine("Mode : Production");
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine();
                    }
                }

            }
            catch (Exception ex)
            {
                string path = Directory.GetCurrentDirectory();
                string newPath = path + "\\wwwroot\\ErrorLogFile\\ErrorLogs.txt";
                using (StreamWriter writer = new StreamWriter(newPath, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Message : " + ex.Message.ToString());
                    writer.WriteLine("Exception : " + ex.ToString());
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine();
                }

            }
        }
    }
}
