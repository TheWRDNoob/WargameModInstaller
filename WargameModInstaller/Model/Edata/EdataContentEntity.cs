using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace WargameModInstaller.Model.Edata
{
    public abstract class EdataContentEntity
    {
        public EdataContentEntity()
        {

        }

        public EdataFile Owner
        {
            get;
            set;
        }

        public int GroupId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public int FileEntrySize
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name.ToString(CultureInfo.CurrentCulture);
        }
    }
}
