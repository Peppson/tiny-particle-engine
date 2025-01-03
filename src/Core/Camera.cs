
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Particle.Global;
using Utility;

namespace Particle.Core;

public class Camera
{
    public struct ViewPort(Matrix projection, Matrix view, Matrix world, Viewport viewport)
    {
        public Matrix projection = projection;
        public Matrix view = view;
        public Matrix world = world;
        public Viewport viewport = viewport;
    }

    public struct CameraStates
    {
        public int RotationX { get; set; }
        public int RotationY { get; set; }
        public int Zoom { get; set; }
    }

    public ViewPort mainViewport;
    public ViewPort smallViewport;
    public CameraStates CameraInfo; // Info text and debugging
    public Vector3 CameraTarget;
    public Vector2 worldMatrixRotation;
    public Vector3 cameraPosition;
    public const float maxZoom = -50;
    public const float minZoom = -700;
    private const float _Tau = (float)(Math.PI * 2);


    public Camera(GraphicsDevice GraphicsDevice)
    {   
        cameraPosition = new Vector3(0f, 0f, Config.mainViewportDefaultZoom);
        CameraTarget = Vector3.Zero;

        // Setup main and small viewports
        SetMainViewport(GraphicsDevice);
        SetSmallViewport();

        // Init
        ResetWorldMatrix();
        UpdateCameraStateZoom();
    }

    public void SetMainViewport(GraphicsDevice GraphicsDevice) 
    {
        // Setup camera perspective and FOV 
        mainViewport.projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45f),
            GraphicsDevice.DisplayMode.AspectRatio,
            1f,
            1000f
        );
        // Transform view and perspective some amount to the right relative to window
        mainViewport.projection *= Matrix.CreateTranslation(0.27f, 0, 0);

        // Create view and world matrices
        mainViewport.view = Matrix.CreateLookAt(cameraPosition, CameraTarget, Vector3.Up);
        mainViewport.world = Matrix.CreateWorld(CameraTarget, Vector3.Forward, Vector3.Up);

        // Set viewport
        mainViewport.viewport = GraphicsDevice.Viewport;
    }

    public void SetSmallViewport() 
    {   
        // Create 100x100 viewport
        smallViewport.viewport = new Viewport
        {
            X = Config.WindowWidth - 110,
            Y = Config.WindowHeight - 110,
            Width = 100,
            Height = 100,
            MinDepth = 0,
            MaxDepth = 1
        };
        // Setup camera perspective and FOV
        smallViewport.projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45f), 
            smallViewport.viewport.Width / (float)smallViewport.viewport.Height, 
            1f, 
            100f
        );
        // Create view matrix
        smallViewport.view = Matrix.CreateLookAt(
            new Vector3(0, 0, Config.smallViewportDefaultZoom), 
            Vector3.Zero, 
            Vector3.Up
        ); 
    }

    public void RotateView(MouseState mouseState, MouseState oldMouseState)
    {
        const float sensitivity = 0.003f;

        // Mouse delta
        float deltaX = mouseState.X - oldMouseState.X;
        float deltaY = mouseState.Y - oldMouseState.Y;

        // Somewhat hacky... rotate the whole worldview instead of the camera,
        // too preserve camera position for zoom and drag
        worldMatrixRotation.X += deltaX * sensitivity;
        worldMatrixRotation.Y -= deltaY * sensitivity;

        UpdateWorldMatrices();
        UpdateCameraStateDegrees();
    }

    public void DragView(MouseState mouseState, MouseState oldMouseState)
    {
        const float sensitivity = 0.1f; 

        // Mouse delta
        float deltaX = mouseState.X - oldMouseState.X;
        float deltaY = mouseState.Y - oldMouseState.Y;

        // Move cameraPosition and cameraTarget, to "drag" the view
        cameraPosition.X    += deltaX * sensitivity;
        cameraPosition.Y    += deltaY * sensitivity;
        CameraTarget.X      += deltaX * sensitivity;
        CameraTarget.Y      += deltaY * sensitivity;
    }

    public void ZoomView(MouseState mouseState, MouseState oldMouseState)
    {
        const float sensitivity = 8f;
        int deltaScroll = mouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue;

        // Zoom in or Out? + Clamp
        if (deltaScroll > 0)
        {
            cameraPosition.Z += sensitivity;
            cameraPosition.Z = Math.Min(cameraPosition.Z, maxZoom);
        }
        else
        {
            cameraPosition.Z -= sensitivity;
            cameraPosition.Z = Math.Max(cameraPosition.Z, minZoom);
        }
        
        UpdateCameraStateZoom();
    }

    public void RotateViewAuto()
    {   
        // Stops if paused or while user is holding a mouse button
        if (State.IsPaused || InputHandler.IsMousePressed || !Config.autoRotation) return;

        // Ease in the auto camera rotation
        State.RotationEaseInFactor += 0.0033f * State.GameSpeedFactor;
        State.RotationEaseInFactor = Math.Min(State.RotationEaseInFactor, 1);

        float rotationSpeed = 0.00008f * State.GameSpeedFactor * State.RotationEaseInFactor;
        worldMatrixRotation.X += rotationSpeed;
        
        UpdateWorldMatrices();
        UpdateCameraStateDegrees();
    }

    public void UpdateWorldMatrices()
    {   
        mainViewport.world = 
            Matrix.CreateRotationY(worldMatrixRotation.X) * 
            Matrix.CreateRotationX(worldMatrixRotation.Y);
        
        smallViewport.world = 
            Matrix.CreateRotationY(worldMatrixRotation.X) * 
            Matrix.CreateRotationX(- worldMatrixRotation.Y); // Inverted for some reason
    }

    public void UpdateViewMatrix()
    {   
        mainViewport.view = Matrix.CreateLookAt(cameraPosition, CameraTarget, Vector3.Up);
    }

    public void Reset()
    {
        CameraTarget = Vector3.Zero;
        cameraPosition = new Vector3(0f, 0f, Config.mainViewportDefaultZoom);
        CameraInfo.RotationX = 0;
        CameraInfo.RotationY = 0;
        ResetWorldMatrix();
        UpdateCameraStateZoom();
    }

    private void ResetWorldMatrix()
    {   
        worldMatrixRotation = Vector2.Zero;
        UpdateWorldMatrices();
    }

    private void UpdateCameraStateZoom() 
    {
        CameraInfo.Zoom = Map(cameraPosition.Z, maxZoom, minZoom, 100, 0);
    }

    private void UpdateCameraStateDegrees()
    {   
        CameraInfo.RotationX = radToDegrees(worldMatrixRotation.X);
        CameraInfo.RotationY = radToDegrees(worldMatrixRotation.Y);
    }

    private static int radToDegrees(float radian)
    {
        // Wrap around Tau and guard against negative nums
        radian %= _Tau;
        if (radian < 0) 
        { 
            radian += _Tau; 
        }
        // Radians to degrees and truncate to nearest whole num
        int degree = (int)(radian * (180f / (float)Math.PI));
        if (degree == 360)
        {
            degree = 0;
        }

        return degree;
    }

    private static int Map(float input, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (int)(toMin + (toMax - toMin) * ((input - fromMin) / (fromMax - fromMin)));
    }

    // Debug
    public void PrintCameraXYZ(string txt = "", bool asInfoText = false)
    {   
        #if DEBUG
            if (txt != "") Console.WriteLine($"---- {txt} ----");
            Console.WriteLine($"Position:         Target:");

            if (asInfoText)
            {
                Console.WriteLine($"X: {CameraInfo.RotationX}°");
                Console.WriteLine($"Y: {CameraInfo.RotationY}°");
                Console.WriteLine($"Z: {CameraInfo.Zoom}%");
            }
            else 
            {
                Console.WriteLine($"X: {cameraPosition.X, 10}     X: {CameraTarget.X, 10}");
                Console.WriteLine($"Y: {cameraPosition.Y, 10}     Y: {CameraTarget.Y, 10}");
                Console.WriteLine($"Z: {cameraPosition.Z, 10}     Z: {CameraTarget.Z, 10}\n");
            }
        #endif
    }
}
