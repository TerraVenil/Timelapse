using FluentAssertions;
using Timelapse.Models;
using Xunit;

namespace Timelapse.Tests
{
    public class CameraModelTests
    {
        [Theory]
        [InlineData("full/cam-name1/AAEAAQAAAAAAAAgyAAAAJDlkNzYzNDBmLWNmNTktNDlkNy05YTZhLWZmNDU4N2ViMGMzMw.jpg")]
        public void CreateFromPath(string path)
        {
            // Act
            var result = CameraModel.CreateFromPath(path);

            // Assert
            result.ShouldBeEquivalentTo(new CameraModel { Cam = "cam-name1", Name = "AAEAAQAAAAAAAAgyAAAAJDlkNzYzNDBmLWNmNTktNDlkNy05YTZhLWZmNDU4N2ViMGMzMw" });
        }
    }
}