using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace MTCG.Model.Cards
{
    
    public class MonsterCard : Card
    {
        public MonsterCard(double damage, ElementType element, CardType type)
        {
            this.damage = damage;
            this.element = element;
            this.type = type;
        }

        public override double CalcDamageMultiplier(Card opponentCard)
        {
            if(opponentCard.Type != CardType.Spell)
            {
                return 1;
            }
            switch (element)
            {
                case ElementType.Normal:
                    if (opponentCard.Element == ElementType.Water)
                        return 2;
                    else if (opponentCard.Element == ElementType.Fire)
                        return 0.5;

                    break;
                case ElementType.Fire:
                    if (opponentCard.Element == ElementType.Normal)
                        return 2;
                    else if (opponentCard.Element == ElementType.Water)
                        return 0.5;

                    break;
                case ElementType.Water:
                    if (opponentCard.Element == ElementType.Fire)
                        return 2;
                    else if (opponentCard.Element == ElementType.Normal)
                        return 0.5;

                    break;
            }

            return 1;
        }

        public override bool isImmuneToMonster(CardType opponentType)
        {
            switch (type)
            {
                case (CardType.Dragon):
                    if (opponentType == CardType.Goblin)
                        return true;
                    break;
                case (CardType.Wizard):
                    if (opponentType == CardType.Ork)
                        return true;
                    break;
                case (CardType.Elve):
                    if (element == ElementType.Fire && opponentType == CardType.Dragon)
                        return true;
                    break;
            }

            return false;
        }
        public override string ToString()
        {
            return "Type: " + type + "; Element: " + element + "; Damage: " + damage + "\n";
        }
    }
}
