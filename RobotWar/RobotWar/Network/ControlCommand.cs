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
    public enum ControlCommand
    {
        TeamAlliance,
        TeamCoalition,
        Lobby,
        StartGameYourTurn,
        StartGameAdversaryTurn,
        YourTurn,
        AdversaryTurn,
        GameEndYouWin,
        GameEndAdversaryWins,
        AdversaryQuit
    }
}
