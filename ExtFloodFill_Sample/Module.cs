using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication2
{
    public static class GraphicsBitmapConverter
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int rasterOperation);
        private const int SRC_COPY = 0xcc0020;

        public static Bitmap GraphicsToBitmap(Graphics g, Rectangle bounds)
        {
            Bitmap bmp = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics bmpGrf = Graphics.FromImage(bmp))
            {
                IntPtr hdc1 = g.GetHdc();
                IntPtr hdc2 = bmpGrf.GetHdc();

                BitBlt(hdc2, 0, 0, bmp.Width, bmp.Height, hdc1, 0, 0, SRC_COPY);

                g.ReleaseHdc(hdc1);
                bmpGrf.ReleaseHdc(hdc2);
            }

            return bmp;
        }
    }

}
