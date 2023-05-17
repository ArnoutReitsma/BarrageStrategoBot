using BarrageStrategoArchieBot.Model;
using BarrageStrategoArchieBot.Model.Commands;

namespace BarrageStrategoArchieBot;

public abstract class Strategy
{
    protected Player Me = Player.Red;

    protected abstract SetupBoardCommand DoSetupBoard(GameInit init);

    protected abstract MoveCommand DoMove(GameState state);

    protected abstract void ProcessOpponentMove(GameState state);

    public SetupBoardCommand SetupBoard(GameInit init)
    {
        Me = init.You;
        return DoSetupBoard(init);
    }

    public MoveCommand? Move(GameState state)
    {
        if (state.ActivePlayer == Me)
        {
            MoveCommand moveResult = DoMove(state);
            if (moveResult == null)
            {
                throw new Exception("No move given!");
            }
            return moveResult;
        }
        else
        {
            ProcessOpponentMove(state);
            return null;
        }
    }
}