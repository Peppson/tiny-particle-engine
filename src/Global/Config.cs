
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace Particle.Global;

public static class Config
{   
    public const string title = "Tiny Particle Engine";
    public const bool showStartupMenu = true;
    public const bool autoRotation = true;
    public const bool isFixedFPS = false; // 1 = 60fps, 0 = system
    
    public const int WindowWidth = 1400;
    public const int WindowHeight = 788;
    public static readonly Color windowBackgroundColor = ColorPalette.Black;
    public const float mainViewportDefaultZoom = -220f;
    public const float smallViewportDefaultZoom = 8f;
    
    // Shapes
    public const ShapeType shapeOnStartup = ShapeType.Sphere;
    public const int shapeSize1D = 24;
    public const int totalShapeSize = shapeSize1D * shapeSize1D * shapeSize1D; // 3D
    public const float cubeScale = 2.173912f; // Align middle cube size to coordinate space (distance between points/2)
    public static readonly Color shapeColor = ColorPalette.Blue;
    public static readonly Color cubeColor = ColorPalette.Orange;
    public static readonly Color asteroidColor = ColorPalette.OrangeMax;
    public const int asteroidMaxCount = 10;

    // Menu
    public const string oldShapeButton = "ShapeBtn1";
    public const bool isMenuDebug = false;

    // Info text
    public static readonly Vector2 infoTextPos = new(11, WindowHeight - 89);
    public static readonly Vector2 fpsPos = infoTextPos + new Vector2(1, 0);
    public static readonly Vector2 zoomPos = infoTextPos + new Vector2(0, 16);
    public static readonly Vector2 camXPos = infoTextPos + new Vector2 (0, 32);
    public static readonly Vector2 camYPos = infoTextPos + new Vector2 (0, 48);
    public static readonly Vector2 countPos = infoTextPos + new Vector2 (0, 64);
}
