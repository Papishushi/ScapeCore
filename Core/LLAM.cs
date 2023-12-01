using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using ScapeCore.Core.Engine;
using ScapeCore.Core.Batching.Events;
using System.Net.NetworkInformation;

namespace ScapeCore.Targets
{
    //Low Level Automation Module
    public class LLAM : Game
    {
        private long _si, _ui, _ri;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public GraphicsDeviceManager Graphics { get => _graphics; }
        public SpriteBatch SpriteBatch { get => _spriteBatch; }
        public static LLAM Instance { get; private set; }

        public readonly List<MonoBehaviour> MonoBehaviours = new();
        public readonly List<GameObject> GameObjects = new();

        internal event UpdateBatchEventHandler OnUpdate;
        internal event StartBatchEventHandler OnStart;
        internal event LoadBatchEventHandler OnLoad;
        internal event RenderBatchEventHandler OnRender;

        public LLAM()
        {
            if (Instance != null) throw new InvalidOperationException("LLAM singleton instance is not null");
            else Instance = this;

            Console.WriteLine("Constructing LLAM...");

            //Very much important indeed
            Core.Batching.Resources.ResourceManager.Ping();

            _graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;
        }

        protected override void Initialize()
        {
            Console.WriteLine("Initializing...");
            // TODO: Add your initialization logic here
            new Ball();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Console.WriteLine("Loading Content...");
            _spriteBatch = new(GraphicsDevice);
            var args = new LoadBatchEventArgs($"Load process | Patch size {OnLoad?.GetInvocationList().Length}");
            OnLoad?.Invoke(this, args);
            OnLoad = null;
        }

        protected override void Update(GameTime gameTime)
        {
            //Debug.WriteLine("Update...");
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            OnStart?.Invoke(this, new($"Start cycle number {_si++} | Patch size {OnStart.GetInvocationList().Length}"));
            OnStart = null;
            OnUpdate?.Invoke(this, new(gameTime, $"Update cycle number {_ui++} | Patch size {OnUpdate.GetInvocationList().Length}"));
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //Debug.WriteLine("Draw...");
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Render Patches
            _spriteBatch.Begin();
            OnRender?.Invoke(this, new(gameTime, $"Render cycle number {_ri++} | Patch size {OnRender.GetInvocationList().Length}"));
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
