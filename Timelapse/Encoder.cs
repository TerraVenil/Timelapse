using System;

namespace Timelapse
{
    public static class Encoder
    {
        public static string DecodeUriComponent(string encodedUriComponent)
        {
            var result = Uri.UnescapeDataString(encodedUriComponent);
            return result;
        }
    }
}