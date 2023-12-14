using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ScapeCore.Core.Batching.Events;
using ScapeCore.Core.Batching.Resources;
using ScapeCore.Core.Engine;
using ScapeCore.Core.Serialization;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        public static WeakReference<LLAM> Instance { get; private set; }

        public readonly List<MonoBehaviour> MonoBehaviours = new();
        public readonly List<GameObject> GameObjects = new();

        internal event UpdateBatchEventHandler OnUpdate;
        internal event StartBatchEventHandler OnStart;
        internal event LoadBatchEventHandler OnLoad;
        internal event RenderBatchEventHandler OnRender;

        public LLAM()
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Async(wt => wt.Console(theme: AnsiConsoleTheme.Code)).CreateLogger();

            if (Instance != null) throw new InvalidOperationException("LLAM singleton instance is not null");
            else Instance = new(this);

            Log.Debug("Constructing LLAM...");

            //Very much important indeed
            RuntimeHelpers.RunClassConstructor(typeof(SerializationManager).TypeHandle);
            RuntimeHelpers.RunClassConstructor(typeof(ResourceManager).TypeHandle);

            _graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;
        }

        protected override void Initialize()
        {
            Log.Information("Initializing...");
            static void successLoad(object a, StartBatchEventArgs b) => Log.Information("Load Sucess!");
            OnStart += successLoad;

            // TODO: Add your initialization logic here
            new Ball();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Log.Information("Loading Content...");
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

        protected override void EndRun()
        {
            // At application shutdown (results in monitors getting StopMonitoring calls)
            Log.CloseAndFlush();
            base.EndRun();
        }
    }
}
