
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Particle.Global;
using Particle.Ui;

namespace Particle.Core;

class InputHandler
{
    public static bool IsMousePressed { get; private set; }
    public static bool IsResetBtnHovered { get; private set; }
    public static bool IsStartupBtnHovered { get; private set; }
    private readonly Menu _menu;
    private readonly Camera _cam;
    private readonly App _app;
    private MouseState _oldMouseState;
    private double _oldButtonClickTime;
    private double _oldAsteroidClickTime;
    private string _oldShapeButton = Config.oldShapeButton;
    private int _buttonCount;

    #if DEBUG_USE_KEYBOARD
        private Stopwatch _stopwatch = new();
    #endif


    public InputHandler(Menu menu, Camera cam, App app)
    {
        _menu = menu;
        _cam = cam;
        _app = app;
        IsMousePressed = false;
        IsResetBtnHovered = false;
        IsStartupBtnHovered = false;
        _oldMouseState = new MouseState();
        _oldButtonClickTime = 0;
        _buttonCount = 0;
        
        #if DEBUG_USE_KEYBOARD
            _stopwatch.Restart();
        #endif
    }

    public bool IsMouseOverSprite(string target, MouseState mouseState)
    {
        return _menu.sprites[target].Rectangle.Contains(mouseState.Position);
    }

    public void IsWindowFocused()
    {   
        if (!_app.IsActive) Thread.Sleep(4 * (int)State.GameSpeedFactor); // Cheap sleep solution
    }

    public void Reset() 
    {
        _oldShapeButton = Config.oldShapeButton;
        _buttonCount = 0;
    }

    public void ProcessMouse(MouseState mouseState)
    {   
        // Don't take any mouse inputs if window isnt in focused
        if (_app.IsActive)  
        {
            if (State.IsStartup) 
                ProcessMouseStartup(mouseState);
            else 
                ProcessMouseNormal(mouseState);
        }

        _oldMouseState = mouseState;
    }

    public void ProcessMouseNormal(MouseState mouseState)
    {   
        // Don't allow any camera movement to start from the menu window
        if (!IsMousePressed && IsMouseOverSprite("MenuBackground", mouseState))
        {
            MenuControl(mouseState);
        }
        else // Mouse click outside menu window
        {
            CameraControl(mouseState);
        }
    }

    public void ProcessMouseStartup(MouseState mouseState)
    {   
        IsStartupBtnHovered = IsMouseOverSprite("StartupBtnBlue", mouseState);

        if (IsStartupBtnHovered && mouseState.LeftButton == ButtonState.Pressed)
        {
            State.IsStartup = false;
        }
    }

    public void ProcessKeyboard()
    {   
        KeyboardState keyboard = Keyboard.GetState();

        #if DEBUG_USE_KEYBOARD
            DebugKeyboard(keyboard);
        #endif

        // Exit on ESC, CTRL + C 
        if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.C) || 
            keyboard.IsKeyDown(Keys.Escape)) 
        {
            _app.Exit();
        }
        
        // Paused?
        if (State.IsPaused || State.IsStartup) return;

        // Fire a new Asteroid with spacebar
        if (keyboard.IsKeyDown(Keys.Space) && IsAsteroidDebounced()) 
        {   
            App.IsAsteroidActive = true;
            App.MakeNewAsteroid = true;
            if (State.IsShotMeEnabled) State.IsShotMeEnabled = false;
        }
        // Hidden meme?
        else if (keyboard.IsKeyDown(Keys.K) && IsButtonsDebounced()) 
        {   
            if (_buttonCount++ > 7) State.IsMemeEnabled = true;
        }
    }

    private void CameraControl(MouseState mouseState)
    {   
        // Zoom with scrollwheel
        if (mouseState.ScrollWheelValue != _oldMouseState.ScrollWheelValue)
        {
            _cam.ZoomView(mouseState, _oldMouseState);
        }
        // Rotate camera
        if (mouseState.LeftButton == ButtonState.Pressed)
        {   
            _cam.RotateView(mouseState, _oldMouseState);
            IsMousePressed = true;
        }
        // Drag up/down/left/right
        else if (mouseState.RightButton == ButtonState.Pressed)
        {
            _cam.DragView(mouseState, _oldMouseState);
            IsMousePressed = true;
        }
        // Reset isMousePressed
        else if (IsMousePressed && mouseState.LeftButton == ButtonState.Released || 
            IsMousePressed && mouseState.RightButton == ButtonState.Released)
        {
            IsMousePressed = false;
        }
    }

    private void MenuControl(MouseState mouseState)
    {   
        // Mouse over reset button?
        IsResetBtnHovered = IsMouseOverSprite("ResetGrey", mouseState);

        // Mouse clicked?
        if (mouseState.LeftButton != ButtonState.Pressed) return;

        // Btn debounce
        if (!IsButtonsDebounced()) return;

        // Buttons
        CheckResetButton(IsResetBtnHovered);
        CheckToggleButtons(mouseState);
        CheckShapeButtons(mouseState);
        CheckColorButtons(mouseState);
    }

    private void CheckShapeButtons(MouseState mouseState)
    {
        if (!IsMouseOverSprite("ShapeBackground", mouseState)) return;

        // Shape buttons
        var buttonMap = new Dictionary<string, (ShapeType shape, AnimationType animation)>
        {
            { "ShapeBtn1", (ShapeType.Sphere,     AnimationType.Collapse)   },
            { "ShapeBtn2", (ShapeType.Impact,     AnimationType.Implode)    },
            { "ShapeBtn3", (ShapeType.Frequency,  AnimationType.Lerp)       },
            { "ShapeBtn4", (ShapeType.Pointcloud, AnimationType.Fusion)     }
        };
        
        // Any button pressed?
        foreach (var button in buttonMap)
        {
            if (IsMouseOverSprite(button.Key, mouseState))
            {
                if (_oldShapeButton == button.Key) return;

                // Update states based on button
                _oldShapeButton = button.Key;
                State.CurrentShape = button.Value.shape;
                State.CurrentAnimation = button.Value.animation;

                // Reset
                this.ResetToggleButtons();
                App.ShapeAsteroidSoftReset();
                _app.ShapeAnimationsReset();
                _app.ShapeEffectsReset();
                _app.AnimationBeginFromGravity();
                return;
            }
        }
    }
    
    private void CheckColorButtons(MouseState mouseState) 
    {   
        if (!IsMouseOverSprite("BtnBackground", mouseState)) return;

        // Any button pressed?
        if (IsMouseOverSprite("BtnWhite", mouseState))       State.CurrentColor = ColorPalette.White;    
        else if (IsMouseOverSprite("BtnBlue", mouseState))   State.CurrentColor = ColorPalette.Blue; 
        else if (IsMouseOverSprite("BtnGreen", mouseState))  State.CurrentColor = ColorPalette.Green; 
        else if (IsMouseOverSprite("BtnOrange", mouseState)) State.CurrentColor = ColorPalette.Orange; 
        
        // Update color
        State.CurrentColorAnimation = ColorAnimationType.Linear;
    }

    private void CheckResetButton(bool IsResetBtnHovered)
    {   
        // Mouse is clicked, are we hovering the reset button?
        if (IsResetBtnHovered) _app.Reset();
    }

    private void CheckToggleButtons(MouseState mouseState)
    {
        if (IsMouseOverSprite("Toggle1Grey", mouseState))
        {
            State.IsPaused =! State.IsPaused;
            State.RotationEaseInFactor = 0.4f;
        } 
        else if (IsMouseOverSprite("Toggle2Grey", mouseState))
        {   
            GravityPressed();
        }
        else if (IsMouseOverSprite("Toggle3Grey", mouseState))
        {    
            EffectPressed();
        } 
    }

    private void ResetToggleButtons()
    {
        State.IsPaused = false;
        State.IsGravity = false;
        State.IsEffect = false;
    }

    private static void GravityPressed() 
    {   
        if (State.IsShotMeEnabled) return;

        State.IsFirstGravity = true;
        State.IsGravity =! State.IsGravity;
        State.GravityState = (State.IsGravity) ? 
            GravityType.Gravity :
            GravityType.AntiGravity;
    }

    private static void EffectPressed() 
    {   
        // Todo if more effects is added...
        if (State.CurrentShape != ShapeType.Impact)
        {
            State.IsEffect =! State.IsEffect;
        }
    }

    private bool IsButtonsDebounced()
    {   
        const int interval = 200; //ms
        if ((State.CurrentGameTime - _oldButtonClickTime) < interval)
        {
            return false;
        } 

        _oldButtonClickTime = State.CurrentGameTime;
        return true;
    }

    private bool IsAsteroidDebounced()
    {   
        const double interval = 250; //ms
        if ((State.CurrentGameTime - _oldAsteroidClickTime) < interval)
        {
            return false;
        } 

        _oldAsteroidClickTime = State.CurrentGameTime;
        return true;
    }

    // Debug
    #if DEBUG_USE_KEYBOARD
        private void DebugKeyboard(KeyboardState keyboard)
        {
            if (keyboard.IsKeyDown(Keys.G))             Key_G();
            if (_stopwatch.ElapsedMilliseconds < 175)    return;
            _stopwatch.Restart();

            if (keyboard.IsKeyDown(Keys.E))             Key_E();
            else if (keyboard.IsKeyDown(Keys.C))        Key_C();   
            else if (keyboard.IsKeyDown(Keys.V))        Key_V();
            else if (keyboard.IsKeyDown(Keys.R))        Key_R();
            else if (keyboard.IsKeyDown(Keys.Q))        Key_Q();

            if (keyboard.IsKeyDown(Keys.Left))          Key_Left();
            else if (keyboard.IsKeyDown(Keys.Right))    Key_Right();
            else if (keyboard.IsKeyDown(Keys.Up))       Key_Up();
            else if (keyboard.IsKeyDown(Keys.Down))     Key_Down();
        }

        private void Key_Q() { State.IsStartup = true; }
        private void Key_G() { }
        private void Key_E() { }
        private void Key_C() { }
        private void Key_V() { }
        private void Key_R() { _app.Reset(); }

        private void Key_Left() { /* _menu.Text6Pos.X--; */ Console.WriteLine($"{nameof(_menu.mainText3)}: {_menu.mainText3}"); }
        private void Key_Right() { /* _menu.Text6Pos.X++; */ Console.WriteLine($"{nameof(_menu.mainText3)}: {_menu.mainText3}"); }
        private void Key_Up() { /* _menu.Text6Pos.Y--; */ Console.WriteLine($"{nameof(_menu.mainText3)}: {_menu.mainText3}"); }
        private void Key_Down() { /* _menu.Text6Pos.Y++; */ Console.WriteLine($"{nameof(_menu.mainText3)}: {_menu.mainText3}"); }

    #endif // DEBUG_USE_KEYBOARD
}
