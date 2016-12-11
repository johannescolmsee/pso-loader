using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LibPSO.PsoPatcher
{
    public class PsoRangePatch : PsoPatchBase
    {
        public PsoRangePatch()
        {
            this.Values = new List<uint>();
        }

        private List<UInt32> _Values;
        public List<UInt32> Values
        {
            get { return this._Values; }
            set { if (!object.Equals(this._Values, value)) { this._Values = value; this.OnPropertyChanged(nameof(Values)); } }
        }

        public override IEnumerable<uint> GetPatchProgramData()
        {
            yield return this.GetAddressWithType((UInt32)PsoPatchType.RangeWord);
            yield return (UInt32)this.Values.Count;
            foreach (var value in this.Values)
            {
                yield return value;
            }
        }

        public override IEnumerable<string> GetErrorsAndWarnings()
        {
            if (this.Values.Count == 0)
            {
                yield return "[Warning] Range of size 0.";
            }
            foreach (var m in base.GetErrorsAndWarnings())
            {
                yield return m;
            }
        }
    }
}
