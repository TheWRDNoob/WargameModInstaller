using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Infrastructure.Containers.Edata;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    //To do: do przeróbki odczytywanie słownika tak aby wykorzystywało nowe elementy modelu.

    public class EdataFileReader : EdataReaderBase, IEdataFileReader
    {
        private String lastEdataFilePath;

        public EdataFile Read(String edataFilePath, bool loadContent = false)
        {
            return ReadEdata(edataFilePath, loadContent);
        }

        public EdataFile Read(String edataFilePath, bool loadContent, CancellationToken token)
        {
            return ReadEdata(edataFilePath, loadContent, token);
        }

        /// <summary>
        /// Reads content described by the given content file, but doesn't assing it to that Content File.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] ReadContent(EdataContentFile file)
        {
            if (file.Owner == null)
            {
                throw new ArgumentException(String.Format("'{0}' is not assigned to any Edata File Owner.", file), "file");
            }
            var contentOwenr = file.Owner;
            if (!File.Exists(contentOwenr.Path))
            {
                throw new IOException(String.Format("Edata content owner file '{0}' doesn't exist.", contentOwenr.Path));
            }

            using (FileStream stream = File.OpenRead(contentOwenr.Path))
            {
                return ReadContent(stream, file.TotalOffset, file.Size);   
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadContent(EdataContentFile file)
        {
            file.Content = ReadContent(file);
            //file.Size = file.Content.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadContent(IEnumerable<EdataContentFile> files)
        {
            foreach (var file in files)
            {
                LoadContent(file);
            }
        }

        /// <remarks>
        /// Method based on enohka's code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected EdataFile ReadEdata(String edataFilePath, bool loadContent, CancellationToken? token = null)
        {
            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            if (!File.Exists(edataFilePath))
            {
                throw new ArgumentException(String.Format("File '{0}' doesn't exist.", edataFilePath), "edataFilePath");
            }
            lastEdataFilePath = edataFilePath;

            EdataHeader header;
            //byte[] postHeaderData;
            IEnumerable<EdataContentFile> contentFiles;

            using (FileStream stream = new FileStream(lastEdataFilePath, FileMode.Open))
            {
                header = ReadHeader(stream);
                //postHeaderData = ReadPostHeaderData(stream, header);
                if (header.Version == 1)
                {
                    //ReadAndWriteDictionaryStats(stream, header, "C:\\ZZ_3.dat.txt");
                    contentFiles = ReadEdatV1Dictionary(stream, header, loadContent);
                }
                else if (header.Version == 2)
                {
                    //ReadAndWriteDictionaryStats(stream, header, "C:\\ZZ_3.dat.txt");
                    contentFiles = ReadEdatV2Dictionary(stream, header, loadContent);
                }
                else
                {
                    throw new NotSupportedException(String.Format("Edata Version {0} is currently not supported", header.Version));
                }
            }

            //EdataFile edataFile = new EdataFile(lastEdataFilePath, header, postHeaderData, contentFiles);
            EdataFile edataFile = new EdataFile(lastEdataFilePath, header, contentFiles);
            //Może to powinien przypiswyać plik edata...?
            foreach (var contentFile in edataFile.ContentFiles)
            {
                contentFile.Owner = edataFile;
            }

            //WriteContentFiles("C:\\cf_mod_sorted.txt", contentFiles);

            return edataFile;
        }

        //private void WriteContentFiles(String path, IEnumerable<EdataContentFile> files)
        //{
        //    var paths = files
        //        .Select(f => f.Path)
        //        .OrderBy(x => x, new EdataDictStringComparer())
        //        .ToList();

        //    using (var stream = File.CreateText(path))
        //    {
        //        foreach (var p in paths)
        //        {
        //            stream.WriteLine(p);
        //        }
        //    }
        //}

    }
}
