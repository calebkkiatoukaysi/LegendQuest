using System.Collections.Generic;
using System.IO;
using LegendQuest.Components.Realtime;

namespace LegendQuest;

/// <summary>
/// The proof run's Hud stage: the real-time HUD drawn from made-up views over a room, in each of
/// HudPreviewScreen's states, and its icons' contact sheet, a picture of each.
/// </summary>
/// <remarks>
/// Run on its own with <c>--proof &lt;dir&gt; Hud</c>: the run starts at this stage rather than the
/// menus, takes its pictures (hud-fight.png, hud-card.png, hud-reach.png, hud-boss.png,
/// hud-icons-1.png and on), writes what it checked of the HUD's sums to hud-checks.txt, and quits.
/// Each state gets a few frames to settle before its picture, so the bars' lags, the glint and the
/// pulses are part way through, as they'd be in play.
/// </remarks>
public partial class LegendQuestGame
{
    /// <summary>How many frames each state gets before its picture. The boss state's glint is started on the frame before, since the frames between pictures aren't evenly spaced in game time.</summary>
    private const int HudSettle = 20;

    /// <summary>The preview screen, once it's up.</summary>
    private HudPreviewScreen _hudPreview;

    /// <summary>The pictures still to take: each state, and which frame of it its picture is on.</summary>
    private int _hudStep;
    private int _hudFrames;

    /// <summary>Starts the run at the Hud stage when that's the part asked for.</summary>
    private void BeginHudProof()
    {
        if (ProofLast == ProofStage.Hud) _proofStage = ProofStage.Hud;
    }

    /// <summary>One frame of the Hud stage: opens the preview, then steps through its states and the icon pages, a picture of each.</summary>
    private string ProofHud()
    {
        if (_hudPreview is null)
        {
            _hudPreview = new HudPreviewScreen(HudPreviewScreen.Shot.Fight);
            _screenManager.AddScreen(_hudPreview);
            _hudFrames = 0;
            List<string> problems = _hudPreview.Check();
            Directory.CreateDirectory(ProofDirectory);
            File.WriteAllLines(Path.Combine(ProofDirectory, "hud-checks.txt"), problems.Count == 0 ? new[] { "Every check passed." } : problems);
            return null;
        }

        int pages = _hudPreview.Pages;
        (HudPreviewScreen.Shot shot, int page, string file, int settle)[] steps = Steps(pages);
        if (_hudStep >= steps.Length)
        {
            _proofStage = ProofStage.Done;
            return null;
        }

        (HudPreviewScreen.Shot state, int pageOf, string name, int wait) = steps[_hudStep];
        if (_hudFrames == 0) _hudPreview.Show(state, pageOf);
        if (state == HudPreviewScreen.Shot.Boss && _hudFrames == wait - 2) _hudPreview.CatchGlint();
        if (++_hudFrames < wait) return null;

        _hudStep++;
        _hudFrames = 0;
        return name;
    }

    /// <summary>The Hud stage's pictures, in order: each state, then each page of the icons.</summary>
    private static (HudPreviewScreen.Shot, int, string, int)[] Steps(int pages)
    {
        var steps = new (HudPreviewScreen.Shot, int, string, int)[4 + pages];
        steps[0] = (HudPreviewScreen.Shot.Fight, 0, "hud-fight.png", HudSettle);
        steps[1] = (HudPreviewScreen.Shot.Card, 0, "hud-card.png", HudSettle);
        steps[2] = (HudPreviewScreen.Shot.Reach, 0, "hud-reach.png", HudSettle);
        steps[3] = (HudPreviewScreen.Shot.Boss, 0, "hud-boss.png", HudSettle);
        for (int i = 0; i < pages; i++) steps[4 + i] = (HudPreviewScreen.Shot.Icons, i, $"hud-icons-{i + 1}.png", 4);
        return steps;
    }
}
