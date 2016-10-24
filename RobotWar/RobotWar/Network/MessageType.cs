/*
 * Class signature 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWar.Network
{

    public enum MessageType
    {
        Control,
        Move,
        Attack,
        EndTurn,
        PowerUpList,
        PowerUp,
        PlayerWon,
        TankList
    }
}
