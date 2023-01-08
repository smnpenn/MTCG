using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Model.Cards;

namespace MTCG.Model.Users
{
    public class User
    {
        private int coins = 20;

        public UserCredentials Credentials
        {
            get;
            private set;
        }

        public UserData Data
        {
            get;
            private set;
        }

        public UserStats Stats
        {
            get;
            private set;
        }

        public List<Card> Stack 
        { 
            get;
            private set;
        }

        public List<Card> Deck
        {
            get;
            private set;
        }

        public int Coins
        {
            get;
            private set;
        }

        public User(UserCredentials credentials, UserData data, UserStats stats)
        {
            Credentials = credentials;
            Data = data;
            Stats = stats;
            Deck = new List<Card>();
            Stack = new List<Card>();
        }

        public User(UserCredentials credentials)
        {
            this.Credentials = credentials;
            Data = new UserData();
            Data.Bio = "Bio";
            Data.Image = ":)";
            Data.Name = credentials.Username;

            Stats = new UserStats();
            Stats.Wins = 0;
            Stats.Losses = 0;
            Stats.Elo = 0;
            Stats.Name = credentials.Username;

            Deck = new List<Card>();
            Stack = new List<Card>();
        }
    }
}
