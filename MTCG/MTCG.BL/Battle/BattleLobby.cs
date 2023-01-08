using MTCG.Model.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.BL.Battle
{
    public sealed class BattleLobby
    {
        private static BattleLobby instance = null;
        private static readonly object padlock = new object();

        public static BattleLobby Instance
        {
            get
            {
                lock (padlock)
                {
                    if(instance == null)
                    {
                        instance = new BattleLobby();
                    }
                    return instance;
                }
            }
        }

        List<Player> players = new List<Player>();
        Battle? battle = null;
        public BattleLobby()
        {

        }

        public void EnterLobby(Player player)
        {
            players.Add(player);

            while(true)
            {
                if(players.Count == 2)
                {
                    battle = new Battle(players[0], players[1]);
                    battle.executeBattle();
                    return;
                }
            }
        }
    }
}
