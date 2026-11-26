using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.StateManagement;

namespace LegendQuest;

/// <summary>
/// The proof run's Sprites part (--proof dir Sprites): contact sheets of the cast's figures in both
/// looks, so the pixel art can be judged by eye against the painting it was made from.
/// </summary>
/// <remarks>
/// It starts straight away, like the Moves part, and doesn't put everyone together first: only the
/// figures on the sheets are made. pixel/lineup.png is the main cast at the size they stand in a
/// room, standing painted, then as pixel art standing, walking and striking. pixel/(id).png is one
/// of them four times as big as an art pixel is in a room: painted and pixel side by side, then the
/// pixel figure walking, jogging, ready, winding up, striking, hurt and down. Each figure is really
/// posed by its clip (a WalkerSprite stepped to the time), so a sheet shows what the game draws.
/// </remarks>
public partial class LegendQuestGame
{
    /// <summary>The screen the Sprites part films.</summary>
    private SpritesProof _spritesProof;

    /// <summary>One frame of the Sprites part: puts its screen up the first time, then names each sheet to save, until they're done.</summary>
    private string ProofSprites()
    {
        if (_spritesProof is null)
        {
            foreach (GameScreen screen in _screenManager.GetScreens()) _screenManager.RemoveScreen(screen);
            _spritesProof = new SpritesProof();
            _screenManager.AddScreen(_spritesProof);
            return null;
        }
        string shot = _spritesProof.Step();
        if (_spritesProof.Done) _proofStage = ProofStage.Done;
        return shot is null ? null : Path.Combine("pixel", shot);
    }

    /// <summary>The sheets, one a few frames, each a grid of figures posed by their clips.</summary>
    private sealed class SpritesProof : GameScreen
    {
        /// <summary>Who's on the sheets: the two player bases, the Soul-Bound, the companions and the opening's cast, two foes and three townsfolk.</summary>
        private static readonly string[] Cast =
        {
            "you-male", "you-female", "soulbound", "brannock", "sera", "mira", "kael", "corra", "tomas", "grell",
            "hollowed-footman", "bandit", "smith", "flower-seller", "pip",
        };

        /// <summary>The paper behind the figures: a room's middle tone and a darker one, so the outlines are judged against what they'll stand on.</summary>
        private static readonly Color Paper = new(132, 150, 128), PaperDark = new(84, 96, 104);

        /// <summary>How many frames a sheet is up before its picture is taken.</summary>
        private const int Settle = 3;

        /// <summary>One figure on a sheet: who, in which look, in which clip (null for standing) how far in, and where its feet are, in world pixels.</summary>
        private sealed record Cell(string Id, ArtLook Look, string Clip, float Time, Vector2 Feet, string Label)
        {
            public WalkerSprite Walker;
        }

        private readonly List<(string Name, int Zoom, List<Cell> Cells)> _sheets = new();
        private ContentManager _content;
        private SpriteFont _font;
        private Texture2D _pixel;
        private int _sheet = -1, _frame;

        /// <summary>Whether every sheet's been filmed.</summary>
        public bool Done { get; private set; }

        public override void Activate()
        {
            _content = new ContentManager(ScreenManager.Game.Services, "Content");
            _font = _content.Load<SpriteFont>("spectral-ui");
            _pixel = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            Plan();
            string only = Environment.GetEnvironmentVariable("LQ_SPRITES_ONLY");
            if (!string.IsNullOrEmpty(only)) _sheets.RemoveAll(sheet => !sheet.Name.Contains(only, StringComparison.Ordinal));
        }

        public override void Unload()
        {
            _pixel?.Dispose();
            _content?.Unload();
        }

        /// <summary>Lays the sheets out.</summary>
        private void Plan()
        {
            // The lineup, at the size they stand in a room.
            const int zoom = 4;
            var lineup = new List<Cell>();
            float column = ScreenWidth / (float)Cast.Length / zoom, row = ScreenHeight / 4f / zoom;
            for (int i = 0; i < Cast.Length; i++)
            {
                float x = (i + 0.45f) * column;
                lineup.Add(new Cell(Cast[i], ArtLook.Painted, null, 0.5f, new Vector2(x, row - 3f), null));
                lineup.Add(new Cell(Cast[i], ArtLook.Pixel, null, 0.5f, new Vector2(x, row * 2f - 3f), null));
                lineup.Add(new Cell(Cast[i], ArtLook.Pixel, "walk", 0.35f, new Vector2(x, row * 3f - 3f), null));
                lineup.Add(new Cell(Cast[i], ArtLook.Pixel, "auto", 0.2f, new Vector2(x, row * 4f - 3f), null));
            }
            _sheets.Add(("lineup", zoom, lineup));

            // Each of them close up.
            foreach (string id in Cast)
            {
                const int close = 8;
                float wide = ScreenWidth / 5f / close, tall = (ScreenHeight - 20f) / 2f / close;
                (ArtLook Look, string Clip, float Time, string Label)[] poses =
                {
                    (ArtLook.Painted, null, 0.5f, "painted"), (ArtLook.Pixel, null, 0.5f, "pixel"), (ArtLook.Pixel, "walk", 0.25f, "walk"),
                    (ArtLook.Pixel, "walk", 0.6f, "walk"), (ArtLook.Pixel, "jog", 0.3f, "jog"),
                    (ArtLook.Pixel, "ready", 0.5f, "ready"), (ArtLook.Pixel, "auto", 0.1f, "wind up"), (ArtLook.Pixel, "auto", 0.2f, "strike"),
                    (ArtLook.Pixel, "hurt", 0.12f, "hurt"), (ArtLook.Pixel, "die", 2f, "down"),
                };
                var cells = new List<Cell>();
                for (int i = 0; i < poses.Length; i++)
                {
                    var feet = new Vector2((i % 5 + (i == 9 ? 0.2f : 0.45f)) * wide, (i / 5 + 1) * tall - 3f);
                    cells.Add(new Cell(id, poses[i].Look, poses[i].Clip, poses[i].Time, feet, poses[i].Label));
                }
                _sheets.Add((id, close, cells));
            }
        }

        /// <summary>Moves on a frame, and names the sheet to save once it's settled.</summary>
        public string Step()
        {
            if (Done) return null;
            if (_sheet < 0 || _frame > Settle)
            {
                _sheet++;
                _frame = 0;
                if (_sheet >= _sheets.Count)
                {
                    Done = true;
                    return null;
                }
                foreach (Cell cell in _sheets[_sheet].Cells) Pose(cell);
            }
            _frame++;
            return _frame == Settle ? _sheets[_sheet].Name + ".png" : null;
        }

        /// <summary>Makes a cell's figure and steps its clip to the cell's time.</summary>
        private void Pose(Cell cell)
        {
            var walker = new WalkerSprite { Art = ImportedArt.Get(cell.Id), Facing = Direction.Right, Position = cell.Feet };
            walker.LoadContent(_content);
            cell.Walker = walker;
            if (cell.Clip is string clip)
            {
                walker.InFight = clip != "walk" && clip != "jog";
                if (clip == "auto") clip = "auto-" + walker.Weapon + "-1";
                if (walker.PlayMove(clip, hold: true) is null) walker.PlayMove("auto-one-hand-1", hold: true);
            }
            for (int i = 0; i < (int)MathF.Round(cell.Time * 60f) + 1; i++) walker.Update(1f / 60f);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_sheet < 0 || _sheet >= _sheets.Count) return;
            (string name, int zoom, List<Cell> cells) = _sheets[_sheet];
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            ArtLook was = ArtSettings.Look;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            spriteBatch.Draw(_pixel, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Paper);
            spriteBatch.Draw(_pixel, new Rectangle(ScreenWidth * 4 / 5, 0, ScreenWidth / 5, ScreenHeight), PaperDark);
            spriteBatch.End();

            foreach (ArtLook look in new[] { ArtLook.Painted, ArtLook.Pixel })
            {
                ArtSettings.Look = look;
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(zoom, zoom, 1f));
                foreach (Cell cell in cells)
                    if (cell.Look == look) cell.Walker?.Draw(spriteBatch, 0.5f, 1f / zoom, Color.White);
                spriteBatch.End();
            }
            ArtSettings.Look = was;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
            foreach (Cell cell in cells)
                if (cell.Label is not null)
                    Text.Draw(spriteBatch, _font, cell.Label.ToUpperInvariant(), cell.Feet.X * zoom, cell.Feet.Y * zoom + 4f, Palette.Bone, Palette.Shadow, centred: true, width: 110f);
            Text.Draw(spriteBatch, _font, name.ToUpperInvariant(), 12f, 8f, Palette.Bone, Palette.Shadow);
            spriteBatch.End();
        }
    }
}
