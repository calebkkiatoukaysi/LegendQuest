using LegendQuest;

// One dev switch so far. --proof <dir> plays through the screens, saves a picture of each one into
// <dir>, and quits. Anything else starts the game. (--simulate comes with combat.)
using var game = new LegendQuestGame
{
    ProofDirectory = args.Length > 1 && args[0] == "--proof" ? args[1] : null,
};
game.Run();
