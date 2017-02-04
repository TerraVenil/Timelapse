namespace Timelapse.Models
{
    public sealed class Rotate
    {
        public int? Degrees { get; set; }

        public string Color { get; set; }

        public void Copy(Rotate rotate)
        {
            if (rotate == null)
                return;

            Degrees = rotate.Degrees;
            Color = rotate.Color;
        }
    }
}