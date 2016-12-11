using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LibPSO.PsoVersionDetector
{
    public class PsoVersionDetectionDefinition
    {
        public PsoVersionDetectionDefinition()
        {
            this.VersionChecks = new List<PsoVersionDetection>();
        }

        [XmlAttribute]
        public UInt32 DefaultReturnValue { get; set; }

        public List<PsoVersionDetection> VersionChecks { get; set; }

        #region Hex Input fields (strings)
        private string _HexDefaultReturnValue;
        [XmlAttribute]
        public string HexDefaultReturnValue
        {
            get
            {
                return _HexDefaultReturnValue;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    this._HexDefaultReturnValue = value;
                    UInt32 parsedValue;
                    if (UInt32.TryParse(value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsedValue))
                    {
                        this.DefaultReturnValue = parsedValue;
                    }
                    else
                    {
                        throw new Exception("Illegal hex.");
                    }
                }
            }
        }
        #endregion

        #region Program Data Generation

        public byte[] GetPsoVersionDetectionProgram()
        {
            var patcherCode = _GetPsoVersionDetectionProgram();
            return
                patcherCode
                .Concat(Helper.GetBytes(this.DefaultReturnValue))
                .Concat(Helper.GetBytes((UInt32)this.VersionChecks.Count))
                .Concat(
                    _GetPsoVersionDetectionProgramData()
                    .SelectMany(x => Helper.GetBytes(x))
                ).ToArray();
        }

        private byte[] _GetPsoVersionDetectionProgram()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "LibPSO.PsoVersionDetector.pso_version_detector.bin";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private IEnumerable<UInt32> _GetPsoVersionDetectionProgramData()
        {
            foreach (var data in this.VersionChecks.SelectMany(x => x.GetVersionDetectionData()))
            {
                yield return data;
            }
        }
        #endregion

        #region Serialization
        public static PsoVersionDetectionDefinition FromXml(string filename)
        {
            using (var fileStream = File.OpenRead(filename))
            {
                return FromXml(fileStream);
            }
        }

        public static PsoVersionDetectionDefinition FromXml(Stream stream)
        {
            XmlSerializer ser = new XmlSerializer(typeof(PsoVersionDetectionDefinition));
            return ser.Deserialize(stream) as PsoVersionDetectionDefinition;
        }

        public void ToXml(string filename)
        {
            using (var fileStream = File.Open(filename, FileMode.Create))
            {
                ToXml(fileStream);
            }
        }

        public void ToXml(Stream stream)
        {
            XmlSerializer ser = new XmlSerializer(typeof(PsoVersionDetectionDefinition));
            ser.Serialize(stream, this);
        }
        #endregion
    }
}
