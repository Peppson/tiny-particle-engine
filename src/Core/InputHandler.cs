
using System;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using monogame;
using GlobalTypeDefinitions;
using GlobalColorPalette;
using GlobalStates;
using CameraClass;
using MenuClass;

namespace InputHandlerClass;

class InputHandler
{
    public bool IsMousePressed { get; private set; }
    public static bool IsResetBtnHovered { get; private set; } 
    private readonly Menu _menu;
    private readonly Camera _cam;
    private readonly App _app;
    private MouseState _prevMouseState;
    private double _prevButtonClickTime;
    private const int _buttonDebounceTime = 150; // ms
    private string _prevShapeButton = "ShapeBtn1";

    #if DEBUG_USE_KEYBOARD
        private Stopwatch _stopwatch = new();
    #endif


    public InputHandler(Menu menu, Camera cam, App app)
    {
        _menu = menu;
        _cam = cam;
        _app = app;
        _prevMouseState = new MouseState();
        _prevButtonClickTime = 0;
        IsMousePressed = false;
        IsResetBtnHovered = false;
        
        #if DEBUG_USE_KEYBOARD
            _stopwatch.Restart();
        #endif
    }

    private bool IsMouseOverSprite(string target, MouseState newMouseState)
    {
        return _menu.sprites[target].Rectangle.Contains(newMouseState.Position);
    }

    public void ProcessMouse()
    {   
        MouseState newMouseState = Mouse.GetState();

        // Don't allow any camera movement to start from the menu window
        if (!IsMousePressed && IsMouseOverSprite("MenuBackground", newMouseState))
        {
            MouseMenuControl(newMouseState);
        }
        // Mouse click outside menu window
        else 
        {
            MouseCameraControl(newMouseState);
        }

        _prevMouseState = newMouseState;
    }

    private void MouseCameraControl(MouseState newMouseState)
    {
        // Rotate camera
        if (newMouseState.LeftButton == ButtonState.Pressed)
        {   
            _cam.RotateView(newMouseState, _prevMouseState);
            IsMousePressed = true;
        }

        // Drag up/down/left/right
        else if (newMouseState.RightButton == ButtonState.Pressed)
        {
            _cam.DragView(newMouseState, _prevMouseState);
            IsMousePressed = true;
        }

        // Zoom with scrollwheel
        else if (newMouseState.ScrollWheelValue != _prevMouseState.ScrollWheelValue)
        {
            _cam.ZoomView(newMouseState, _prevMouseState);
        }

        // Reset isMousePressed
        else if (IsMousePressed && newMouseState.LeftButton == ButtonState.Released || 
            IsMousePressed && newMouseState.RightButton == ButtonState.Released)
        {
            IsMousePressed = false;
        }
    }

    private void MouseMenuControl(MouseState newMouseState)
    {   
        // Mouse over reset button?
        IsResetBtnHovered = IsMouseOverSprite("ResetGrey", newMouseState);

        // Mouse clicked?
        if (newMouseState.LeftButton != ButtonState.Pressed) 
            return;

        // Btn debounce
        if (!IsButtonsDebounced())
            return;

        // All buttons
        CheckResetButton(IsResetBtnHovered);
        CheckToggleButton(newMouseState);
        CheckShapePicker(newMouseState);
        CheckColorPicker(newMouseState);
    }

    private bool IsButtonsDebounced()
    {   
        if ((State.GameTotalTime - _prevButtonClickTime) < _buttonDebounceTime) 
            return false;

        _prevButtonClickTime = State.GameTotalTime;
        return true;
    }

    private void CheckResetButton(bool IsResetBtnHovered)
    {
        if (IsResetBtnHovered) _app.Reset();
    }

    private void CheckToggleButton(MouseState newMouseState)
    {
        if (IsMouseOverSprite("Toggle1Grey", newMouseState))
        {
            State.AutoRotation = !State.AutoRotation; 
            State.AutoRotationEaseInFactor = 0;
        }
        else if (IsMouseOverSprite("Toggle2Grey", newMouseState))
        {   
            State.Gravity =! State.Gravity; // Just for UI btn

            State.CurrentAnimation = State.Gravity ?
                AnimationType.Gravity : 
                AnimationType.AntiGravity;

            State.AnimationStartTime = State.GameTotalTime;
        } 
    }

    private void CheckShapePicker(MouseState newMouseState)
    {
        if (!IsMouseOverSprite("ShapeBackground", newMouseState))
            return;

        // Shape buttons
        var shapeButtons = new Dictionary<string, (ShapeType shape, AnimationType animation)>
        {
            { "ShapeBtn1", (ShapeType.Sphere,     AnimationType.Collapse)   },
            { "ShapeBtn2", (ShapeType.Torus,      AnimationType.Implode)    },
            { "ShapeBtn3", (ShapeType.Frequency,  AnimationType.Lerp)       },
            { "ShapeBtn4", (ShapeType.Pointcloud, AnimationType.Fusion)     }
        };

        // Any button pressed?
        foreach (var button in shapeButtons)
        {
            if (IsMouseOverSprite(button.Key, newMouseState))
            {
                if (_prevShapeButton == button.Key) return;

                // Reset "Gravity" if transitioning from gravity to new shape
                if (State.Gravity) State.Gravity = false;

                // Update state based on button
                State.CurrentShape = button.Value.shape;
                State.CurrentAnimation = button.Value.animation;
                _prevShapeButton = button.Key;

                // Update animation timestamp
                State.AnimationStartTime = State.GameTotalTime;
                return;
            }
        }
    }
    
    private void CheckColorPicker(MouseState newMouseState) 
    {   
        if (!IsMouseOverSprite("BtnBackground", newMouseState))
            return;

        // Any button pressed?
        if (IsMouseOverSprite("BtnWhite", newMouseState))
            State.CurrentColor = NordColors.White;    
        else if (IsMouseOverSprite("BtnBlue", newMouseState))
            State.CurrentColor = NordColors.Blue; 
        else if (IsMouseOverSprite("BtnGreen", newMouseState))
           State.CurrentColor = NordColors.Green; 
        else if (IsMouseOverSprite("BtnOrange", newMouseState))
            State.CurrentColor = NordColors.Orange; 
        
        // Update color
        _app.SetShapeColor(State.CurrentColor);
    }

    public void IsWindowFocused()
    {
        if (!_app.IsActive) Thread.Sleep(30); // Cheap sleep solution
    }

    public void Reset() 
    {
        _prevShapeButton = "ShapeBtn1";
    }

    public void ProcessKeyboard()
    {   
        KeyboardState keyboard = Keyboard.GetState();

        // Exit on ESC, CTRL + C 
        if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.C) || 
            keyboard.IsKeyDown(Keys.Escape)) 
        {
            _app.Exit();
        }

        #if DEBUG_USE_KEYBOARD
            DebugKeyboard(keyboard);
        #endif
    }
    
    #if DEBUG_USE_KEYBOARD
        private void DebugKeyboard(KeyboardState keyboard)
        {
            if (keyboard.IsKeyDown(Keys.G))             Key_G();
            if (_stopwatch.ElapsedMilliseconds < 50)    return;
            _stopwatch.Restart();

            // Things
            if (keyboard.IsKeyDown(Keys.E))             Key_E();
            else if (keyboard.IsKeyDown(Keys.C))        Key_C();   
            else if (keyboard.IsKeyDown(Keys.V))        Key_V();
            else if (keyboard.IsKeyDown(Keys.R))        Key_R();
            else if (keyboard.IsKeyDown(Keys.Q))        Key_Q();

            // Move XY
            if (keyboard.IsKeyDown(Keys.Left))          Key_Left();
            else if (keyboard.IsKeyDown(Keys.Right))    Key_Right();
            else if (keyboard.IsKeyDown(Keys.Up))       Key_Up();
            else if (keyboard.IsKeyDown(Keys.Down))     Key_Down();
        }

        private void Key_Q() {  }
        private void Key_G() { _app._newStep++; }
        private void Key_E() { State.CurrentAnimation = AnimationType.AntiGravity; }
        private void Key_C() { State.CurrentAnimation = AnimationType.Implode; }
        private void Key_V() { State.CurrentAnimation = AnimationType.Gravity; }
        private void Key_R() { _app.Reset(); }

        private void Key_Left() { /* _menu.Text6Pos.X--; */ Console.WriteLine($"{nameof(_menu.Text6Pos)}: {_menu.Text6Pos}"); }
        private void Key_Right() { /* _menu.Text6Pos.X++; */ Console.WriteLine($"{nameof(_menu.Text6Pos)}: {_menu.Text6Pos}"); }
        private void Key_Up() { /* _menu.Text6Pos.Y--; */ Console.WriteLine($"{nameof(_menu.Text6Pos)}: {_menu.Text6Pos}"); }
        private void Key_Down() { /* _menu.Text6Pos.Y++; */ Console.WriteLine($"{nameof(_menu.Text6Pos)}: {_menu.Text6Pos}"); }

    #endif // DEBUG_USE_KEYBOARD
}
