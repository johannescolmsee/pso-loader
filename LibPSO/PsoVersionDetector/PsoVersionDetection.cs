using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LibPSO.PsoVersionDetector
{
    public class PsoVersionDetection
    {
        [XmlAttribute]
        public UInt32 Address { get; set; }

        [XmlAttribute]
        public UInt32 ComparisonValue { get; set; }

        [XmlAttribute]
        public UInt32 ReturnValue { get; set; }

        [XmlAttribute]
        public byte[] Bytes { get; set; }

        #region Hex Input fields (strings)
        private string _HexReturnValue;
        [XmlAttribute]
        public string HexReturnValue
        {
            get
            {
                return _HexReturnValue;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    this._HexReturnValue = value;
                    UInt32 parsedValue;
                    if (UInt32.TryParse(value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsedValue))
                    {
                        this.ReturnValue = parsedValue;
                    }
                    else
                    {
                        throw new Exception("Illegal hex.");
                    }
                }
            }
        }

        private string _HexComparisonValue;
        [XmlAttribute]
        public string HexComparisonValue
        {
            get
            {
                return _HexComparisonValue;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    this._HexComparisonValue = value;
                    UInt32 parsedValue;
                    if (UInt32.TryParse(value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsedValue))
                    {
                        this.ComparisonValue = parsedValue;
                    }
                    else
                    {
                        throw new Exception("Illegal hex.");
                    }
                }
            }
        }

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
                this._HexAddress = value;
                if (!String.IsNullOrEmpty(value))
                {
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

        public IEnumerable<uint> GetVersionDetectionData()
        {
            yield return this.Address;
            yield return this.ComparisonValue;
            yield return this.ReturnValue;
        }
    }
}
