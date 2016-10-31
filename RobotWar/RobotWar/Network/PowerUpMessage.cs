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

    /// <summary>
    /// Class documentation
    /// </summary>
    public class PowerUpMessage : Message
    {

        [JsonProperty("powerUp")]
        public PowerUp powerUp;

        public PowerUpMessage(MessageType msgType, PowerUp powerUp)
            : base(msgType)
        {
            this.powerUp = powerUp;
        }
    }
}
