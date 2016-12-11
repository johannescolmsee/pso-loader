using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPSO.PsoServices.Interfaces
{
    public class PsoMessage : NotifyPropertyChangedBase
    {

        public PsoMessage(Direction direction, byte[] message, byte[] messagecrypted)
        {
            this.Direction = direction;
            this.Message = message;
            this.MessageCrypted = messagecrypted;
        }

        private Direction _Direction;
        public Direction Direction
        {
            get
            {
                return _Direction;
            }
            private set
            {
                if (_Direction != value)
                {
                    _Direction = value;
                    OnPropertyChanged(nameof(Direction));
                }
            }
        }

        private byte[] _Message;
        public byte[] Message
        {
            get
            {
                return _Message;
            }
            private set
            {
                if (_Message != value)
                {
                    _Message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }


        private byte[] _MessageCrypted;
        public byte[] MessageCrypted
        {
            get
            {
                return _MessageCrypted;
            }
            private set
            {
                if (_MessageCrypted != value)
                {
                    _MessageCrypted = value;
                    OnPropertyChanged(nameof(MessageCrypted));
                }
            }
        }

    }
}
