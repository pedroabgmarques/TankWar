/*
 * Class signature 
 */

using Newtonsoft.Json;
using RobotWar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWar.Network
{
    public class PowerUpListMessage : Message
    {

        [JsonProperty("listaPowerUps")]
        public List<PowerUp> listaPowerUps;

        public PowerUpListMessage(MessageType msgType, List<PowerUp> listaPowerUps)
            : base(msgType)
        {
            this.listaPowerUps = listaPowerUps;
        }

        public override byte[] ByteMessage()
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}
