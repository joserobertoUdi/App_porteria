using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class Program
{
    static void Main()
    {
        var basePath = @"C:\Users\becadesarrollo\Downloads\porteria_27082026_Alberto\porteria\app_universidad\assets\branding";
        var tmpDir = @"C:\Users\becadesarrollo\Downloads\porteria_27082026_Alberto\porteria\tools\crop_logo\tmp";
        Directory.CreateDirectory(tmpDir);

        ReEncode(Path.Combine(basePath, "logo-udi-blanco.png"), Path.Combine(tmpDir, "blanco.png"));
        ReEncode(Path.Combine(basePath, "logo-udi-color.png"), Path.Combine(tmpDir, "color.png"));
        ReEncode(Path.Combine(basePath, "icon-foreground-cropped.png"), Path.Combine(tmpDir, "fg.png"));

        File.Copy(Path.Combine(tmpDir, "blanco.png"), Path.Combine(basePath, "logo-udi-blanco.png"), true);
        File.Copy(Path.Combine(tmpDir, "color.png"), Path.Combine(basePath, "logo-udi-color.png"), true);
        File.Copy(Path.Combine(tmpDir, "fg.png"), Path.Combine(basePath, "icon-foreground-cropped.png"), true);

        Directory.Delete(tmpDir, true);
        Console.WriteLine("All images re-encoded and replaced.");
    }

    static void ReEncode(string src, string dst)
    {
        using var img = new Bitmap(src);
        Console.Write($"{Path.GetFileName(src)}: {img.Width}x{img.Height} -> ");
        using var standard = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(standard);
        g.DrawImage(img, 0, 0, img.Width, img.Height);
        standard.Save(dst, ImageFormat.Png);
        Console.WriteLine("OK");
    }
}
