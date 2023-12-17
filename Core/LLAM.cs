using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProtoBuf;
using ScapeCore.Core.Batching.Events;
using ScapeCore.Core.Batching.Resources;
using ScapeCore.Core.SceneManagement;
using ScapeCore.Core.Serialization;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Runtime.CompilerServices;

namespace ScapeCore.Targets
{
    //Low Level Automation Module
    public class LLAM : Game
    {
        private long _si, _ui, _ri;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch? _spriteBatch;

        public GraphicsDeviceManager Graphics { get => _graphics; }
        public SpriteBatch? SpriteBatch { get => _spriteBatch; }
        public static WeakReference<LLAM?> Instance { get; private set; }

        internal event UpdateBatchEventHandler? OnUpdate;
        internal event StartBatchEventHandler? OnStart;
        internal event LoadBatchEventHandler? OnLoad;
        internal event RenderBatchEventHandler? OnRender;

        private readonly static Type[] _managers =
        {
            typeof(SerializationManager),
            typeof(ResourceManager),
            typeof(SceneManager)
        };

        static LLAM()
        {
            Instance ??= new(null);
        }

        public LLAM()
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Async(wt => wt.Console(theme: AnsiConsoleTheme.Code)).CreateLogger();
            Log.Information("Constructing Game...");

            Log.Debug("Setting singleton pattern.");
            if (Instance.TryGetTarget(out var target))
            {
                var ex = new InvalidOperationException("There is already a valid LLAM instance set up.");
                Log.Error(ex.Message);
                throw ex;
            }
            else
                Instance.SetTarget(this);

            Log.Debug("Singleton pattern was set.");

            try
            {
                foreach (var manager in _managers)
                {
                    Log.Debug("Initializing {ty} ...", manager);
                    RuntimeHelpers.RunClassConstructor(manager.TypeHandle);
                }

            }
            catch (Exception ex)
            {
                Log.Error("Manager constructor errror:{ex}\n{exin}", ex.Message, ex.InnerException?.Message);
                throw;
            }

            Log.Debug("Managers were correctly initialized.");

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

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Log.Information("Loading Content...");
            _spriteBatch = new(GraphicsDevice);
            var args = new LoadBatchEventArgs($"Load process | Patch size {OnLoad?.GetInvocationList().Length ?? 0}");
            OnLoad?.Invoke(this, args);
            OnLoad = null;
        }

        protected override void Update(GameTime gameTime)
        {
            //Debug.WriteLine("Update...");
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            OnStart?.Invoke(this, new(string.Empty));
            Log.Verbose("{{{@source}}}\t{@args}", GetHashCode(), $"Start cycle number\t{_si++}\t|\tPatch size\t{OnStart?.GetInvocationList().Length ?? 0}");
            OnStart = null;
            OnUpdate?.Invoke(this, new(gameTime, string.Empty));
            Log.Verbose("{{{@source}}}\t{@args}", GetHashCode(), $"Update cycle number\t{_ui++}\t|\tPatch size\t{OnUpdate?.GetInvocationList().Length ?? 0}");

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //Debug.WriteLine("Draw...");
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Render Patches
            _spriteBatch!.Begin();
            OnRender?.Invoke(this, new(gameTime, string.Empty));
            Log.Verbose("{{{@source}}}\t{@args}", GetHashCode(), $"Render cycle number\t{_ri++}\t|\tPatch size\t{OnRender?.GetInvocationList().Length ?? 0}");
            _spriteBatch!.End();

            base.Draw(gameTime);
        }

        protected override void EndRun()
        {
            // At application shutdown (results in monitors getting StopMonitoring calls)
            Log.CloseAndFlush();
            base.EndRun();
        }

        [ProtoAfterDeserialization]
        private void OnAfterDeserialize()
        {
            Instance = new(this);
        }
    }
}
