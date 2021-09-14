using System;
namespace NextLevelTrainingApi.Models
{
    public class S3Settings
    {
        public S3Settings()
        {
        }

        public string BucketName { get; set; }

        public string Key { get; set; }

        public string Secret { get; set; }

        public string BaseUrl { get; set; }
    }
}
