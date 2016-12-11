using LibPSO.PsoPatcher;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PsoPatchEditor.Models.OldFormat
{
    [XmlType(TypeName = "PsoRangePatch")]
    public class PsoRangePatchOld
    {
        [XmlAttribute]
        public String Name { get; set; }

        [XmlAttribute]
        public UInt32 Address { get; set; }

        public List<UInt32> Values { get; set; }

        #region Hex Input fields (strings)
        private string _HexAddress;
        [XmlAttribute]
        public string HexAddress
        {
            get
            {
                return _HexAddress;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    this._HexAddress = value;
                    UInt32 parsedValue;
                    if (UInt32.TryParse(value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsedValue))
                    {
                        this.Address = parsedValue;
                    }
                    else
                    {
                        throw new Exception("Illegal hex.");
                    }
                }
            }
        }
        #endregion
    }
}
