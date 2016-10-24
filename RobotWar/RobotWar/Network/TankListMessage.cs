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
    public class TankListMessage : Message
    {

        [JsonProperty("listaPowersEVida")]
        public List<Tuple<Team, int, int, int>> listaPowersEVida;

        public TankListMessage(MessageType msgType, List<Tuple<Team, int, int, int>> listaPowersEVida) : base (msgType){
            this.listaPowersEVida = listaPowersEVida;
        }

        public override byte[] ByteMessage()
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}
