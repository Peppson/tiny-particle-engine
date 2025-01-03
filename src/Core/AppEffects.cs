
using System;
using Microsoft.Xna.Framework;
using Particle.Global;
using Utility;

namespace Particle.Core;

public partial class App
{ 
    private float _sphereOffset = 1;
    private float _frequencyOffset = 1;
    private float _pointCloudOffset = 0;
    private float _elapsedPointCloudTime = 0;


    public void ShapeEffect(float deltaTime)
    {           
        // Enabled?
        if (!State.IsEffect || State.CurrentAnimation != AnimationType.None) return;

        switch (State.CurrentShape)
        {
            case ShapeType.Sphere:      EffectSphere();                 break;
            case ShapeType.Impact:      EffectImpact();                 break;
            case ShapeType.Frequency:   EffectFrequency();              break;
            case ShapeType.Pointcloud:  EffectPointcloud(deltaTime);    break;
        }
    }

    public void ShapeEffectsReset()
    {
        _sphereOffset = 1;
        _frequencyOffset = 1;
        _pointCloudOffset = 0;
        _elapsedPointCloudTime = 0;
    }

    private void EffectSphere()
    {   
        _sphereOffset += 0.000013f * State.GameSpeedFactor;

        // Slowly morph
        var shape = GetSphere(_sphereOffset);
        for (int i = 0; i < Config.totalShapeSize; i++)
        {
            if (!_isDebris[i]) _currentShape[i].Position = shape[i];
        }
    }
    
    private void EffectImpact() 
    {
        //todo
    }

    private void EffectFrequency()
    {   
        const float distance = 15f;
        _frequencyOffset += 0.00004f * State.GameSpeedFactor;

        var shape = GetFrequency(_frequencyOffset);
        for (int i = 0; i < Config.totalShapeSize; i++)
        {   
            if (!_isDebris[i]) _currentShape[i].Position = shape[i];
        }

        ColorShapeFromOrigin(distance);
    }

    private void EffectPointcloud(float deltaTime)
    {   
        _elapsedPointCloudTime += deltaTime;

        if (_elapsedPointCloudTime < 2300)
            _pointCloudOffset -= 0.002f * State.GameSpeedFactor;
        else if (_elapsedPointCloudTime < 41000)
            _pointCloudOffset += 0.002f * State.GameSpeedFactor;
        else
            _elapsedPointCloudTime = 0;
            
        var shape = GetPointCloud(_pointCloudOffset);
        for (int i = 0; i < Config.totalShapeSize; i++)
        {
            if (!_isDebris[i]) _currentShape[i].Position = shape[i];
        }
    }
}
