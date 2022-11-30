using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection.Metadata;
using MTCG.Model.Cards;
using MTCG.Model.Users;

namespace MTCG.BL
{
    public class Battle
    {
        User player1;
        User player2;
        Random random = new Random();

        public Battle(User player1, User player2)
        {
            this.player1 = player1;
            this.player2 = player2;
        }

        public void executeBattle()
        {
            int count = 0;
            while(player1.Deck.Count != 0 && player2.Deck.Count != 0 && count < 100)
            {
                Card card1 = player1.Deck[random.Next(0, player1.Deck.Count)];
                Card card2 = player2.Deck[random.Next(0, player2.Deck.Count)];
                int result = compareCards(card1, card2);

                if(result == 1)
                {
                    Console.WriteLine("Card 1 Wins!");
                    player2.RemoveCardFromDeck(card2);
                    player1.AddCardToDeck(card2);
                }else if(result == 2)
                {
                    player1.RemoveCardFromDeck(card1);
                    player2.AddCardToDeck(card1);
                    Console.WriteLine("Card 2 Wins");
                }
                else
                {
                    Console.WriteLine("Draw!");
                }

                Console.WriteLine("\nRound " + count);
                Console.WriteLine($"Player 1 Deck Count {player1.Deck.Count}");
                Console.WriteLine($"Player 2 Deck Count {player2.Deck.Count}\n");

                ++count;
            }
        }

        public int compareCards(Card card1, Card card2)
        {
            double damageCard1 = card1.Damage * card1.CalcDamageMultiplier(card2);
            double damageCard2 = card2.Damage * card2.CalcDamageMultiplier(card1);

            Console.WriteLine($"{card1.Element}{card1.Type} ({card1.Damage}) vs {card2.Element}{card2.Type} ({card2.Damage})");
            Console.WriteLine($"Actual damage: {damageCard1} vs {damageCard2}");
            if (damageCard1 > damageCard2 || card1.isImmuneToMonster(card2.Type))
            {
                return 1;
            }
            else if (damageCard1 < damageCard2 || card2.isImmuneToMonster(card1.Type))
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }
    }
}