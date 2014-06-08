using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WargameModInstaller.Model.Edata
{
    //To do: spr�bowa� wywali� to postheaderData, bo wydaje si� �e zawiera ono tylko ci�g zer a� do ofssetu s�ownika, sam s�ownik, i ci�g zer
    //       a� do kontentu plik�w. Poprostu trzeba okre�li� obie d� ciagu zer (pierwsza to napewno chyba do 1037Bajta) i wpisa� pomi�dzy nie s�ownik
    //       kwestia tylko tego jak to wyglada przy zagniezdzonych pakietach, pewnie bedzie problem, bo to jest tu chyba po to zeby od razu mozna by�o wpsia�
    //       ca�o�� z pamieci.

    public class EdataFile
    {
        private IDictionary<String, EdataContentFile> contentFilesDictionary;

        /// <summary>
        /// Creates an instance of EdataFile which doesn't reefer to any physical Edata file.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="contentFiles"></param>
        public EdataFile(
            EdataHeader header,
            byte[] postHeaderData, 
            IEnumerable<EdataContentFile> contentFiles)
        {
            this.Header = header;
            this.PostHeaderData = postHeaderData;
            this.contentFilesDictionary = contentFiles.ToDictionary(x => x.Path);
            this.IsVirtual = true;

            AssignOwnership(this.ContentFiles);
        }

        /// <summary>
        /// Creates an instance of EdataFile which represent physical EdataFile witha a file path. 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="header"></param>
        /// <param name="contentFiles"></param>
        public EdataFile(
            String path, 
            EdataHeader header,
            byte[] postHeaderData, 
            IEnumerable<EdataContentFile> contentFiles)
        {
            this.Path = path;
            this.Header = header;
            this.PostHeaderData = postHeaderData;
            this.contentFilesDictionary = contentFiles.ToDictionary(x => x.Path);
            this.IsVirtual = false;

            AssignOwnership(this.ContentFiles);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// From now it might not have a path.
        /// </remarks>
        public String Path 
        {
            get; 
            private set; 
        }

        /// <summary>
        /// 
        /// </summary>
        public EdataHeader Header
        {
            get;
            private set;
        }

        /// <summary>
        /// Ca�o�� danych pomiedzy ostatnim bajtem nag��wka, a pierwszym bajtem contentu plik�w.
        /// W sumie to s� g��wnie zera, s�ownik, zera... (po co to tak naprawde by�a?, wiem ze niedzialo bez tego...)
        /// </summary>
        public byte[] PostHeaderData
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsVirtual
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasContentFilesCollectionChanged
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<EdataContentFile> ContentFiles
        {
            get
            {
                return contentFilesDictionary.Values;
            }
        }

        public EdataContentFile GetContentFileByPath(String contentPath)
        {
            EdataContentFile result;
            if (contentFilesDictionary.TryGetValue(contentPath, out result))
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException(
                    String.Format(Properties.Resources.ContentFileNotFoundParametrizedMsg, contentPath));
            }
        }

        public void AddContentFile(EdataContentFile contentFile)
        {
            if(String.IsNullOrEmpty(contentFile.Path))
            {
                throw new ArgumentException(
                    String.Format("Cannot add a content file without the specified path."), 
                    "contentFile");
            }

            //Check for whitespaces

            if (contentFilesDictionary.ContainsKey(contentFile.Path))
            {
                throw new ArgumentException(
                    String.Format("Cannot add a content file with the follwing path \"{0}\", because a content file with this path already exists."), 
                    "contentFile");
            }

            contentFilesDictionary.Add(contentFile.Path, contentFile);
            contentFile.Owner = this;
            HasContentFilesCollectionChanged = true;
        }

        public void RemoveContentFile(EdataContentFile contentFile)
        {
            if (!contentFilesDictionary.ContainsKey(contentFile.Path))
            {
                throw new InvalidOperationException(
                    String.Format("Cannot remove a content file with the follwing path \"{0}\", because it doesn't exist."));
            }

            contentFilesDictionary.Remove(contentFile.Path);
            contentFile.Owner = null;
            HasContentFilesCollectionChanged = true;
        }

        protected void AssignOwnership(IEnumerable<EdataContentFile> contentFiles)
        {
            foreach (var cf in contentFiles)
            {
                cf.Owner = this;
            }
        }

    }
}
