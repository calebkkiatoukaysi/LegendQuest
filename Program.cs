using System;
using LegendQuest;

// One dev switch so far. --proof <dir> plays through the screens, saves a picture of each one into
// <dir>, and quits; --proof <dir> <part> stops after that part of the run (a ProofStage, like Docks).
// Anything else starts the game. (--simulate comes with combat.)
bool proof = args.Length > 1 && args[0] == "--proof";
using var game = new LegendQuestGame
{
    ProofDirectory = proof ? args[1] : null,
    ProofLast = proof && args.Length > 2 && Enum.TryParse(args[2], out ProofStage last) ? last : ProofStage.Done,
};
game.Run();
