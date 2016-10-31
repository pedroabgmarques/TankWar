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
    public class EndTurnMessage : Message
    {

        public EndTurnMessage(MessageType msgType)
            : base(msgType) { }

    }
}
