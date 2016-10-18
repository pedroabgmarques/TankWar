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
    public class ControlMessage : Message
    {
        [JsonProperty("msgType")]
        MessageType msgType;
        [JsonProperty("command")]
        ControlCommand command;

        public ControlMessage(MessageType msgType, ControlCommand command)
        {
            this.msgType = msgType;
            this.command = command;
        }

        public override byte[] byteMessage()
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}
