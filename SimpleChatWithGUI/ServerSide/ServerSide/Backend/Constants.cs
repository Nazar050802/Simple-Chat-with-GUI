using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    internal class Constants
    {
        public const int EncryptSizeBytes = 128;

        public const int BufferSize = 6144;

        public const bool DebugMode = true;

        public const int DefaultPort = 11000;

        public const string DefaultIP = "127.0.0.1";

        public const string ServerMessageGetListOfRooms = "[GET_LIST_ROOMS]";

        public const string ServerMessageSendMessage = "[SEND_MESSAGE]";

        public const string ServerMessageGetMessage = "[GET_MESSAGE]";

        public const string ServerMessageWrongName = "[WRONG_NAME]";

        public const string ServerMessageSetName = "[SET_NAME]";

        public const string ServerMessageSuccessJoinToRoom = "[SUCCESS_JOIN_TO_ROOM]";

        public const string ServerMessageWrongRoomPassword = "[WRONG_PASSWORD]";
    }
}
