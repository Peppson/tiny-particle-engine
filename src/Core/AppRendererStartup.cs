
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particle.Global;
using Particle.Ui;
using Utility;

namespace Particle.Core;

public partial class App
{   
    private void RenderStartupWindow()
    {   
        Sprite menu = _menu.sprites["MenuBackground"];
        RenderStartupBlur(menu);
        RenderStartupMenu(menu);
        RenderStartupLine();
        RenderStartupButton();
        RenderStartupText();
    }

    private void RenderStartupBlur(Sprite blur) 
    {   
        _spriteBatch.Draw(
            blur.Texture,
            new Vector2(-300, -300),
            null,
            new Color(255, 255, 255, 170),
            0f, 
            Vector2.Zero,
            blur.Scale * 10f,
            SpriteEffects.None, 
            0f
        );
    }

    private void RenderStartupMenu(Sprite menu) 
    {
        const float rotation = 1.5707964f; // 90° to rads
        const int alpha = 240;

        _spriteBatch.Draw(
            menu.Texture,
            _menu.startupMenuPos,
            null,
            new Color(255, 255, 255, alpha),
            rotation, 
            Vector2.Zero,
            menu.Scale,
            SpriteEffects.None, 
            0f
        );
    }

    private void RenderStartupLine() 
    {   
        Render(_menu.sprites["LineStartup"]);
    }

    private void RenderStartupButton()
    {   
        string button = InputHandler.IsStartupBtnHovered ? "StartupBtnDark" : "StartupBtnBlue";
        Render(_menu.sprites[button]);
    }

    private void RenderStartupText()
    {      
        _spriteBatch.DrawString(_font.large, Config.title, _menu.startupText1, ColorPalette.White);
        _spriteBatch.DrawString(_font.medium, "Spacebar:", _menu.startupText2, ColorPalette.Orange);
        _spriteBatch.DrawString(_font.medium, 
            "                                         Shoot asteroid", 
            _menu.startupText2, 
            ColorPalette.White
        );
        _spriteBatch.DrawString(_font.medium, "Left click:", _menu.startupText3, ColorPalette.Orange);
        _spriteBatch.DrawString(_font.medium, 
            "                                         Rotate camera", 
            _menu.startupText3, 
            ColorPalette.White
        );
        _spriteBatch.DrawString(_font.medium, "Right click:", _menu.startupText4, ColorPalette.Orange);
        _spriteBatch.DrawString(_font.medium, 
            "                                         Pan camera", 
            _menu.startupText4, 
            ColorPalette.White
        );
        _spriteBatch.DrawString(_font.medium, "Scrool:", _menu.startupText5, ColorPalette.Orange);
        _spriteBatch.DrawString(_font.medium, 
            "                                         Zoom in/out", 
            _menu.startupText5, 
            ColorPalette.White
        );
        _spriteBatch.DrawString(_font.large, "Ok", _menu.startupText6, ColorPalette.White);
    }
}

// Get txt pos 
/* 
Vector2 length = _font.large.MeasureString("Ok");
Vector2 window = new Vector2(Config.WindowWidth / 2, Config.WindowHeight / 2);
float test = window.X - length.X / 2;
Console.WriteLine(test); 
*/

// Get sprite pos
/* 
float length = _menu.sprites["StartupBtnGrey"].Rectangle.Width / 2;
Vector2 window = new Vector2(Config.WindowWidth / 2, Config.WindowHeight / 2);
float test = window.X - length;
//Console.WriteLine(test); 
*/
