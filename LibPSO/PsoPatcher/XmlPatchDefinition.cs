using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LibPSO.PsoPatcher
{
    [XmlType(TypeName = "Patch")]
    public class XmlPatchDefinition : PsoPatchBase
    {
        public XmlPatchDefinition()
        {
            this.AddTerminatingZero = true;
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

        
        private string _StringValue;
        [XmlAttribute]
        public string StringValue
        {
            get { return this._StringValue; }
            set { if (!object.Equals(this._StringValue, value)) { this._StringValue = value; this.OnPropertyChanged(nameof(StringValue)); } }
        }

        private bool _AddTerminatingZero;
        [XmlAttribute]
        public bool AddTerminatingZero
        {
            get { return this._AddTerminatingZero; }
            set { if (!object.Equals(this._AddTerminatingZero, value)) { this._AddTerminatingZero = value; this.OnPropertyChanged(nameof(AddTerminatingZero)); } }
        }

        private byte[] _ByteValues;
        [XmlAttribute(DataType = "hexBinary")]
        public byte[] ByteValues
        {
            get { return this._ByteValues; }
            set { if (!object.Equals(this._ByteValues, value)) { this._ByteValues = value; this.OnPropertyChanged(nameof(ByteValues)); } }
        }

        public override uint GetPatchCount()
        {
            var patches = _GetPatches();
            return (UInt32)patches.Sum(x => x.GetPatchCount());
        }

        public override IEnumerable<uint> GetPatchProgramData()
        {
            var patches = _GetPatches();
            foreach (var p in patches)
            {
                foreach (var v in p.GetPatchProgramData())
                {
                    yield return v;
                }
            }
        }

        private IEnumerable<PsoPatchBase> _GetPatches()
        {
            byte[] bytes;
            if (!String.IsNullOrEmpty(this.StringValue))
            {
                var stringBytes = this.StringValue.Select(x => (byte)x);

                if (this.AddTerminatingZero)
                {
                    stringBytes = stringBytes.Concat(new[] { (byte)0 });
                }
                bytes = stringBytes.ToArray();
            }
            else
            {
                bytes = this.ByteValues;
            }
            bytes = bytes ?? new byte[0];

            //partition in 4char blocks (word)
            var byteGroups = bytes
                .Select((x, idx) => new { Byte = x, Index = idx })
                .GroupBy(x => x.Index / 4, x => x.Byte)
                .Select(x => x.ToArray());

            var fullWordGroups = byteGroups
                .Where(x => x.Length == 4)
                .ToArray();//can evaluate right away, need a 2nd time

            var address = this.Address;

            if (fullWordGroups.Any())
            {
                var rangeUints = fullWordGroups
                        .Select(x => x.Select(y => (byte)y))
                        .Select(x => Helper.GetUInt32(x))
                        .ToList();
                yield return new PsoRangePatch() { Address = address, Values = rangeUints };
                address += (UInt32)(rangeUints.Count * 4);
            }

            var remaining = byteGroups.Where(x => x.Length < 4).SelectMany(x => x).Select(x => (byte)x);
            if (remaining.Any())
            {
                if (remaining.Count() >= 2)
                {
                    var halfword = Helper.GetUInt16(remaining.Take(2));
                    yield return new PsoPatch() { PatchType = PsoPatchType.SingleHalfWord, Address = address, Value = halfword };
                    address += 2;

                    remaining = remaining.Skip(2);
                }
                foreach (var cg in remaining.Select(x => (byte)x))
                {
                    yield return new PsoPatch() { PatchType = PsoPatchType.SingleByte, Address = address, Value = (UInt32)cg };
                    address += 1;
                }
            }
        }
    }
}
