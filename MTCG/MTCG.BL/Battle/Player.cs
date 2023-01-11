using MTCG.Model.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Model.Users
{
    public class Player
    {
        public List<Card> Deck
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public Player(List<Card> deck, string name)
        {
            Deck = deck;
            Name = name;
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
