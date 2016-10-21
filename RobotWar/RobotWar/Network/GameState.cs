using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWar.Network
{

    /// <summary>
    /// Class documentation
    /// </summary>
    public enum GameState
    {
        WaitingForServer,
        ServerUnavailable,
        MyTurn,
        OtherTurn,
        GameOver
    }
}
