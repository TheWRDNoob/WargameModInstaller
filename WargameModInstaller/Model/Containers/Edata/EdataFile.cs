using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WargameModInstaller.Model.Containers.Edata
{
    //To do: spr�bowa� wywali� to postheaderData, bo wydaje si� �e zawiera ono tylko ci�g zer a� do ofssetu s�ownika, sam s�ownik, i ci�g zer
    //       a� do kontentu plik�w. Poprostu trzeba okre�li� obie d� ciagu zer (pierwsza to napewno chyba do 1037Bajta) i wpisa� pomi�dzy nie s�ownik
    //       kwestia tylko tego jak to wyglada przy zagniezdzonych pakietach, pewnie bedzie problem, bo to jest tu chyba po to zeby od razu mozna by�o wpsia�
    //       ca�o�� z pamieci.

    //To do: To wszystko tutaj jest do przerobionia, bo zak��da tylko stworzenie obiektu i uniemo�liwia jego modyfikacj� co raczej jest konieczne
    //w wersji gdzie ten obiket ca�yczas posaida poprawne dane, a nei tylko dane z odczytu, oraz moze by� modyfikowany poprzez dodanie nowych ContentFiles.

    //Zak�adj�c �e chcemy umo�liwi� zmiane zawrto�ci tego pliku (chodzi o pliki contentu), trzeba jko� rozr�ni� stan oryginalny od zmodyfikowanego.
    //Raczej nie mo�na sobie ot tak nadpiswywa� co popadnie, lub zmienia� warto�ci odpoiwadajace za lokalizacji contentu w pliku fizycnzym, bo wtedy
    //nie mo�liwe b�dzie odczytywanie z pliku. Trzeba doda� jakies dodatkowe list przechwoujace stan zmodyfikowane, kt�ry w trkacie persystencji jest
    //zamienianie na stan normalny odpowiadajacy fizycznemu plikowi. Wtedy te� takie wpisy dostawa�y by poprawne warto�ci.

    /// <summary>
    /// 
    /// </summary>
    public class EdataFile : IContainerFile
    {
        private IDictionary<String, IContentFile> contentFilesDictionary;

        /// <summary>
        /// Creates an instance of EdataFile which doesn't reefer to any physical Edata file.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="contentFiles"></param>
        public EdataFile(
            EdataHeader header,
            /*byte[] postHeaderData,*/
            IEnumerable<IContentFile> contentFiles)
        {
            this.Header = header;
            //this.PostHeaderData = postHeaderData;
            this.contentFilesDictionary = contentFiles.ToDictionary(x => x.Path);
            //this.IsVirtual = true;

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
            /*byte[] postHeaderData,*/
            IEnumerable<IContentFile> contentFiles)
        {
            this.Path = path;
            this.Header = header;
            //this.PostHeaderData = postHeaderData;
            this.contentFilesDictionary = contentFiles.ToDictionary(x => x.Path);
            //this.IsVirtual = false;

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
            set; 
        }

        /// <summary>
        /// 
        /// </summary>
        public EdataHeader Header
        {
            get;
            set;
        }

        /// <summary>
        /// Ca�o�� danych pomiedzy ostatnim bajtem nag��wka, a pierwszym bajtem contentu plik�w.
        /// W sumie to s� g��wnie zera, s�ownik, zera... (po co to tak naprawde by�a?, wiem ze niedzialo bez tego...)
        /// 
        /// Update: Wraz z implementacj� budowy s�ownika edata od zera, te dane nie b�d� potrzebne. P�ki
        /// P�ki co dla kompatybilnowsci z poprzednimi klasami, nie korzystajcymi z budowy s�ownika, musi zosta�.
        /// </summary>
        //public byte[] PostHeaderData
        //{
        //    get;
        //    private set;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //public bool IsVirtual
        //{
        //    get;
        //    private set;
        //}

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
        public IReadOnlyCollection<IContentFile> ContentFiles
        {
            get
            {
                return contentFilesDictionary.Values.ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        public IContentFile GetContentFileByPath(String contentPath)
        {
            IContentFile result;
            if (contentFilesDictionary.TryGetValue(contentPath, out result))
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException(
                    String.Format(Properties.Resources.ContentFileNotFoundParamMsg, contentPath));
            }
        }

        public bool ContainsContentFileWithPath(String path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException(
                    String.Format("Cannot vaerify existance of the file without the specified content path."), 
                    "contentFile");
            }

            return contentFilesDictionary.ContainsKey(path);
        }

        public void AddContentFile(IContentFile contentFile)
        {
            if(String.IsNullOrEmpty(contentFile.Path))
            {
                throw new ArgumentException(
                    String.Format("Cannot add a content file without the specified content path."), 
                    "contentFile");
            }

            //Check for whitespaces

            if (contentFilesDictionary.ContainsKey(contentFile.Path))
            {
                throw new ArgumentException(
                    String.Format("Cannot add a content file with the follwing path \"{0}\"" + 
                    "because a content file with this path already exists.", contentFile.Path), 
                    "contentFile");
            }

            contentFilesDictionary.Add(contentFile.Path, contentFile);
            contentFile.Owner = this;
            HasContentFilesCollectionChanged = true;
        }

        public void RemoveContentFile(IContentFile contentFile)
        {
            if (!contentFilesDictionary.ContainsKey(contentFile.Path))
            {
                throw new InvalidOperationException(
                    String.Format("Cannot remove a content file with the follwing path \"{0}\", because it doesn't exist.",
                    contentFile.Path));
            }

            contentFilesDictionary.Remove(contentFile.Path);
            contentFile.Owner = null;
            HasContentFilesCollectionChanged = true;
        }

        protected void AssignOwnership(IEnumerable<IContentFile> contentFiles)
        {
            foreach (var cf in contentFiles)
            {
                cf.Owner = this;
            }
        }

    }
}
