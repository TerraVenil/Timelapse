using System.Text.RegularExpressions;
using Amazon.Lambda.Core;

namespace Timelapse.Models
{
    public sealed class CameraModel
    {
        public static CameraModel CreateFromPath(string path)
        {
            var result = Regex.Match(path, @"^full\/(.*)\/(.*)\.jpg", RegexOptions.IgnoreCase);
            foreach (Group group in result.Groups)
            {
                LambdaLogger.Log($"Parse path -> { group.Value }.");
            }

            var cameraModel = new CameraModel
                              {
                                  Cam = result.Groups[1].Value,
                                  Name = Encoder.DecodeUriComponent(result.Groups[2].Value)
                              };
            return cameraModel;
        }

        public string Cam { get; set; }

        public string Name { get; set; }
    }
}