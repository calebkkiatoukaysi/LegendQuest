using System;
using LegendQuest;
using LegendQuest.Combat;

// Three dev switches. --simulate <encounter-id> [runs] fights an encounter headless with the AI on both
// sides, prints how it went, and quits without ever opening a window (--simulate all fights every
// one). --proof <dir> plays through the screens, saves a picture of each one into <dir>, and quits;
// --proof <dir> <part> stops after that part of the run (a ProofStage, like Docks). Anything else
// starts the game. --validate reads every zone and checks every conversation, shop and loot table, the
// way the game does as it starts, and quits: it prints what's wrong and exits 1, or exits 0.
if (args.Length > 1 && args[0] == "--simulate")
{
    CombatSimulator.Run(args[1], args.Length > 2 && int.TryParse(args[2], out int runs) ? runs : CombatSimulator.DefaultRuns);
    return;
}

if (args.Length > 0 && args[0] == "--validate")
{
    try
    {
        LegendQuestGame.LoadAndCheckData();
    }
    catch (Exception fault)
    {
        Console.Error.WriteLine(fault.Message);
        Environment.Exit(1);
    }
    Console.WriteLine("validate: every zone, conversation, shop and loot table checks out.");
    return;
}

bool proof = args.Length > 1 && args[0] == "--proof";
using var game = new LegendQuestGame
{
    ProofDirectory = proof ? args[1] : null,
    ProofLast = proof && args.Length > 2 && Enum.TryParse(args[2], out ProofStage last) ? last : ProofStage.Done,
};
game.Run();
