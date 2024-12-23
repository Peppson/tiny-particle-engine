
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using SpriteClass;
using GlobalConfig;

namespace MenuClass;

public class Menu
{   
    // Positions relative to Config.MenuPos
    public static readonly Vector2 ShapePickerPos = new(27, 280);
    public static readonly Vector2 ColorPickerPos = new(27, 374);
    public static readonly Vector2 FPSTextPos = new(7, Config.WindowHeight - 20); // Absolute pos
    public readonly Vector2 Text1Pos = new(118, 27);
    public readonly Vector2 Text2Pos = new(28, 252);
    public readonly Vector2 Text3Pos = new(28, 346);
    public readonly Vector2 Text4Pos = new(41, 293); 
    public readonly Vector2 Text5Pos = new(48, 104);
    public readonly Vector2 Text6Pos = new(48, 164);
    
    // Sprites
    public Dictionary<string, Sprite> sprites = [];

    // Sprite list
    private readonly string[] spritesToLoad = 
    [
        "MenuBackground",
        "MenuDebug",
        "Line",
        "ResetGrey",
        "ResetBlue",

        "Toggle1Grey",
        "Toggle1Blue",
        "Toggle1Circle",
        "Toggle2Grey",
        "Toggle2Blue",
        "Toggle2Circle",

        "ShapeBackground",
        "ShapeBtn1",
        "ShapeBtn2",
        "ShapeBtn3",
        "ShapeBtn4",
        "ShapeBtn1Big",
        "ShapeBtn2Big",
        "ShapeBtn3Big",
        "ShapeBtn4Big",

        "BtnBackground",
        "BtnBlue",
        "BtnBlueBig",
        "BtnGreen",
        "BtnGreenBig",
        "BtnOrange",
        "BtnOrangeBig",
        "BtnWhite",
        "BtnWhiteBig",
    ];


    public void LoadAllSprites(ContentManager Content)
    {   
        foreach (var spriteName in spritesToLoad)
        {
            Texture2D texture = Content.Load<Texture2D>($"Sprites/{spriteName}");   // Load texture
            Vector2 size = GetSize(spriteName);                                     // Grab size 
            Rectangle position = GetPosition(texture, spriteName, size);            // Get position 

            // Save
            Sprite sprite = new(texture, position, size);
            sprites.Add(spriteName, sprite);
        }
    }

    private Rectangle GetPosition(Texture2D texture, string spriteName, Vector2 scaleFactor)
    {
        Vector2 spriteSize = new(texture.Width, texture.Height);
        Vector2 menu = Config.menuPos;

        switch (spriteName)
        {
            // Window
            case "MenuBackground":          break;
            case "MenuDebug":               break;

            // Reset/Btn
            case "Line":                    menu += new Vector2(11, 60);  break;
            case "ResetGrey": 
            case "ResetBlue":               menu += new Vector2(306, 26); break;

            // Toggle
            case "Toggle1Grey":
            case "Toggle1Blue":             menu += new Vector2(258, 100); break;
            case "Toggle1Circle":           menu += new Vector2(263, 104); break;
            case "Toggle2Grey":
            case "Toggle2Blue":             menu += new Vector2(258, 160); break;
            case "Toggle2Circle":           menu += new Vector2(263, 164); break;

            // Shape btn
            case "ShapeBackground":         menu += ShapePickerPos;                           break;
            case "ShapeBtn1":               menu += ShapePickerPos + new Vector2(1, 1);       break;
            case "ShapeBtn2":               menu += ShapePickerPos + new Vector2(76, 1);      break;
            case "ShapeBtn3":               menu += ShapePickerPos + new Vector2(151, 1);     break;
            case "ShapeBtn4":               menu += ShapePickerPos + new Vector2(226, 1);     break;
            case "ShapeBtn1Big":            menu += ShapePickerPos + new Vector2(0, -3);      break;
            case "ShapeBtn2Big":            menu += ShapePickerPos + new Vector2(75, -3);     break;
            case "ShapeBtn3Big":            menu += ShapePickerPos + new Vector2(150, -3);    break;
            case "ShapeBtn4Big":            menu += ShapePickerPos + new Vector2(225, -3);    break;

            // Color btn
            case "BtnBackground":           menu += ColorPickerPos;                           break;
            case "BtnWhite":                menu += ColorPickerPos + new Vector2(1, 1);       break;
            case "BtnBlue":                 menu += ColorPickerPos + new Vector2(76, 1);      break;
            case "BtnGreen":                menu += ColorPickerPos + new Vector2(151, 1);     break;
            case "BtnOrange":               menu += ColorPickerPos + new Vector2(226, 1);     break;

            case "BtnWhiteBig":             menu += ColorPickerPos + new Vector2(0, -3);      break;
            case "BtnBlueBig":              menu += ColorPickerPos + new Vector2(75, -3);     break;
            case "BtnGreenBig":             menu += ColorPickerPos + new Vector2(150, -3);    break;
            case "BtnOrangeBig":            menu += ColorPickerPos + new Vector2(225, -3);    break;
            
            default: 
                Debug.Assert(false, $"\n\n----- GetPosition() Invalid sprite name - {spriteName} -----\n\n");
                break;
        }

        return new Rectangle(
            (int)menu.X, 
            (int)menu.Y, 
            (int)(spriteSize.X * scaleFactor.X), 
            (int)(spriteSize.Y * scaleFactor.Y)
        );
    }

    private Vector2 GetSize(string spriteName)
    {   
        Vector2 noScaling = new(1f, 1f);
        
        // In the case something needs to be resized later on
        switch (spriteName)
        {
            case "MenuBackground":
            case "MenuDebug":           return noScaling;

            // Reset/Btn 
            case "Line":                return noScaling;
            case "ResetGrey": 
            case "ResetBlue":           return new Vector2(0.8f, 0.8f);

            // Toggle
            case "Toggle1Grey":
            case "Toggle1Blue":             
            case "Toggle1Circle":           
            case "Toggle2Grey":
            case "Toggle2Blue":             
            case "Toggle2Circle":

            // Shape btn
            case "ShapeBackground":         
            case "ShapeBtn1":               
            case "ShapeBtn2":               
            case "ShapeBtn3":               
            case "ShapeBtn4":               
            case "ShapeBtn1Big":         
            case "ShapeBtn2Big":         
            case "ShapeBtn3Big":         
            case "ShapeBtn4Big":

            // Color btn
            case "BtnBackground":     
            case "BtnBlue":                 
            case "BtnBlueBig":              
            case "BtnGreen":                
            case "BtnGreenBig":             
            case "BtnOrange":               
            case "BtnOrangeBig":            
            case "BtnWhite":                
            case "BtnWhiteBig":         return noScaling;
            
            default: 
                Debug.Assert(false, $"\n\n----- GetSize() Invalid sprite name - {spriteName} -----\n\n");
                return noScaling;
        }
    }
}
