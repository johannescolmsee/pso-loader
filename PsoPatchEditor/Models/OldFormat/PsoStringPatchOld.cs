using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PsoPatchEditor.Models.OldFormat
{
    [XmlType(TypeName = "PsoStringPatch")]
    public class PsoStringPatchOld
    {
        public PsoStringPatchOld()
        {
            this.AddTerminatingZero = true;
        }

        [XmlAttribute]
        public String Name { get; set; }

        [XmlAttribute]
        public UInt32 Address { get; set; }

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
                    var valueFixed = value;
                    if (valueFixed.StartsWith("0x"))
                    {
                        valueFixed = valueFixed.Substring(2);
                    }
                    UInt32 parsedValue;
                    if (UInt32.TryParse(valueFixed, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsedValue))
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

        [XmlAttribute]
        public string Value { get; set; }

        [XmlAttribute]
        public bool AddTerminatingZero { get; set; }
    }
}
