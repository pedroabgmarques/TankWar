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
        public float x;
        [JsonProperty("y")]
        public float y;
        [JsonProperty("tank")]
        public int tankID;

        public GameMoveMessage(MessageType msgType, float x, float y, int tank) : base(msgType)
        {
            this.x = x;
            this.y = y;
            this.tankID = tank;
        }
    }
}
