using Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LibPSO.PsoPatcher
{
    public abstract class PsoPatchBase : NotifyPropertyChangedBase
    {
        private String _Name;
        [XmlAttribute]
        public String Name
        {
            get { return this._Name; }
            set { if (!object.Equals(this._Name, value)) { this._Name = value; this.OnPropertyChanged(nameof(Name)); } }
        }

        private UInt32 _Address;
        [XmlAttribute]
        public UInt32 Address
        {
            get { return this._Address; }
            set { if (!object.Equals(this._Address, value)) { this._Address = value; this.OnPropertyChanged(nameof(Address)); } }
        }

        public virtual uint GetPatchCount()
        {
            return 1;
        }

        protected UInt32 GetAddressWithType(UInt32 type)
        {
            if (type > 3)
            {
                throw new Exception("Type must be <= 3");
            }

            var addressAndType = (type << 30) | (this.Address & 0x3FFFFFFF);
            return addressAndType;
        }

        public abstract IEnumerable<UInt32> GetPatchProgramData();

        //public IEnumerable<string> GetWarningsAndErrors

        public virtual IEnumerable<string> GetErrorsAndWarnings()
        {
            if (this.Address < 0x80000000
                ||
                (this.Address >= 0x81800000 && this.Address < 0xC0000000)
                ||
                this.Address >= 0xC1800000
                )
            {
                yield return "[Error] Address is out of range.";
            }
            if (this.Address >= 0x81800000)
            {
                yield return "[Warning] Write to uncached memory should not be necessary - the RAM will be synced after patching.";
            }
        }
    }
}
