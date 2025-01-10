// Version: 1.2.0
using System.Collections.Generic;
using AssetKits.ParticleImage.Enumerations;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AssetKits.ParticleImage {
    public class Particle
    {
        private ParticleImage _source;
        private Transform _transform;
        
        private Vector2 _modifiedPosition;
        private Vector2 _position;
        private Vector2 _startVelocity;
        private Vector2 _gravityVelocity;
        private Vector2 _velocity;
        private Vector3 _startRotation;
        private Vector3 _startSize;
        private float _time;
        private float _normalizedTime;
        private Color _startColor;
        private float _lifetime;
        
        private Vector3 _size;
        private Color _color;
        private Vector3 _rotation;

        private float _sizeLerp;
        private float _colorLerp;
        private float _rotateLerp;
        private float _attractorLerp;
        private float _gravityLerp;
        private float _vortexLerp;
        private float _frameOverTimeLerp;
        private float _velocityLerp;
        private float _speedLerp;
        private float _startFrameLerp;
        private float _ratioRandom;
        private Vector2 _attractorTargetPoint;

        private Vector3 _lastTransformPosition;
        private Quaternion _lastTransformRotation;
        
        private Vector3 _transformDeltaRotation;

        private Vector2 _lastPosition;
        private Vector2 _deltaPosition;
        
        private Vector3 _direction;

        private Vector2 _trailLastPos;
        private Vector2 _trailDeltaPos;
        private bool _hasTrail;

        private float _frameDelta;
        private int _frameId;
        private int _sheetId;
        
        private List<TrailPoint> _trailPoints = new List<TrailPoint>(128);

        public List<TrailPoint> trailPoints
        {
            get => _trailPoints;
        }

        private Vector2[] _points = new Vector2[4];
        private Vector2[] _rotations = new Vector2[4];
        
        private Vector2 lastTrailPoint;
        
        public Vector2[] points
        {
            get => _points;
        }

        public struct TrailPoint
        {
            public Vector2 point;
            public float time;

            public TrailPoint(Vector2 p, float t)
            {
                point = p;
                time = t;
            }
        }

        public Particle(ParticleImage source)
        {
            _source = source;
            _transform = source.transform;
            _trailLastPos = _position;
        }

        public void Initialize(Vector2 startPosition, Vector2 startVelocity, Vector3 startRotation, Color startColor, Vector3 startSize, float lifetime, float startTime = 0f)
        {
            _sizeLerp = Random.value;
            _colorLerp = Random.value;
            _rotateLerp = Random.value;
            _attractorLerp = Random.value;
            _gravityLerp = Random.value;
            _vortexLerp = Random.value;
            _startFrameLerp = Random.value;
            _frameOverTimeLerp = Random.value;
            _velocityLerp = Random.value;
            _speedLerp = Random.value;
            _ratioRandom = Random.value;
            _attractorTargetPoint = new Vector2(Random.value, Random.value);
            
            _position = startPosition;
            _startVelocity = startVelocity;
            _startColor = startColor;
            _startSize = startSize;
            _startRotation = startRotation;
            _lifetime = lifetime;
            _rotation = _startRotation;
            
            _lastTransformPosition = _transform.position;
            
            _modifiedPosition = _position;
            _velocity = Vector2.zero;
            _gravityVelocity = Vector2.zero;
            _deltaPosition = Vector2.zero;
            
            _transformDeltaRotation = Vector3.zero;
            
            _direction = Vector3.zero;
            _color = _startColor;
            _size = _startSize;
            
            _lastPosition = _position;

            _time = startTime;//0f;
            _normalizedTime = 0f;
            _frameId = 0;

            _frameId += (int)_source.textureSheetStartFrame.Evaluate(_time.Remap(0f, _lifetime, 0f, 1f), _startFrameLerp);
            
            _rotations[0] = new Vector2(_size.x/2, _size.y/2);
            _rotations[1] = new Vector2(-_size.x/2, _size.y/2);
            _rotations[2] = new Vector2(-_size.x/2, -_size.y/2);
            _rotations[3] = new Vector2(_size.x/2, -_size.y/2);
            
            if (_source.trailsEnabled)
            {
                _trailPoints.Clear();
                _trailPoints.Add(new TrailPoint(_position, 0f));
                lastTrailPoint = _position;
                
                _hasTrail = _ratioRandom <= _source.trailRatio;
            }
        }

        public void Simulate(float deltaTime)
        {
            _time += deltaTime;
            _normalizedTime = _time.Remap(0f, _lifetime, 0f, 1f);
            
            _velocity = _startVelocity * _source.speedOverLifetime.Evaluate(_normalizedTime, _speedLerp);
            
            if (_source.space == Simulation.World)
            {
                var inversePoint = _transform.InverseTransformPoint(_lastTransformPosition);
                _modifiedPosition += new Vector2(inversePoint.x, inversePoint.y);
                
                _transformDeltaRotation = Quaternion.Inverse(_transform.rotation).eulerAngles-Quaternion.Inverse(_lastTransformRotation).eulerAngles;
                
                _modifiedPosition = RotatePointAroundCenter(_modifiedPosition, _transformDeltaRotation);
                
                _startVelocity = RotatePointAroundCenter(_startVelocity, _transformDeltaRotation);
                
                _lastTransformPosition = _transform.position;
                _lastTransformRotation = _transform.rotation;
            }

            #region VELOCITY

            if (_source.velocityEnabled)
            {
                if(_source.velocitySpace == Simulation.World)
                {
                    _velocity += RotatePointAroundCenter(_source.velocityOverLifetime.Evaluate(_normalizedTime, _velocityLerp), Quaternion.Inverse(_transform.rotation).eulerAngles);
                }
                else
                {
                    _velocity += _source.velocityOverLifetime.EvaluateXY(_normalizedTime, _velocityLerp);
                }
            }

            #endregion
            
            #region GRAVITY

            if (_source.gravityEnabled)
            {
                _gravityVelocity += RotatePointAroundCenter(new Vector2(0,_source.gravity.Evaluate(_normalizedTime,_gravityLerp)), Quaternion.Inverse(_transform.rotation).eulerAngles) * deltaTime;
            }

            #endregion

            #region NOISE

            if (_source.noiseEnabled)
            {
                float noise = 0f;
                
                if (_source.space == Simulation.Local)
                {
                    noise = _source.noise.GetNoise(_position.x + _source.noiseOffset.x, _position.y + _source.noiseOffset.y);
                }
                else
                {
                    var localPosition = _transform.localPosition;
                    var pos = _position + new Vector2(localPosition.x, localPosition.y);
                    noise = _source.noise.GetNoise(pos.x + _source.noiseOffset.x, pos.y + _source.noiseOffset.y);
                }
                
                _velocity += new Vector2(
                    Mathf.Cos(noise * Mathf.PI), 
                    Mathf.Sin(noise * Mathf.PI)) * _source.noiseStrength;
            }
            
            #endregion
            
            _velocity += _gravityVelocity;
            
            _modifiedPosition += _velocity * (deltaTime * 100);
            
            #region VORTEX

            if (_source.vortexEnabled)
            {
                _modifiedPosition = RotatePointAroundCenter(_modifiedPosition, new Vector3(0,0,_source.vortexStrength.Evaluate(_normalizedTime, _vortexLerp) * deltaTime * 100));
            }

            #endregion
            
            #region ATTRACTOR

            if (_source.attractorEnabled && _source.attractorTarget)
            {
                Vector3 targetPos;
                
                if (_source.attractorTarget is RectTransform)
                {
                    targetPos = _transform.InverseTransformPoint(_source.attractorTarget.position);
                }
                else
                {
                    Vector3 viewportPos = _source.WorldToViewportPoint(_source.attractorTarget.position);
                    _source.attractorType = AttractorType.Pivot;

                    if (_source.canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    {
                        targetPos = new Vector3(
                            ((viewportPos.x.Remap(0.5f, 1.5f,0f,_source.canvasRect.rect.width) - _source.canvasRect.InverseTransformPoint(_transform.position).x + _source.canvasRect.localPosition.x) / _transform.lossyScale.x) * _source.canvasRect.localScale.x, 
                            ((viewportPos.y.Remap(0.5f, 1.5f,0f,_source.canvasRect.rect.height) - _source.canvasRect.InverseTransformPoint(_transform.position).y + _source.canvasRect.localPosition.y) / _transform.lossyScale.y) * _source.canvasRect.localScale.y, 
                            0);
                    }
                    else
                    {
                        targetPos = new Vector3(
                            (viewportPos.x.Remap(0.5f, 1.5f, 0f, _source.canvasRect.rect.width) -
                             _source.canvasRect.InverseTransformPoint(_transform.position).x) / _transform.lossyScale.x * _source.canvasRect.localScale.x,
                            (viewportPos.y.Remap(0.5f, 1.5f, 0f, _source.canvasRect.rect.height) -
                             _source.canvasRect.InverseTransformPoint(_transform.position).y) / _transform.lossyScale.y * _source.canvasRect.localScale.y,
                            0);
                    }
                }

                if(_source.attractorType == AttractorType.Pivot)
                    _position = Vector3.LerpUnclamped(_modifiedPosition, targetPos, _source.attractorLerp.Evaluate(_normalizedTime, _attractorLerp));
                else
                {
                    var rt = _source.attractorTarget as RectTransform;
                    
                    _position = Vector3.LerpUnclamped(_modifiedPosition,
                        new Vector2(
                            targetPos.x + _attractorTargetPoint.x.Remap(0f, 1f, -rt.sizeDelta.x / 2, rt.sizeDelta.x / 2),
                            targetPos.y + _attractorTargetPoint.y.Remap(0f, 1f, -rt.sizeDelta.y / 2, rt.sizeDelta.y / 2)),
                        _source.attractorLerp.Evaluate(_normalizedTime, _attractorLerp));
                }
            }
            else
            {
                _position = _modifiedPosition;
            }

            #endregion

            _deltaPosition = _position - _lastPosition;
            _lastPosition = _position;

            var normalizedSpeed = _deltaPosition.magnitude * (1f / deltaTime) / 100f;
            
            if(float.IsNaN(normalizedSpeed))
                normalizedSpeed = 0f;

            //Apply color
            Color c = _source.colorOverLifetime.Evaluate(_normalizedTime, _colorLerp);
            _color = _startColor * c * _source.colorBySpeed.Evaluate(normalizedSpeed.Remap(_source.colorSpeedRange.from, _source.colorSpeedRange.to, 0f, 1f));

            //Apply size
            Vector3 sol = _source.sizeOverLifetime.Evaluate(_normalizedTime, _sizeLerp);
            Vector3 sbs = _source.sizeBySpeed.Evaluate(normalizedSpeed.Remap(_source.sizeSpeedRange.from, _source.sizeSpeedRange.to, 0f, 1f), _sizeLerp);
            
            _size = Vector3.Scale(_startSize, Vector3.Scale(sbs, sol));
            
            //Apply rotation
            _direction = _deltaPosition;
            
            if (_direction.magnitude == 0f)
            {
                _direction = _velocity;
            }

            _direction = _direction.normalized;
            
            Vector3 rol = Vector3.zero;

            if (_source.rotationOverLifetime.separated)
            {
                float x = 0f;
                float y = 0f;
                float z = 0f;
            
                if (_source.rotationOverLifetime.xCurve.mode == ParticleSystemCurveMode.Constant ||
                    _source.rotationOverLifetime.xCurve.mode == ParticleSystemCurveMode.TwoConstants)
                {
                    x = _time.Remap(0f, _lifetime, 0f,
                        _source.rotationOverLifetime.xCurve.Evaluate(_rotateLerp, _rotateLerp));
                }
                else
                {
                    x = _source.rotationOverLifetime.xCurve.Evaluate(_normalizedTime, _rotateLerp);
                }
                if (_source.rotationOverLifetime.yCurve.mode == ParticleSystemCurveMode.Constant ||
                    _source.rotationOverLifetime.yCurve.mode == ParticleSystemCurveMode.TwoConstants)
                {
                    y = _time.Remap(0f, _lifetime, 0f,
                        _source.rotationOverLifetime.yCurve.Evaluate(_rotateLerp, _rotateLerp));
                }
                else
                {
                    y = _source.rotationOverLifetime.yCurve.Evaluate(_normalizedTime, _rotateLerp);
                }
                if (_source.rotationOverLifetime.zCurve.mode == ParticleSystemCurveMode.Constant ||
                    _source.rotationOverLifetime.zCurve.mode == ParticleSystemCurveMode.TwoConstants)
                {
                    z = _time.Remap(0f, _lifetime, 0f,
                        _source.rotationOverLifetime.zCurve.Evaluate(_rotateLerp, _rotateLerp));
                }
                else
                {
                    z = _source.rotationOverLifetime.zCurve.Evaluate(_normalizedTime, _rotateLerp);
                }
                
                rol = new Vector3(x, y, z);
            
                if (!_source.alignToDirection)
                {
                    rol += Quaternion.Inverse(_source.transform.rotation).eulerAngles;
                }
            }
            else
            {
                switch (_source.rotationOverLifetime.mainCurve.mode)
                {
                    case ParticleSystemCurveMode.Constant:
                    case ParticleSystemCurveMode.TwoConstants:
                        rol = new Vector3(0, 0, _time.Remap(0f,_lifetime,0f,_source.rotationOverLifetime.mainCurve.Evaluate(_normalizedTime, _rotateLerp)));
                        break;
                    case ParticleSystemCurveMode.Curve:
                    case ParticleSystemCurveMode.TwoCurves:
                        rol = new Vector3(0, 0, _source.rotationOverLifetime.mainCurve.Evaluate(_normalizedTime, _rotateLerp));
                        break;
                }
                
                if (!_source.alignToDirection)
                {
                    rol += new Vector3(0,0,Quaternion.Inverse(_source.transform.rotation).eulerAngles.z);
                }
            }
            
            Vector3 rbs;

            if (_source.rotationBySpeed.separated)
            {
                rbs = _source.rotationBySpeed.Evaluate(normalizedSpeed.Remap(_source.rotationSpeedRange.from, _source.rotationSpeedRange.to, 0f, 1f), _rotateLerp);
            }
            else
            {
                rbs = _source.rotationBySpeed.EvaluateZ(normalizedSpeed.Remap(_source.rotationSpeedRange.from, _source.rotationSpeedRange.to, 0f, 1f), _rotateLerp);
            }
            
            if (_source.alignToDirection)
            {
                Quaternion q = Quaternion.FromToRotation(Vector3.up, _direction);
                _rotation = _startRotation + Quaternion.Euler(new Vector3(0,0, q.eulerAngles.z)).eulerAngles;
            }
            else
            {
                _rotation = _startRotation;
            }
            
            _rotation += rol + rbs;
            
            //Render
            
            //Trail
            if (_source.trailsEnabled && _trailPoints.Count > 0)
            {
                var trailVertices = new NativeArray<Vector3>(_trailPoints.Count * 2, Allocator.Temp);
                var trailColors = new NativeArray<Color>(_trailPoints.Count * 2, Allocator.Temp);
                var trailTriangles = new NativeArray<int>((_trailPoints.Count - 1) * 6, Allocator.Temp);

                //Attach first point to the current position
                if (_time < _lifetime)
                {
                    TrailPoint tp = new TrailPoint(_position, _time);
                    _trailPoints[_trailPoints.Count - 1] = tp;
                }

                //Last point lerp
                if (trailPoints.Count > 1)
                {
                    var lastFollowingPoint = _trailPoints[0];
                    lastFollowingPoint.point = Vector2.Lerp(trailPoints[1].point, lastTrailPoint, Mathf.Abs(_time.Remap(_trailPoints[0].time+_source.trailLifetime, _trailPoints[1].time+_source.trailLifetime, 0f, 1f)));
                    _trailPoints[0] = lastFollowingPoint;
                }
                
                var trailLength = Vector3.Distance(_trailPoints[0].point, _position);
                
                for (var i = 0; i < _trailPoints.Count; i++)
                {
                    Vector2 pointDirection = _trailPoints[i].point;
                    float pointDistance = (i > 0) ? Vector3.Distance(_trailPoints[0].point, _trailPoints[i].point).Remap(0, trailLength, 1f, 0f) : 1f;
                    float pointWidth = _size.x * _source.trailWidth.Evaluate(pointDistance, _sizeLerp);

                    if (_trailPoints.Count > 1)
                    {
                        if (i < _trailPoints.Count - 1)
                        {
                            pointDirection = _trailPoints[i + 1].point - _trailPoints[i].point;
                        }
                        else
                        {
                            pointDirection = _trailPoints[i].point - _trailPoints[i - 1].point;
                        }
                    }
                    
                    
                    Vector2 mid = _trailPoints[i].point; // the mid-point between start and end.
                    Vector2 perp = Vector2.Perpendicular(pointDirection.normalized); // vector of length 1 perpendicular to v.

                    Color tc = _source.trailColorOverTrail.Evaluate(pointDistance, _colorLerp) *
                               _source.trailColorOverLifetime.Evaluate(_normalizedTime, _colorLerp);
                    
                    if (_source.inheritParticleColor)
                    { 
                        tc *= _color;
                    }
                    
                    trailVertices[i * 2 + 1] = mid + (perp * pointWidth) / 2;
                    trailVertices[i * 2] = mid - (perp * pointWidth) / 2;
                    trailColors[i * 2] = tc;
                    trailColors[i * 2 + 1] = tc;
                }
                
                for (int i = 0; i < trailPoints.Count - 1; i++)
                {
                    trailTriangles[i * 6] = i * 2;
                    trailTriangles[i * 6 + 1] = i * 2 + 1;
                    trailTriangles[i * 6 + 2] = i * 2 + 2;
                    trailTriangles[i * 6 + 3] = i * 2 + 2;
                    trailTriangles[i * 6 + 4] = i * 2 + 1;
                    trailTriangles[i * 6 + 5] = i * 2 + 3;
                }
                
                _source.particleTrailRenderer.UpdateMeshData(trailVertices, trailTriangles, trailColors);
                
                if (_time >= _trailPoints[0].time+_source.trailLifetime)
                {
                    _trailPoints.RemoveAt(0);
                    lastTrailPoint = _trailPoints[0].point;
                }
                
                if (_time < _lifetime && _hasTrail)
                {
                    _trailDeltaPos = _trailLastPos - _position;
                    if (_trailDeltaPos.magnitude > _source.minimumVertexDistance)
                    {
                        _trailLastPos = _position;
                        _trailPoints.Add(new TrailPoint(_position, _time));
                    }
                }
            }

            var sheets = _source.sheetsArray;

            switch (_source.textureSheetType)
            {
                case SheetType.Speed:
                    _frameId = (int)_velocity.magnitude.Remap(_source.textureSheetFrameSpeedRange.from, _source.textureSheetFrameSpeedRange.to, 0f, sheets.Length);
                    break;
                case SheetType.Lifetime:
                    _frameId = (int)(_source.textureSheetFrameOverTime.Evaluate(_normalizedTime,
                        _frameOverTimeLerp)*_source.textureSheetCycles)+(int)_source.textureSheetStartFrame.Evaluate(_normalizedTime, _startFrameLerp);
                    break;
                case SheetType.FPS:
                    float dur = 1f / _source.textureSheetFPS;
                    _frameDelta += deltaTime;
                    while(_frameDelta >= dur)
                    {
                        _frameDelta -= dur;
                        _frameId ++;
                    }
                    break;
            }

            _sheetId = (int)Mathf.Repeat(_frameId, sheets.Length);
            
            _rotations[0] = new Vector3(_size.x/2, _size.y/2);
            _rotations[1] = new Vector3(-_size.x/2, _size.y/2);
            _rotations[2] = new Vector3(-_size.x/2, -_size.y/2);
            _rotations[3] = new Vector3(_size.x/2, -_size.y/2);
                
            RotatePointsAroundCenter(_rotations, _rotation);
            
            _points[0] = _position + _rotations[0];
            _points[1] = _position + _rotations[1];
            _points[2] = _position + _rotations[2];
            _points[3] = _position + _rotations[3];
        }
        
        public int GetSheetId
        {
            get
            {
                if (_source.textureSheetEnabled)
                {
                    return _sheetId;
                }

                return 0;
            }
        }

        private void RotatePointsAroundCenter(Vector2[] points, Vector3 angles)
        {
            Quaternion rotation = Quaternion.Euler(angles);

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = rotation * points[i];
            }
        }
        
        private Vector2 RotatePointAroundCenter(Vector2 point, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point);
        }

        public Vector2 Position => _position;
        public Vector2 Velocity => _velocity;
        public Vector2 Size => _size;
        public float TimeSinceBorn => _time;
        public float Lifetime => _lifetime;
        public Color Color => _color;
    }

    public struct SpriteSheet
    {
        public Vector2 size;
        public Vector2 pos;

        public SpriteSheet(Vector2 s, Vector2 p)
        {
            size = s;
            pos = p;
        }
    }
}

