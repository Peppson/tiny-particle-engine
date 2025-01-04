
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Particle.Global;
using Particle.Ui;
using Utility;

namespace Particle.Core;

public partial class App : Game
{   
    private struct Fonts 
    {
        public SpriteFont small;
        public SpriteFont medium;
        public SpriteFont large;
    }
    private readonly Random _random = new();
    private readonly Menu _menu = new();
    private InputHandler _input;
    private Camera _cam;

    // Graphics
    private readonly GraphicsDeviceManager _graphics;
    private VertexBuffer _vertexBuffer;
    private SpriteBatch _spriteBatch;
    private BasicEffect _basicEffect;
    private Fonts _font;
    private int _FPS = 0;
    private int _frameCount = 0;
    private float _totalElapsedTime = 0;

    #if PERF_BENCHMARK
        private int _cycleCount = 0;
        private bool _isCycleActive = false;
    #endif


    public App()
    {   
        _graphics = new GraphicsDeviceManager(this) 
        {
            PreferredBackBufferWidth = Config.WindowWidth,
            PreferredBackBufferHeight = Config.WindowHeight,
            PreferMultiSampling = true // Anti-aliasing
        };

        this.Content.RootDirectory = "Assets";
        this.IsFixedTimeStep = Config.isFixedFPS;
        this.IsMouseVisible = true;
        Window.AllowAltF4 = true;
        Window.Title = Config.title;

        const float RotationSpeed = 0.012f;
        _rotationMatrix = 
            Matrix.CreateRotationX(RotationSpeed) *
            Matrix.CreateRotationY(RotationSpeed) *
            Matrix.CreateRotationZ(RotationSpeed);
    }

    protected override void Initialize()
    {
        base.Initialize();
        _cam = new Camera(GraphicsDevice);
        _input = new InputHandler(_menu, _cam, this);
        
        // For rendering geometry
        _basicEffect = new BasicEffect(GraphicsDevice);
        _basicEffect.VertexColorEnabled = true;

        // Align model
        _modelRotation = Matrix.CreateRotationX(MathHelper.ToRadians(270));
        
        #if USE_GPU_VERTEX_BUFFER
            InitVertexBuffer(); // Geometry as buffer instead of stream to GPU
        #endif

        #if DEBUG
            DebugInfo();
        #endif
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _font = new Fonts() 
        {
            small = Content.Load<SpriteFont>("Fonts/fontSmall"),
            medium = Content.Load<SpriteFont>("Fonts/fontMedium"),
            large = Content.Load<SpriteFont>("Fonts/fontLarge")
        };
        _model = Content.Load<Model>("Models/Helicopter");
        _menu.LoadAllSprites(Content);
        this.LoadAllShapes();
    }

    // Main game loop
    protected override void Update(GameTime gameTime)
    {   
        #if PERF_BENCHMARK
            TrackPerformance(true);
        #endif   

        // Calculate gamespeed factor
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        ComputeGameSpeed(deltaTime);
        ComputeFPS(deltaTime);

        // Update time and mouse
        MouseState mouseState = Mouse.GetState();
        State.CurrentGameTime = gameTime.TotalGameTime.TotalMilliseconds;

        // Logic
        _input.IsWindowFocused();
        _input.ProcessKeyboard();
        _input.ProcessMouse(mouseState);
        _cam.RotateViewAuto();
        _cam.UpdateViewMatrix();

        // Animations/transitions/effects
        if (!State.IsPaused)
        {
            ShapeColor();
            ShapeAnimation(deltaTime);
            ShapeEffect(deltaTime);
            ShapeAsteroid(mouseState);
            ShapeGravity(deltaTime);
        }

        base.Update(gameTime);
    }

    // Main draw loop
    protected override void Draw(GameTime gameTime)
    {   
        GraphicsDevice.Clear(Config.windowBackgroundColor);

        if (State.IsStartup)
            RenderStartup();
        else    
            RenderNormal();

        base.Draw(gameTime);

        #if PERF_BENCHMARK
            TrackPerformance(false);
        #endif
    }

    private static void ComputeGameSpeed(float deltaTime)
    {   
        // Animations made @ 240fps (frametimes ≈ 4ms), 
        // scale accordingly for other framerates.
        const float targetFrameTime = 1000f / 240f;
        State.GameSpeedFactor = deltaTime / targetFrameTime;
    }

    private void ComputeFPS(float deltaTime)
    {   
        if (!IsActive)
        {
            _FPS = 1;
            _frameCount = 0;
            _totalElapsedTime = 0;
            return;
        }

        _totalElapsedTime += deltaTime;
        _frameCount++;

        if (_totalElapsedTime >= 1000)
        {   
            _FPS = _frameCount;
            _frameCount = 0;
            _totalElapsedTime = 0;
        }
    }

    private void InitVertexBuffer()
    {   
        _vertexBuffer = new VertexBuffer(GraphicsDevice,
            typeof(VertexPositionColor),
            Config.totalShapeSize,
            BufferUsage.WriteOnly
        );
        _vertexBuffer.SetData<VertexPositionColor>(_currentShape);
    }

    public void Reset()
    {   
        _cam.Reset();
        _input.Reset();
        State.Reset();
        ShapeColorsReset();
        ShapeAnimationsReset();
        ShapeEffectsReset();
        ShapeGravityReset();
        ShapeAsteroidReset();
        SetCurrentShape(Config.shapeOnStartup);
    }

    #if PERF_BENCHMARK
        private void TrackPerformance(bool fromUpdate)
        {   
            if (fromUpdate) _cycleCount++;

            // Interval and warmup guard
            if (_cycleCount < 42 || State.CurrentGameTime < 500f) return;

            // Start
            if (fromUpdate && !_isCycleActive) 
            {   
                Utils.BenchmarkStart();
                _isCycleActive = true;
            }
            // Stop
            else if (_isCycleActive)
            {
                Utils.BenchmarkStop();
                _isCycleActive = false;
                _cycleCount = 0;
            }
        }
    #endif

    #if DEBUG
        private static void DebugInfo()
        {   
            Utils.PrintColor("\n------- Configuration: DEBUG -------\n", ConsoleColor.Green);
            
            #if PERF_BENCHMARK
                Console.WriteLine("> PERF_BENCHMARK");
            #endif
            #if ANIMATION_LOGGING
                Console.WriteLine("> ANIMATION_LOGGING");
            #endif
            #if DEBUG_USE_KEYBOARD
                Console.WriteLine("> DEBUG_USE_KEYBOARD");
            #endif
            #if DEBUG_COMET
                Console.WriteLine("> DEBUG_COMET"); 
            #endif
            #if DEBUG_GRAVITY
                Console.WriteLine("> DEBUG_GRAVITY");
            #endif
            #if USE_GPU_VERTEX_BUFFER
                Console.WriteLine("> USE_GPU_VERTEX_BUFFER");
            #endif
        }
    #endif
}
