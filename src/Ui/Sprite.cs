
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Particle.Ui;

public struct Sprite
{   
    public Texture2D Texture { get; private set; }
    public Rectangle Rectangle { get; private set; }
    public Vector2 Scale { get; private set; }
    public Vector2 Position
    {
        get { return new Vector2(Rectangle.X, Rectangle.Y); }
        set {}
    }
    
    public Sprite(Texture2D Texture, Rectangle Rectangle, Vector2 Scale)
    {
        this.Texture = Texture;
        this.Rectangle = Rectangle;
        this.Scale = Scale;
    }
}
