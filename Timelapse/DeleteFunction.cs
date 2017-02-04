using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3.Util;
using Timelapse.Models;

namespace Timelapse
{
    public class DeleteFunction
    {
        public async Task<bool> DeleteImageHandler(S3Event @event)
        {
            if (@event.Records == null)
                return false;

            var deletedTasks = @event.Records.Select(record => HandleDeletion(record.S3));

            await Task.WhenAll(deletedTasks);

            return true;
        }

        public async Task HandleDeletion(S3EventNotification.S3Entity s3Entity)
        {
            var keyName = Encoder.DecodeUriComponent(s3Entity.Object.Key);
            LambdaLogger.Log($"Processing -> { keyName } deletion.");

            var bucketName = s3Entity.Bucket.Name;
            LambdaLogger.Log($"BucketName -> { bucketName }.");

            var cameraModel = CameraModel.CreateFromPath(keyName);
            LambdaLogger.Log($"cam -> { cameraModel.Cam } and name -> { cameraModel.Name }.");

            var configs = await ImageManager.GetConfig(bucketName, cameraModel);
            if (configs.Resize != null && configs.Resize.Length > 0)
            {
                var deletedTasks = configs.Resize
                                          .Select(resize => $"{cameraModel.Cam}/{resize.Folder}/{cameraModel.Name}.jpg")
                                          .Select(key => ImageManager.DeleteImage(bucketName, key));

                await Task.WhenAll(deletedTasks);
            }
        }
    }
}