using System;
using System.Collections.Generic;
using System.Text;

namespace SetSoftware_Enterprise.UTILS.TICKETS
{
    public static class ImageTicketUtils
    {
        private static readonly byte[] ALIGN_LEFT = { 0x1B, 0x61, 0x00 };
        private static readonly byte[] ALIGN_CENTER = { 0x1B, 0x61, 0x01 };
        /// Converts an image to ESC/POS raster bit image format        
        public static byte[] PrintImage(string imagePath)
        {
            try
            {
                using (Bitmap bitmap = new Bitmap(imagePath))
                {
                    // Resize image to fit 80mm printer (max width ~530 pixels)
                    int maxWidth = 510;
                    int newWidth = Math.Min(bitmap.Width, maxWidth);
                    int newHeight = (int)(bitmap.Height * ((float)newWidth / bitmap.Width));

                    using (Bitmap resized = new Bitmap(newWidth, newHeight))
                    using (Graphics g = Graphics.FromImage(resized))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(bitmap, 0, 0, newWidth, newHeight);

                        // Convert to monochrome
                        return ConvertImageToESCPOS(resized);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error printing image: {ex.Message}");
                return new byte[0];
            }
        }

        /// Converts bitmap to ESC/POS bit image command        
        private static byte[] ConvertImageToESCPOS(Bitmap bitmap)
        {
            var commands = new List<byte>();

            // Center alignment for image
            commands.AddRange(ALIGN_CENTER);

            int width = bitmap.Width;
            int height = bitmap.Height;

            // ESC * m nL nH d1...dk - Bit image command
            for (int y = 0; y < height; y += 24)
            {
                commands.Add(0x1B); // ESC
                commands.Add(0x2A); // *
                commands.Add(33);   // 24-dot double-density
                commands.Add((byte)(width % 256)); // nL
                commands.Add((byte)(width / 256)); // nH

                for (int x = 0; x < width; x++)
                {
                    for (int k = 0; k < 3; k++) // 24 dots = 3 bytes
                    {
                        byte slice = 0;
                        for (int b = 0; b < 8; b++)
                        {
                            int yPos = y + (k * 8) + b;
                            if (yPos < height)
                            {
                                Color pixel = bitmap.GetPixel(x, yPos);
                                int brightness = (pixel.R + pixel.G + pixel.B) / 3;
                                if (brightness < 128) // Threshold for black/white
                                {
                                    slice |= (byte)(1 << (7 - b));
                                }
                            }
                        }
                        commands.Add(slice);
                    }
                }
                commands.Add(0x0A); // Line feed
            }

            commands.AddRange(ALIGN_LEFT);
            return commands.ToArray();
        }
    }
}
