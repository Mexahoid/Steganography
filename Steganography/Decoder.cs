using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Steganography
{
    /// <summary>
    /// Синглтон Decoder
    /// </summary>
    public class Decoder
    {
        /// <summary>
        /// Экземпляр синглтона Decoder
        /// </summary>
        private static Decoder _instance;

        private Bitmap _bitmap;

        /// <summary>
        /// Возвращает экземпляр синглтона класса Decoder.
        /// </summary>
        public static Decoder Instance => _instance ?? (_instance = new Decoder());

        /// <summary>
        /// Закрытый конструктор класса.
        /// </summary>
        private Decoder()
        {
        }



        public void Encode(string input, string path)
        {
            Bitmap btm = _bitmap;

            int length = input.Length * 4; // пиксели для длины
            // long == 64 бита == 32 блока

            StringBuilder sb = new StringBuilder();
            sb.Append(Convert.ToString(length, 2).PadLeft(32, '0'));
            foreach (char c in input)
            {
                short ci = (short)c;
                sb.Append(Convert.ToString(ci, 2).PadLeft(16, '0'));
            }

            int i = 0;
            StringBuilder sbb = new StringBuilder();
            for (int x = 0; x < btm.Height; x++)
            {
                for (int y = 0; y < btm.Width; y++)
                {
                    if (i >= sb.Length)
                    {
                        btm.Save(path);
                        return;
                    }
                    Color realPixel = btm.GetPixel(x, y);
                    sbb.Clear();
                    for (int j = 0; j < 4; j++, i++)
                    {
                        sbb.Append(sb[i]);
                    }

                    byte bt = Convert.ToByte(sbb.ToString(), 2);
                    byte green = (byte)((realPixel.G & 0b11111100) | ((bt >> 2) & 0b00000011));
                    byte blue = (byte)((realPixel.B & 0b11111100) | (bt & 0b00000011));

                    Color ciphered = Color.FromArgb(realPixel.R, green, blue);

                    btm.SetPixel(x, y, ciphered);
                }
            }

            
            //return Bitmap2BitmapImage(btm);
        }

        public string Decode(BitmapImage bitmap)
        {
            string a = null;
            BitmapImage2Bitmap(bitmap);

            StringBuilder sb = new StringBuilder();
            int n = 8;
            int i = 0;
            for (int x = 0; x < _bitmap.Height; x++)
            {
                for (int y = 0; y < _bitmap.Width; y++)
                {
                    if (i++ >= n)
                        break;

                    Color lp = _bitmap.GetPixel(x, y);

                    byte G = (byte) (lp.G & 0b00000011);
                    byte B = (byte) (lp.B & 0b00000011);
                    B += (byte)(G << 2);

                    sb.Append(Convert.ToString(B, 2).PadLeft(4, '0'));
                }
                if (i >= n)
                    break;
            }

            int length = Convert.ToInt32(sb.ToString(), 2);
            i--; // выровнять

            sb.Clear();

            n = 0;
            int k = 0;
            for (int x = 0; x < _bitmap.Height; x++)
            {
                for (int y = 0; y < _bitmap.Width; y++)
                {
                    if (n++ < i)
                        continue;
                    if (k++ >= length)
                        break;
                    Color lp = _bitmap.GetPixel(x, y);

                    byte G = (byte)(lp.G & 0b00000011);
                    byte B = (byte)(lp.B & 0b00000011);
                    B += (byte)(G << 2);

                    sb.Append(Convert.ToString(B, 2).PadLeft(4, '0'));
                }
                if (k > length)
                    break;
            }

            StringBuilder tmp = new StringBuilder();
            StringBuilder fin = new StringBuilder();
            int ass = sb.Length;
            for (int j = 0; j < sb.Length / 16; j++)
            {
                tmp.Clear();
                for (int l = 0; l < 16; l++)
                {
                    tmp.Append(sb[j * 16 + l]);
                }

                fin.Append((char)Convert.ToInt16(tmp.ToString(), 2));
            }

            return fin.ToString();
        }

        public int GetCapacity()
        {
            // 4, т.к. не рассматриваем красный канал
            return _bitmap.Height * _bitmap.Width / 2;
        }


        public void BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                _bitmap = new Bitmap(outStream);
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapImage retval;

            try
            {
                retval = (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }
    }
}
