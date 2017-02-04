using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Timelapse.Models;

namespace Timelapse
{
    public static class ImageManager
    {
        private static BasicAWSCredentials BasicAwsCredentials => new BasicAWSCredentials("", "");

        private static RegionEndpoint RegionEndpoint => RegionEndpoint.EUCentral1;

        private static string ConfigJson = "config.json";

        public static async Task<ConfigModel> GetConfig(string bucketName, CameraModel cameraModel)
        {
            LambdaLogger.Log("GetConfigs executing.");

            ConfigModel bucketConfigModel = await GetBucketConfig(bucketName);

            ConfigModel cameraConfigModel = await GetCameraConfig(bucketName, cameraModel.Cam);

            LambdaLogger.Log("GetConfigs executed.");

            LambdaLogger.Log("Prepare to merge configs.");
            if (bucketConfigModel == null && cameraConfigModel == null)
                return ConfigModel.Empty;

            if (bucketConfigModel == null)
                return cameraConfigModel;

            if (cameraConfigModel == null)
                return bucketConfigModel;

            return bucketConfigModel.Merge(cameraConfigModel);
        }

        public static async Task WriteImage(string bucketName, string key, string tmpFile)
        {
            using (IAmazonS3 amazonS3Client = CreateAmazonS3Client())
            {
                GetObjectRequest request = new GetObjectRequest { BucketName = bucketName, Key = key };
                using (GetObjectResponse response = await amazonS3Client.GetObjectAsync(request))
                {
                    await response.WriteResponseStreamToFileAsync(tmpFile, false, CancellationToken.None);
                }
            }
        }

        public static async Task UpdateImage(string bucketName, string key, Stream fileStream)
        {
            using (IAmazonS3 amazonS3Client = CreateAmazonS3Client())
            {
                var request = new PutObjectRequest { BucketName = bucketName, Key = key, InputStream = fileStream, ContentType = "image/jpeg" };
                PutObjectResponse response = await amazonS3Client.PutObjectAsync(request, CancellationToken.None);
                LambdaLogger.Log($"Resized image uploaded to -> ${ key } with status -> { response.HttpStatusCode }.");
            }
        }

        public static async Task DeleteImage(string bucketName, string key)
        {
            using (IAmazonS3 amazonS3Client = CreateAmazonS3Client())
            {
                DeleteObjectRequest request = new DeleteObjectRequest { BucketName = bucketName, Key = key };
                DeleteObjectResponse response = await amazonS3Client.DeleteObjectAsync(request, CancellationToken.None);
                LambdaLogger.Log($"Image { key } deleted with status -> { response.HttpStatusCode }.");
            }
        }

        private static AmazonS3Client CreateAmazonS3Client()
        {
            return new AmazonS3Client(BasicAwsCredentials, RegionEndpoint);
        }

        private static async Task<ConfigModel> GetBucketConfig(string bucketName)
        {
            return await GetConfig(bucketName, ConfigJson);
        }

        private static async Task<ConfigModel> GetCameraConfig(string bucketName, string cam)
        {
            // Fix with adding /full path to key -> The specified key is not found ?!
            var key = $"full/{cam}/config.json";
            return await GetConfig(bucketName, key);
        }

        private static async Task<ConfigModel> GetConfig(string bucketName, string key)
        {
            using (IAmazonS3 amazonS3Client = CreateAmazonS3Client())
            {
                GetObjectRequest request = new GetObjectRequest { BucketName = bucketName, Key = key };
                using (GetObjectResponse response = await amazonS3Client.GetObjectAsync(request))
                {
                    LambdaLogger.Log($"Deserialization of { bucketName } and { key }.");
                    return Serializer.Deserialize<ConfigModel>(response.ResponseStream);
                }
            }
        }
    }
}