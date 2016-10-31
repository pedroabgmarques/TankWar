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
        [JsonProperty("command")]
        public ControlCommand command;

        public ControlMessage(MessageType msgType, ControlCommand command)
            : base(msgType)
        {
            this.command = command;
        }
    }
}
