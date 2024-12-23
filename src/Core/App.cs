
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MenuClass;
using CameraClass;
using InputHandlerClass;
using GlobalConfig;
using GlobalStates;

namespace monogame;
 
public partial class App : Game
{   
    // Components
    private readonly Menu _menu = new();
    private InputHandler _input;
    private Camera _cam;

    // Graphics
    private GraphicsDeviceManager _graphics;
    private VertexBuffer _vertexBuffer;
    private SpriteBatch _spriteBatch;
    private BasicEffect _basicEffect;
    private SpriteFont _font;

    // Fps
    private float _FPS = 0;
    private float _totalElapsedTime = 0;
    private float _frameCounter = 0;


    public App()
    {   
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Assets";

        // Window settings
        IsFixedTimeStep = Config.isFixedFPS;
        IsMouseVisible = true;
        Window.AllowAltF4 = true;
        Window.Title = Config.windowTitle;
        _graphics.PreferredBackBufferWidth = Config.WindowWidth;
        _graphics.PreferredBackBufferHeight = Config.WindowHeight;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        base.Initialize();
        _cam = new Camera(GraphicsDevice);
        _input = new InputHandler(_menu, _cam, this);

        // BasicEffect, for rendering simple geometry
        _basicEffect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = true
        };

        // Send geometry in a buffer instead of stream to GPU
        #if USE_GPU_VERTEX_BUFFER
            InitVertexBuffer();
        #endif

        #if DEBUG
            DebugInfo();
        #endif
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load assets
        _font = Content.Load<SpriteFont>("Fonts/font");
        _menu.LoadAllSprites(Content);
        this.LoadAllShapes();
    }

    // Main game loop
    protected override void Update(GameTime gameTime)
    {   
        // Calculate gamespeed factor from delta frametimes
        float frameDeltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        ComputeGameSpeed(frameDeltaTime);
        ComputeFPS(frameDeltaTime);

        // Update elapsed gametime 
        State.GameTotalTime = gameTime.TotalGameTime.TotalMilliseconds;

        _input.IsWindowFocused();
        _input.ProcessKeyboard();
        _input.ProcessMouse();
        _cam.RotateViewAuto(_input.IsMousePressed);
        _cam.UpdateViewMatrix();

        // Ongoing animations/transitions
        ShapeTransitionAnimation();
        AnimationComet(); // TODO

        base.Update(gameTime);
    }

    // Main draw loop
    protected override void Draw(GameTime gameTime)
    {   
        GraphicsDevice.Clear(Config.windowBackgroundColor);

        // Updated camera view
        _basicEffect.World = _cam.WorldMatrix;
        _basicEffect.View = _cam.ViewMatrix;
        _basicEffect.Projection = _cam.ProjectionMatrix;

        #if USE_GPU_VERTEX_BUFFER
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
        #endif

        RenderGeometry();
        RenderSprites();

        base.Draw(gameTime);
    }

    private static void ComputeGameSpeed(float frameDeltaTime)
    {   
        // Made all animations @ 240fps (frametimes ≈ 4ms), 
        // scale accordingly for other framerates.
        const float targetFrameTime = 1000f / 240f;
        State.GameSpeedFactor = frameDeltaTime / targetFrameTime;
    }

    private void ComputeFPS(float frameDeltaTime)
    {   
        _totalElapsedTime += frameDeltaTime;
        _frameCounter++;

        // Update FPS every second
        if (_totalElapsedTime >= 1000)
        {   
            _FPS = _frameCounter;
            _frameCounter = 0;
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
        SetCurrentShape(State.CurrentShape);

        // Comet todo
        RenderComet = false;
        Array.Clear(_debrisDirection);
        Array.Clear(_isDebris);
        _newStep = 0; 
        _oldStep = 0;
    }

    #if DEBUG
        private static void DebugInfo()
        {
            Console.WriteLine("\n------- Configuration: DEBUG -------");
        }
    #endif
}
