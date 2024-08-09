using System;
using System.Linq;
using System.Threading.Tasks;

namespace LunarLabs.Fonts
{
    public static class DistanceFieldUtils
    {
        /*
        Computes a distance field transform of a high resolution RGBA source channel and returns the result as a low resolution RGBA channel.

        scale_down : The amount the source channel will be scaled down.
        A value of 8 means the destination image will be 1/8th the size of the source

        spread: The spread in pixels before the distance field clamps to (zero/one). 
        The value is specified in units of the destination image. The spread in the source image will be spread*scale_down.
        */
        public static GlyphBitmap CreateDistanceField(GlyphBitmap source, int scale, float spread)
        {
            // Assuming GlyphBitmap now handles RGBA pixels, where each pixel has 4 bytes
            var result = new GlyphBitmap(source.Width / scale, source.Height / scale);

            // Process each channel separately (R, G, B, A)
            float[] valuesR = new float[source.Width * source.Height];
            float[] valuesG = new float[source.Width * source.Height];
            float[] valuesB = new float[source.Width * source.Height];
            float[] valuesA = new float[source.Width * source.Height];

            Parallel.For(0, source.Height, y =>
            {
                for (int x = 0; x < source.Width; x++)
                {
                    int i = (x + y * source.Width) * 4;
                    valuesR[x + y * source.Width] = (source.Pixels[i] / 255.0f) - 0.5f;
                    valuesG[x + y * source.Width] = (source.Pixels[i + 1] / 255.0f) - 0.5f;
                    valuesB[x + y * source.Width] = (source.Pixels[i + 2] / 255.0f) - 0.5f;
                    valuesA[x + y * source.Width] = (source.Pixels[i + 3] / 255.0f) - 0.5f;
                }
            });

            Parallel.For(0, result.Height, y =>
            {
                for (int x = 0; x < result.Width; x++)
                {
                    var sdR = OptimizedSignedDistance(valuesR, source.Width, source.Height, x * scale, y * scale, spread);
                    var sdG = OptimizedSignedDistance(valuesG, source.Width, source.Height, x * scale, y * scale, spread);
                    var sdB = OptimizedSignedDistance(valuesB, source.Width, source.Height, x * scale, y * scale, spread);
                    var sdA = OptimizedSignedDistance(valuesA, source.Width, source.Height, x * scale, y * scale, spread);

                    var nR = (sdR + spread) / (spread * 2.0f);
                    var nG = (sdG + spread) / (spread * 2.0f);
                    var nB = (sdB + spread) / (spread * 2.0f);
                    var nA = (sdA + spread) / (spread * 2.0f);

                    var cR = (byte)(nR * 255);
                    var cG = (byte)(nG * 255);
                    var cB = (byte)(nB * 255);
                    var cA = (byte)(nA * 255);

                    var offset = (x + y * result.Width) * 4;
                    result.Pixels[offset] = cR;
                    result.Pixels[offset + 1] = cG;
                    result.Pixels[offset + 2] = cB;
                    result.Pixels[offset + 3] = cA;
                }
            });

            return result;
        }

        private static float OptimizedSignedDistance(float[] source, int w, int h, int cx, int cy, float clamp)
        {
            var cd = source[cx + cy * w];

            int min_x = Math.Max(0, cx - (int)clamp);
            int max_x = Math.Min(w - 1, cx + (int)clamp);
            int min_y = Math.Max(0, cy - (int)clamp);
            int max_y = Math.Min(h - 1, cy + (int)clamp);

            float distanceSquared = clamp * clamp;

            for (int y = min_y; y <= max_y; y++)
            {
                for (int x = min_x; x <= max_x; x++)
                {
                    float d = source[x + y * w];
                    if (cd * d < 0)
                    {
                        float dx = x - cx;
                        float dy = y - cy;
                        float distSq = dx * dx + dy * dy;
                        if (distSq < distanceSquared)
                        {
                            distanceSquared = distSq;
                        }
                    }
                }
            }

            float distance = (float)Math.Sqrt(distanceSquared);

            return cd > 0 ? distance : -distance;
        }
    }
}
