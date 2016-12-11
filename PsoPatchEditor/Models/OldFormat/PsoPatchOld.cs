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
    [XmlType(TypeName = "PsoPatch")]
    public class PsoPatchOld
    {
        [XmlAttribute]
        public String Name { get; set; }

        [XmlAttribute]
        public UInt32 Address { get; set; }
        [XmlAttribute]
        public PatchType PatchType { get; set; }
        [XmlAttribute]
        public UInt32 Value { get; set; }
        
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
        private string _HexValue;
        [XmlAttribute]
        public string HexValue
        {
            get
            {
                return _HexValue;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    this._HexValue = value;
                    UInt32 parsedValue;
                    if (UInt32.TryParse(value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsedValue))
                    {
                        this.Value = parsedValue;
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
