using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Model.Cards;

namespace MTCG.Model
{
    public class User
    {
        private Credentials credentials;
        private int coins = 20;
        private List<Card> stack = new List<Card>();
        private List<Card> deck = new List<Card>();
        public User(Credentials credentials)
        {
            this.credentials = credentials;
        }

        public string GetUsername()
        {
            return credentials.Username;
        }

        public List<Card> GetDeck()
        {
            return deck;
        }

        public List<Card> Stack { get; }

        public void AddCardToStack(Card card)
        {
            if(card != null)
                stack.Add(card);
        }

        //TODO: Check if cards in stack, check if no duplicates
        public bool ChooseDeck(Card[] cards){

            for (int i = 0; i < 4; ++i)
            {
                if (cards[i] != null)
                {
                    deck.Add(cards[i]);
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
                deck.Add(card);
            else
                Console.WriteLine("ERROR while adding card to deck");
        }

        public void RemoveCardFromDeck(Card card)
        {
            if(deck.Contains(card))
            {
                deck.Remove(card);
            }
            else
            {
                Console.WriteLine("ERROR while removing card from deck");
            }
        }
    }
}
