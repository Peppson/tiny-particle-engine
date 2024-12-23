
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GlobalTypeDefinitions;
using GlobalColorPalette;
using GlobalConfig;
using GlobalStates;
using InputHandlerClass;
using SpriteClass;
using UtilityClass;

namespace monogame;

public partial class App : Game
{   
    public void RenderGeometry()
    {   
        foreach (var pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            // Current shape
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.PointList, _currentShape, 0, _currentShape.Length);

            // Origin cube
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _centerCube, 0, 12);

            // Comet
            if (RenderComet)
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.PointList, _comet, 0, _comet.Length);

            #if DEBUG_COMET
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, debugComet, 0, 12);
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, debugCometTrajectory, 0, 1);
            #endif
        }
    }

    public void RenderSprites()
    {   
        _spriteBatch.Begin();
        RenderMenuWindow();
        RenderResetButton();        
        RenderLine();
        RenderShapePicker();
        RenderColorPicker();
        RenderText();
        RenderToggleButton("Toggle1");
        RenderToggleButton("Toggle2");
        _spriteBatch.End();
    }

    private void Render(Sprite sprite, int alpha = 255)
    {
        _spriteBatch.Draw(
            sprite.Texture,
            sprite.Position,
            null,
            new Color(255, 255, 255, alpha),
            0f, 
            Vector2.Zero, 
            sprite.Scale,
            SpriteEffects.None, 
            0f
        );
    }

    private void Render(Sprite sprite, Vector2 position)
    {
        _spriteBatch.Draw(
            sprite.Texture,
            sprite.Position + position,
            null,
            Color.White,
            0f, 
            Vector2.Zero, 
            sprite.Scale,
            SpriteEffects.None, 
            0f
        );
    }

    private void RenderMenuWindow()
    {   
        const string menu = Config.isMenuDebug ? "MenuDebug" : "MenuBackground";
        const int alpha = 240;

        Render(_menu.sprites[menu], alpha);
    }   

    private void RenderResetButton()
    {
        string button = InputHandler.IsResetBtnHovered ? "ResetBlue" : "ResetGrey";
        Render(_menu.sprites[button]);
    }

    private void RenderLine() 
    {   
        Render(_menu.sprites["Line"]);
    }

    private void RenderShapePicker() 
    {   
        Render(_menu.sprites["ShapeBackground"]);
        Render(_menu.sprites["ShapeBtn1"]);
        Render(_menu.sprites["ShapeBtn2"]);
        Render(_menu.sprites["ShapeBtn3"]);
        Render(_menu.sprites["ShapeBtn4"]);

        // Rendering selected btn ontop of the small atm...
        switch (State.CurrentShape)
        {
            case ShapeType.Sphere:
                Render(_menu.sprites["ShapeBtn1Big"]);        
                break;
            case ShapeType.Torus:
                Render(_menu.sprites["ShapeBtn2Big"]);
                break;
            case ShapeType.Frequency:
                Render(_menu.sprites["ShapeBtn3Big"]);
                break;
            case ShapeType.Pointcloud:
                Render(_menu.sprites["ShapeBtn4Big"]);
                break;
        }
    } 

    private void RenderColorPicker()
    {   
        Render(_menu.sprites["BtnBackground"]);
        Render(_menu.sprites["BtnBlue"]);
        Render(_menu.sprites["BtnGreen"]);
        Render(_menu.sprites["BtnOrange"]);
        Render(_menu.sprites["BtnWhite"]);

        // Rendering selected btn ontop of the small atm...
        if (State.CurrentColor == NordColors.White)
            Render(_menu.sprites["BtnWhiteBig"]);
        else if (State.CurrentColor == NordColors.Blue)
            Render(_menu.sprites["BtnBlueBig"]);
        else if (State.CurrentColor == NordColors.Green)
            Render(_menu.sprites["BtnGreenBig"]);
        else if (State.CurrentColor == NordColors.Orange)
            Render(_menu.sprites["BtnOrangeBig"]);
    }

    private void RenderToggleButton(string button)
    {   
        bool buttonState = (button == "Toggle1") ? State.AutoRotation : State.Gravity;

        // "ON"
        if (buttonState)
        {
            Render(_menu.sprites[$"{button}Blue"]);
            Render(_menu.sprites[$"{button}Circle"], new Vector2(20, 0));
        }
        // "OFF"
        else 
        {
            Render(_menu.sprites[$"{button}Grey"]);
            Render(_menu.sprites[$"{button}Circle"]);
        }
    }  

    private void RenderText()
    {   
        _spriteBatch.DrawString(_font,          // Font
            Config.windowTitle,                 // String
            Config.menuPos + _menu.Text1Pos,    // Position
            NordColors.White                    // Color
        );
        _spriteBatch.DrawString(_font, 
            "Auto rotation", 
            Config.menuPos + _menu.Text5Pos, 
            NordColors.White
        );
        _spriteBatch.DrawString(_font, 
            "Gravity", 
            Config.menuPos + _menu.Text6Pos, 
            NordColors.White
        );
        _spriteBatch.DrawString(_font, 
            "Sphere           Torus              Freq              Cube", 
            Config.menuPos + _menu.Text4Pos, 
            NordColors.White
        );
        _spriteBatch.DrawString(_font, 
            "Shape", 
            Config.menuPos + _menu.Text2Pos, 
            NordColors.White
        );
        _spriteBatch.DrawString(_font, 
            "Color", 
            Config.menuPos + _menu.Text3Pos, 
            NordColors.White
        );
        
        // Info text
        _spriteBatch.DrawString(_font, 
            $"Fps: {_FPS}", 
            Config.fpsPos, 
            NordColors.White
        );
        _spriteBatch.DrawString(_font, 
            $"Zoom: {_cam.CameraState.Zoom}%", 
            Config.zoomPos, 
            NordColors.White
        );
        _spriteBatch.DrawString(_font, 
            $"Cam X: {_cam.CameraState.RotationX}°", 
            Config.camXPos, 
            NordColors.White
        );
        _spriteBatch.DrawString(_font, 
            $"Cam Y: {_cam.CameraState.RotationY}°", 
            Config.camYPos, 
            NordColors.White
        );
    } 
}
