/*
 * Class signature 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{

    /// <summary>
    /// Class documentation
    /// </summary>
    public class Game
    {
        public Player player1;
        public Player player2;
        public Player currentPlayer;
        public Stack<GameState> estadosJogo;

        public Game(Player player1, Player player2)
        {
            this.player1 = player1;
            this.player2 = player2;
            this.currentPlayer = player1;
            estadosJogo = new Stack<GameState>();
        }
    }
}
