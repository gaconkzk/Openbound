using GunboundImageFix.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GunboundImageFix.Helper
{
    public class PaintHelper
    {
        public static void OpenPictureFromOutputFolder(string filename)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo procInfo = new System.Diagnostics.ProcessStartInfo();
                procInfo.FileName = ("mspaint.exe");
                procInfo.Arguments = Parameters.SpritesheetOutputDirectory + filename;
                System.Diagnostics.Process.Start(procInfo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
