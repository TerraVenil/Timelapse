namespace Timelapse.Models
{
    public sealed class Crop
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        public void Copy(Crop crop)
        {
            if (crop == null)
                return;

            Height = crop.Height;
            Width = crop.Width;
            Left = crop.Left;
            Top = crop.Top;
        }
    }
}