using FluentAssertions;
using Xunit;

namespace Timelapse.Tests
{
    public class EncoderTests
    {
        [Theory]
        [InlineData("full%2Fcam-name1%2FAAEAAQAAAAAAAAgyAAAAJDlkNzYzNDBmLWNmNTktNDlkNy05YTZhLWZmNDU4N2ViMGMzMw.jpg", "full/cam-name1/AAEAAQAAAAAAAAgyAAAAJDlkNzYzNDBmLWNmNTktNDlkNy05YTZhLWZmNDU4N2ViMGMzMw.jpg")]
        public void DecodeUriComponent(string encodedPath, string expectedPath)
        {
            // Act
            var result = Encoder.DecodeUriComponent(encodedPath);

            // Assert
            result.ShouldBeEquivalentTo(expectedPath);
        }
    }
}