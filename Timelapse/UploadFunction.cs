using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Util;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Timelapse.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Timelapse
{
    public class UploadFunction
    {
        private IAmazonS3 _amazonS3Client;

        private readonly INodeServices _nodeServices;

        public UploadFunction()
        {
            var services = new ServiceCollection();
            services.AddNodeServices(options =>
                                     {
                                         options.ProjectPath = "./";
                                     });
            LambdaLogger.Log("NodeServices added to ServiceCollection.");

            services.AddLogging();
            LambdaLogger.Log("Logging added to ServiceCollection.");

            var serviceProvider = services.BuildServiceProvider();
            LambdaLogger.Log("BuildServiceProvider() executed.");

            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddLambdaLogger();
            LambdaLogger.Log("AddLambdaLogger() executed.");

            _nodeServices = serviceProvider.GetRequiredService<INodeServices>();
            LambdaLogger.Log("Get INodeServices instance executed.");
        }

        public UploadFunction(IAmazonS3 amazonS3Client)
        {
            _amazonS3Client = amazonS3Client;

            var services = new ServiceCollection();

            services.AddLogging();

            services.AddNodeServices(options =>
                                     {
                                         options.ProjectPath = "./";
                                     });

            var serviceProvider = services.BuildServiceProvider();

            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory
                .AddLambdaLogger()
                .AddConsole()
                .AddDebug();

            _nodeServices = serviceProvider.GetRequiredService<INodeServices>();
        }

        public async Task<bool> ProcessImageHandler(S3Event @event)
        {
            S3EventNotification.S3Entity s3Entity = @event.Records[0].S3;
            if (s3Entity == null)
                return false;

            var keyName = Encoder.DecodeUriComponent(s3Entity.Object.Key);
            LambdaLogger.Log($"Processing -> { keyName }.");

            var bucketName = s3Entity.Bucket.Name;
            LambdaLogger.Log($"BucketName -> { bucketName }.");

            // Do we really need generation Guid similar to uuid.v4()?
            var tmpFile = $"/tmp/{ Guid.NewGuid() }";
            LambdaLogger.Log($"TmpFile -> { tmpFile }.");

            var cameraModel = CameraModel.CreateFromPath(keyName);
            LambdaLogger.Log($"cam -> { cameraModel.Cam } and name -> { cameraModel.Name }.");

            await ImageManager.WriteImage(bucketName, keyName, tmpFile);

            var configs = await ImageManager.GetConfig(bucketName, cameraModel);

            var imageFormatterJsFile = "./js/imageFormatter";
            LambdaLogger.Log($"imageFormatter.js in path -> { imageFormatterJsFile }.js is exists -> { File.Exists($"{imageFormatterJsFile}.js") } ?");

            if (configs.Crop != null || configs.Rotate != null)
            {
                var done = await CropAndRotateImage(tmpFile, imageFormatterJsFile, configs.Crop, configs.Rotate);
                LambdaLogger.Log($"Image cropped and rotated -> { done }.");
            }

            if (configs.Resize != null && configs.Resize.Length > 0)
                foreach (var resize in configs.Resize)
                {
                    var fileStream = await ResizeImage(tmpFile, imageFormatterJsFile, resize);
                    LambdaLogger.Log($"Image for { resize.Folder } resized.");

                    var key = $"{cameraModel.Cam}/{resize.Folder}/{cameraModel.Name}";
                    await ImageManager.UpdateImage(bucketName, key, fileStream);
                }

            await ImageManager.DeleteImage(bucketName, tmpFile);

            return true;
        }

        public async Task<Stream> ResizeImage(string tempFile, string pathToJsScript, Resize resize)
        {
            LambdaLogger.Log($"Resizing image -> { JsonConvert.SerializeObject(resize) }.");

            var result = await _nodeServices.InvokeExportAsync<Stream>(pathToJsScript, "resizeImage", tempFile, resize);
            return result;
        }

        public async Task<bool> CropAndRotateImage(string tempFile, string pathToJsScript, Crop crop, Rotate rotate)
        {
            LambdaLogger.Log($"Cropping and rotating image -> { JsonConvert.SerializeObject(crop) }, { JsonConvert.SerializeObject(rotate) }.");

            var result = await _nodeServices.InvokeExportAsync<bool>(pathToJsScript, "cropAndRotateImage", tempFile, crop, rotate);
            return result;
        }
    }
}
