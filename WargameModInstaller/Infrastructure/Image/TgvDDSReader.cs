using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Utilities;
using WargameModInstaller.Utilities.Image.DDS;

namespace WargameModInstaller.Infrastructure.Image
{
    public class TgvDDSReader : ITgvReader
    {
        public TgvDDSReader(String ddsFilePath)
        {
            this.DDSFilePath = ddsFilePath;
        }

        protected String DDSFilePath
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public virtual TgvImage Read()
        {
            byte[] rawDDSData = File.ReadAllBytes(DDSFilePath);

            int contentSize = rawDDSData.Length - Marshal.SizeOf(typeof(DDSFormat.Header)) - Marshal.SizeOf((typeof(uint)));

            var file = new TgvImage();

            using (var ms = new MemoryStream(rawDDSData))
            {
                var buffer = new byte[4];
                ms.Read(buffer, 0, buffer.Length);

                if (BitConverter.ToUInt32(buffer, 0) != DDSFormat.MagicHeader)
                {
                    throw new ArgumentException("Wrong DDS magic");
                }

                buffer = new byte[Marshal.SizeOf(typeof(DDSFormat.Header))];
                ms.Read(buffer, 0, buffer.Length);

                var header = MiscUtilities.ByteArrayToStructure<DDSFormat.Header>(buffer);
                int mipSize = contentSize;
                if (header.MipMapCount == 0)
                {
                    header.MipMapCount = 1;
                }
                else
                {
                    mipSize -= contentSize / header.MipMapCount;
                }

                for (ushort i = 0; i < header.MipMapCount; i++)
                {
                    buffer = new byte[mipSize];
                    ms.Read(buffer, 0, buffer.Length);

                    var mip = new TgvMipMap { Content = buffer };
                    file.MipMaps.Add(mip);

                    mipSize /= 4;
                }

                file.Height = header.Height;
                file.ImageHeight = header.Height;
                file.Width = header.Width;
                file.ImageHeight = header.Width;
                file.MipMapCount = (ushort)header.MipMapCount;

                DDSHelper.ConversionFlags conversionFlags;
                file.Format = DDSHelper.GetDXGIFormat(ref header.PixelFormat, out conversionFlags); //PixelFormats.BC3_UNORM_SRGB;
            }

            return file;

        }

    }
}
