
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using GlobalTypeDefinitions;
using GlobalConfig;
using GlobalStates;
using UtilityClass;

namespace CameraClass;

public class Camera
{
    public Matrix ProjectionMatrix { get; private set; }
    public Matrix ViewMatrix { get; private set; }
    public Matrix WorldMatrix { get; private set; }
    public CameraState CameraState;
    private Vector3 _cameraTarget;
    private Vector3 _cameraPosition;
    private Vector2 _worldMatrixRotation;
    private const float _Tau = (float)(Math.PI * 2);


    public Camera(GraphicsDevice GraphicsDevice)
    {   
        _cameraTarget = new Vector3(0f, 0f, 0f);
        _cameraPosition = new Vector3(0f, 0f, Config.cameraDefaultZoom);

        // Setup camera perspective and FOV 
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45f),
            GraphicsDevice.DisplayMode.AspectRatio,
            1f,
            1000f
        );
 
        // Transform view and perspective some amount to the right of the viewport
        ProjectionMatrix *= Matrix.CreateTranslation(0.27f, 0, 0);

        // Create view and world matrices
        ViewMatrix = Matrix.CreateLookAt(_cameraPosition, _cameraTarget, Vector3.Up);
        WorldMatrix = Matrix.CreateWorld(_cameraTarget, Vector3.Forward, Vector3.Up);

        // Init
        ResetWorldMatrix();
        UpdateCameraStateZoom();
    }

    public void RotateView(MouseState newMouseState, MouseState oldMouseState)
    {
        const float sensitivity = 0.003f;

        // Mouse delta since last frame
        float deltaX = newMouseState.X - oldMouseState.X;
        float deltaY = newMouseState.Y - oldMouseState.Y;

        // Somewhat hacky... rotate the whole worldview instead of the camera,
        // too preserve camera position for zoom and drag
        _worldMatrixRotation.X += deltaX * sensitivity;
        _worldMatrixRotation.Y -= deltaY * sensitivity;
        WorldMatrix = Matrix.CreateRotationY(_worldMatrixRotation.X) * Matrix.CreateRotationX(_worldMatrixRotation.Y);

        UpdateCameraStateDegrees();
    }

    public void DragView(MouseState newMouseState, MouseState oldMouseState)
    {
        const float sensitivity = 0.1f; 

        // Mouse delta since last frame
        float deltaX = newMouseState.X - oldMouseState.X;
        float deltaY = newMouseState.Y - oldMouseState.Y;

        // Move cameraPosition and cameraTarget, to "drag" the view
        _cameraPosition.X    += deltaX * sensitivity;
        _cameraPosition.Y    += deltaY * sensitivity;
        _cameraTarget.X      += deltaX * sensitivity;
        _cameraTarget.Y      += deltaY * sensitivity; 
    }

    public void ZoomView(MouseState newMouseState, MouseState oldMouseState)
    {
        const float sensitivity = 5f;
        int deltaScroll = newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue;

        // Zoom in or Out? Clamp at edges
        if (deltaScroll > 0)
        {
            _cameraPosition.Z += sensitivity;
            _cameraPosition.Z = Math.Min(_cameraPosition.Z, -10);
        }
        else
        {
            _cameraPosition.Z -= sensitivity;
            _cameraPosition.Z = Math.Max(_cameraPosition.Z, -600);
        }

        UpdateCameraStateZoom();
    }

    public void RotateViewAuto(bool isMousePressed)
    {   
        // Stops camera auto rotation if toggled off,
        // also stop while user is holding a mouse button
        if (!State.AutoRotation || isMousePressed) return;

        // Ease in the rotation
        State.AutoRotationEaseInFactor += 0.0033f * State.GameSpeedFactor;
        State.AutoRotationEaseInFactor = Math.Min(State.AutoRotationEaseInFactor, 1);

        float rotationSpeed = 0.00014f * State.GameSpeedFactor * State.AutoRotationEaseInFactor;

        _worldMatrixRotation.X += rotationSpeed;
        WorldMatrix = Matrix.CreateRotationY(_worldMatrixRotation.X) * Matrix.CreateRotationX(_worldMatrixRotation.Y);

        UpdateCameraStateDegrees();
    }

    public void RotateViewForTorus()
    {   
        // Change Y-axis small amount when showing "Torus"
        _worldMatrixRotation.Y -= 0.28f;
        WorldMatrix = Matrix.CreateRotationY(_worldMatrixRotation.X) * Matrix.CreateRotationX(_worldMatrixRotation.Y);
        UpdateCameraStateDegrees();
    }

    public void UpdateViewMatrix()
    {
        ViewMatrix = Matrix.CreateLookAt(_cameraPosition, _cameraTarget, Vector3.Up);
    }

    public void Reset()
    {
        _cameraTarget = Vector3.Zero;
        _cameraPosition = new Vector3(0f, 0f, Config.cameraDefaultZoom);
        ResetWorldMatrix();
        UpdateCameraStateZoom();
    }

    private void ResetWorldMatrix()
    {   
        _worldMatrixRotation = new Vector2(MathHelper.ToRadians(0), 0);
        WorldMatrix = Matrix.CreateRotationY(_worldMatrixRotation.X) * Matrix.CreateRotationX(_worldMatrixRotation.Y);
    }

    private void UpdateCameraStateDegrees()
    {   
        CameraState.RotationX = ConvertRadianToDegrees(_worldMatrixRotation.X);
        CameraState.RotationY = ConvertRadianToDegrees(_worldMatrixRotation.Y);
    }

    private static int ConvertRadianToDegrees(float angle)
    {
        // Wrap around Tau and guard against negative values
        float radian = angle % _Tau;
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

    private void UpdateCameraStateZoom() 
    {
        CameraState.Zoom = Map(_cameraPosition.Z, -10, -600, 100, 0);
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
                Console.WriteLine($"X: {CameraState.RotationX}°");
                Console.WriteLine($"Y: {CameraState.RotationY}°");
                Console.WriteLine($"Z: {CameraState.Zoom}%");
            }
            else 
            {
                Console.WriteLine($"X: {_cameraPosition.X, 10}     X: {_cameraTarget.X, 10}");
                Console.WriteLine($"Y: {_cameraPosition.Y, 10}     Y: {_cameraTarget.Y, 10}");
                Console.WriteLine($"Z: {_cameraPosition.Z, 10}     Z: {_cameraTarget.Z, 10}\n");
            }
        #endif
    }
}
