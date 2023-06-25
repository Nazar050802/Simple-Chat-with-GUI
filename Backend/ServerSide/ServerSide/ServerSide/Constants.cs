using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    internal class Constants
    {
        public const int TimeOutToDisconnectClient = 3 * 60000;

        public const int EncryptSizeBytes = 128;

        public const int BufferSize = 6144;
    }
}
