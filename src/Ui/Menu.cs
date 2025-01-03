
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Particle.Ui;

public class Menu
{   
    // Positions relative to mainMenuPos
    public readonly Vector2 mainMenuPos = new(30, 50);
    public static readonly Vector2 shapePickerPos = new(27, 280);
    public static readonly Vector2 colorPickerPos = new(27, 374);
    public readonly Vector2 mainText1 = new(109, 27);
    public readonly Vector2 mainText2 = new(48, 106); 
    public readonly Vector2 mainText3 = new(47, 194);
    public readonly Vector2 mainText4 = new(48, 150);
    public readonly Vector2 mainText5 = new(28, 252);
    public readonly Vector2 mainText6 = new(28, 346);
    public readonly Vector2 mainText7 = new(42, 293); 
    public readonly Vector2 mainText8 = new(118, 293); 
    public readonly Vector2 mainText9 = new(200, 293); 
    public readonly Vector2 mainText10 = new(273, 293); 

    // Startup Menu
    public readonly Vector2 startupMenuPos = new(930, 217);
    private const float _startupPosX = 590f;
    private const float _startupPosY = 294f;
    public readonly Vector2 startupText1 = new (608, 250);
    public readonly Vector2 startupText2 = new(_startupPosX - 1, _startupPosY + 30);
    public readonly Vector2 startupText3 = new(_startupPosX, _startupPosY + 30 * 2);
    public readonly Vector2 startupText4 = new(_startupPosX, _startupPosY + 30 * 3); 
    public readonly Vector2 startupText5 = new(_startupPosX - 1, _startupPosY + 30 * 4);
    public readonly Vector2 startupText6 = new(687, 490);

    
    public Dictionary<string, Sprite> sprites = [];

    private readonly string[] spritesToLoad = 
    [
        "MenuBackground",
        "MenuDebug",
        "Line",
        "LineStartup",
        "ResetGrey",
        "ResetBlue",

        "Toggle1Grey",
        "Toggle1Blue",
        "Toggle1Circle",
        "Toggle2Grey",
        "Toggle2Blue",
        "Toggle2Circle",
        "Toggle3Grey",
        "Toggle3Blue",
        "Toggle3Circle",

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
        "StartupBtnBlue",
        "StartupBtnDark",
        "ScreenGrid",
        "meme"
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
        Vector2 menu = mainMenuPos;

        switch (spriteName)
        {
            // Window
            case "MenuBackground":          break;
            case "MenuDebug":               break;

            // Reset/Btn
            case "Line":                    menu += new Vector2(11, 60);     break;
            case "LineStartup":             menu = new Vector2(479, 293);    break;
            case "ResetGrey": 
            case "ResetBlue":               menu += new Vector2(300, 27);    break;

            // Toggle btn
            case "Toggle1Grey":
            case "Toggle1Blue":             menu += new Vector2(258, 102);   break;
            case "Toggle1Circle":           menu += new Vector2(263, 106);   break;
            case "Toggle2Grey":
            case "Toggle2Blue":             menu += new Vector2(258, 190);   break;
            case "Toggle2Circle":           menu += new Vector2(263, 194);   break;
            case "Toggle3Grey":
            case "Toggle3Blue":             menu += new Vector2(258, 146);   break;
            case "Toggle3Circle":           menu += new Vector2(263, 150);   break;

            // Shape btn
            case "ShapeBackground":         menu += shapePickerPos;                           break;
            case "ShapeBtn1":               menu += shapePickerPos + new Vector2(1, 1);       break;
            case "ShapeBtn2":               menu += shapePickerPos + new Vector2(76, 1);      break;
            case "ShapeBtn3":               menu += shapePickerPos + new Vector2(151, 1);     break;
            case "ShapeBtn4":               menu += shapePickerPos + new Vector2(226, 1);     break;
            case "ShapeBtn1Big":            menu += shapePickerPos + new Vector2(0, -3);      break;
            case "ShapeBtn2Big":            menu += shapePickerPos + new Vector2(75, -3);     break;
            case "ShapeBtn3Big":            menu += shapePickerPos + new Vector2(150, -3);    break;
            case "ShapeBtn4Big":            menu += shapePickerPos + new Vector2(225, -3);    break;

            // Color btn
            case "BtnBackground":           menu += colorPickerPos;                           break;
            case "BtnWhite":                menu += colorPickerPos + new Vector2(1, 1);       break;
            case "BtnBlue":                 menu += colorPickerPos + new Vector2(76, 1);      break;
            case "BtnGreen":                menu += colorPickerPos + new Vector2(151, 1);     break;
            case "BtnOrange":               menu += colorPickerPos + new Vector2(226, 1);     break;
            case "BtnWhiteBig":             menu += colorPickerPos + new Vector2(0, -3);      break;
            case "BtnBlueBig":              menu += colorPickerPos + new Vector2(75, -3);     break;
            case "BtnGreenBig":             menu += colorPickerPos + new Vector2(150, -3);    break;
            case "BtnOrangeBig":            menu += colorPickerPos + new Vector2(225, -3);    break;

            case "StartupBtnBlue":          menu = new Vector2(550, 480);                     break;
            case "StartupBtnDark":          menu = new Vector2(550, 480);                     break;
            case "ScreenGrid":              menu = new Vector2(0, 0);                         break;
            case "meme":                    menu = new Vector2(500, 500);                     break;
            
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
            case "Line": 
            case "LineStartup":         return noScaling; 
            case "ResetGrey": 
            case "ResetBlue":           return new Vector2(0.9f, 0.9f);

            // Toggle btn
            case "Toggle1Grey":
            case "Toggle1Blue":             
            case "Toggle1Circle":           
            case "Toggle2Grey":
            case "Toggle2Blue":             
            case "Toggle2Circle":
            case "Toggle3Grey":
            case "Toggle3Blue":             
            case "Toggle3Circle":

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
            case "BtnWhiteBig":

            case "StartupBtnBlue":
            case "StartupBtnDark":
            case "ScreenGrid":          return noScaling;
            case "meme":                return new Vector2(0.5f, 0.5f);
            
            default: 
                Debug.Assert(false, $"\n\n----- GetSize() Invalid sprite name - {spriteName} -----\n\n");
                return noScaling;
        }
    }
}
