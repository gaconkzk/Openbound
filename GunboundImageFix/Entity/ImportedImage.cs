using System.Drawing;

namespace GunboundImageFix.Entity
{
    public class ImportedImage
    {
        public string FilePath;
        public Bitmap Image;
        public (int, int) Pivot;

        public ImportedImage(string path)
        {
            FilePath = path;
            Image = (Bitmap)System.Drawing.Image.FromFile(path);
        }
    }
}
