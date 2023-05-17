using BarrageStrategoArchieBot;
using BarrageStrategoArchieBot.Model;
using Newtonsoft.Json;

var strategy = new RandomStrategy();

// bot-start
Console.WriteLine("bot-start");

// Read game init
var input = Console.ReadLine() ?? throw new Exception("No GameInit");
var init = JsonConvert.DeserializeObject<GameInit>(input) ?? throw new Exception("GameInit deserialization error");

// Write your initial board
Console.WriteLine(JsonConvert.SerializeObject(strategy.SetupBoard(init)));

while (true)
{
    // Read game state
    input = Console.ReadLine() ?? throw new Exception("No GameState");
    var state = JsonConvert.DeserializeObject<GameState>(input) ?? throw new Exception("Bad Deserialize GameState");
    var moveCmd = strategy.Move(state);
    // Skip if its not your turn
    if (moveCmd != null)
    {
        // Write move command
        Console.WriteLine(JsonConvert.SerializeObject(moveCmd));
    }
}