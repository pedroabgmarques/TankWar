/*
 * Class signature 
 */

using RobotWar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Common
{

    /// <summary>
    /// Class documentation
    /// </summary>
    public class Player
    {

        private int msgCounter;
        public int MsgCounter
        {
            get 
            { 
                return msgCounter++;
            }
            set { msgCounter = value; }
        }
        

        private TcpClient client;
        public TcpClient Client
        {
            get { return client; }
            set { client = value; }
        }
        
        private Team equipa;
        public Team Equipa
        {
            get { return equipa; }
            set { equipa = value; }
        }

        private PlayerStatus playerStatus;
        public PlayerStatus PlayerStatus
        {
            get { return playerStatus; }
            set { playerStatus = value; }
        }

        private Game currentGame;

        public Game CurrentGame
        {
            get { return currentGame; }
            set { currentGame = value; }
        }
        
        

        public Player(TcpClient client, Team equipa)
        {
            this.client = client;
            this.equipa = equipa;
            this.playerStatus = PlayerStatus.Waiting;
            this.MsgCounter = 0;
        }

        
    }
}
