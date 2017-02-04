namespace Timelapse.Models
{
    public sealed class Resize
    {
        public string Folder { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int? Quality { get; set; }

        public void Copy(Resize resize)
        {
            if (resize == null)
                return;

            Folder = resize.Folder;
            Width = resize.Width;
            Height = resize.Height;
            Quality = resize.Quality;
        }
    }
}