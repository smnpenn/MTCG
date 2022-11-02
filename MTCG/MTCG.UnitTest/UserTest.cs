using MTCG.BL;
using MTCG.Model;
using MTCG.Model.Cards;

namespace MTCG.UnitTest
{
    [TestClass]
    public class UserTest
    {
        Card goblin = new MonsterCard("FireGoblin", 10, ElementType.Fire, CardType.Goblin);
        Card dragon = new MonsterCard("Dragon", 5, ElementType.Normal, CardType.Dragon);
        Card wizard = new MonsterCard("WaterWizard", 25, ElementType.Water, CardType.Wizard);
        Card ork = new MonsterCard("Ork", 20, ElementType.Normal, CardType.Ork);
        Card knight = new MonsterCard("Knight", 20, ElementType.Normal, CardType.Knight);
        Card elve = new MonsterCard("FireElve", 25, ElementType.Fire, CardType.Elve);
        Card troll = new MonsterCard("WaterTroll", 30, ElementType.Water, CardType.Troll);

        Card waterSpell = new SpellCard("WaterSpell", 20, ElementType.Water);
        Card fireSpell = new SpellCard("FireSpell", 20, ElementType.Fire);
        Card normalSpell = new SpellCard("NormalSpell", 20, ElementType.Normal);

        public Battle instantiateBattle()
        {
            Credentials credentials1 = new Credentials();
            credentials1.Username = "Simon";
            credentials1.Password = "123";
            User user1 = new User(credentials1);

            Credentials credentials2 = new Credentials();
            credentials2.Username = "Max";
            credentials2.Password = "123";
            User user2 = new User(credentials2);

            BL.Battle battle = new BL.Battle(user1, user2);

            return battle;
        }

        [TestMethod]
        public void TestBattleLogic_GoblinVSOrk()
        {
            Battle battle = instantiateBattle();
            Assert.AreEqual(battle.compareCards(goblin, ork), 2);
        }

        [TestMethod]
        public void TestBattleLogic_DragonVSGoblin()
        {
            Battle battle = instantiateBattle();
            Assert.AreEqual(battle.compareCards(dragon, goblin), 1);
        }

        [TestMethod]
        public void TestBattleLogic_FireElveVSDragon()
        {
            Battle battle = instantiateBattle();
            Assert.AreEqual(battle.compareCards(dragon, elve), 2);
        }

        [TestMethod]
        public void TestBattleLogic_KnightVSKnight()
        {
            Battle battle = instantiateBattle();
            Assert.AreEqual(battle.compareCards(knight, knight), 0);
        }

        [TestMethod]
        public void TestBattleLogic_WaterSpellVSFireSpell()
        {
            Battle battle = instantiateBattle();
            Assert.AreEqual(battle.compareCards(waterSpell, fireSpell), 1);
        }

        [TestMethod]
        public void TestBattleLogic_NormalSpellVSFireSpell()
        {
            Battle battle = instantiateBattle();
            Assert.AreEqual(battle.compareCards(normalSpell, fireSpell), 2);
        }

        [TestMethod]
        public void TestBattleLogic_NormalSpellVSWaterSpell()
        {
            Battle battle = instantiateBattle();
            Assert.AreEqual(battle.compareCards(normalSpell, waterSpell), 1);
        }

        [TestMethod]
        public void TestGetCredentials()
        {
            //useless test
            MTCG.Model.Credentials credentials = new Credentials();
            credentials.Username = "Simon";
            credentials.Password = "123";
            MTCG.Model.User user = new User(credentials);

            string res = user.GetUsername();
            Assert.AreEqual(res, "Simon");
        }
    }
}