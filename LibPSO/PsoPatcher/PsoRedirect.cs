using Common;
using LibPSO.PsoServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace LibPSO.PsoPatcher
{
    public class PsoRedirect : NotifyPropertyChangedBase
    {
        private string _Name;
        [XmlAttribute]
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Name
        {
            get { return this._Name; }
            set { if (!object.Equals(this._Name, value)) { this._Name = value; this.OnPropertyChanged(nameof(Name)); } }
        }

        private string _IPAddress;
        [XmlAttribute]
        public string IPAddress
        {
            get { return this._IPAddress; }
            set { if (!object.Equals(this._IPAddress, value)) { this._IPAddress = value; this.OnPropertyChanged(nameof(IPAddress)); } }
        }

        private UInt16 _Port;
        [XmlAttribute]
        public UInt16 Port
        {
            get { return this._Port; }
            set { if (!object.Equals(this._Port, value)) { this._Port = value; this.OnPropertyChanged(nameof(Port)); } }
        }

        public byte[] GetRedirectPacket(ClientType clientType)
        {
            var ipmatch = Regex.Match(this.IPAddress, @"(?<ip1>\d+)\.(?<ip2>\d+)\.(?<ip3>\d+)\.(?<ip4>\d+)");
            UInt32 ip = 
                ((UInt32.Parse(ipmatch.Groups["ip1"].Value) & 0xFF) << 24)
                |
                ((UInt32.Parse(ipmatch.Groups["ip2"].Value) & 0xFF) << 16)
                |
                ((UInt32.Parse(ipmatch.Groups["ip3"].Value) & 0xFF) << 8)
                |
                ((UInt32.Parse(ipmatch.Groups["ip4"].Value) & 0xFF) << 0)
                ;
            UInt16 port = this.Port;
            return Packets.GetRedirectPacket(ip, port, clientType);
        }
    }
}
