using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Image_HE_DHE_AHE_CLAHE
{
    public partial class Form1 : Form
    {
        private string openFileName;
        private Bitmap oriBitmap;
        private Bitmap resBitmap;
        private int iWidth;
        private int iHeight;
        private string saveFileName;
        private ImageBase imageBase;
        private bool isGray;
        private bool isRGB;

        public Form1()
        {
            InitializeComponent();
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "所有图像文件|*.bmp;*.png;*.pcx;*.jpg;*.gif;*.tif;*.ico";//文件格式可以再继续添加
            openFileDialog.Title = "打开图像文件";
            if(openFileDialog.ShowDialog()==DialogResult.OK)
            {
                openFileName = openFileDialog.FileName;
                try
                {
                    oriBitmap = (Bitmap)Image.FromFile(openFileName);
                }
                catch (Exception exp) 
                {
                    MessageBox.Show(exp.Message); 
                }
                iWidth = oriBitmap.Width;
                iHeight = oriBitmap.Height;
                label2.Text = iWidth.ToString() + "*" + iHeight.ToString();
                imageBase = new ImageBase();
                isGray = imageBase.IsGRAY(oriBitmap);
                isRGB = imageBase.IsRGB(oriBitmap);
                pictureBox1.Refresh();
                pictureBox1.Image = oriBitmap;
                int[,] orihist = imageBase.GetHist(oriBitmap);                                
            }
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "BMP(*.bmp)|*.bmp|JPEG(*.jpg)|*.jpg|PNG(*.png)|*.png";
            saveFileDialog.Title = "保存处理结果";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                saveFileDialog.AddExtension = true;//自动添加扩展名
                saveFileName = saveFileDialog.FileName;
                if(saveFileName!=""&&saveFileName!=null)
                { 
                string fileExtension = saveFileName.Substring(saveFileName.LastIndexOf('.') + 1).ToString();//获得保存图像的扩展名
                ImageFormat imageFormat = null;
                    if (fileExtension != "")
                    {
                        switch (fileExtension)
                        {
                            case "bmp":
                                {
                                    imageFormat = ImageFormat.Bmp;
                                    break;
                                }
                            case "jpg":
                                {
                                    imageFormat = ImageFormat.Jpeg;
                                    break;
                                }
                            case "png":
                                {
                                    imageFormat = ImageFormat.Png;
                                    break;
                                }
                            default:
                                {
                                    fileExtension = "jpg";
                                    saveFileName = saveFileName +"."+ fileExtension;
                                    imageFormat = ImageFormat.Jpeg;
                                    break;
                                }
                        }
                        try
                        {
                            resBitmap.Save(saveFileName, imageFormat);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show(exp.Message);
                        }
                    }
                }
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Color pointColor = oriBitmap.GetPixel(e.X, e.Y);
            label4.Text = e.X.ToString() + "," + e.Y.ToString();
            if (isRGB)
            {
                label6.Text = "R" + pointColor.R.ToString() + ",G" + pointColor.G.ToString() + ",B" + pointColor.B.ToString();
            }
            if (isGray)
            {
                label6.Text = pointColor.R.ToString();
            }
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            Color pointColor = resBitmap.GetPixel(e.X, e.Y);
            label9.Text = e.X.ToString() + "," + e.Y.ToString();
            if (isRGB)
            {
                label7.Text = "R" + pointColor.R.ToString() + ",G" + pointColor.G.ToString() + ",B" + pointColor.B.ToString();
            }
            if (isGray)
            {
                label7.Text = pointColor.R.ToString();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (oriBitmap != null)
            {
                if (resBitmap != null)
                    resBitmap.Dispose();
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        {
                            resBitmap = imageBase.HE(oriBitmap);
                            break;
                        }
                    case 1:
                        {
                            resBitmap = imageBase.DHE(oriBitmap);
                            break;
                        }
                    case 2:
                        {
                            int windows = Int32.Parse(textBox1.Text);
                            if(windows>Math.Min(iWidth,iHeight))
                            {
                                windows = Math.Min(iWidth, iHeight);
                                textBox1.Text = windows.ToString();
                            }
                            if(windows<2)
                            {
                                windows = 2;
                                textBox1.Text = windows.ToString();
                            }
                            resBitmap = imageBase.AHE(oriBitmap,windows);
                            break;
                        }
                    case 3:
                        {
                            double level = Double.Parse(textBox2.Text);
                            resBitmap = imageBase.CLHE(oriBitmap, level);
                            break;
                        }
                    case 4:
                        {
                            int windows = Int32.Parse(textBox1.Text);
                            if (windows > Math.Min(iWidth, iHeight))
                            {
                                windows = Math.Min(iWidth, iHeight);
                                textBox1.Text = windows.ToString();
                            }
                            if (windows < 2)
                            {
                                windows = 2;
                                textBox1.Text = windows.ToString();
                            }
                            double level = Double.Parse(textBox2.Text);
                            resBitmap = imageBase.CLAHE(oriBitmap, windows, level);
                            break;
                        }
                    default:
                        {
                            resBitmap = imageBase.OnlyClone(oriBitmap);
                            break;
                        }
                }
                int[,] reshist = imageBase.GetHist(resBitmap);
                pictureBox2.Image = resBitmap;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(comboBox1.SelectedIndex)
            {
                case 2:
                    {
                        label11.Enabled = true;
                        label11.Visible = true;
                        textBox1.Enabled = true;
                        textBox1.Visible = true;
                        label12.Enabled = false;
                        label12.Visible = false;
                        textBox2.Enabled = false;
                        textBox2.Visible = false;
                        break;
                    }
                case 3:
                    {
                        label11.Enabled = false;
                        label11.Visible = false;
                        textBox1.Enabled = false;
                        textBox1.Visible = false;
                        label12.Enabled = true;
                        label12.Visible = true;
                        textBox2.Enabled = true;
                        textBox2.Visible = true;
                        break;
                    }
                case 4:
                    {
                        label11.Enabled = true;
                        label11.Visible = true;
                        textBox1.Enabled = true;
                        textBox1.Visible = true;
                        label12.Enabled = true;
                        label12.Visible = true;
                        textBox2.Enabled = true;
                        textBox2.Visible = true;
                        break;
                    }
                default:
                    {
                        label11.Enabled = false;
                        label11.Visible = false;
                        textBox1.Enabled = false;
                        textBox1.Visible = false;
                        label12.Enabled = false;
                        label12.Visible = false;
                        textBox2.Enabled = false;
                        textBox2.Visible = false;
                        break;
                    }
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar)&&e.KeyChar!='\b')
                e.Handled = true;
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != '.')
                e.Handled = true;
            if (e.KeyChar == '.' && textBox2.Text.Trim() == "")
                e.Handled = true;
            if (e.KeyChar == '.' && textBox2.Text.Contains("."))
                e.Handled = true;
        }
    }
}
