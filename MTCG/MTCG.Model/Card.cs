namespace MTCG.Model
{

    public enum ElementType
    {
        normal = 0,
        fire = 1,
        water = 2

    }

    public class Card
    {
        private int damage;
        private string name;
        private ElementType element;


        public Card(string name, int damage, ElementType element)
        {
            this.name = name;
            this.damage = damage;
            this.element = element;
        }
        public int Damage { get; }
    }
}