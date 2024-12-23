
using System.Numerics;
using ColorPalette;
using GlobalStates;
using Color = Microsoft.Xna.Framework.Color;

namespace GlobalConfig;

public static class Config
{   
    public const bool isFixedFPS = false; // true = 60fps, false = system
    public const bool autoRotationOnStartup = true;
    public const State.ShapeType shapeOnStartup = State.ShapeType.Sphere;
 
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
}
