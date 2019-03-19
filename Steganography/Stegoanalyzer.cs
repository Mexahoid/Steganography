using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Steganography
{
    /// <summary>
    /// Синглтон Stegoanalyzer
    /// </summary>
    public class Stegoanalyzer
    {
        /// <summary>
        /// Экземпляр синглтона Stegoanalyzer
        /// </summary>
        private static Stegoanalyzer _instance;

        private Bitmap _bitmapFirst, _bitmapSecond;
        private string _raw;
        public int Bits { get; set; }
        public int Flag { get; set; }

        private List<byte> _diffFirst, _diffSecond;

        private const bool LSB_2 = true;

        /// <summary>
        /// Возвращает экземпляр синглтона класса Stegoanalyzer.
        /// </summary>
        public static Stegoanalyzer Instance => _instance ?? (_instance = new Stegoanalyzer());

        /// <summary>
        /// Закрытый конструктор класса.
        /// </summary>
        private Stegoanalyzer() 
        {
        }

        public string Reset()
        {
            StringBuilder sb = new StringBuilder();


            string str = "";
            for (int i = 0; i < _diffFirst.Count; i++)
            {
                byte f = _diffFirst[i];
                byte s = _diffSecond[i];

                switch (Flag)
                {
                    case 1:
                        str = Convert.ToString(f, 2).PadLeft(8, '0').Substring(8 - Bits, Bits);
                        break;
                    case 2:
                        str = Convert.ToString(s, 2).PadLeft(8, '0').Substring(8 - Bits, Bits);
                        break;
                    case 3:
                        str = Convert.ToString((byte)(f ^ s), 2).PadLeft(8, '0').Substring(8 - Bits, Bits);
                        break;
                }
                sb.Append(str);
            }

            if(sb.Length % 8 != 0)
                throw new Exception("Не подходит по длине");

            StringBuilder tmp = new StringBuilder();
            StringBuilder osb = new StringBuilder();
            for (int i = 0; i < sb.Length / 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    tmp.Append(sb[i * 8 + j]);
                }
                byte bt = Convert.ToByte(tmp.ToString(), 2);
                osb.Append((char) bt);
                tmp.Clear();
            }


            return osb.ToString();
        }

        public void BitmapImage2Bitmap(BitmapImage bitmapImage, bool first)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                if(first)
                    _bitmapFirst = new Bitmap(bitmap);
                else
                    _bitmapSecond = new Bitmap(bitmap);
                
            }
        }

        private List<byte> GetBitmapAsInts(Bitmap btm)
        {
            List<byte> list = new List<byte>();

            for (int i = 0; i < btm.Width; i++)
            {
                for (int j = 0; j < btm.Height; j++)
                {
                    var ss = btm.GetPixel(i, j);
                    int a = btm.GetPixel(i, j).ToArgb();
                    //string ass = Convert.ToString(a, 2);
                    //list.Add((byte)(a >> 24));
                    list.Add((byte)(a >> 16));
                    list.Add((byte)(a >> 8));
                    list.Add((byte)a);
                }
            }

            return list;
        }

        public string GetData()
        {
            var first = GetBitmapAsInts(_bitmapFirst);
            var second = GetBitmapAsInts(_bitmapSecond);
            FindDiff(first, second);



            return "";
        }

        private void FindDiff(List<byte> first, List<byte> second)
        {
            string[] variants = new string[2];

            if(first.Count != second.Count)
                throw new Exception("Разные изображения");

            _diffFirst = new List<byte>();
            _diffSecond = new List<byte>();

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < first.Count; i++)
            {
                byte f = first[i];
                byte s = second[i];

                if (f == s)
                    continue;
                _diffFirst.Add(f);
                _diffSecond.Add(s);

                //byte diff1 = (byte)((f ^ s) & s);
                sb.Append($"{i}\n");
                string ff = Convert.ToString(f, 2).PadLeft(8, '0');
                sb.Append(ff);
                sb.Append('\n');
                string ss = Convert.ToString(s, 2).PadLeft(8, '0');
                sb.Append(ss);
                sb.Append('\n');
                //string dd = Convert.ToString(diff1, 2).PadLeft(8, '0');

            }

            using (StreamWriter sw = new StreamWriter("itams.txt"))
            {
                sw.Write(sb.ToString());
            }

        }

        private string[] GetDataFromBitmap(bool first)
        {
            Bitmap btm = first ? _bitmapFirst : _bitmapSecond;

            string[] arr = new string[2];

            List<byte> pixelList = GetBitmapAsInts(btm);
            StringBuilder sb = new StringBuilder();

            int bound = LSB_2 ? 4 : 8;
            byte shift = LSB_2 ? 2 : 1;
            byte mask = LSB_2 ? 0b00000011 : 0b00000001;
            for (int i = 0; i < pixelList.Count / bound; i+= bound)
            {
                byte res = 0;
                for (int j = 0; j < bound; j++)
                {
                    res <<= shift;
                    byte pixelData = (byte)(pixelList[i + j] & mask);
                    string ass = Convert.ToString(pixelData, 2).PadLeft(8, '0');
                    res += pixelData;
                    string ress = Convert.ToString(res, 2).PadLeft(8, '0');
                }

                sb.Append((char) res);
            }

            _raw = sb.ToString();
            arr[0] = _raw;

            return arr;
        }
    }
}
