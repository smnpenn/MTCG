using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace MTCG.Model.Cards
{

    public enum ElementType
    {
        Normal = 0,
        Fire = 1,
        Water = 2

    }

    public enum CardType
    {
        Spell = 0,
        Goblin = 1,
        Dragon = 2,
        Wizard = 3,
        Ork = 4,
        Knight = 5,
        Elve = 6,
        Troll = 7
    }

    public abstract class Card
    {
        protected double damage;
        protected ElementType element;
        protected CardType type;

        public double Damage { get { return damage; } }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ElementType Element { get { return element; } }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CardType Type { get { return type; } }

        public abstract bool isImmuneToMonster(CardType opponentType);

        public abstract double CalcDamageMultiplier(Card opponentCard);
    }
}