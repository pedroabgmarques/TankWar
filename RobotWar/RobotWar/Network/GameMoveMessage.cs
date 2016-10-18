/*
 * Class signature 
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWar.Network
{

    /// <summary>
    /// Class documentation
    /// </summary>
    public class GameMoveMessage : Message
    {
        int x;
        int y;
        MessageType msgType;

        public override byte[] byteMessage()
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}
