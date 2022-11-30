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
            this.Credentials = credentials;
            this.Data = data;
            this.Stats = stats;
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

        public void AddCardToStack(Card card)
        {
            if (card != null)
                Stack.Add(card);
        }

        //TODO: Check if cards in stack, check if no duplicates
        public bool ChooseDeck(Card[] cards)
        {

            for (int i = 0; i < 4; ++i)
            {
                if (cards[i] != null)
                {
                    Deck.Add(cards[i]);
                }
                else
                {
                    Console.WriteLine("ERROR while picking deck");
                    return false;
                }
            }

            return true;
        }

        public void AddCardToDeck(Card card)
        {
            if (card != null)
                Deck.Add(card);
            else
                Console.WriteLine("ERROR while adding card to deck");
        }

        public void RemoveCardFromDeck(Card card)
        {
            if (Deck.Contains(card))
            {
                Deck.Remove(card);
            }
            else
            {
                Console.WriteLine("ERROR while removing card from deck");
            }
        }
    }
}
