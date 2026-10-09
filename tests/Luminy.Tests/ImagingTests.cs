namespace Luminy.Tests
{
    using System;
    using System.IO;
    using Luminy.Imaging;
    using Luminy.Model;

    public class ImagingTests
    {
        [Test]
        public void ColorScale_InterpolatesCorrectly_WithDichotomy()
        {
            var scale = ColorScale.BlueWhite;

            var atStart = scale[0f];
            var atEnd = scale[1f];
            var atMid = scale[0.5f];

            Assert.That(atStart.Blue, Is.GreaterThan(0.4f));
            Assert.That(atEnd.Red, Is.EqualTo(1f));
            Assert.That(atMid.Red, Is.GreaterThan(atStart.Red));
            Assert.That(atMid.Red, Is.LessThan(atEnd.Red));
        }

        [Test]
        public void ColorScale_Presets_AreValid()
        {
            Assert.That(ColorScale.Inferno.Steps.Length, Is.EqualTo(5));
            Assert.That(ColorScale.OrangeWhiteBlue.Steps.Length, Is.EqualTo(3));
            Assert.That(ColorScale.BlueWhite.Steps.Length, Is.EqualTo(2));

            var midDiverging = ColorScale.OrangeWhiteBlue[0.5f];
            Assert.That(midDiverging.Red, Is.EqualTo(1f));
            Assert.That(midDiverging.Green, Is.EqualTo(1f));
            Assert.That(midDiverging.Blue, Is.EqualTo(1f));
        }

        [Test]
        public void BitmapBuffer_AllocatesAndAccessesPixels()
        {
            const int width = 100;
            const int height = 50;

            using var buffer = new BitmapBuffer<byte>(width, height);

            Assert.That(buffer.Width, Is.EqualTo((nint)width));
            Assert.That(buffer.Height, Is.EqualTo((nint)height));
            Assert.That((long)(buffer.Stride % 64), Is.EqualTo(0L));

            buffer[10, 20] = 42;
            buffer[99, 49] = 255;

            Assert.That(buffer[10, 20], Is.EqualTo(42));
            Assert.That(buffer[99, 49], Is.EqualTo(255));
            Assert.That(buffer[0, 0], Is.EqualTo(0));
        }

        [Test]
        public void PngColor_Creates256EntryLut()
        {
            var lut = PngColor.CreateLut256(ColorScale.Inferno);

            Assert.That(lut.Length, Is.EqualTo(256));
            Assert.That(lut[0].Red, Is.EqualTo(0));
            Assert.That(lut[255].Red, Is.GreaterThan(200));
        }

        [Test]
        public void IndexedPngEncoder_EncodesValidPngStream_AllFilterModes()
        {
            var modes = new[] { PngFilterMode.None, PngFilterMode.Sub, PngFilterMode.Up, PngFilterMode.Adaptive };

            foreach (var mode in modes)
            {
                using var buffer = new BitmapBuffer<byte>(32, 32);
                for (var y = 0; y < 32; y++)
                {
                    for (var x = 0; x < 32; x++)
                    {
                        buffer[x, y] = (byte)((x + y) * 4);
                    }
                }

                var lut = PngColor.CreateLut256(ColorScale.Inferno);
                using var stream = new MemoryStream();

                IndexedPngEncoder.Encode(buffer, lut, stream, new PngEncodingOptions { FilterMode = mode });

                var bytes = stream.ToArray();

                // Check PNG signature: 89 50 4E 47 0D 0A 1A 0A
                byte[] expectedSig = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
                Assert.That(bytes[..8], Is.EqualTo(expectedSig));

                // Check IHDR type at offset 12..16
                Assert.That(bytes[12..16], Is.EqualTo("IHDR"u8.ToArray()));

                // Verify file ends with IEND CRC
                Assert.That(bytes[^8..^4], Is.EqualTo("IEND"u8.ToArray()));
            }
        }

        [Test]
        public void IndexedPngEncoder_EncodesHeatmap_ToStreamAndFile()
        {
            const int rows = 64;
            const int cols = 128;
            var values = new float[rows, cols];

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    values[r, c] = MathF.Sin(r * 0.1f) * MathF.Cos(c * 0.05f);
                }
            }

            var heatmap = new Heatmap(values, ColorScale.Inferno);
            using var stream = new MemoryStream();

            IndexedPngEncoder.Encode(heatmap, stream);

            Assert.That(stream.Length, Is.GreaterThan(100));

            var tempPath = Path.Combine(Path.GetTempPath(), "test_tensor_heatmap.png");
            try
            {
                IndexedPngEncoder.Save(tempPath, heatmap);
                Assert.That(File.Exists(tempPath), Is.True);
                Assert.That(new FileInfo(tempPath).Length, Is.GreaterThan(100));
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [Test]
        public void GenerateSampleHeatmapArtifact()
        {
            const int size = 256;
            var values = new float[size, size];

            for (var r = 0; r < size; r++)
            {
                for (var c = 0; c < size; c++)
                {
                    var x = (c - size / 2f) / 32f;
                    var y = (r - size / 2f) / 32f;
                    var r2 = x * x + y * y;
                    values[r, c] = MathF.Exp(-r2 * 0.1f) * MathF.Cos(MathF.Sqrt(r2) * 3f);
                }
            }

            var heatmap = new Heatmap(values, ColorScale.Inferno);
            IndexedPngEncoder.Save("sample_tensor_heatmap.png", heatmap);
            Assert.That(File.Exists("sample_tensor_heatmap.png"), Is.True);
        }
    }
}
