using BarrageStrategoCSharpBot;
using BarrageStrategoCSharpBot.Model;
using Newtonsoft.Json;

var strategy = new RandomStrategy();

// bot-start
Console.WriteLine("bot-start");

// Read game init
var input = Console.ReadLine();
var init = JsonConvert.DeserializeObject<GameInit>(input);

// Write your initial board
Console.WriteLine(JsonConvert.SerializeObject(strategy.SetupBoard(init)));

while (true)
{
    // Read game state
    input = Console.ReadLine();
    var state = JsonConvert.DeserializeObject<GameState>(input);
    var moveCmd = strategy.Move(state);
    // Skip if its not your turn
    if (moveCmd != null)
    {
        Console.WriteLine(JsonConvert.SerializeObject(moveCmd));
    }
}