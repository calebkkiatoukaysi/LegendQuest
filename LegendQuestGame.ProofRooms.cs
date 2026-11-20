using System;
using System.Collections.Generic;
using System.Linq;
using LegendQuest.Characters;
using LegendQuest.StateManagement;
using LegendQuest.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest;

/// <summary>The Rooms proof renders every room through the game's own scene loader, with a figure for scale.</summary>
public partial class LegendQuestGame
{
    private RoomGallery _roomGallery;

    private string ProofRooms()
    {
        if (_roomGallery is null)
        {
            ClearScreens();
            _roomGallery = new RoomGallery();
            _screenManager.AddScreen(_roomGallery);
            return null;
        }
        string shot = _roomGallery.Step();
        if (_roomGallery.Done) _proofStage = ProofStage.Done;
        return shot;
    }

    private sealed class RoomGallery : GameScreen
    {
        private readonly Camera2D _camera = new(new Point(ScreenWidth, ScreenHeight));
        private readonly List<(string Zone, RoomDefinition Room, float Centre)> _views = new();
        private ContentManager _content;
        private SpriteFont _font;
        private WalkerSprite _figure;
        private RoomScene _scene;
        private RoomDefinition _room;
        private string _caption;
        private int _index, _frames;
        public bool Done { get; private set; }

        public override void Activate()
        {
            _content = new ContentManager(ScreenManager.Game.Services, "Content");
            _font = _content.Load<SpriteFont>("spectral-ui");
            _figure = new WalkerSprite { Art = ImportedArt.For(Origins.Create("brannock")) };
            _figure.LoadContent(_content);
            foreach (ZoneDefinition zone in ZoneCatalog.All)
                foreach (RoomDefinition room in zone.Rooms)
                    foreach (float centre in new[] { 200f, room.Width / 2f, room.Width - 200f }.Distinct())
                        _views.Add((zone.Id, room, centre));
        }

        public string Step()
        {
            if (_index >= _views.Count) { Done = true; return null; }
            (string zone, RoomDefinition room, float centre) = _views[_index];
            if (_frames == 0)
            {
                if (_room != room)
                {
                    _scene?.Unload();
                    _room = room;
                    _scene = new RoomScene(room);
                    _scene.LoadContent(ScreenManager.Game.Services, ScreenManager.GraphicsDevice);
                    _camera.LockTo(_scene.Bounds, (int)ZoneCatalog.RoomScale);
                }
                _camera.SnapTo(new Vector2(centre, ZoneCatalog.RoomHeight / 2f));
                _figure.Position = new Vector2(centre, (room.BackEdgeAt(centre) + room.FrontEdgeAt(centre)) / 2f);
                _figure.Facing = Direction.Right;
                _caption = $"{zone}/{room.Id}    view {_index + 1}/{_views.Count}";
            }
            var time = new GameTime(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1f / 60f));
            _scene.Update(time);
            _figure.Update(1f / 60f);
            if (++_frames < 3) return null;
            _frames = 0;
            return $"rooms/{zone}-{room.Id}-{++_index:00}.png";
        }

        public override void Draw(GameTime gameTime)
        {
            if (_scene is null) return;
            SpriteBatch batch = ScreenManager.SpriteBatch;
            _scene.DrawBack(gameTime, batch, _camera, _room.Tint);
            batch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: _camera.Transform);
            _figure.Draw(batch, _scene.Depth(_figure.Position), _camera.Pixel, _room.Tint);
            batch.End();
            _scene.DrawFront(batch, _camera, _room.Tint);
            _scene.DrawDark(batch, _camera);
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
            batch.DrawString(_font, _caption, new Vector2(22, 18), Color.Black * .8f);
            batch.DrawString(_font, _caption, new Vector2(20, 16), Color.White);
            batch.End();
        }

        public override void Unload()
        {
            _scene?.Unload();
            _content?.Unload();
        }
    }
}
