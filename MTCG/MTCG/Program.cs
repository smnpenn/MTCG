
using MTCG;
using MTCG.BL;
using MTCG.DAL;
using MTCG.Model;
using MTCG.Model.Cards;
using MTCG.Model.Users;

HttpServer requestHandler = new HttpServer("http://localhost:10001/");
requestHandler.StartServer();

/*Card[] cards = new Card[4];

cards[0] = new MonsterCard(35, ElementType.Fire, CardType.Ork);
cards[1] = new SpellCard(30, ElementType.Water);
cards[2] = new MonsterCard(15, ElementType.Normal, CardType.Knight);
cards[3] = new MonsterCard(30, ElementType.Normal, CardType.Dragon);

Credentials credentials1 = new Credentials();
credentials1.Username = "simon";
credentials1.Password = "1";

Credentials credentials2 = new Credentials();
credentials2.Username = "max";
credentials2.Password = "2";

User player1 = new User(credentials1);
User player2 = new User(credentials2);

player1.ChooseDeck(cards);
player2.ChooseDeck(cards);

Battle battle = new Battle(player1, player2);

battle.executeBattle();
*/
