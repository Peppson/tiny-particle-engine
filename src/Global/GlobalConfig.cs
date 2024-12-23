
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;
using GlobalTypeDefinitions;
using GlobalColorPalette;

namespace GlobalConfig;

public static class Config
{   
    public const bool isFixedFPS = false; // true = 60fps, false = system
    public const bool autoRotationOnStartup = true;
    public const ShapeType shapeOnStartup = ShapeType.Sphere;
 
    // Window
    public const int WindowWidth = 1400;
    public const int WindowHeight = 788;
    public const string windowTitle = "Tiny particle engine";
    public static readonly Color windowBackgroundColor = NordColors.Black;
    public const float cameraDefaultZoom = -210f;
    
    // Shapes
    public const int shapeSize = 24; // n^3
    public const int totalShapeSize = shapeSize * shapeSize * shapeSize;
    public const float cubeScale = 2.173912f; // Align middle cube size to coordinate space (distance between points/2)
    public static readonly Color shapeColor = NordColors.Blue;
    public static readonly Color cubeColor = NordColors.Orange;

    // Menu
    public static readonly Vector2 menuPos = new(30, 50);
    public const bool isMenuDebug = false;

    // Info text
    public static readonly Vector2 infoTextPos = new(10, WindowHeight - 70);
    public static readonly Vector2 fpsPos = infoTextPos + new Vector2(1, 0);
    public static readonly Vector2 zoomPos = infoTextPos + new Vector2(0, 16);
    public static readonly Vector2 camXPos = infoTextPos + new Vector2 (0, 32);
    public static readonly Vector2 camYPos = infoTextPos + new Vector2 (0, 48);
}
