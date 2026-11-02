using System;
using LegendQuest;
using LegendQuest.Combat;

// Two dev switches. --simulate <encounter-id> [runs] fights an encounter headless with the AI on both
// sides, prints how it went, and quits without ever opening a window (--simulate all fights every
// one). --proof <dir> plays through the screens, saves a picture of each one into <dir>, and quits;
// --proof <dir> <part> stops after that part of the run (a ProofStage, like Docks). Anything else
// starts the game.
if (args.Length > 1 && args[0] == "--simulate")
{
    CombatSimulator.Run(args[1], args.Length > 2 && int.TryParse(args[2], out int runs) ? runs : CombatSimulator.DefaultRuns);
    return;
}

bool proof = args.Length > 1 && args[0] == "--proof";
using var game = new LegendQuestGame
{
    ProofDirectory = proof ? args[1] : null,
    ProofLast = proof && args.Length > 2 && Enum.TryParse(args[2], out ProofStage last) ? last : ProofStage.Done,
};
game.Run();
