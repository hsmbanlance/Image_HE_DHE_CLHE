using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;


namespace Image_HE_DHE_AHE_CLAHE
{
    class ImageBase
    {
        /*
         ImageBase类
        存放用到的图像处理方法
        IsGray、IsRGB判断颜色空间
        GetHist获取直方图
        HE直方图均衡化
        DHE动态直方图均衡化
        OnlyClone复制图像
        AHE自适应直方图均衡化
        CLHE限制对比度的直方图均衡化
        CLAHE限制对比度的自适应直方图均衡化
         */
        //判断图像颜色空间
        public bool IsGRAY(Bitmap bitmap)
        {
            int flags = bitmap.Flags;//图像的Flags属性
            if ((flags & 64) == 64)//Gray空间为含64
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsRGB(Bitmap bitmap)
        {
            int flags = bitmap.Flags;//图像的Flags属性
            if ((flags & 16) == 16)//RGB空间为含16
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //获取直方图
        public int[,] GetHist(Bitmap bitmap)
        {
            int width = bitmap.Width;//宽度
            int heigth = bitmap.Height;//高度
            if (IsGRAY(bitmap))//Gray空间
            {
                int[,] hist = new int[256, 1];//存放直方图
                for (int j = 0; j < heigth; j++)
                    for (int i = 0; i < width; i++)
                    {
                        int grey = bitmap.GetPixel(i, j).R;//读取灰度值
                        hist[grey, 0]++;//对应项+1
                    }
                return hist;
            }
            else//依照RGB空间处理
            {
                int[,] hist = new int[256, 3];//存放直方图
                for (int j = 0; j < heigth; j++)
                    for (int i = 0; i < width; i++)
                    {
                        uint[] grey = new uint[3];//存放直方图，0-2RGB
                        grey[0] = bitmap.GetPixel(i, j).R;//读取灰度值
                        grey[1] = bitmap.GetPixel(i, j).G;
                        grey[2] = bitmap.GetPixel(i, j).B;
                        hist[grey[0], 0]++;//对应项+1
                        hist[grey[1], 1]++;
                        hist[grey[2], 2]++;
                    }
                return hist;
            }
        }
        //HE
        public Bitmap HE(Bitmap bitmap)
        {
            int width = bitmap.Width;//宽度
            int heigth = bitmap.Height;//高度
            Rectangle rectangle = new Rectangle(0, 0, width, heigth);//矩形框
            PixelFormat pixelFormat = bitmap.PixelFormat;//像素格式
            Bitmap bitmap1 = bitmap.Clone(rectangle,pixelFormat);//Clone图片
            if (IsGRAY(bitmap))//Gray空间
            {
                int grey;//灰度
                Color color = new Color();
                int[,] hist = GetHist(bitmap);
                int[,] outhist= new int[256, 1];//用于映射的灰度表
                double[,] middlehist = new double[256, 1];//中间处理
                double p = (double)255 / (width * heigth);
                for (int i = 1; i < 256; i++)//累计
                    middlehist[i, 0] = middlehist[i - 1, 0] + hist[i, 0];
                for (int i = 0; i < 256; i++)
                {
                    middlehist[i, 0] = middlehist[i, 0] * p;//归一化后*255
                    outhist[i, 0] = (int)middlehist[i, 0];//映射表
                }
                for (int j = 0; j < heigth; j++)
                    for (int i = 0; i < width; i++)
                    {
                        grey = bitmap.GetPixel(i, j).R;//读映射表
                        color = Color.FromArgb(outhist[grey,0], outhist[grey, 0], outhist[grey, 0]);//获得颜色
                        bitmap1.SetPixel(i, j, color);//设置像素
                    }
            }
            else
            {
                int r, g, b;//RGB
                Color color = new Color();
                int[,] hist = GetHist(bitmap);//获取直方图
                int[,] outhist = new int[256, 3];//输出直方图
                double[,] middlehist = new double[256, 3];//中间处理
                double p = (double)255 / (width * heigth);
                for (int i = 1; i < 256; i++)
                    for (int j = 0; j < 3; j++)
                        middlehist[i, j] = middlehist[i - 1, j] + hist[i, j];//累加
                for (int i = 0; i < 256; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        middlehist[i, j] = middlehist[i, j] * p;
                        outhist[i, j] = (int)middlehist[i, j];//建立映射表
                    }
                for (int j = 0; j < heigth; j++)
                    for (int i = 0; i < width; i++)
                    {
                        r = bitmap.GetPixel(i, j).R;//读表
                        g = bitmap.GetPixel(i, j).G;
                        b = bitmap.GetPixel(i, j).B;
                        color = Color.FromArgb(outhist[r,0],outhist[g,1],outhist[b,2]);//获得颜色
                        bitmap1.SetPixel(i, j, color);//设置像素
                    }
            }
            return bitmap1;
        }
        //OnlyClone
        public Bitmap OnlyClone(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int heigth = bitmap.Height;
            Rectangle rectangle = new Rectangle(0, 0, width, heigth);
            PixelFormat pixelFormat = bitmap.PixelFormat;
            Bitmap bitmap1 = bitmap.Clone(rectangle, pixelFormat);
            return bitmap1;
        }
        //DHE
        public Bitmap DHE(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int heigth = bitmap.Height;
            Rectangle rectangle = new Rectangle(0, 0, width, heigth);
            PixelFormat pixelFormat = bitmap.PixelFormat;
            Bitmap bitmap1 = bitmap.Clone(rectangle, pixelFormat);
            if (IsGRAY(bitmap))
            {
                int grey;
                Color color = new Color();
                int[,] hist = GetHist(bitmap);
                int[,] outhist = new int[256, 1];
                int locp = 0;
                int[] loc = new int[256];//极小值表
                loc[0] = 0;
                for (int i = 1; i < 255; i++)//寻找极小值
                {
                    if (hist[i - 1, 0] > hist[i, 0] && hist[i, 0] < hist[i + 1, 0])
                    {
                        locp++;
                        loc[locp] = i;
                    }
                }
                locp++;
                loc[locp] = 255;
                locp++;
                double[,] middlehist = new double[locp, 256];//中间表
                for (int z = 1; z < locp; z++)
                    for (int i = loc[z - 1]; i < loc[z]; i++)
                        middlehist[z, i] = (double)hist[i, 0];//划分直方图
                for (int z = 1; z < locp; z++)
                    for (int i = loc[z - 1] + 1; i < loc[z]; i++)
                        middlehist[z, i] = middlehist[z, i] + middlehist[z, i - 1];//累计
                for (int z = 1; z < locp; z++)
                    for (int i = loc[z - 1]; i < loc[z]; i++)
                    {
                        double p = (loc[z - 1] - loc[z]) / middlehist[z, loc[z] - 1];//归一化
                        middlehist[z, i] = Math.Abs((middlehist[z, i] * p) + loc[z - 1]);//建立映射表
                    }
                for (int z = 1; z < locp; z++)
                    for (int i = 0; i < 256; i++)
                        middlehist[z, i] = middlehist[z-1,i] + middlehist[z, i];//移动到最后一行
                for (int i = 0; i < 256; i++)
                    outhist[i, 0] = (int)middlehist[locp-1, i];//最终映射表
                outhist[255, 0] = 255;
                for (int j = 0; j < heigth; j++)
                    for (int i = 0; i < width; i++)
                    {
                        grey = bitmap.GetPixel(i, j).R;
                        color = Color.FromArgb(outhist[grey, 0], outhist[grey, 0], outhist[grey, 0]);
                        bitmap1.SetPixel(i, j, color);
                    }
            }
            else
            {
                int r, b, g;
                Color color = new Color();
                int[,] hist = GetHist(bitmap);
                int[,] outhist = new int[256, 3];
                for(int k=0;k<3;k++)
                {
                    int locp = 0;
                    int[] loc = new int[256];//极小值表
                    loc[0] = 0;
                    for (int i = 1; i < 255; i++)//寻找极小值
                    {
                        if (hist[i - 1, k] > hist[i, k] && hist[i, k] < hist[i + 1, k])
                        {
                            locp++;
                            loc[locp] = i;
                        }
                    }
                    locp++;
                    loc[locp] = 255;
                    locp++;
                    double[,] middlehist = new double[locp, 256];//中间表
                    for (int z = 1; z < locp; z++)
                        for (int i = loc[z - 1]; i < loc[z]; i++)
                            middlehist[z, i] = hist[i, k];//划分直方图
                    for (int z = 1; z < locp; z++)
                        for (int i = loc[z - 1] + 1; i < loc[z]; i++)
                            middlehist[z, i] = middlehist[z, i] + middlehist[z, i - 1];//累计
                    for (int z = 1; z < locp; z++)
                        for (int i = loc[z - 1]; i < loc[z]; i++)
                        {
                            double p = (loc[z - 1] - loc[z]) / middlehist[z, loc[z] - 1];//归一化
                            middlehist[z, i] = Math.Abs((middlehist[z, i] * p) + loc[z - 1]);//建立映射表
                        }
                    for (int z = 1; z < locp; z++)
                        for (int i = 0; i < 256; i++)
                            middlehist[z, i] = middlehist[z - 1, i] + middlehist[z, i];//移动到最后一行
                    for (int i = 0; i < 256; i++)
                        outhist[i, k] = (int)middlehist[locp-1, i];//最终映射表
                    outhist[255, k] = 255;
                }
                for (int j = 0; j < heigth; j++)
                    for (int i = 0; i < width; i++)
                    {
                        r = bitmap.GetPixel(i, j).R;
                        g = bitmap.GetPixel(i, j).G;
                        b = bitmap.GetPixel(i, j).B;
                        color = Color.FromArgb(outhist[r, 0], outhist[g, 1], outhist[b, 2]);
                        bitmap1.SetPixel(i, j, color);
                    }
            }
            return bitmap1;
        }
        public Bitmap AHE(Bitmap bitmap,int windows)
        {
            int width = bitmap.Width;
            int heigth = bitmap.Height;
            PixelFormat pixelFormat = bitmap.PixelFormat;
            int r = (int)(Math.Floor((double)(windows - 1) / 2));
            Bitmap middlebitmap = new Bitmap(width + 2 * r + 1, heigth + 2 * r + 1, pixelFormat);//制作原图像的扩展
            for(int j=r;j>=0;j--)
                for (int i = r; i < width+r; i++)
                {
                    Color color = bitmap.GetPixel(i-r, j);
                    middlebitmap.SetPixel(i, j, color);
                }
            for(int j=heigth+2*r-1;j>heigth+r;j--)
                for(int i=r;i<width+r;i++)
                {
                    Color color = bitmap.GetPixel(i-r,2*heigth-j+r);
                    middlebitmap.SetPixel(i, j, color);
                }
            for(int j=0;j<heigth;j++)
                for(int i=0;i<width;i++)
                {
                    Color color = bitmap.GetPixel(i, j);
                    middlebitmap.SetPixel(i + r, j + r, color);
                }
            for(int j=0;j<heigth+2*r;j++)
                for(int i=0;i<r;i++)
                {
                    Color color = middlebitmap.GetPixel(2*r - i, j);
                    middlebitmap.SetPixel(i, j, color);
                }
            for(int j=0;j<heigth+2*r;j++)
                for(int i=0;i<r;i++)
                {
                    Color color = middlebitmap.GetPixel(width + r - i, j);
                    middlebitmap.SetPixel(width + r + i, j, color);
                }
            Bitmap bitmap1 = new Bitmap(width, heigth, pixelFormat);
            for(int j=0;j<heigth;j++)
                for(int i=0;i<width;i++)
                {
                    Rectangle rectangle = new Rectangle(i, j, windows, windows);
                    Bitmap windowsBitmap = middlebitmap.Clone(rectangle, pixelFormat);
                    windowsBitmap = HE(windowsBitmap);
                    Color color = windowsBitmap.GetPixel(r, r);
                    windowsBitmap.Dispose();
                    bitmap1.SetPixel(i, j, color);
                }
            middlebitmap.Dispose();
            return bitmap1;
        }
        public Bitmap CLHE(Bitmap bitmap,double level)
        {
            int width = bitmap.Width;
            int heigth = bitmap.Height;
            Rectangle rectangle = new Rectangle(0, 0, width, heigth);
            PixelFormat pixelFormat = bitmap.PixelFormat;
            Bitmap bitmap1 = bitmap.Clone(rectangle, pixelFormat);
            if(IsGRAY(bitmap))
            {
                int grey;
                Color color = new Color();
                level = Math.Ceiling(level * width * heigth);
                int[,] hist = GetHist(bitmap);
                double[,] middlehist = new double[256, 1];//中间处理
                double p = (double)255 / (width * heigth);
                int[,] outhist = hist;
                for (int i = 0; i < 256; i++)
                    middlehist[i, 0] = hist[i, 0] ;
                int totalExcess = 0;
                for (int i=0;i<256;i++)
                {
                    double excess = Math.Max(hist[i, 0] - level, 0);
                    totalExcess = totalExcess + (int)excess;
                }
                int averageIncrease = (int)Math.Floor((double)totalExcess / 256);
                int upperLimit = (int)level - averageIncrease;
                for(int i=0;i<256;i++)
                {
                    if(hist[i,0]>level)
                    {
                        middlehist[i, 0] = level;
                    }
                    else
                    {
                        if (hist[i, 0] > upperLimit)
                        {
                            middlehist[i, 0] = level;
                            totalExcess = totalExcess - (hist[i, 0] - upperLimit);
                        }
                        else
                        {
                            middlehist[i, 0] = hist[i, 0] + averageIncrease;
                            totalExcess = totalExcess - averageIncrease;
                        }
                    }
                }
                while(totalExcess!=0)
                {
                    Random random = new Random();
                    double rand = random.NextDouble();
                    int startIndex = (int)(Math.Floor(255 * rand));
                    for(int i=startIndex;i<256;i++)
                    {
                        if (middlehist[i, 0] < level)
                            middlehist[i, 0] = middlehist[i, 0] + 1;
                        totalExcess--;
                        if (totalExcess == 0)
                            break;
                    }
                }
                for(int i=1;i<256;i++)
                {
                    middlehist[i, 0] = middlehist[i - 1, 0] + middlehist[i, 0];
                }
                for(int i=0;i<256;i++)
                {
                    middlehist[i, 0] = middlehist[i,0] * p;
                    outhist[i, 0] = (int)middlehist[i, 0];
                }
                for (int j = 0; j < heigth; j++)
                    for (int i = 0; i < width; i++)
                    {
                        grey = bitmap.GetPixel(i, j).R;
                        color = Color.FromArgb(outhist[grey, 0], outhist[grey, 0], outhist[grey, 0]);
                        bitmap1.SetPixel(i, j, color);
                    }
            }
            else
            {
                int r, g, b;
                Color color = new Color();
                level = Math.Ceiling(level * width * heigth);
                int[,] hist = GetHist(bitmap);
                int[,] outhist = hist;
                double[,] middlehist = new double[256, 3];
                double p = (double)255 / (width * heigth);
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < 256; i++)
                        middlehist[i, k] = hist[i, k];
                    int totalExcess = 0;
                    for (int i = 0; i < 256; i++)
                    {
                        double excess = Math.Max(hist[i, k] - level, 0);
                        totalExcess = totalExcess + (int)excess;
                    }
                    int averageIncrease = (int)Math.Floor((double)totalExcess / 256);
                    int upperLimit = (int)level - averageIncrease;
                    for (int i = 0; i < 256; i++)
                    {
                        if (hist[i, k] > level)
                        {
                            middlehist[i, k] = level;
                        }
                        else
                        {
                            if (hist[i, k] > upperLimit)
                            {
                                middlehist[i, k] = level;
                                totalExcess = totalExcess - (hist[i, k] - upperLimit);
                            }
                            else
                            {
                                middlehist[i, k] = hist[i, k] + averageIncrease;
                                totalExcess = totalExcess - averageIncrease;
                            }
                        }
                    }
                    while (totalExcess != 0)
                    {
                        Random random = new Random();
                        double rand = random.NextDouble();
                        int startIndex = (int)(Math.Floor(255 * rand));
                        for (int i = startIndex; i < 256; i++)
                        {
                            if (middlehist[i, k] < level)
                                middlehist[i, k] = middlehist[i, k] + 1;
                            totalExcess--;
                            if (totalExcess == 0)
                                break;
                        }
                    }
                    for (int i = 1; i < 256; i++)
                    {
                        middlehist[i, k] = middlehist[i - 1, k] + middlehist[i, k];
                    }
                    for (int i = 0; i < 256; i++)
                    {
                        middlehist[i, k] = middlehist[i, k] * p;
                        outhist[i, k] = (int)middlehist[i, k];
                    }
                }
                    for (int j = 0; j < heigth; j++)
                        for (int i = 0; i < width; i++)
                        {
                            r = bitmap.GetPixel(i, j).R;
                            g = bitmap.GetPixel(i, j).G;
                            b = bitmap.GetPixel(i, j).B;
                            color = Color.FromArgb(outhist[r, 0], outhist[g, 1], outhist[b, 2]);
                            bitmap1.SetPixel(i, j, color);
                        }
                }
            return bitmap1;
        }
        public Bitmap CLAHE(Bitmap bitmap,int windows,Double level)
        {
            int width = bitmap.Width;
            int heigth = bitmap.Height;
            PixelFormat pixelFormat = bitmap.PixelFormat;
            int r = (int)(Math.Floor((double)(windows - 1) / 2));
            Bitmap middlebitmap = new Bitmap(width + 2 * r + 1, heigth + 2 * r + 1, pixelFormat);//制作原图像的扩展
            for (int j = r; j >= 0; j--)
                for (int i = r; i < width + r; i++)
                {
                    Color color = bitmap.GetPixel(i - r, j);
                    middlebitmap.SetPixel(i, j, color);
                }
            for (int j = heigth + 2 * r - 1; j > heigth + r; j--)
                for (int i = r; i < width + r; i++)
                {
                    Color color = bitmap.GetPixel(i - r, 2 * heigth - j + r);
                    middlebitmap.SetPixel(i, j, color);
                }
            for (int j = 0; j < heigth; j++)
                for (int i = 0; i < width; i++)
                {
                    Color color = bitmap.GetPixel(i, j);
                    middlebitmap.SetPixel(i + r, j + r, color);
                }
            for (int j = 0; j < heigth + 2 * r; j++)
                for (int i = 0; i < r; i++)
                {
                    Color color = middlebitmap.GetPixel(2 * r - i, j);
                    middlebitmap.SetPixel(i, j, color);
                }
            for (int j = 0; j < heigth + 2 * r; j++)
                for (int i = 0; i < r; i++)
                {
                    Color color = middlebitmap.GetPixel(width + r - i, j);
                    middlebitmap.SetPixel(width + r + i, j, color);
                }
            Bitmap bitmap1 = new Bitmap(width, heigth, pixelFormat);
            for (int j = 0; j < heigth; j++)
                for (int i = 0; i < width; i++)
                {
                    Rectangle rectangle = new Rectangle(i, j, windows, windows);
                    Bitmap windowsBitmap = middlebitmap.Clone(rectangle, pixelFormat);
                    windowsBitmap = CLHE(windowsBitmap,level);
                    Color color = windowsBitmap.GetPixel(r, r);
                    windowsBitmap.Dispose();
                    bitmap1.SetPixel(i, j, color);
                }
            middlebitmap.Dispose();
            return bitmap1;
        }
    }
}
