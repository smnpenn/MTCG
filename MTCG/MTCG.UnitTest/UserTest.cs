using MTCG.BL;
using MTCG.Model.Cards;
using MTCG.Model.Users;

namespace MTCG.UnitTest
{
    [TestClass]
    public class UserTest
    {
        Card goblin = new MonsterCard(10, ElementType.Fire, CardType.Goblin);
        Card dragon = new MonsterCard(5, ElementType.Normal, CardType.Dragon);
        Card wizard = new MonsterCard(25, ElementType.Water, CardType.Wizard);
        Card ork = new MonsterCard(20, ElementType.Normal, CardType.Ork);
        Card knight = new MonsterCard(20, ElementType.Normal, CardType.Knight);
        Card elve = new MonsterCard(25, ElementType.Fire, CardType.Elve);
        Card troll = new MonsterCard(30, ElementType.Water, CardType.Troll);

        Card waterSpell = new SpellCard(20, ElementType.Water);
        Card fireSpell = new SpellCard(20, ElementType.Fire);
        Card normalSpell = new SpellCard(20, ElementType.Normal);

        public Battle instantiateBattle()
        {
            UserCredentials credentials1 = new UserCredentials("Simon", "123");
            User user1 = new User(credentials1);

            UserCredentials credentials2 = new UserCredentials("Max", "123");
            User user2 = new User(credentials2);

            BL.Battle battle = new Battle(user1, user2);

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
            UserCredentials credentials = new UserCredentials("Simon", "123");
            User user = new User(credentials);

            string res = user.Credentials.Username;
            Assert.AreEqual(res, "Simon");
        }
    }
}