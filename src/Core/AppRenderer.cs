
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particle.Global;
using Particle.Ui;
using Utility;

namespace Particle.Core;

public partial class App
{   
    public void RenderNormal()
    {
        // Set main viewport camera matrices
        SetMainViewport();

        // Render main viewport
        RenderGeometry();
        RenderSprites();

        // Set small viewport camera matrices and render
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.Viewport = _cam.smallViewport.viewport;
        RenderSmallViewport();
    }

    public void RenderStartup()
    {   
        // Set small viewport matrices and render
        GraphicsDevice.Viewport = _cam.smallViewport.viewport;
        RenderSmallViewport();

        // Set main viewport matrices and render
        SetMainViewport();
        RenderGeometry();
        RenderSprites(true);
    }

    private void RenderGeometry()
    {   
        #if !DEBUG_GRAVITY
            var renderTarget = GetRenderTarget();
        #endif

        foreach (var pass in _basicEffect.CurrentTechnique.Passes)
        {   
            pass.Apply();

            // Current shape
            #if !DEBUG_GRAVITY
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.PointList, renderTarget, 0, Config.totalShapeSize);
            #endif

            // Center cube
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _miscShapes[ShapeType.CenterCube], 0, 12);

            // Asteroid(s)
            if (_asteroids.Count > 0) RenderAsteroids();

            // Debug
            #if DEBUG_COMET
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _debugAsteroid, 0, 12);
            #endif

            #if DEBUG_GRAVITY
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.PointList, _currentShapeCopy, 0, Config.totalShapeSize);
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.PointList, _currentShape, 0, Config.totalShapeSize);
            #endif
        }
    }

    private void RenderSmallViewport()
    {   
        if (State.IsMemeEnabled) return;

        foreach (ModelMesh mesh in _model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.World = _modelRotation * _cam.smallViewport.world;
                effect.View = _cam.smallViewport.view;
                effect.Projection = _cam.smallViewport.projection;
            }

            mesh.Draw();
        }
    }

    private void RenderSprites(bool isStartup = false)
    {   
        _spriteBatch.Begin();

        RenderMeme();
        RenderMenu();
        RenderResetButton();        
        RenderToggleButtons();
        RenderLine();
        RenderShapeButtons();
        RenderColorButtons();
        RenderText();
        RenderShotMeText();
        //RenderScreenGridDebug();

        if (isStartup) 
            RenderStartupWindow();

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

    private void RenderAsteroids() 
    {   
        if (State.IsMemeEnabled) return;

        for (int i = 0; i < _asteroids.Count; i++)
        {
            GraphicsDevice.DrawUserPrimitives(
                PrimitiveType.LineStrip,
                _asteroids[i].vertexData, 
                0, 
                _asteroids[i].vertexData.Length / 2
            );
        }
    }

    private void RenderToggleButtons()
    {   
        var buttonMap = new Dictionary<string, bool>
        {
            { "Toggle1", State.IsPaused },
            { "Toggle2", State.IsGravity },
            { "Toggle3", State.IsEffect }
        };

        foreach (var (button, buttonState) in buttonMap)
        {
            // "ON"
            if (buttonState)
            {
                Render(_menu.sprites[$"{button}Blue"]);
                Render(_menu.sprites[$"{button}Circle"], new Vector2(20, 0));
            }
            else // "OFF" 
            {
                Render(_menu.sprites[$"{button}Grey"]);
                Render(_menu.sprites[$"{button}Circle"]);
            }
        }
    } 

    private void RenderMenu()
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

    private void RenderShapeButtons()
    {   
        Render(_menu.sprites["ShapeBackground"]);
        Render(_menu.sprites["ShapeBtn1"]);
        Render(_menu.sprites["ShapeBtn2"]);
        Render(_menu.sprites["ShapeBtn3"]);
        Render(_menu.sprites["ShapeBtn4"]);

        switch (State.CurrentShape)
        {
            case ShapeType.Sphere:      Render(_menu.sprites["ShapeBtn1Big"]);  break;
            case ShapeType.Impact:      Render(_menu.sprites["ShapeBtn2Big"]);  break;
            case ShapeType.Frequency:   Render(_menu.sprites["ShapeBtn3Big"]);  break;
            case ShapeType.Pointcloud:  Render(_menu.sprites["ShapeBtn4Big"]);  break;
        }
    } 

    private void RenderColorButtons()
    {   
        Render(_menu.sprites["BtnBackground"]);
        Render(_menu.sprites["BtnBlue"]);
        Render(_menu.sprites["BtnGreen"]);
        Render(_menu.sprites["BtnOrange"]);
        Render(_menu.sprites["BtnWhite"]);

        if (State.CurrentColor == ColorPalette.White)         Render(_menu.sprites["BtnWhiteBig"]);
        else if (State.CurrentColor == ColorPalette.Blue)     Render(_menu.sprites["BtnBlueBig"]);
        else if (State.CurrentColor == ColorPalette.Green)    Render(_menu.sprites["BtnGreenBig"]);
        else if (State.CurrentColor == ColorPalette.Orange)   Render(_menu.sprites["BtnOrangeBig"]);
    }

    private void RenderScreenGridDebug() 
    {   
        var grid = _menu.sprites["ScreenGrid"];
        _spriteBatch.Draw(
            grid.Texture,
            new Vector2(0, 0),
            null,
            Color.White,
            0f, 
            Vector2.Zero, 
            grid.Scale,
            SpriteEffects.None,
            0f
        );
    }

    private void RenderText()
    {   
        // Menu window
        _spriteBatch.DrawString(_font.medium, Config.title,  _menu.mainMenuPos + _menu.mainText1, ColorPalette.White);
        _spriteBatch.DrawString(_font.small, "Pause",        _menu.mainMenuPos + _menu.mainText2, ColorPalette.White);
        _spriteBatch.DrawString(_font.small, "Gravity",      _menu.mainMenuPos + _menu.mainText3, ColorPalette.White);
        _spriteBatch.DrawString(_font.small, "Effect",       _menu.mainMenuPos + _menu.mainText4, ColorPalette.White);
        _spriteBatch.DrawString(_font.small, "Shape",        _menu.mainMenuPos + _menu.mainText5, ColorPalette.White);
        _spriteBatch.DrawString(_font.small, "Color",        _menu.mainMenuPos + _menu.mainText6, ColorPalette.White);
        _spriteBatch.DrawString(_font.small, "Sphere",       _menu.mainMenuPos + _menu.mainText7, ColorPalette.White);
        _spriteBatch.DrawString(_font.small, "Freq",         _menu.mainMenuPos + _menu.mainText9, ColorPalette.White);
        _spriteBatch.DrawString(_font.small, "Impact",       _menu.mainMenuPos + _menu.mainText8, ColorPalette.White);
        _spriteBatch.DrawString(_font.small, "Cube",         _menu.mainMenuPos + _menu.mainText10, ColorPalette.White);

        // Info text
        _spriteBatch.DrawString(_font.small, $"Fps:",                                             Config.fpsPos,  ColorPalette.Blue);
        _spriteBatch.DrawString(_font.small, $"           {_FPS}",                                Config.fpsPos,  ColorPalette.WhiteMin);
        _spriteBatch.DrawString(_font.small, $"Zoom:",                                            Config.zoomPos, ColorPalette.Blue);
        _spriteBatch.DrawString(_font.small, $"               {_cam.CameraInfo.Zoom}%",          Config.zoomPos, ColorPalette.WhiteMin);
        _spriteBatch.DrawString(_font.small, $"Cam X:",                                           Config.camXPos, ColorPalette.Blue);
        _spriteBatch.DrawString(_font.small, $"                 {_cam.CameraInfo.RotationX}°",   Config.camXPos, ColorPalette.WhiteMin);
        _spriteBatch.DrawString(_font.small, $"Cam Y:",                                           Config.camYPos, ColorPalette.Blue);
        _spriteBatch.DrawString(_font.small, $"                 {_cam.CameraInfo.RotationY}°",   Config.camYPos, ColorPalette.WhiteMin);
        
        // Meme or normal asteroids counter
        if (State.IsMemeEnabled)
        {
            _spriteBatch.DrawString(_font.small, $"Kalvar:", Config.countPos, ColorPalette.OrangeMax);
            _spriteBatch.DrawString(_font.small, $"                 {AsteroidCount}/{Config.asteroidMaxCount}",    
                Config.countPos, 
                ColorPalette.WhiteMin
            );
        }
        else 
        {
            _spriteBatch.DrawString(_font.small, $"Asteroids:", Config.countPos, ColorPalette.Blue);
            _spriteBatch.DrawString(_font.small, $"                        {AsteroidCount}/{Config.asteroidMaxCount}",    
                Config.countPos, 
                ColorPalette.WhiteMin
            );
        }
    }
    
    private void RenderShotMeText()
    {   
        if (!State.IsShotMeEnabled) return;

        Vector3 textPosition = new(
            _origin.X - 17f,
            _origin.Y,
            _origin.Z
        );

        Vector3 screenPosition = _cam.mainViewport.viewport.Project(
            textPosition,
            _cam.mainViewport.projection,
            _cam.mainViewport.view,
            Matrix.Identity
        );

        _spriteBatch.DrawString(
            _font.small, 
            $"<-  Press                       to shot an Asteroid!", 
            new Vector2(screenPosition.X, screenPosition.Y - 8), 
            ColorPalette.WhiteMin
        );
        _spriteBatch.DrawString(
            _font.small, 
            $"                    Spacebar", 
            new Vector2(screenPosition.X, screenPosition.Y - 8), 
            ColorPalette.Orange
        );
    }

    private void RenderMeme() // This is good
    {   
        if (!State.IsMemeEnabled) return;

        Sprite meme = _menu.sprites["meme"];
        RenderMemeAsteroids(meme);

        _spriteBatch.Draw(
            meme.Texture,
            new Vector2(Config.WindowWidth - 120, Config.WindowHeight - 125),
            null,
            Color.White,
            0f, 
            Vector2.Zero, 
            meme.Scale * 0.8f,
            SpriteEffects.None, 
            0f
        );
    }

    private void RenderMemeAsteroids(Sprite meme) 
    {
        Matrix worldRotationMatrix =
            Matrix.CreateRotationX(_cam.worldMatrixRotation.Y) * 
            Matrix.CreateRotationY(_cam.worldMatrixRotation.X);

        // Foreach "asteroid"
        for (int i = 0; i < _asteroids.Count; i++)
        {
            float scale = 1 / _asteroids[i].lerpFactor;

            // Project the 3D to 2D screen space for sprites
            Vector3 screenPosition = _cam.mainViewport.viewport.Project(
                _asteroids[i].oldPosition,
                _cam.mainViewport.projection,
                _cam.mainViewport.view,
                worldRotationMatrix
            );

            _spriteBatch.Draw(
                meme.Texture,
                new Vector2(screenPosition.X, screenPosition.Y),
                null,
                Color.White,
                0f, 
                Vector2.Zero, 
                meme.Scale * scale * 0.2f,
                SpriteEffects.None, 
                0f
            );
        }
    }

    private static VertexPositionColor[] GetRenderTarget() 
    {
        if (State.RenderTarget == RenderType.Normal)
            return _currentShape;
        else 
            return _currentShapeCopy;
    }

    private void SetMainViewport()
    {
        GraphicsDevice.Viewport = _cam.mainViewport.viewport;
        _basicEffect.World = _cam.mainViewport.world;
        _basicEffect.View = _cam.mainViewport.view;
        _basicEffect.Projection = _cam.mainViewport.projection;
    }
}
