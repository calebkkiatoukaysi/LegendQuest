using System;
using LegendQuest;
using LegendQuest.Combat;

// Three dev switches. --simulate <encounter-id> [runs] [story|balanced|tactician] fights an encounter
// headless with the AI on both sides, prints how it went against its targets, and quits without ever
// opening a window, exiting 1 if anything failed (--simulate all fights every one; Balanced if no
// difficulty's named, and Tactician's verdicts are only for information). --proof <dir> plays
// through the screens, saves a picture of each one into <dir>, and quits;
// --proof <dir> <part> stops after that part of the run (a ProofStage, like Docks). Anything else
// starts the game. --validate reads every zone and checks every conversation, shop and loot table and
// every imported character's picture, the way the game does as it starts, and quits: it prints what's
// wrong and exits 1, or exits 0.
if (args.Length > 1 && args[0] == "--simulate")
{
    int runs = CombatSimulator.DefaultRuns;
    Difficulty difficulty = Difficulty.Balanced;
    for (int i = 2; i < args.Length; i++)
    {
        bool number = int.TryParse(args[i], out int count);
        if (number && count > 0) runs = count;
        else if (!number && Enum.TryParse(args[i], true, out Difficulty named) && Enum.IsDefined(named)) difficulty = named;
        else
        {
            Console.Error.WriteLine($"--simulate: \"{args[i]}\" is neither a number of runs nor story, balanced or tactician.");
            Environment.ExitCode = 1;
            return;
        }
    }
    if (!CombatSimulator.Run(args[1], runs, difficulty)) Environment.ExitCode = 1;
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
    Console.WriteLine("validate: every zone, conversation, shop, loot table, imported picture and character part checks out.");
    return;
}

bool proof = args.Length > 1 && args[0] == "--proof";
using var game = new LegendQuestGame
{
    ProofDirectory = proof ? args[1] : null,
    ProofLast = proof && args.Length > 2 && Enum.TryParse(args[2], out ProofStage last) ? last : ProofStage.Done,
};
game.Run();
