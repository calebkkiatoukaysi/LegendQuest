using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The fields: foes who keep a wild room once the story's past them (EncounterCatalog.Field), with
/// red names over their heads, a click on one to fight them, the aggressive ones coming at you when
/// you come near, and the room filling again a minute after they're beaten or when you come back.
/// </summary>
/// <remarks>
/// <para>
/// Story/11-combat.md §2.1's other two ways into a fight. A field's foes are npcs of the room, the
/// zone file's or ones EncounterCatalog puts down, so the fight that starts is the ordinary one: it
/// starts where everyone stands, you target the foe you clicked (or the one that came at you) and
/// your auto attack walks you into its reach. Once it's won its foes are gone from the room for
/// EncounterCatalog.FieldRespawnSeconds of play; leaving the room and coming back fills it at once,
/// the way the fields of the game this borrows the loop from do.
/// </para>
/// <para>
/// An aggressive field doesn't come at you in the first moments after you come into its room or a
/// fight ends, so a room that's just filled again doesn't pull you straight back in; nor while a
/// conversation's up. The proof run's story visits walk past the fields without being pulled in
/// (they're scripted to the frame); its Arena stage turns the pull on (ProofFieldAggro).
/// </para>
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How long after you come into a room, or a fight ends, before an aggressive field comes at you, in seconds.</summary>
    private const float FieldCalmSeconds = 2f;

    /// <summary>Each field beaten in this room while you've been in it, by its fight's id, and the play time it was beaten at.</summary>
    private readonly Dictionary<string, double> _fieldCleared = new();

    /// <summary>How much longer the room's aggressive fields leave you be, in seconds.</summary>
    private float _fieldCalm;

    /// <summary>Whether the proof run lets an aggressive field come at the party. Off for the story's visits, which are scripted to the frame; the Arena stage turns it on.</summary>
    internal static bool ProofFieldAggro { get; set; }

    /// <summary>Puts down the foes the room's fields bring that the zone file hasn't got, and forgets what was beaten here before: coming into a room fills its fields again.</summary>
    /// <remarks>EnterRoom calls it once the zone file's people are in _everyNpc and before RefreshPresence.</remarks>
    private void PlaceField()
    {
        _fieldCleared.Clear();
        _fieldCalm = FieldCalmSeconds;
        foreach (FieldGround ground in EncounterCatalog.GroundsIn(_zone.Id, _room.Id))
        {
            EncounterDefinition fight = EncounterCatalog.Get(ground.Encounter);
            foreach (FieldFoe foe in ground.Foes)
            {
                if (foe.At is not Vector2 at) continue;
                EnemyDefinition enemy = EnemyCatalog.Get(fight.Foes.First(line => line.Npc == foe.Npc).Enemy);
                var placed = new MapNpc(foe.Npc, enemy.Name, at / ZoneCatalog.RoomScale, Direction.Left, null, enemy.Creature is null ? enemy.Cast : null, null,
                    enemy.Creature, System.Array.Empty<string>(), null, 0f, ground.Presence);
                _everyNpc.Add(Dressed(new Npc(placed, _walk)));
            }
        }
    }

    /// <summary>The field someone in the room stands in, or null if they're nobody's foe.</summary>
    /// <param name="npc">Who.</param>
    private FieldGround FieldOf(Npc npc) => EncounterCatalog.GroundOf(_zone.Id, _room.Id, npc.Definition.Id);

    /// <summary>Whether someone in the room is a field's foe you can fight now: there, and no fight on.</summary>
    /// <param name="npc">Who.</param>
    private bool Hostile(Npc npc) => !InCombat && _npcs.Contains(npc) && FieldOf(npc) is not null;

    /// <summary>Whether a field's foe is away from the room for now, beaten here less than a minute of play ago.</summary>
    /// <param name="npc">Who.</param>
    private bool Resting(Npc npc) => FieldOf(npc) is FieldGround ground && _fieldCleared.ContainsKey(ground.Encounter);

    /// <summary>Starts a field's fight on one of its foes: the fight where everyone stands, that foe your target, and your auto attack on, which walks you into its reach.</summary>
    /// <param name="npc">The foe clicked, or the one that came at you.</param>
    private void Engage(Npc npc)
    {
        if (InCombat || _talking || FieldOf(npc) is not FieldGround ground) return;
        StopPath();
        StartFight(EncounterCatalog.Get(ground.Encounter));
        if (_battle?.Leader is not Fighter leader || _battle.Find(npc.Definition.Id) is not Fighter foe) return;
        _battle.Command(BattleCommand.Target(leader.Id, foe.Id));
        _battle.Command(BattleCommand.AutoAttack(leader.Id, true));
    }

    /// <summary>Runs the fields a frame on: a beaten one back once its minute's up, and an aggressive one coming at you if you're near enough.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <returns>True if a fight started.</returns>
    private bool UpdateField(GameTime gameTime)
    {
        _fieldCalm = System.MathF.Max(0f, _fieldCalm - (float)gameTime.ElapsedGameTime.TotalSeconds);

        if (_fieldCleared.Count > 0)
        {
            int before = _fieldCleared.Count;
            foreach (string id in _fieldCleared.Where(each => _game.PlaytimeSeconds - each.Value >= EncounterCatalog.FieldRespawnSeconds).Select(each => each.Key).ToList())
                _fieldCleared.Remove(id);
            if (_fieldCleared.Count != before)
            {
                RefreshPresence();
                _fieldCalm = FieldCalmSeconds;
            }
        }

        if (_fieldCalm > 0f || _talking || ProofSeed is not null && !ProofFieldAggro) return false;
        foreach (Npc npc in _npcs)
        {
            if (FieldOf(npc) is not { Aggressive: true } || Vector2.Distance(npc.Walker.Position, Leader.Position) > EncounterCatalog.AggroReach) continue;
            Engage(npc);
            return InCombat;
        }
        return false;
    }

    /// <summary>Notes a field fight that's over and won: its foes leave the room for a minute of play (or until you come back into it) rather than for the visit, as the dead of a story fight do, and go back to their own spots, so the field fills again where it stood and not on top of where you won.</summary>
    /// <remarks>EndFight calls it before working out who's in the room again. A field fight lost ends at the game-over screen, which loads the save from before it.</remarks>
    private void ClearedField()
    {
        _fieldCalm = FieldCalmSeconds;
        if (_encounter?.Kind != EncounterKind.Random) return;
        foreach (FieldGround ground in EncounterCatalog.GroundsIn(_zone.Id, _room.Id).Where(each => each.Encounter == _encounter.Id))
        {
            foreach (Npc npc in _everyNpc)
            {
                if (FieldOf(npc)?.Encounter != ground.Encounter) continue;
                npc.Walker.Release();
                npc.Walker.Position = npc.Definition.Feet;
                npc.Walker.Facing = npc.Definition.Facing;
                npc.Walker.IsWalking = false;
                npc.Walker.Opacity = 1f;
            }
            _fieldCleared[ground.Encounter] = _game.PlaytimeSeconds;
            foreach (FieldFoe foe in ground.Foes) _fallen.Remove($"{_room.Id}/{foe.Npc}");
        }
    }
}
