using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        [DllImport("gdi32.dll")]
        private static extern int BitBlt(IntPtr targetHandle, int targetX, int targetY, int targetWidth, int targetHeight, IntPtr sourceHandle, int sourceX, int sourceY, int rasterOperation);
        private const int SRC_COPY = 0xcc0020;

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        public static extern bool ExtFloodFill(IntPtr hdc, int nXStart, int nYStart, int crColor, uint fuFillType);
        public static uint FLOODFILLSURFACE = 1;

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(int crColor);

        [DllImport("gdi32.dll")]
        public static extern int GetPixel(IntPtr hdc, int x, int y);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern IntPtr DeleteDC(IntPtr hDc);

        public Form1()
        {
            InitializeComponent();
        }

        //Load Sample Image
        private void Form1_Load(object sender, EventArgs e)
        {
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string file_name = System.IO.Path.GetDirectoryName(strExeFilePath) + "\\sample.bmp";

            //Load Sample Image
            PictureBox pic = new PictureBox();
            pic.Load(file_name);
            if (pic.Image != null)
                picBox1.Image = pic.Image;    
        }

        //Copy the image and draw a border.
        private void button1_Click(object sender, EventArgs e)
        {
            picBox2.Image = myDrwaRectangle(picBox1.Image, 0, 0, Color.Red);
            picBox2.Invalidate();
        }

        //Drawing border
        private Image myDrwaRectangle(Image oImage, int x, int y, Color oNewColor)
        {
            Bitmap bmp = new Bitmap(oImage.Width, oImage.Height, PixelFormat.Format24bppRgb);

            Graphics gfx = Graphics.FromImage(bmp);
            gfx.DrawImage(oImage, new Point(0, 0));

            Pen nPen = new Pen(Color.FromArgb(255, 255, 0, 0), 6); //6은 구께
            gfx.DrawRectangle(nPen, 0, 0, oImage.Width, oImage.Height);
            return bmp;
        }

        //ExtFloodFill with gdi32.dll
        private void button2_Click(object sender, EventArgs e)
        {
            //Get the DC of the original image.
            Graphics g1 = picBox1.CreateGraphics();
            IntPtr g1_HDC = g1.GetHdc();

            //Create the compatible DC.
            IntPtr g2_HDC = CreateCompatibleDC(g1_HDC);
            IntPtr g2_Bmp = CreateCompatibleBitmap(g1_HDC, picBox1.Image.Width, picBox1.Image.Height);
            IntPtr g2_PreviouseBitmap = SelectObject(g2_HDC, g2_Bmp);

            //copy from g1_HDC to g2_HDC
            BitBlt(g2_HDC, 0, 0, picBox1.Image.Width, picBox1.Image.Height, g1_HDC, 0, 0, SRC_COPY);

            //Create new brush
            IntPtr g2_Brush = CreateSolidBrush(ColorTranslator.ToWin32(Color.Red));
            IntPtr g2_PreviouseBrush = SelectObject(g2_HDC, g2_Brush);

            //FloodFill with a position
            int cx = 140;
            int cy = 104;
            int color = GetPixel(g2_HDC, cx, cy); //color = 32768; 
            ExtFloodFill(g2_HDC, cx, cy, color, 1);

            //Restore to the original brush.
            SelectObject(g2_HDC, g2_PreviouseBrush); 
            DeleteObject(g2_Brush);

            //create an empty image
            Bitmap bmp = new Bitmap(picBox1.Image.Width, picBox1.Image.Height);
            Graphics g3 = Graphics.FromImage(bmp);
            IntPtr g3_HDC = g3.GetHdc();

            //Copy the resulting image
            BitBlt(g3_HDC, 0, 0, bmp.Width, bmp.Height, g2_HDC, 0, 0, SRC_COPY);
            picBox2.Image = bmp;
            picBox2.Invalidate();

            //Restore to the oreviouse bitmap.
            SelectObject(g2_HDC, g2_PreviouseBitmap);
            DeleteObject(g2_Bmp);

            //Remove all DC object
            DeleteDC(g2_HDC);
            g1.ReleaseHdc(g1_HDC);
            g3.ReleaseHdc(g3_HDC);
            g1.Dispose();
            g3.Dispose();
        }        
    }
}
