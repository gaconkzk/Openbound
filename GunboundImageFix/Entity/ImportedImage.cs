using System.Drawing;

namespace GunboundImageFix.Entity
{
    public class ImportedImage
    {
        public string FilePath;
        public Bitmap BitmapImage;
        public (int, int) Pivot;
        public (int, int) RiderOffset;
        public (int, int) RealRiderOffset;

        public ImportedImage(Bitmap bitmapImage, (int, int) pivot, (int, int) riderOffset)
        {
            BitmapImage = bitmapImage;
            Pivot = pivot;
            RealRiderOffset = riderOffset;
            RealRiderOffset = (Pivot.Item1 + RealRiderOffset.Item1, Pivot.Item2 + RealRiderOffset.Item2);
        }

        public ImportedImage(string path)
        {
            FilePath = path;
            BitmapImage = (Bitmap)Image.FromFile(path);
        }
    }
}
