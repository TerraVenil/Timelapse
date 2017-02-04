using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Util;
using FluentAssertions;
using NSubstitute;
using Timelapse.Models;

namespace Timelapse.Tests
{
    public class UploadFunctionTest
    {
        [Fact]
        public async Task UploadFunction_When_EmptyS3Event()
        {
            // Arrange
            S3Event s3Event = new S3Event { Records = new List<S3EventNotification.S3EventNotificationRecord> { new S3EventNotification.S3EventNotificationRecord { S3 = null } } };
            var uploadFunction = new UploadFunction();

            // Act
            var result = await uploadFunction.ProcessImageHandler(s3Event);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UploadFunction_When_NotEmptyS3Event()
        {
            // Arrange
            S3Event s3Event =
                new S3Event
                {
                    Records = new List<S3EventNotification.S3EventNotificationRecord>
                              {
                                  new S3EventNotification.S3EventNotificationRecord
                                  {
                                      S3 = new S3EventNotification.S3Entity
                                           {
                                               Bucket = new S3EventNotification.S3BucketEntity
                                                        {
                                                            Name = "my-hubsy-image-bucket-name1"
                                                        },
                                               Object = new S3EventNotification.S3ObjectEntity
                                                        {
                                                            Key = "full%2Fcam-name1%2FBA.jpg"
                                                        }
                                           }
                                  }
                              }
                };
            var uploadFunction = new UploadFunction();

            // Act
            var result = await uploadFunction.ProcessImageHandler(s3Event);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CropAndRotateImage()
        {
            // Arrange
            var pathToJsScript = @"~\Timelapse\Timelapse\js\imageFormatter.js";
            var configModel = CreateConfigModel();

            var amazonS3Client = Substitute.For<IAmazonS3>();
            var uploadFunction = new UploadFunction(amazonS3Client);

            // Act
            var result = await uploadFunction.CropAndRotateImage("BA.jpg", pathToJsScript, configModel.Crop, configModel.Rotate);

            // Assert
            result.ShouldBeEquivalentTo(new List<IDictionary<string, string>>());
        }

        [Fact]
        public async Task GetConfigs()
        {
            // Arrange
            var expectedConfigModel = CreateConfigModel();

            // Act
            var result = await ImageManager.GetConfig("my-hubsy-image-bucket-name1", new CameraModel { Cam = "cam-name1", Name = "BA" });

            // Assert
            result.ShouldBeEquivalentTo(expectedConfigModel);
        }

        private ConfigModel CreateConfigModel()
        {
            return new ConfigModel
                   {
                       Resize = new[]
                                {
                                    new Resize
                                    {
                                        Width = 1920,
                                        Height = 1080,
                                        Quality = 50,
                                        Folder = "resized/fhd"
                                    },
                                    new Resize
                                    {
                                        Width = 1080,
                                        Height = 720,
                                        Quality = 50,
                                        Folder = "resized/hd"
                                    },
                                    new Resize
                                    {
                                        Width = 500,
                                        Height = 500,
                                        Quality = 50,
                                        Folder = "resized/small"
                                    }
                                },
                       Crop = new Crop
                              {
                                  Width = 10,
                                  Height = 30,
                                  Left = 30,
                                  Top = 70
                              },
                       Rotate = new Rotate
                                {
                                    Degrees = 35,
                                    Color = "black"
                                }
                   };
        }
    }
}
