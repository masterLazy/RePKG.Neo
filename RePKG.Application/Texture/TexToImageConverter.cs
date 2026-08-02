using RePKG.Application.Texture.Helpers;
using RePKG.Core.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RePKG.Application.Texture {
    public class TexToImageConverter {
        public ImageResult[] ConvertToImage(ITex tex, MipmapFormat format, IExtractProgress ep) {
            ArgumentNullException.ThrowIfNull(tex);

            if (tex.IsMultiple)
                return ConvertMultiple(tex, ep);

            var sourceMipmap = tex.FirstImage.FirstMipmap;

            if (tex.IsVideoTexture) {
                if (sourceMipmap.Bytes.Length < 12) {
                    throw new InvalidOperationException("Expected mp4 magic header");
                }

                var mp4magic = Encoding.ASCII.GetString(sourceMipmap.Bytes, 4, 8);

                if (!mp4magic.Equals("ftypisom", StringComparison.OrdinalIgnoreCase)
                    && !mp4magic.Equals("ftypmsnv", StringComparison.OrdinalIgnoreCase)
                    && !mp4magic.Equals("ftypmp42", StringComparison.OrdinalIgnoreCase)) {
                    throw new InvalidOperationException("Expected mp4 magic header");
                }

                ep.Forward();
                return [new ImageResult {
                    Bytes = sourceMipmap.Bytes,
                    Format = MipmapFormat.VideoMp4
                }];
            }

            if (sourceMipmap.Format.IsCompressed())
                throw new InvalidOperationException("Raw mipmap format must be uncompressed");

            // Decide whether to encode to PNG based on the actual mipmap pixel format,
            // not the caller-supplied output format (which is ImagePNG for raw formats).
            if (sourceMipmap.Format.IsRawFormat()) {
                if (ep.IsDryRunning) {
                    ep.Forward(1);
                    return null;
                }

                var image = ImageFromRawFormat(sourceMipmap.Format, sourceMipmap.Bytes, sourceMipmap.Width, sourceMipmap.Height);

                if (sourceMipmap.Width > tex.Header.ImageWidth ||
                    sourceMipmap.Height > tex.Header.ImageHeight)
                    image.Mutate(x => x.Crop(tex.Header.ImageWidth, tex.Header.ImageHeight));
                else if (sourceMipmap.Width < tex.Header.ImageWidth ||
                    sourceMipmap.Height < tex.Header.ImageHeight)
                    image.Mutate(x => x.Resize(tex.Header.ImageWidth, tex.Header.ImageHeight));

                using var memoryStream = new MemoryStream();
                if (!ep.IsDryRunning) image.SaveAsPng(memoryStream);

                ep.Forward();
                return [new ImageResult {
                        Bytes = memoryStream.ToArray(),
                        Format = MipmapFormat.ImagePNG
                    }];
            }

            ep.Forward();
            return [new ImageResult {
                Bytes = sourceMipmap.Bytes,
                Format = format
            }];
        }

        public static MipmapFormat GetConvertedFormat(ITex tex) {
            ArgumentNullException.ThrowIfNull(tex);

            if (tex.IsVideoTexture) {
                return MipmapFormat.VideoMp4;
            }

            var format = tex.FirstImage.FirstMipmap.Format;

            if (format.IsCompressed())
                throw new InvalidOperationException("Raw mipmap format must be uncompressed");

            return format.IsRawFormat() ? MipmapFormat.ImagePNG : format;
        }

        private static ImageResult[] ConvertMultiple(ITex tex, IExtractProgress ep) {
            var frameFormat = tex.FirstImage.FirstMipmap.Format;
            if (!frameFormat.IsRawFormat())
                throw new InvalidOperationException(
                    "Only raw mipmap formats are supported right now while converting multi-frame image");

            if (ep.IsDryRunning) {
                // GIF is saved as a single file, other multi-frame formats produce one PNG per frame
                var count = frameFormat == MipmapFormat.ImageGIF ? 1 : tex.FrameInfoContainer.Frames.Count;
                ep.Forward(count);
                return null;
            }

            // Extract all frames
            var sequenceImages = new Image[tex.ImagesContainer.Images.Count];
            var frames = new List<Image>(tex.FrameInfoContainer.Frames.Count);

            for (var i = 0; i < sequenceImages.Length; i++) {
                var mipmap = tex.ImagesContainer.Images[i].FirstMipmap;
                sequenceImages[i] = ImageFromRawFormat(frameFormat, mipmap.Bytes, mipmap.Width, mipmap.Height);
            }

            foreach (var frameInfo in tex.FrameInfoContainer.Frames) {
                // Frames can be turned to fit into the map so we need to compute cropping coordinates first
                // We're keeping width and height signed for the rotation angle calculation
                var width = frameInfo.Width != 0 ? frameInfo.Width : frameInfo.HeightX;
                var height = frameInfo.Height != 0 ? frameInfo.Height : frameInfo.WidthY;
                var x = Math.Min(frameInfo.X, frameInfo.X + width);
                var y = Math.Min(frameInfo.Y, frameInfo.Y + height);

                // This formula gives us the angle for which we need to turn the frame,
                // assuming that either Width or HeightX is 0 (same with Height and WidthY)
                var rotationAngle = -(Math.Atan2(Math.Sign(height), Math.Sign(width)) - Math.PI / 4);

                var frame = sequenceImages[frameInfo.ImageId].Clone(
                    context => context.Crop(new Rectangle(
                        (int)x,
                        (int)y,
                        (int)Math.Abs(width),
                        (int)Math.Abs(height))
                    ).Rotate((float)Math.Round(rotationAngle * 180 / Math.PI)));

                var metadata = frame.Frames.RootFrame.Metadata.GetFormatMetadata(GifFormat.Instance);
                metadata.FrameDelay = (int)Math.Round(frameInfo.Frametime * 100.0f);

                frames.Add(frame);
            }

            // Check format
            if (frameFormat == MipmapFormat.ImageGIF) {
                return ConvertGif(tex, frames, frameFormat, ep);
            }

            var result = new ImageResult[frames.Count];
            for (var i = 0; i < frames.Count; i++) {
                using var memoryStream = new MemoryStream();
                frames[i].SaveAsPng(memoryStream);
                result[i] = new ImageResult() {
                    Bytes = memoryStream.ToArray(),
                    Format = MipmapFormat.ImagePNG
                };
                ep.Forward();
            }

            return result;
        }

        private static ImageResult[] ConvertGif(ITex tex, List<Image> frames, MipmapFormat format, IExtractProgress ep) {
            // Remove first black frame
            frames.RemoveAt(0);

            var image = ImageFromRawFormat(format, null,
                tex.FrameInfoContainer.Width,
                tex.FrameInfoContainer.Height);

            foreach (var frame in frames) {
                image.Frames.AddFrame(frame.Frames[0]);
            }

            using var memoryStream = new MemoryStream();
            if (!ep.IsDryRunning) image.SaveAsGif(memoryStream, new GifEncoder { ColorTableMode = GifColorTableMode.Local });
            ep.Forward();
            return [new ImageResult {
                    Bytes = memoryStream.ToArray(),
                    Format = MipmapFormat.ImageGIF
                } ];
        }

        private static Image ImageFromRawFormat(MipmapFormat format, byte[] bytes, int width, int height) {
            switch (format) {
                case MipmapFormat.R8:
                    return bytes == null
                        ? new Image<L8>(width, height)
                        : Image.LoadPixelData<L8>(bytes, width, height);

                case MipmapFormat.RG88:
                    return bytes == null
                        ? new Image<RG88>(width, height)
                        : Image.LoadPixelData<RG88>(bytes, width, height);

                case MipmapFormat.RGBA8888:
                    return bytes == null
                        ? new Image<Rgba32>(width, height)
                        : Image.LoadPixelData<Rgba32>(bytes, width, height);

                default:
                    throw new InvalidOperationException($"Mipmap format: {format} is not supported");
            }
        }
    }

    public class ImageResult {
        public byte[] Bytes { get; set; }
        public MipmapFormat Format { get; set; }
    }

    public interface IExtractProgress {
        public void Forward(int count = 1);
        public bool IsDryRunning { get; }
    }
}