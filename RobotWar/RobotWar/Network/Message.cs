/*
 * Class signature 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWar.Network
{

    /// <summary>
    /// Class documentation
    /// </summary>
    public abstract class Message
    {
        public abstract byte[] byteMessage();
    }
}
