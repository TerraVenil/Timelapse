using System.Linq;

namespace Timelapse.Models
{
    public sealed class ConfigModel
    {
        public static ConfigModel Empty => new ConfigModel();

        public Crop Crop { get; set; }

        public Rotate Rotate { get; set; }

        public Resize[] Resize { get; set; }

        public ConfigModel Merge(ConfigModel configModel)
        {
            if (Crop != null)
                Crop.Copy(configModel.Crop);
            else
                Crop = configModel.Crop;

            if (Resize != null && configModel.Resize != null)
                foreach (var resize in Resize)
                {
                    var cameraResize = configModel.Resize.FirstOrDefault(x => x.Folder == resize.Folder);
                    resize.Copy(cameraResize);
                }
            else if (configModel.Resize != null)
                Resize = configModel.Resize;

            if (Rotate != null)
                Rotate.Copy(configModel.Rotate);
            else
                Rotate = configModel.Rotate;

            return this;
        }
    }
}