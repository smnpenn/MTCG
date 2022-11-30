using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Model.Cards
{
    public class SpellCard : Card
    {
        public SpellCard(int damage, ElementType element)
        {
            this.damage = damage;
            this.element = element;
            this.type = CardType.Spell;
        }

        public override double CalcDamageMultiplier(Card opponentCard)
        {
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
            return false;
        }
    }
}
