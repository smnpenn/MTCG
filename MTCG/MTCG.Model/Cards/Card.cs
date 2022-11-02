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
        protected int damage;
        protected string name;
        protected ElementType element;
        protected CardType type;

        public int Damage { get { return damage; } }
        public string Name { get { return name; } }
        public ElementType Element { get { return element; } }
        public CardType Type { get { return type; } }

        public abstract bool isImmuneToMonster(CardType opponentType);

        public abstract double CalcDamageMultiplier(Card opponentCard);
    }
}