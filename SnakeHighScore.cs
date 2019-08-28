using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake_Game
{
    /// <summary>
    /// Object of this class stores a player highscore
    /// </summary>
    public class SnakeHighscore
    {
        /// <summary>
        /// Player name of highscore entry
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Score of highscore entry
        /// </summary>
        public int Score { get; set; } 
    }
}
