using Common;
using LibPSO.PsoServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace PsoPatchEditor.Models.OldFormat
{
    [XmlType(TypeName = "PsoRedirect")]
    public class PsoRedirectOld
    {
        [XmlAttribute]
        public string IPAddresse { get; set; }
        [XmlAttribute]
        public UInt16 Port { get; set; }
    }
}
