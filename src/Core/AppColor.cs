
using System;
using Color = Microsoft.Xna.Framework.Color;
using Particle.Global;
using Utility;

namespace Particle.Core;

public partial class App
{
    private int _linearIndex = 0;


    public void ShapeColor()
    {   
        // Any ongoing?
        if (State.CurrentColorAnimation == ColorAnimationType.None) return;

        switch (State.CurrentColorAnimation) // Add more?
        {
            case ColorAnimationType.Linear:     ColorLinear();      break;
        }
    }

    public void ShapeColorsReset()
    {
        State.CurrentColorAnimation = ColorAnimationType.None;
        _linearIndex = 0;
    }

    public void SetShapeColor(Color color)
    {   
        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            _currentShape[i].Color = color;
        }
    }

    public void SetCubeColor(Color color)
    {   
        var cube = _miscShapes[ShapeType.CenterCube];
        
        for (int i = 0; i < cube.Length; i++)
        {
            cube[i].Color = color;
        }
    }

    private void ColorLinear() 
    {   
        Color color = State.CurrentColor;
        int speed = (int)(State.GameSpeedFactor * 30f);

        // Done?
        if (_linearIndex + speed >= Config.totalShapeSize)
        {   
            ColorLinearCleanup(color);
            return;
        }
        // Only color points not impacted by a comet
        ColorLinearApply(color, speed);
        _linearIndex += speed;
    }

    private void ColorLinearApply(Color color, int speed) 
    {   
        // Special case
        if (State.GravityState == GravityType.AntiGravity)
        {
            for (int i = _linearIndex; i < _linearIndex + speed; i++)
            {   
                if (!_isDebris[i]) _currentShape[i].Color = _currentShapeCopy[i].Color = color;
            }
            return;
        }
        // Normal
        for (int i = _linearIndex; i < _linearIndex + speed; i++)
        {   
            if (!_isDebris[i]) _currentShape[i].Color = color;
        }
    }

    private void ColorLinearCleanup(Color color) 
    {   
        for (int i = _linearIndex; i < Config.totalShapeSize; i++)
        {   
            if (!_isDebris[i]) _currentShape[i].Color = color;
        }

        ShapeColorsReset();
    }

    private void ColorShapeFromOrigin(float distance) 
    {
        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            if (_currentShape[i].Position.Length() < distance)
            {
                _currentShape[i].Color = Config.cubeColor;
            }
        }
    }
}
