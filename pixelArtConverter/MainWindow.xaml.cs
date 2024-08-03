using Microsoft.Win32;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Windows.Interop;
using System.Drawing.Imaging;
using System;

namespace pixelArtConverter
{
    //https://github.com/BrandonHilde/PixelArt
    //hugest thanks to ^^^^

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int pixelDensity = 6;

        public MainWindow()
        {
            InitializeComponent();
            saveButton.IsEnabled = true;
            convertButton.IsEnabled = false;
            pixelDesnityCB.IsEnabled = false;
        }

        Bitmap original = new Bitmap(1,1);
        Bitmap newImage = new Bitmap(1,1);

        System.Drawing.Color[] clrs = new System.Drawing.Color[1];
        Graphics g = null;

        //selects and uploads an image file and displays it in the originalImageBox control.
        private void selectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileSelector = new OpenFileDialog();
            fileSelector.Filter = "Image Files (JPG,PNG,GIF)|*.JPG;*.PNG;*.GIF";

            string filePath = string.Empty;

            bool? response = fileSelector.ShowDialog();
            if (response == true)
            {
                filePath = fileSelector.FileName;
                try
                {
                    original = new Bitmap(filePath);
                    BitmapImage orig = new BitmapImage(new Uri(filePath));
                    originalImageBox.Source = orig;
                }
                catch
                {
                    MessageBox.Show("Failed to upload image, try again.");
                }
            }
            else
            {
                MessageBox.Show("Unable to select a file, try again.");
            }

            convertButton.IsEnabled = true;
            pixelDesnityCB.IsEnabled = true;
            pixelDesnityCB.SelectedIndex = 3;
        }

        //convert image to pixel art version and display in the newImageBox control.
        private void convertButton_Click(object sender, RoutedEventArgs e)
        {
            string colourString = string.Empty;

            if (original != null)
            {
                if (original.Width > 1)
                {
                    colourString = "#FFFFFF,#000000,";

                    Random r = new Random(DateTime.Now.Millisecond);

                    for (int i = 0; i < 16; i++)
                    {
                        int x = r.Next(0, Convert.ToInt32(original.Width));
                        int y = r.Next(0, Convert.ToInt32(original.Height));
                        colourString += ColorTranslator.ToHtml(original.GetPixel(x, y));
                        colourString += ",";
                    }
                }
            }
            string[] colourArr = colourString.Split(',');
            clrs = new System.Drawing.Color[colourArr.Length];
            

            for (int v = 0; v < colourArr.Length; v++)
            {
                try
                {
                    clrs[v] = ColorTranslator.FromHtml(colourArr[v]);
                }
                catch
                {
                    clrs[v] = System.Drawing.Color.Transparent;
                }
            }

            int num = pixelDesnityCB.SelectedIndex + 1;
            newImage = new Bitmap(original.Width, original.Height);

            using (g = Graphics.FromImage(newImage))
            {
                List<System.Drawing.Color> block = new List<System.Drawing.Color>();

                System.Drawing.Rectangle rec = new System.Drawing.Rectangle();

                SolidBrush sb = new SolidBrush(System.Drawing.Color.Black);

                System.Drawing.Color final = System.Drawing.Color.Black;

                for (int x = 0; x < original.Width; x += num)
                {
                    for (int y = 0; y < original.Height; y += num)
                    {
                        block = new List<System.Drawing.Color>();

                        for (int v = 0; v < num; v++)
                        {
                            for (int c = 0; c < num; c++)
                            {
                                if (x + v < original.Width && y + c < original.Height)
                                {
                                    block.Add(original.GetPixel(x + v, y + c));
                                }
                            }
                        }

                        if (block.Count > 0)
                        {
                            final = Clr(block.ToArray());

                            sb.Color = final;

                            rec.X = x;
                            rec.Y = y;
                            rec.Width = num;
                            rec.Height = num;

                            g.FillRectangle(sb, rec);
                        }
                    }
                }
                BitmapImage temp = Bitmap2BitmapImage(newImage);
                newImageBox.Source = temp;

                saveButton.IsEnabled = true;
            }    
        }

        //https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }


        System.Drawing.Color Clr(System.Drawing.Color[] cs)
        {
            System.Drawing.Color c = System.Drawing.Color.Black;

            int r = 0;
            int g = 0;
            int b = 0;

            for (int i = 0; i < cs.Length; i++)
            {
                r += cs[i].R;
                g += cs[i].G;
                b += cs[i].B;
            }

            r /= cs.Length;
            g /= cs.Length;
            b /= cs.Length;

            int near = 1000;
            int ind = 0;

            for (int cl = 0; cl < clrs.Length; cl++)
            {
                int valR = (clrs[cl].R - r);
                int valG = (clrs[cl].G - g);
                int valB = (clrs[cl].B - b);

                if (valR < 0) valR = -valR;
                if (valG < 0) valG = -valG;
                if (valB < 0) valB = -valB;

                int total = valR + valG + valB;

                if (total < near)
                {
                    ind = cl;
                    near = total;
                }
            }

            c = clrs[ind];

            return c;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sv = new SaveFileDialog();
            sv.Filter = "Image Files (JPG,PNG,GIF)|*.JPG;*.PNG;*.GIF";
            sv.ShowDialog();

            newImage.Save(sv.FileName, ImageFormat.Png);
        }
    }
}