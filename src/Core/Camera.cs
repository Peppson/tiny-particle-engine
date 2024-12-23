
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using GlobalConfig;
using GlobalStates;

namespace CameraClass;

public class Camera
{
    public Matrix ProjectionMatrix { get; private set; }
    public Matrix ViewMatrix { get; private set; }
    public Matrix WorldMatrix { get; set; }
    private Vector3 _cameraTarget;
    private Vector3 _cameraPosition;
    public Vector2 _worldMatrixRotation;


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

        // Rotate worldMatrix 90 degrees to align XYZ
        ResetWorldMatrix();
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
    }

    public void RotateViewAuto(bool isMousePressed)
    {   
        // Stops camera auto rotation if toggled off,
        // also stop while user is holding a mouse button
        if (!State.AutoRotation || isMousePressed) return;

        // Ease in the rotation
        State.AutoRotationEaseInFactor += 0.0033f * State.GameSpeedFactor;
        State.AutoRotationEaseInFactor = Math.Min(State.AutoRotationEaseInFactor, 1);

        float rotationSpeed = 0.00016f * State.GameSpeedFactor * State.AutoRotationEaseInFactor;

        _worldMatrixRotation.X += rotationSpeed;
        WorldMatrix = Matrix.CreateRotationY(_worldMatrixRotation.X) * Matrix.CreateRotationX(_worldMatrixRotation.Y);
    }

    public void RotateViewForTorus()
    {   
        // Change Y-axis small amount when showing "Torus"
        _worldMatrixRotation.Y -= 0.28f;
        WorldMatrix = Matrix.CreateRotationY(_worldMatrixRotation.X) * Matrix.CreateRotationX(_worldMatrixRotation.Y);
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
    }

    private void ResetWorldMatrix()
    {
        _worldMatrixRotation = new Vector2(MathHelper.ToRadians(-180), 0);
        WorldMatrix = Matrix.CreateRotationY(_worldMatrixRotation.X) * Matrix.CreateRotationX(_worldMatrixRotation.Y);
    }
      
    public void PrintCameraXYZ(string txt = "")
    {   
        #if DEBUG
            if (txt != "") Console.WriteLine($"---- {txt} ----");

            Console.WriteLine($"Position:         Target:");
            Console.WriteLine($"X: {_cameraPosition.X, 10}     X: {_cameraTarget.X, 10}");
            Console.WriteLine($"Y: {_cameraPosition.Y, 10}     Y: {_cameraTarget.Y, 10}");
            Console.WriteLine($"Z: {_cameraPosition.Z, 10}     Z: {_cameraTarget.Z, 10}\n");
        #endif
    }
}
