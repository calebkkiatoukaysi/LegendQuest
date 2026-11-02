using LegendQuest.Characters;

namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// The capital's shopkeepers: the Trader at the blue stall in the Lantern Market, the smith at
/// Halden's Forge, the Chandler down by the south canal, and the spice seller at the teal stall, who
/// keeps the apothecary's shelf while her sister's away.
/// </summary>
/// <remarks>
/// Mine, written round the bible: the Watch sent east, the eastern ships gone quiet, the Red Gulls
/// working the harbour. Each keeper has a first conversation, which sets a flag, and something
/// shorter to say every time after; either way, asking to see what they've got opens their shop
/// once the talking's done (DialogueEffect.Shop, a shop in ShopCatalog). The smith inside the forge
/// and the one outside its door share an id on their maps, so they share a conversation, and it's
/// written so either can say it.
/// </remarks>
public static class Shops
{
    private const string SoulBound = Origins.SoulBound;

    private const string TraderMet = "met-trader";
    private const string SmithMet = "met-smith";
    private const string ChandlerMet = "met-chandler";
    private const string ApothecaryMet = "met-apothecary";

    /// <summary>What each keeper's shop is in ShopCatalog.</summary>
    private const string LanternMarket = "lantern-market";
    private const string Forge = "forge";
    private const string Chandler = "chandler";
    private const string Apothecary = "apothecary";

    /// <summary>The Trader the first time: sizing you up, and the famous half price.</summary>
    public static readonly DialogueScript TraderHello = new()
    {
        Id = "shop-trader-hello",
        With = "trader",
        Unless = new[] { TraderMet },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = "trader", Line = "Travellers! I can always tell. Dusty boots, a blade apiece, and that look of being short of everything.",
                Effects = new[] { DialogueEffect.Set(TraderMet) },
            },
            new() { Speaker = SoulBound, Line = "Short of most things. Not opinions." },
            new()
            {
                Speaker = "trader", Line = "Then you've come to the right stall. Potions, gear for the road, a blade or two, and a few curios I'm not to say where I got. Everything's half price.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Let's have a look.", Effects = new[] { DialogueEffect.Shop(LanternMarket) }, Next = DialogueScript.End },
                    new() { Text = "Half of what?", Next = "half" },
                    new() { Text = "Another time.", Next = "later" },
                },
            },
            new()
            {
                Id = "half", Speaker = "trader", Direction = "beaming", Line = "Half of a higher price. It's called business, and I'm very good at it.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Let's have a look anyway.", Effects = new[] { DialogueEffect.Shop(LanternMarket) }, Next = DialogueScript.End },
                    new() { Text = "Another time.", Next = "later" },
                },
            },
            new() { Id = "later", Speaker = "trader", Line = "The stall's here from first bell to last lantern. So am I, more's the pity." },
        },
    };

    /// <summary>The Trader after that.</summary>
    public static readonly DialogueScript TraderAgain = new()
    {
        Id = "shop-trader",
        With = "trader",
        Needs = new[] { TraderMet },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = "trader", Line = "Back again! The prices missed you.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Let's have a look.", Effects = new[] { DialogueEffect.Shop(LanternMarket) }, Next = DialogueScript.End },
                    new() { Text = "Just passing.", Next = "passing" },
                },
            },
            new() { Id = "passing", Speaker = "trader", Line = "Pass slower next time. It's good for business." },
        },
    };

    /// <summary>The smith the first time: what the forge makes, what the Watch took east, and who Halden was.</summary>
    public static readonly DialogueScript SmithHello = new()
    {
        Id = "shop-smith-hello",
        With = "smith",
        Unless = new[] { SmithMet },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = "smith", Direction = "between hammer blows", Line = "Halden's Forge. If it cuts, guards, or stops an arrow, we made it, and we made it well.",
                Effects = new[] { DialogueEffect.Set(SmithMet) },
            },
            new()
            {
                Speaker = "smith", Line = "Half the Watch's blades went east with the Watch. What's left is good steel, and I don't haggle.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Show me the steel.", Effects = new[] { DialogueEffect.Shop(Forge) }, Next = DialogueScript.End },
                    new() { Text = "Who's Halden?", Next = "halden" },
                    new() { Text = "Another time.", Next = "later" },
                },
            },
            new()
            {
                Id = "halden", Speaker = "smith", Line = "My grandmother. She'd have liked you. She liked anyone who paid.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Show me the steel.", Effects = new[] { DialogueEffect.Shop(Forge) }, Next = DialogueScript.End },
                    new() { Text = "Another time.", Next = "later" },
                },
            },
            new() { Id = "later", Speaker = "smith", Line = "Mind your elbows on the way. Everything near a forge is hot." },
        },
    };

    /// <summary>The smith after that.</summary>
    public static readonly DialogueScript SmithAgain = new()
    {
        Id = "shop-smith",
        With = "smith",
        Needs = new[] { SmithMet },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = "smith", Line = "Something need an edge, or something need stopping?",
                Options = new DialogueOption[]
                {
                    new() { Text = "Show me the steel.", Effects = new[] { DialogueEffect.Shop(Forge) }, Next = DialogueScript.End },
                    new() { Text = "Just warming my hands.", Next = "warm" },
                },
            },
            new() { Id = "warm", Speaker = "smith", Line = "Warm them on your own time. I'm joking. Mostly." },
        },
    };

    /// <summary>The Chandler the first time: what a ship needs, and what landfolk buy instead.</summary>
    public static readonly DialogueScript ChandlerHello = new()
    {
        Id = "shop-chandler-hello",
        With = "shopkeeper",
        Unless = new[] { ChandlerMet },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = "shopkeeper", Line = "Welcome to the Chandler's. Rope, wax, lamp oil, and everything else a ship needs but the sea.",
                Effects = new[] { DialogueEffect.Set(ChandlerMet) },
            },
            new()
            {
                Speaker = "shopkeeper", Line = "Landfolk mostly want the boots and the cloaks. And the crossbows, for the gulls. The big gulls.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Let's see your shelves.", Effects = new[] { DialogueEffect.Shop(Chandler) }, Next = DialogueScript.End },
                    new() { Text = "Big gulls?", Next = "gulls" },
                    new() { Text = "Not today.", Next = "later" },
                },
            },
            new()
            {
                Id = "gulls", Speaker = "shopkeeper", Direction = "dropping their voice",
                Line = "Red ones. Two legs, bandanas, no manners. The Watch says they've gone quiet. The Watch says a lot of things.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Let's see your shelves.", Effects = new[] { DialogueEffect.Shop(Chandler) }, Next = DialogueScript.End },
                    new() { Text = "Not today.", Next = "later" },
                },
            },
            new() { Id = "later", Speaker = "shopkeeper", Line = "Fair winds, then. Or fair streets. Whatever it is landfolk say." },
        },
    };

    /// <summary>The Chandler after that.</summary>
    public static readonly DialogueScript ChandlerAgain = new()
    {
        Id = "shop-chandler",
        With = "shopkeeper",
        Needs = new[] { ChandlerMet },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = "shopkeeper", Line = "Fair winds! What'll it be?",
                Options = new DialogueOption[]
                {
                    new() { Text = "Let's see your shelves.", Effects = new[] { DialogueEffect.Shop(Chandler) }, Next = DialogueScript.End },
                    new() { Text = "Just looking.", Next = "looking" },
                },
            },
            new() { Id = "looking", Speaker = "shopkeeper", Line = "Look all you like. Looking's free. Touching's a copper." },
        },
    };

    /// <summary>The spice seller the first time: the remedies under the counter, and the sister they belong to.</summary>
    public static readonly DialogueScript ApothecaryHello = new()
    {
        Id = "shop-apothecary-hello",
        With = "spice-seller",
        Unless = new[] { ApothecaryMet },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = "spice-seller", Line = "Pepper, saffron, ember-salt! And under the counter, for them as asks nicely, remedies.",
                Effects = new[] { DialogueEffect.Set(ApothecaryMet) },
            },
            new()
            {
                Speaker = "spice-seller",
                Line = "My sister's the apothecary. She's at the river farms till the plums come in, so I keep her shelf: healing draughts, a clear-head elixir, and fire in a flask.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Show me the remedies.", Effects = new[] { DialogueEffect.Shop(Apothecary) }, Next = DialogueScript.End },
                    new() { Text = "Fire in a flask?", Next = "flask" },
                    new() { Text = "Just the pepper, thanks.", Next = "pepper" },
                },
            },
            new()
            {
                Id = "flask", Speaker = "spice-seller", Direction = "easing the lid back on a jar",
                Line = "Alchemist's fire. Throw it, don't drop it, and never, ever sniff it.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Show me the remedies.", Effects = new[] { DialogueEffect.Shop(Apothecary) }, Next = DialogueScript.End },
                    new() { Text = "I'll think about it.", Next = "think" },
                },
            },
            new() { Id = "pepper", Speaker = "spice-seller", Line = "Ha! Nobody ever wants just the pepper.", Next = DialogueScript.End },
            new() { Id = "think", Speaker = "spice-seller", Line = "Think quickly. The plums are late, but they're not that late." },
        },
    };

    /// <summary>The spice seller after that.</summary>
    public static readonly DialogueScript ApothecaryAgain = new()
    {
        Id = "shop-apothecary",
        With = "spice-seller",
        Needs = new[] { ApothecaryMet },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = "spice-seller", Line = "Spices out front, remedies under the counter. Which is it today?",
                Options = new DialogueOption[]
                {
                    new() { Text = "The remedies, please.", Effects = new[] { DialogueEffect.Shop(Apothecary) }, Next = DialogueScript.End },
                    new() { Text = "Neither, thanks.", Next = "neither" },
                },
            },
            new() { Id = "neither", Speaker = "spice-seller", Line = "Then mind the ember-salt on your way past. It bites." },
        },
    };
}
