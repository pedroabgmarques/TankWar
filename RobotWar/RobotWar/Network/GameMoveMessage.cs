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
        [JsonProperty("x")]
        int x;
        [JsonProperty("y")]
        int y;

        public GameMoveMessage(MessageType msgType, int x, int y) : base(msgType)
        {
            this.x = x;
            this.y = y;
        }

        public override byte[] byteMessage()
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}
