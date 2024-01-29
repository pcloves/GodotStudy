using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GodotStudy.niceeffort1.main;

public partial class Main : Node2D
{
    private GpuParticles2D _boidParticles;

    private int _numBoids = 10000;
    private readonly List<Vector2> _boidPos = new();
    private readonly List<Vector2> _boidVel = new();

    [Export(PropertyHint.Range, "0, 50")] private float _friendRadius = 30.0f;
    [Export(PropertyHint.Range, "0, 50")] private float _avoidRadius = 15.0f;
    [Export(PropertyHint.Range, "0, 100")] private float _minVel = 25.0f;
    [Export(PropertyHint.Range, "0, 100")] private float _maxVel = 50.0f;
    [Export(PropertyHint.Range, "0, 100")] private float _alignmentFactor = 10.0f;
    [Export(PropertyHint.Range, "0, 100")] private float _cohesionFactor = 1.0f;
    [Export(PropertyHint.Range, "0, 100")] private float _separationFactor = 2.0f;

    private int _imageSize = (int)Math.Ceiling(Math.Sqrt(100));
    private Image _boidData;
    private ImageTexture _boidDataTexture;

    public override void _Ready()
    {
        _boidParticles = GetNodeOrNull<GpuParticles2D>("%BoidParticles");
        _imageSize = (int)Math.Ceiling(Math.Sqrt(_numBoids));

        GenerateBoids();

        for (var i = 0; i < _boidPos.Count; i++)
        {
            GD.Print($"Boid:{i}, Pos:{_boidPos[i]}, Vel:{_boidVel[i]}");
        }

        _boidData = Image.Create(_imageSize, _imageSize, false, Image.Format.Rgbah);
        _boidDataTexture = ImageTexture.CreateFromImage(_boidData);


        _boidParticles.Amount = _numBoids;
        var material = _boidParticles.ProcessMaterial as ShaderMaterial;
        material!.SetShaderParameter("boid_data", _boidDataTexture);
    }

    private void GenerateBoids()
    {
        for (var i = 0; i < _numBoids; i++)
        {
            _boidPos.Add(new Vector2((float)(Random.Shared.NextDouble() * GetViewportRect().Size.X),
                (float)(Random.Shared.NextDouble() * GetViewportRect().Size.Y)));
            _boidVel.Add(new Vector2((float)((Random.Shared.NextDouble() * 2.0f - 1.0f) * _maxVel),
                (float)((Random.Shared.NextDouble() * 2.0f - 1.0f) * _maxVel)));
        }
    }

    private void UpdateDataTexture()
    {
        for (var i = 0; i < _numBoids; i++)
        {
            var pixelPos = new Vector2I(i / _imageSize, i % _imageSize);
            _boidData.SetPixelv(pixelPos, new Color(_boidPos[i].X, _boidPos[i].Y, _boidVel[i].Angle(), 0));
        }

        _boidDataTexture.Update(_boidData);
    }

    public override void _Process(double delta)
    {
        UpdateBoidsCpu(delta);
        UpdateDataTexture();
    }

    private void UpdateBoidsCpu(double delta)
    {
        var viewportRect = GetViewportRect();
        ParallelEnumerable.Range(0, _numBoids)
            .ForAll(i =>
            {
                var myPos = _boidPos[i];
                var myVel = _boidVel[i];

                var avgVel = Vector2.Zero;
                var midPoint = Vector2.Zero;
                var separationVec = Vector2.Zero;

                var numFriends = 0;
                var numAvoids = 0;

                for (var j = 0; j < _numBoids; j++)
                {
                    if (i == j) continue;

                    var otherPos = _boidPos[j];
                    var otherVel = _boidVel[j];
                    var distSquared = myPos.DistanceSquaredTo(otherPos);

                    if (distSquared < _friendRadius * _friendRadius)
                    {
                        numFriends += 1;
                        //此处计算的是所有朋友的总速度之和
                        avgVel += otherVel;
                        //同理，此处计算的是所有朋友的总位置之和
                        midPoint += otherPos;

                        if (distSquared < _avoidRadius * _avoidRadius)
                        {
                            numAvoids += 1;
                            //
                            separationVec += myPos - otherPos;
                        }
                    }
                }

                if (numFriends > 0)
                {
                    avgVel /= numFriends;
                    myVel += avgVel.Normalized() * _alignmentFactor;

                    midPoint /= numFriends;
                    myVel += (midPoint - myPos).Normalized() * _cohesionFactor;

                    if (numAvoids > 0)
                    {
                        myVel += separationVec.Normalized() * _separationFactor;
                    }
                }

                var velMag = myVel.Length();
                velMag = Mathf.Clamp(velMag, _minVel, _maxVel);

                myVel = myVel.Normalized() * velMag;

                myPos += myVel * (float)delta;
                myPos = new Vector2(Mathf.Wrap(myPos.X, 0, viewportRect.Size.X), Mathf.Wrap(myPos.Y, 0, viewportRect.Size.Y));

                _boidPos[i] = myPos;
                _boidVel[i] = myVel;
            });
    }
}