using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LibPSO.PsoPatcher
{
    public class PsoPatchDefinition : NotifyPropertyChangedBase
    {
        public PsoPatchDefinition()
        {
            this.Patches = new ObservableCollection<XmlPatchDefinition>();
        }

        private ObservableCollection<XmlPatchDefinition> _Patches;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<XmlPatchDefinition> Patches
        {
            get { return this._Patches; }
            set { if (!object.Equals(this._Patches, value)) { this._Patches = value; this.OnPropertyChanged(nameof(Patches)); } }
        }

        private PsoRedirect _Redirect;
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public PsoRedirect Redirect
        {
            get { return this._Redirect; }
            set { if (!object.Equals(this._Redirect, value)) { this._Redirect = value; this.OnPropertyChanged(nameof(Redirect)); } }
        }




        #region Program Data Generation

        public byte[] GetPatchProgram()
        {
            var patcherCode = _GetPatcherCode();
            return
                patcherCode
                .Concat(
                    _GetPatchProgramData()
                    .SelectMany(x => Helper.GetBytes(x))
                ).ToArray();
        }

        private byte[] _GetPatcherCode()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "LibPSO.PsoPatcher.pso_patcher.bin";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private IEnumerable<UInt32> _GetPatchProgramData()
        {
            var allPatches = this.Patches;
            yield return (UInt32)(allPatches.Sum(x => x.GetPatchCount()));

            foreach (var patchdata in allPatches.SelectMany(x => x.GetPatchProgramData()))
            {
                yield return patchdata;
            }
        }
        #endregion

        #region Serialization
        public static PsoPatchDefinition FromXml(string filename)
        {
            using (var fileStream = File.OpenRead(filename))
            {
                return FromXml(fileStream);
            }
        }

        public static PsoPatchDefinition FromXml(Stream stream)
        {
            XmlSerializer ser = new XmlSerializer(typeof(PsoPatchDefinition));
            return ser.Deserialize(stream) as PsoPatchDefinition;
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
            XmlSerializer ser = new XmlSerializer(typeof(PsoPatchDefinition));
            ser.Serialize(stream, this);
        }
        #endregion

        public IEnumerable<string> GetErrorsAndWarnings()
        {
            return this.Patches
                .Select(x => new { Patch = x, ErrorsAndWarnings = x.GetErrorsAndWarnings() ?? Enumerable.Empty<string>() })
                .Where(x => x.ErrorsAndWarnings.Any())
                .SelectMany(x => x.ErrorsAndWarnings
                                    .Select(y => String.Format(@"{0}: {1})", !String.IsNullOrEmpty(x.Patch.Name) ? x.Patch.Name : String.Format(@"[Type:{0}]", x.Patch.GetType().Name), y))
                           );
        }
    }
}
