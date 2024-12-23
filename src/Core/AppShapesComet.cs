
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GlobalConfig;
using ColorPalette;
using UtilityClass;

namespace monogame;

public partial class App : Game
{
    public bool RenderComet { get; private set; }
    private readonly Vector3 _CometPosStart = new(25, 85, 0);
    private readonly Vector3 _CometPosEnd = new(-180, -100, 0);
    private VertexPositionColor[] _comet;
    private Vector3 _oldCometPos;
    private Vector3[] _debrisDirection;
    private bool[] _isDebris;
    public int _newStep = 0; //TODO
    public int _oldStep = 0; //TODO

    #if DEBUG_COMET
        public VertexPositionColor[] debugComet;
        public VertexPositionColor[] debugCometTrajectory = new VertexPositionColor[2];
    #endif

    
    public void InitAnimationComet() // mock constructor for now
    {
        RenderComet = false;
        _debrisDirection = new Vector3[Config.totalShapeSize];
        _isDebris = new bool[Config.totalShapeSize];
        _comet = CreateComet();

        #if DEBUG_COMET
            debugComet = CreateCube(1.1f);
            DebugShowTrajectory();
        #endif
    }

    public void AnimationComet()
    {
        const float RotationSpeed = 0.012f;

        if (_newStep == _oldStep) return; //TODO

        _oldStep = _newStep;
        RenderComet = true;

        //Utility.BenchmarkStart();
        Vector3 cometPos = UpdateCometPos();
        MoveComet(ref cometPos, RotationSpeed);
        MoveDebris(cometPos);
        //Utility.BenchmarkStop();
    }
    
    private Vector3 UpdateCometPos()
    {
        // Move the comet along its trajectory in steps
        float steps = _newStep / 400f;
        
        return Vector3.Lerp(_CometPosStart, _CometPosEnd, steps);
    }

    private void MoveComet(ref Vector3 cometPos, float rotationSpeed)
    {   
        if (IsCometOutOfBounds(ref cometPos)) return;
            
        // Position delta 
        Vector3 movementDelta = cometPos - _oldCometPos;
        _oldCometPos = cometPos;

        // Move and rotate!
        for (int i = 0; i < _comet.Length; i++)
        {
            _comet[i].Position += movementDelta;
        }
        RotateComet(_comet, cometPos, rotationSpeed);

        #if DEBUG_COMET
            DebugShowCube(cometPos);
        #endif
    }

    private bool IsCometOutOfBounds(ref Vector3 cometPos)
    {   
        // Turn of rendering comet, but keep updating it's position for future collisions
        if (cometPos.X < -55)
        {
            RenderComet = false;
        }

        // Comet position no longer needed, all points already impacted
        if (cometPos.X < -88)
        {
            cometPos = Vector3.Zero;
            return true;
        }

        return false;
    }

    private static void RotateComet(VertexPositionColor[] comet, Vector3 cometPos, float speed)
    {
        Matrix rotationMatrix = Matrix.CreateRotationX(speed) *
                                Matrix.CreateRotationY(speed) *
                                Matrix.CreateRotationZ(speed);
                                
        // Rotate each point around center of mass #spicy maths
        for (int i = 0; i < comet.Length; i++)
        {
            comet[i].Position = Vector3.Transform(comet[i].Position - cometPos, rotationMatrix) + cometPos;
        }
    }

    private void MoveDebris(Vector3 cometPos)
    {
        const float Sensitivity = 30f;
        for (int i = 0; i < _currentShape.Length; i++)
        {
            // Used to have some bounding box logic to filter the points,
            // but just checking indices was MUCH faster.
            if (i < 5000 || i > 8500) continue; // TODO for each shape

            #if DEBUG_COMET
                _currentShape[i].Color = Color.Red;
            #endif

            float distance = Vector3.Distance(cometPos, _currentShape[i].Position);

            if (distance <= Sensitivity)
            {
                // Direction away from the comet
                Vector3 directionAway = _currentShape[i].Position - cometPos;
                directionAway.Normalize();

                // Closer points gets more velocity
                float velocity = Map(distance, Sensitivity - 0.2f, Sensitivity, 1f, 1.5f);

                // Move the points!
                _currentShape[i].Position += directionAway * velocity;
                _currentShape[i].Color = NordColors.Yellow;

                // Store impacted points as "debris" for future frames
                _debrisDirection[i] = directionAway;
                _isDebris[i] = true;
            }

            // Continue moving debris
            else if (_isDebris[i])
            {
                _currentShape[i].Position += _debrisDirection[i] * 0.05f;
            }
        }
    }

    #if DEBUG_COMET
        private void DebugShowCube(Vector3 cometPos)
        {
            debugComet = CreateCube(1.1f);
            for (int i = 0; i < debugComet.Length; i++)
            {
                debugComet[i].Position += cometPos;
                debugComet[i].Color = Color.Red;
            }
        }

        private void DebugShowTrajectory()
        {
            for (int i = 0; i < debugComet.Length; i++)
            {
                debugComet[i].Position += _CometPosStart;
                debugComet[i].Color = Color.Red;
            }
            
            debugCometTrajectory[0].Color = Color.Red;
            debugCometTrajectory[0].Position = _CometPosStart;
            debugCometTrajectory[1].Position = _CometPosEnd;
        }

        private static int[] DebugLoadCometLines()
        {
            var lines = File.ReadAllLines("CometLines.txt");
            List<int> indices = [];

            foreach (var line in lines)
            {
                var parts = line.Split(' ');
                if (parts.Length == 2)
                {
                    indices.Add(int.Parse(parts[0]));
                    indices.Add(int.Parse(parts[1]));
                }
            }

            return indices.ToArray();
        }
    #endif
}
