using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LibPSO.PsoPatcher
{
    public class PsoPatch : PsoPatchBase
    {
        private PsoPatchType _PatchType;
        [XmlAttribute]
        public PsoPatchType PatchType
        {
            get { return this._PatchType; }
            set { if (!object.Equals(this._PatchType, value)) { this._PatchType = value; this.OnPropertyChanged(nameof(PatchType)); } }
        }


        private UInt32 _Value;
        [XmlAttribute]
        public UInt32 Value
        {
            get { return this._Value; }
            set { if (!object.Equals(this._Value, value)) { this._Value = value; this.OnPropertyChanged(nameof(Value)); } }
        }

        public override IEnumerable<uint> GetPatchProgramData()
        {
            yield return this.GetAddressWithType((UInt32)this.PatchType);
            yield return this.Value;
        }
    }
}
