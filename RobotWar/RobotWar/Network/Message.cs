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
    public class Message
    {
        [JsonProperty("msgType")]
        public MessageType msgType { get; set; }
        [JsonProperty("msgNumber")]
        public int msgNumber { get; set; }
        public Message(MessageType msgType)
        {
            this.msgType = msgType;
        }
        public byte[] ByteMessage()
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}
