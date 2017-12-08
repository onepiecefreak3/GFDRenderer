using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace GFDFontRenderer.GFD
{
    public class GFDTextRenderer
    {
        string fontTextureBase;
        string fontTextureExt;

        GFDv1 gfdv1 = null;
        GFDv2 gfdv2 = null;

        List<Bitmap> fontTextures = new List<Bitmap>();

        float FontSize;
        float origFontSize;
        int lineHeight;

        /// <summary>
        /// Initializes a new GFDTextRenderer instance
        /// </summary>
        /// <param name="GFDPath">Filepath to the GFD</param>
        /// <param name="fontTextureBase">Path to the Directory containing font textures, with the texture name base</param>
        /// <param name="fontTextureExt">Give an extended font texture name, written after the texture ID</param>
        /// <param name="FontSize">Defines the size of the letters</param>
        /// <param name="lineHeight">Height of a line of text in pixels</param>
        public GFDTextRenderer(string GFDPath, string fontTextureBase, string fontTextureExt, float FontSize, int lineHeight)
        {
            var res = Support.Identify(GFDPath);

            if (res == Support.Ident.NotFound)
                throw new Exception("Provided GFD was not found!");

            if (res == Support.Ident.NotSupported)
                throw new Exception("GFD Version is not supported!");

            if (res == Support.Ident.Version1)
            {
                gfdv1 = new GFDv1(GFDPath);
                origFontSize = gfdv1.Header.FontSize;

                for (int i = 0; i < gfdv1.Header.FontTexCount; i++)
                    if (!File.Exists(fontTextureBase + $"{i:00}" + fontTextureExt + ".png"))
                        throw new Exception(fontTextureBase + $"{i:00}" + fontTextureExt + ".png was not found!");

                for (int i = 0; i < gfdv1.Header.FontTexCount; i++)
                    fontTextures.Add((Bitmap)Image.FromFile(fontTextureBase + $"{i:00}" + fontTextureExt + ".png"));
            }
            else if (res == Support.Ident.Version2)
            {
                gfdv2 = new GFDv2(GFDPath);
                origFontSize = gfdv2.Header.FontSize;

                for (int i = 0; i < gfdv2.Header.FontTexCount; i++)
                    if (!File.Exists(fontTextureBase + $"{i:00}" + fontTextureExt + ".png"))
                        throw new Exception(fontTextureBase + $"{i:00}" + fontTextureExt + ".png was not found!");

                for (int i = 0; i < gfdv2.Header.FontTexCount; i++)
                    fontTextures.Add((Bitmap)Image.FromFile(fontTextureBase + $"{i:00}" + fontTextureExt + ".png"));
            }

            this.FontSize = FontSize;
            this.fontTextureBase = fontTextureBase;
            this.fontTextureExt = fontTextureExt;
            this.lineHeight = lineHeight;
        }

        /// <summary>
        /// Measures a given text by font
        /// </summary>
        /// <param name="text">The text, which gets measured</param>
        /// <returns></returns>
        public Size MeasureText(string text)
        {
            var lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            if (gfdv1 != null)
            {
                return lines.Aggregate(new Size(0, 0), (output, l) =>
                {
                    int tmp = l.Aggregate(0, (wOut, c) => wOut + (int)(((gfdv1.Characters.Find(ch => ch.Character == c) != null ? gfdv1.Characters.FirstOrDefault(ch => ch.Character == c).CharWidth : 16) + 1) / origFontSize * FontSize));
                    output.Width = (output.Width < tmp) ? tmp : output.Width;
                    output.Height += (int)(lineHeight / origFontSize * FontSize);
                    return output;
                });
            }
            else if (gfdv2 != null)
            {
                return lines.Aggregate(new Size(0, 0), (output, l) =>
                {
                    int tmp = l.Aggregate(0, (wOut, c) => wOut + (int)(((gfdv2.Characters.Find(ch => ch.Character == c) != null ? gfdv2.Characters.FirstOrDefault(ch => ch.Character == c).CharWidth : 16) + 1) / origFontSize * FontSize));
                    output.Width = (output.Width < tmp) ? tmp : output.Width;
                    output.Height += (int)(lineHeight / origFontSize * FontSize);
                    return output;
                });
            }

            return new Size(0, 0);
        }

        /// <summary>
        /// Draws text on a given GDI-Object
        /// </summary>
        /// <param name="gdi">The GDI-Object, on which the text gets drawn</param>
        /// <param name="text">The text, which gets drawn</param>
        /// <param name="point">The Coordinate, at which the drawing will begin</param>
        /// <param name="color">The color used to draw the text</param>
        public void DrawText(Graphics gdi, string text, Point point, Color color)
        {
            int x = point.X;
            int origX = point.X;
            int y = point.Y;

            var lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (var l in lines)
            {
                foreach (var c in l)
                {
                    if (gfdv1 != null)
                    {
                        var usedChar = gfdv1.Characters.Find(ch => ch.Character == c) != null ? gfdv1.Characters.FirstOrDefault(ch => ch.Character == c) : null;

                        if (usedChar != null && usedChar.GlyphWidth != 0 && usedChar.GlyphHeight != 0)
                        {
                            var letterGlyph = fontTextures[usedChar.TexID].Clone(new Rectangle(usedChar.GlyphPosX, usedChar.GlyphPosY, usedChar.GlyphWidth, usedChar.GlyphHeight), fontTextures[usedChar.TexID].PixelFormat);
                            if (FontSize != origFontSize) letterGlyph = ChangeSize(letterGlyph, FontSize);
                            if (color != Color.White) ChangeColor(letterGlyph, color);
                            gdi.DrawImage(letterGlyph, new Point(x, y));

                            x += (int)((usedChar.CharWidth + 1) / origFontSize * FontSize);
                        }
                        else
                        {
                            x += (int)((16 + 1) / origFontSize * FontSize);
                        }
                    }
                    else if (gfdv2 != null)
                    {
                        var usedChar = gfdv2.Characters.Find(ch => ch.Character == c) != null ? gfdv2.Characters.FirstOrDefault(ch => ch.Character == c) : null;

                        if (usedChar != null && usedChar.GlyphWidth != 0 && usedChar.GlyphHeight != 0)
                        {
                            var letterGlyph = fontTextures[usedChar.TexID].Clone(new Rectangle(usedChar.GlyphPosX, usedChar.GlyphPosY, usedChar.GlyphWidth, usedChar.GlyphHeight), fontTextures[usedChar.TexID].PixelFormat);
                            if (FontSize != origFontSize) letterGlyph = ChangeSize(letterGlyph, FontSize);
                            if (color != Color.White) ChangeColor(letterGlyph, color);
                            gdi.DrawImage(letterGlyph, new Point(x + usedChar.XCorrection, y + usedChar.YCorrection));

                            x += (int)((usedChar.CharWidth + 1) / origFontSize * FontSize);
                        }
                        else
                        {
                            x += (int)((16 + 1) / origFontSize * FontSize);
                        }
                    }
                }

                x = origX;
                y += lineHeight;
            }
        }

        private Bitmap ChangeSize(Bitmap img, double size)
        {
            var newWidth = (double)img.Width / origFontSize * size;
            var newHeight = (double)img.Height / origFontSize * size;

            newWidth = ((newWidth > 0 && newWidth < 1) || newWidth == 0) ? 1 : newWidth;
            newHeight = ((newHeight > 0 && newHeight < 1) || newHeight == 0) ? 1 : newHeight;

            return ResizeImage(img, (int)newWidth, (int)newHeight);
        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void ChangeColor(Bitmap img, Color color)
        {
            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                {
                    img.SetPixel(x, y, Color.FromArgb(img.GetPixel(x, y).A, color.R, color.G, color.B));
                }
        }
    }
}
