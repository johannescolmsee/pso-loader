using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsoPatchEditor.Models.OldFormat
{
    using LibPSO.PsoPatcher;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Common.EnumerableExtensions;
    using LibPSO;

    [XmlType(TypeName = "PsoPatchDefinition")]
    public class PsoPatchDefinitionOld
    {
        public PsoPatchDefinitionOld()
        {
            this.SingleValuePatches = new List<PsoPatchOld>();
            this.RangePatches = new List<PsoRangePatchOld>();
            this.StringPatches = new List<PsoStringPatchOld>();
        }

        public List<PsoPatchOld> SingleValuePatches { get; set; }
        public List<PsoRangePatchOld> RangePatches { get; set; }
        public List<PsoStringPatchOld> StringPatches { get; set; }
        public PsoRedirectOld Redirect { get; set; }

        #region Serialization
        public static PsoPatchDefinitionOld FromXml(string filename)
        {
            using (var fileStream = File.OpenRead(filename))
            {
                return FromXml(fileStream);
            }
        }

        public static PsoPatchDefinitionOld FromXml(Stream stream)
        {
            XmlSerializer ser = new XmlSerializer(typeof(PsoPatchDefinitionOld));
            return ser.Deserialize(stream) as PsoPatchDefinitionOld;
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
            XmlSerializer ser = new XmlSerializer(typeof(PsoPatchDefinitionOld));
            ser.Serialize(stream, this);
        }
        #endregion

        public PsoPatchDefinition GetPsoPatchDefinition()
        {
            return new PsoPatchDefinition()
            {
                Patches = this._GetXmlPatchDefinitions().ToObservableCollection(),
                Redirect = new LibPSO.PsoPatcher.PsoRedirect()
                {
                    IPAddress = this.Redirect.IPAddresse,
                    Port = this.Redirect.Port,
                }
            };
        }

        private IEnumerable<XmlPatchDefinition> _GetXmlPatchDefinitions()
        {
            foreach (var p in this.SingleValuePatches)
            {
                yield return new XmlPatchDefinition()
                {
                    Address = p.Address,
                    Name = p.Name,
                    ByteValues = _GetBytes(p),
                };
            }
            foreach (var p in this.RangePatches)
            {
                yield return new XmlPatchDefinition()
                {
                    Address = p.Address,
                    Name = p.Name,
                    ByteValues = _GetBytes(p),
                };
            }
            foreach (var p in this.StringPatches)
            {
                yield return new XmlPatchDefinition()
                {
                    Address = p.Address,
                    Name = p.Name,
                    StringValue = p.Value,
                };
            }
        }

        private byte[] _GetBytes(PsoRangePatchOld p)
        {
            return p.Values
                .SelectMany(x => Helper.GetBytes(x))
                .ToArray();
        }

        private byte[] _GetBytes(PsoPatchOld p)
        {
            switch (p.PatchType)
            {
                case PatchType.Byte:
                    return new byte[] { (byte)(p.Value & 0xFF) };
                case PatchType.HalfWord:
                    return new byte[] { (byte)((p.Value >> 8) & 0xFF), (byte)(p.Value & 0xFF) };
                case PatchType.Word:
                    return new byte[] { (byte)((p.Value >> (8 * 3)) & 0xFF), (byte)((p.Value >> (8 * 2)) & 0xFF), (byte)((p.Value >> 8) & 0xFF), (byte)(p.Value & 0xFF) };
                default:
                    throw new Exception("Unexpected PsoPatchType");
            }
        }
    }
}