using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection.Metadata;
using MTCG.Model.Cards;
using MTCG.Model.Users;

namespace MTCG.BL.Battle
{
    public class Battle
    {
        Player player1;
        Player player2;
        Random random = new Random();

        public string Log
        {
            get;
            private set;
        }

        public Battle(Player player1, Player player2)
        {
            this.player1 = player1;
            this.player2 = player2;
            Log = "";
        }

        public void executeBattle()
        {
            int count = 1;
            while (player1.Deck.Count != 0 && player2.Deck.Count != 0 && count < 100)
            {
                Log = Log + "\nRound " + count;
                Log = Log + $"\n{player1.Name} Cards: {player1.Deck.Count}";
                Log = Log + $"\n{player2.Name} Cards: {player2.Deck.Count}";

                Card card1 = player1.Deck[random.Next(0, player1.Deck.Count)];
                Card card2 = player2.Deck[random.Next(0, player2.Deck.Count)];
                int result = compareCards(card1, card2);

                if (result == 1)
                {
                    player2.RemoveCardFromDeck(card2);
                    player1.AddCardToDeck(card2);
                    Log = Log + $"\n{card1.Element}{card1.Type} ({player1.Name}) wins!";
                }
                else if (result == 2)
                {
                    player1.RemoveCardFromDeck(card1);
                    player2.AddCardToDeck(card1);
                    Log = Log + $"\n{card2.Element}{card2.Type} ({player2.Name}) wins!";
                }
                else
                {
                    Log = Log + "\nDraw";
                }

                Log = Log + "\n";
                ++count;
            }
        }

        public int compareCards(Card card1, Card card2)
        {
            double damageCard1 = card1.Damage * card1.CalcDamageMultiplier(card2);
            double damageCard2 = card2.Damage * card2.CalcDamageMultiplier(card1);

            Log = Log + $"\n{card1.Element}{card1.Type} ({card1.Damage}) vs {card2.Element}{card2.Type} ({card2.Damage})";
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