using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace LegendQuest;

/// <summary>
/// The proof run's frame times: how long each frame's update and draw took, by the part of the run
/// and the screen on top, written to frame-times.txt when the run ends. It's how the run says
/// whether the game keeps up (a frame at 60 a second has 16.7 ms), and which screens stall.
/// </summary>
/// <remarks>
/// The run goes as fast as it can, not at 60 frames a second, so a frame's time here is the work it
/// took. Composing menu figures is synchronous in the run (PartCatalog.Synchronous) and is the
/// player's off the main thread, so the creation screen's slow frames here aren't the player's.
/// </remarks>
public partial class LegendQuestGame
{
    /// <summary>How long a frame may take at 60 frames a second.</summary>
    private const double FrameBudgetMs = 1000.0 / 60.0;

    /// <summary>How many of the slowest frames the report lists.</summary>
    private const int SlowestListed = 30;

    private readonly Stopwatch _frameWatch = new();

    /// <summary>What a frame spent its time on, noted by the code doing it (Note), in a proof run: the frame, what, and how long.</summary>
    private static readonly List<(int Frame, string What, double Ms)> Notes = new();
    private static int _notingFrame;
    private static bool _noting;

    /// <summary>In a proof run, notes how long something in this frame took, for frame-times.txt's breakdown of the slow frames.</summary>
    /// <param name="what">What it was.</param>
    /// <param name="ms">How long it took.</param>
    public static void Note(string what, double ms)
    {
        if (_noting) Notes.Add((_notingFrame, what, ms));
    }
    private readonly List<(int Frame, string Stage, string Screen, double Ms)> _frameTimes = new();

    /// <summary>Starts timing a frame, in a proof run.</summary>
    private void StartFrameTime()
    {
        if (ProofDirectory is null) return;
        _noting = true;
        _frameWatch.Restart();
    }

    /// <summary>Stops timing a frame once it's drawn, and keeps it.</summary>
    private void EndFrameTime()
    {
        if (ProofDirectory is null || !_frameWatch.IsRunning) return;
        _frameWatch.Stop();
        StateManagement.GameScreen[] screens = _screenManager.GetScreens();
        string top = screens.Length > 0 ? screens[^1].GetType().Name : "none";
        _frameTimes.Add((_proofFrame, _proofStage.ToString(), top, _frameWatch.Elapsed.TotalMilliseconds));
        _notingFrame = _proofFrame + 1;
    }

    /// <summary>Writes frame-times.txt: each part of the run's frames (how many, the typical, the slowest one in twenty, the slowest, how many over budget), then the slowest frames of all.</summary>
    private void WriteFrameTimes()
    {
        if (_frameTimes.Count == 0) return;
        var report = new StringBuilder();
        CultureInfo c = CultureInfo.InvariantCulture;
        report.AppendLine("Frame times in the proof run (update + draw, ms). A frame at 60 a second has " + FrameBudgetMs.ToString("0.0", c) + " ms.");
        report.AppendLine();
        report.AppendLine("part of the run           frames   median    p95      max   over budget");
        foreach (var stage in _frameTimes.GroupBy(f => f.Stage))
        {
            double[] ms = stage.Select(f => f.Ms).OrderBy(v => v).ToArray();
            int over = ms.Count(v => v > FrameBudgetMs);
            report.AppendLine(string.Format(c, "{0,-24} {1,7} {2,8:0.0} {3,8:0.0} {4,8:0.0} {5,8}", stage.Key, ms.Length,
                ms[ms.Length / 2], ms[(int)(ms.Length * 0.95)], ms[^1], over));
        }
        report.AppendLine();
        report.AppendLine("The slowest frames:");
        foreach (var f in _frameTimes.OrderByDescending(f => f.Ms).Take(SlowestListed))
        {
            report.AppendLine(string.Format(c, "  {0,8:0.0} ms  frame {1,6}  {2,-20} {3}", f.Ms, f.Frame, f.Stage, f.Screen));
            foreach (var n in Notes.Where(n => n.Frame == f.Frame))
                report.AppendLine(string.Format(c, "             {0,8:0.0} ms  {1}", n.Ms, n.What));
        }
        File.WriteAllText(Path.Combine(ProofDirectory, "frame-times.txt"), report.ToString());
    }
}
