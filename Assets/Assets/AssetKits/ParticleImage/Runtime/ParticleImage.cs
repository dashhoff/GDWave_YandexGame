// Version: 1.2.0
#if UNITY_BURST && UNITY_MATHEMATICS && UNITY_COLLECTIONS
#define PARTICLE_IMAGE_JOBS
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using AssetKits.ParticleImage.Enumerations;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.Sprites;
using PlayMode = AssetKits.ParticleImage.Enumerations.PlayMode;

#if PARTICLE_IMAGE_JOBS
using AssetKits.ParticleImage.Jobs;
using Unity.Jobs;
using Unity.Mathematics;
#endif

namespace AssetKits.ParticleImage
{
    [AddComponentMenu("UI/Particle Image/Particle Image")]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ParticleImage : MaskableGraphic
    {
        [SerializeField]
        private ParticleImage _main;
        [SerializeField]
        private ParticleImage[] _children;

        /// <summary>
        /// Child emitters of this emitter.
        /// </summary>
        public ParticleImage[] children
        {
            get
            {
                return _children;
            }
            private set
            {
                _children = value;
            }
        }
        
        /// <summary>
        /// Root emitter of this system.
        /// </summary>
        public ParticleImage main
        {
            get
            {
                if (_main == null) _main = GetMain();
                return _main;
            }
            private set
            {
                _main = value;
            }
        }

        /// <summary>
        /// Returns true if this emitter is the root emitter of this system.
        /// </summary>
        public bool isMain
        {
            get
            {
                return main == this;
            }
        }

        private RectTransform _canvasRect;
        public RectTransform canvasRect
        {
            get
            {
                return _canvasRect;
            }
            set
            {
                _canvasRect = value;
            }
        }

        private Mesh _mesh;

        public Mesh mesh
        {
            get
            {
                if(_mesh == null)
                {
                    _mesh = new Mesh();
                    _mesh.MarkDynamic();
                }
                
                return _mesh;
            }
        }
        
        private Mesh.MeshDataArray _meshDataArray;
        private Mesh.MeshData _meshData;
        
        private Mesh.MeshDataArray _trailMeshDataArray;
        private Mesh.MeshData _trailMeshData;
        
        [SerializeField]
        private Simulation _space = Simulation.Local;
        public Simulation space
        {
            get => _space;
            set
            {
                _space = value;
            }
        }
        
        [SerializeField]
        private TimeScale _timeScale = TimeScale.Normal;
        public TimeScale timeScale
        {
            get => _timeScale;
            set
            {
                _timeScale = value;
            }
        }

        [SerializeField]
        private Module _emitterConstraintEnabled = new Module(false);
        public bool emitterConstraintEnabled
        {
            get
            {
                return _emitterConstraintEnabled.enabled;
            }
            set
            {
                _emitterConstraintEnabled.enabled = value;
            }
        }
        
        [SerializeField]
        private Transform _emitterConstraintTransform;
        public Transform emitterConstraintTransform
        {
            get => _emitterConstraintTransform;
            set
            {
                _emitterConstraintTransform = value;
            }
        }
        
        [SerializeField]
        private EmitterShape _shape = EmitterShape.Circle;
        /// <summary>
        ///   <para>The type of shape to emit particles from.</para>
        /// </summary>
        public EmitterShape shape
        {
            get => _shape;
            set => _shape = value;
        }
        
        [SerializeField]
        private SpreadType _spread = SpreadType.Random;
        
        /// <summary>
        ///  <para>The type of spread to use when emitting particles.</para>
        /// </summary>
        public SpreadType spreadType
        {
            get => _spread;
            set => _spread = value;
        }
        
        [SerializeField]
        private float _spreadLoop = 1;
        
        /// <summary>
        /// Loop count for spread.
        /// </summary>
        public float spreadLoop
        {
            get => _spreadLoop;
            set => _spreadLoop = value;
        }
        
        [SerializeField]
        private float _startDelay = 0;
        
        /// <summary>
        ///   <para>Start delay in seconds.</para>
        /// </summary>
        public float startDelay
        {
            get => _startDelay;
            set => _startDelay = value;
        }
        
        [SerializeField]
        private float _radius = 50;
        /// <summary>
        ///   <para>Radius of the circle shape to emit particles from.</para>
        /// </summary>
        public float circleRadius
        {
            get => _radius;
            set => _radius = value;
        }
        
        [SerializeField]
        private float _width = 100;
        /// <summary>
        ///   <para>Width of the rectangle shape to emit particles from.</para>
        /// </summary>
        public float rectWidth
        {
            get => _width;
            set => _width = value;
        }
        
        [SerializeField]
        private float _height = 100;
        /// <summary>
        ///   <para>Height of the rectangle shape to emit particles from.</para>
        /// </summary>
        public float rectHeight
        {
            get => _height;
            set => _height = value;
        }
        
        [SerializeField]
        private float _angle = 45;
        /// <summary>
        ///   <para>Angle of the directional shape to emit particles from.</para>
        /// </summary>
        public float directionAngle
        {
            get => _angle;
            set => _angle = value;
        }

        [SerializeField]
        private float _length = 100f;
        /// <summary>
        ///   <para>Length of the line shape to emit particles from.</para>
        /// </summary>
        public float lineLength
        {
            get => _length;
            set => _length = value;
        }
        
        [SerializeField]
        private bool _fitRect;
        
        public bool fitRect
        {
            get
            {
                return _fitRect;
            }
            set
            {
                _fitRect = value;
                if(value) 
                    FitRect();
            }
        }
        
        [SerializeField]
        private bool _emitOnSurface = true;
        /// <summary>
        ///   <para>Emit on the whole surface of the current shape.</para>
        /// </summary>
        public bool emitOnSurface
        {
            get => _emitOnSurface;
            set => _emitOnSurface = value;
        }
        
        [SerializeField]
        private float _emitterThickness;
        /// <summary>
        ///   <para>Thickness of the shape's edge from which to emit particles if emitOnSurface is disabled.</para>
        /// </summary>
        public float emitterThickness
        {
            get => _emitterThickness;
            set => _emitterThickness = value;
        }
        
        [SerializeField]
        private bool _loop = true;
        /// <summary>
        ///   <para>Determines whether the Particle Image is looping.</para>
        /// </summary>
        public bool loop
        {
            get => _loop;
            set => _loop = value;
        }
        
        [SerializeField]
        private bool _prewarm;
        
        public bool prewarm
        {
            get => _prewarm;
            set => _prewarm = value;
        }
        
        [SerializeField]
        private float _duration = 5f;
        /// <summary>
        ///   <para>The duration of the Particle Image in seconds</para>
        /// </summary>
        public float duration
        {
            get => _duration;
            set => _duration = value;
        }
        
        [SerializeField]
        private PlayMode _playMode = PlayMode.OnAwake;
        public PlayMode PlayMode
        {
            get
            {
                return _playMode;
            }
            set
            {
                _playMode = value;
                if (isMain && children != null)
                {
                    foreach (var particleImage in children)
                    {
                        particleImage._playMode = value;
                    }
                }
                else if(!isMain)
                {
                    main.PlayMode = value;
                }
            }
        }
        
        [SerializeField]
        private SeparatedMinMaxCurve _startSize = new SeparatedMinMaxCurve(40f);
        public SeparatedMinMaxCurve startSize
        {
            get => _startSize;
            set => _startSize = value;
        }
        
        [SerializeField]
        private ParticleSystem.MinMaxGradient _startColor = new ParticleSystem.MinMaxGradient(Color.white);
        public ParticleSystem.MinMaxGradient startColor
        {
            get => _startColor;
            set => _startColor = value;
        }
        
        [SerializeField]
        private ParticleSystem.MinMaxCurve _lifetime = new ParticleSystem.MinMaxCurve(1f);
        public ParticleSystem.MinMaxCurve lifetime
        {
            get => _lifetime;
            set => _lifetime = value;
        }
        
        [SerializeField]
        private ParticleSystem.MinMaxCurve _startSpeed = new ParticleSystem.MinMaxCurve(2f);
        public ParticleSystem.MinMaxCurve startSpeed
        {
            get => _startSpeed;
            set => _startSpeed = value;
        }
        
        [SerializeField]
        private ParticleSystem.MinMaxGradient _colorOverLifetime = new ParticleSystem.MinMaxGradient(new Gradient());
        public ParticleSystem.MinMaxGradient colorOverLifetime
        {
            get => _colorOverLifetime;
            set => _colorOverLifetime = value;
        }
        
        [SerializeField]
        private ParticleSystem.MinMaxGradient _colorBySpeed = new ParticleSystem.MinMaxGradient(new Gradient());
        public ParticleSystem.MinMaxGradient colorBySpeed
        {
            get => _colorBySpeed;
            set => _colorBySpeed = value;
        }
        
        [SerializeField]
        private SpeedRange _colorSpeedRange = new SpeedRange(0f, 1f);
        public SpeedRange colorSpeedRange
        {
            get => _colorSpeedRange;
            set => _colorSpeedRange = value;
        }
        
        [SerializeField]
        private SeparatedMinMaxCurve _sizeOverLifetime = new SeparatedMinMaxCurve(new AnimationCurve(new []{new Keyframe(0f,1f), new Keyframe(1f,1f)}));
        public SeparatedMinMaxCurve sizeOverLifetime
        {
            get => _sizeOverLifetime;
            set => _sizeOverLifetime = value;
        }

        [SerializeField]
        private SeparatedMinMaxCurve _sizeBySpeed = new SeparatedMinMaxCurve(new AnimationCurve(new []{new Keyframe(0f,1f), new Keyframe(1f,1f)}));
        public SeparatedMinMaxCurve sizeBySpeed
        {
            get => _sizeBySpeed;
            set => _sizeBySpeed = value;
        }
        
        [SerializeField]
        private SpeedRange _sizeSpeedRange = new SpeedRange(0f, 1f);
        public SpeedRange sizeSpeedRange
        {
            get => _sizeSpeedRange;
            set => _sizeSpeedRange = value;
        }
        
        [SerializeField]
        private SeparatedMinMaxCurve _startRotation = new SeparatedMinMaxCurve(0f);
        public SeparatedMinMaxCurve startRotation
        {
            get => _startRotation;
            set => _startRotation = value;
        }
        
        [SerializeField]
        private SeparatedMinMaxCurve _rotationOverLifetime = new SeparatedMinMaxCurve(0f);
        public SeparatedMinMaxCurve rotationOverLifetime
        {
            get => _rotationOverLifetime;
            set => _rotationOverLifetime = value;
        }
        
        [SerializeField]
        private SeparatedMinMaxCurve _rotationBySpeed = new SeparatedMinMaxCurve(new AnimationCurve(new []{new Keyframe(0f,1f), new Keyframe(1f,1f)}));
        public SeparatedMinMaxCurve rotationBySpeed
        {
            get => _rotationBySpeed;
            set => _rotationBySpeed = value;
        }
        
        [SerializeField]
        private SpeedRange _rotationSpeedRange = new SpeedRange(0f, 1f);
        public SpeedRange rotationSpeedRange
        {
            get => _rotationSpeedRange;
            set => _rotationSpeedRange = value;
        }
        
        [SerializeField]
        private ParticleSystem.MinMaxCurve _speedOverLifetime = new ParticleSystem.MinMaxCurve(1f);
        public ParticleSystem.MinMaxCurve speedOverLifetime
        {
            get => _speedOverLifetime;
            set => _speedOverLifetime = value;
        }

        [SerializeField]
        private bool _alignToDirection;
        /// <summary>
        ///   <para>Align particles based on their direction of travel.</para>
        /// </summary>
        public bool alignToDirection
        {
            get => _alignToDirection;
            set => _alignToDirection = value;
        }
        
        [SerializeField]
        private ParticleSystem.MinMaxCurve _gravity = new ParticleSystem.MinMaxCurve(-9.81f);
        public ParticleSystem.MinMaxCurve gravity
        {
            get => _gravity;
            set => _gravity = value;
        }

        [SerializeField]
        private Module _targetModule = new Module(false);
        public bool attractorEnabled
        {
            get
            {
                return _targetModule.enabled;
            }
            set
            {
                _targetModule.enabled = value;
            }
        }
        
        [SerializeField]
        private Transform _attractorTarget;
        public Transform attractorTarget
        {
            get => _attractorTarget;
            set => _attractorTarget = value;
        }
        
        [SerializeField]
        private ParticleSystem.MinMaxCurve _toTarget = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new []{new Keyframe(0f,0f), new Keyframe(1f,1f)}));
        public ParticleSystem.MinMaxCurve attractorLerp
        {
            get => _toTarget;
            set => _toTarget = value;
        }
        
        [SerializeField]
        private AttractorType _targetMode = AttractorType.Pivot;
        public AttractorType attractorType
        {
            get => _targetMode;
            set => _targetMode = value;
        }
        
        [SerializeField]
        private Module _noiseModule = new Module(false);
        public bool noiseEnabled
        {
            get
            {
                return _noiseModule.enabled;
            }
            set
            {
                _noiseModule.enabled = value;
            }
        }
        
        [SerializeField]
        private Module _gravityModule = new Module(false);
        public bool gravityEnabled
        {
            get
            {
                return _gravityModule.enabled;
            }
            set
            {
                _gravityModule.enabled = value;
            }
        }
        
        [SerializeField]
        private Module _vortexModule = new Module(false);
        public bool vortexEnabled
        {
            get
            {
                return _vortexModule.enabled;
            }
            set
            {
                _vortexModule.enabled = value;
            }
        }
        
        [SerializeField]
        private Module _velocityModule = new Module(false);
        public bool velocityEnabled
        {
            get
            {
                return _velocityModule.enabled;
            }
            set
            {
                _velocityModule.enabled = value;
            }
        }
        
        [SerializeField]
        private Simulation _velocitySpace;
        public Simulation velocitySpace
        {
            get => _velocitySpace;
            set => _velocitySpace = value;
        }
        
        [SerializeField]
        private SeparatedMinMaxCurve _velocityOverLifetime = new SeparatedMinMaxCurve(0f, true, false);
        public SeparatedMinMaxCurve velocityOverLifetime
        {
            get => _velocityOverLifetime;
            set => _velocityOverLifetime = value;
        }
        
        [SerializeField]
        private ParticleSystem.MinMaxCurve _vortexStrength;
        public ParticleSystem.MinMaxCurve vortexStrength
        {
            get => _vortexStrength;
            set => _vortexStrength = value;
        }

        [SerializeField]
        private Module _sheetModule = new Module(false);
        public bool textureSheetEnabled
        {
            get
            {
                return _sheetModule.enabled;
            }
            set
            {
                _sheetModule.enabled = value;
            }
        }
        
        [SerializeField]
        private Vector2Int _textureTile = Vector2Int.one;
        public Vector2Int textureTile
        {
            get => _textureTile;
            set => _textureTile = value;
        }

        [SerializeField]
        private SheetType _sheetType = SheetType.FPS;
        public SheetType textureSheetType
        {
            get => _sheetType;
            set => _sheetType = value;
        }

        public NativeArray<SpriteSheet> sheetsArray;

        [SerializeField]
        private ParticleSystem.MinMaxCurve _frameOverTime;
        public ParticleSystem.MinMaxCurve textureSheetFrameOverTime
        {
            get => _frameOverTime;
            set => _frameOverTime = value;
        }

        [SerializeField]
        private ParticleSystem.MinMaxCurve _startFrame = new ParticleSystem.MinMaxCurve(0f);
        public ParticleSystem.MinMaxCurve textureSheetStartFrame
        {
            get => _startFrame;
            set => _startFrame = value;
        }

        [SerializeField]
        private SpeedRange _frameSpeedRange = new SpeedRange(0f, 1f);
        public SpeedRange textureSheetFrameSpeedRange
        {
            get => _frameSpeedRange;
            set => _frameSpeedRange = value;
        }

        [SerializeField]
        private int _textureSheetFPS = 25;
        public int textureSheetFPS
        {
            get => _textureSheetFPS;
            set => _textureSheetFPS = value;
        }

        [SerializeField]
        private int _textureSheetCycles = 1;
        public int textureSheetCycles
        {
            get => _textureSheetCycles;
            set => _textureSheetCycles = value;
        }

        private List<Particle> _particles = new List<Particle>(128);
        
        /// <summary>
        /// List of particles in the system.
        /// </summary>
        public List<Particle> particles => _particles;
        
        private ParticlePool _pool;

        public ParticlePool pool
        {
            get
            {
                if (_pool == null)
                {
                    _pool = new ParticlePool((int)(_rate + _rateOverLifetime + _rateOverDistance),this);
                }

                return _pool;
            }
        }

        public int particleCount
        {
            get
            {
#if PARTICLE_IMAGE_JOBS
                if(multithreadEnabled)
                    return JobParticles.Length;
#endif
                return _particles.Count;
            }
        }
        
#if PARTICLE_IMAGE_JOBS
        private NativeList<ParticleData> jobParticles;

        public ref NativeList<ParticleData> JobParticles
        {
            get
            {
                if(jobParticles.IsCreated == false && multithreadEnabled)
                    jobParticles = new NativeList<ParticleData>(Allocator.Persistent);
                
                return ref jobParticles;
            }
            //set => jobParticles = value;
        }
        
        private NativeList<FixedList4096Bytes<TrailPointData>> trailData;
        
        public NativeList<FixedList4096Bytes<TrailPointData>> TrailData
        {
            get
            {
                if(trailData.IsCreated == false && multithreadEnabled)
                    trailData = new NativeList<FixedList4096Bytes<TrailPointData>>(Allocator.Persistent);
                
                return trailData;
            }
        }

        private ParticleJob _particleJob;
        private JobHandle _particleJobHandle;
        
        private TrailJob _trailJob;
        private JobHandle _trailJobHandle;
        
        private Vector3 _lastTransformPosition;
        private Quaternion _lastTransformRotation;
        private Vector3 _transformDeltaRotation;
#endif
        
        [SerializeField]
        private float _rate = 50;
        
        /// <summary>
        /// The rate at which the emitter spawns new particles per second.
        /// </summary>
        public float rateOverTime
        {
            get => _rate;
            set => _rate = value;
        }
        
        [SerializeField]
        private float _rateOverLifetime = 0;
        
        /// <summary>
        /// The rate at which the emitter spawns new particles over emitter duration.
        /// </summary>
        public float rateOverLifetime
        {
            get => _rateOverLifetime;
            set => _rateOverLifetime = value;
        }
        
        [SerializeField]
        private float _rateOverDistance = 0;
        
        /// <summary>
        /// The rate at which the emitter spawns new particles over distance per pixel.
        /// </summary>
        public float rateOverDistance
        {
            get => _rateOverDistance;
            set => _rateOverDistance = value;
        }
        
        [SerializeField]
        private List<Burst> _bursts = new List<Burst>(); 

        [FormerlySerializedAs("_trailRenderer")] [SerializeField]
        private ParticleTrailRenderer _particleTrailRenderer;

        public ParticleTrailRenderer particleTrailRenderer
        {
            get
            {
                if (trailsEnabled)
                {
                    if (!_particleTrailRenderer)
                    {
                        _particleTrailRenderer = GetComponentInChildren<ParticleTrailRenderer>();

                        if (!_particleTrailRenderer)
                        {
                            GameObject tr = new GameObject("Trails");
                            tr.transform.parent = transform;
                            tr.transform.localPosition = Vector3.zero;
                            tr.transform.localScale = Vector3.one;
                            tr.transform.localEulerAngles = Vector3.zero;
                            tr.AddComponent<CanvasRenderer>();
                            ParticleTrailRenderer r = tr.AddComponent<ParticleTrailRenderer>();
                            r.raycastTarget = false;
                            _particleTrailRenderer = r;
                        }
                    }
                    return _particleTrailRenderer;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _particleTrailRenderer = value;
            }
        }

        [SerializeField] private Module _trailModule;

        /// <summary>
        /// The trails enabled.
        /// </summary>
        public bool trailsEnabled
        {
            get => _trailModule.enabled;
            set => _trailModule.enabled = value;
        }

        [SerializeField] private ParticleSystem.MinMaxCurve _trailWidth = new ParticleSystem.MinMaxCurve(1f,new AnimationCurve(new []{new Keyframe(0f,1f), new Keyframe(1f,0f)}));

        /// <summary>
        /// The width of the trail in pixels.
        /// </summary>
        public ParticleSystem.MinMaxCurve trailWidth
        {
            get => _trailWidth;
            set => _trailWidth = value;
        }
        
        [SerializeField] private float _trailLifetime = 1f;
        
        /// <summary>
        /// Trail lifetime in seconds
        /// </summary>
        public float trailLifetime
        {
            get => _trailLifetime;
            set => _trailLifetime = value;
        }
        
        [SerializeField] private float _minimumVertexDistance = 10f;
        
        /// <summary>
        /// Vertex distance in canvas pixels
        /// </summary>
        public float minimumVertexDistance
        {
            get => _minimumVertexDistance;
            set => _minimumVertexDistance = value;
        }
        
        [SerializeField] private ParticleSystem.MinMaxGradient _trailColorOverLifetime = new ParticleSystem.MinMaxGradient(Color.white);
        
        /// <summary>
        /// The color of the trail over its lifetime.
        /// </summary>
        public ParticleSystem.MinMaxGradient trailColorOverLifetime
        {
            get => _trailColorOverLifetime;
            set => _trailColorOverLifetime = value;
        }
        
        [SerializeField] private ParticleSystem.MinMaxGradient _trailColorOverTrail = new ParticleSystem.MinMaxGradient(Color.white);
        
        /// <summary>
        /// The color of the trail over the lifetime of the trail.
        /// </summary>
        public ParticleSystem.MinMaxGradient trailColorOverTrail
        {
            get => _trailColorOverTrail;
            set => _trailColorOverTrail = value;
        }
        
        [SerializeField] private Material _trailMaterial;
        
        /// <summary>
        ///  The material used to render the trail.
        /// </summary>
        public Material trailMaterial
        {
            get => _trailMaterial;
            set
            {
                _trailMaterial = value;
                if (_particleTrailRenderer)
                {
                    _particleTrailRenderer.material = value;
                    particleTrailRenderer.SetMaterialDirty();
                }
            }
        }
        

        [SerializeField] private bool _inheritParticleColor;

        public bool inheritParticleColor
        {
            get => _inheritParticleColor;
            set => _inheritParticleColor = value;
        }

        [SerializeField] private bool _dieWithParticle = false;
        
        public bool dieWithParticle
        {
            get => _dieWithParticle;
            set => _dieWithParticle = value;
        }
        
        [Range(0f,1f)]
        [SerializeField] private float _trailRatio = 1f;
        
        public float trailRatio
        {
            get => _trailRatio;
            set
            {
                _trailRatio = Mathf.Clamp01(value);
            } 
        }
        
        private float _time;
        public float time => _time;

        private float _playback;
        
        public float playback => _playback;

        private float _loopTimer;

        private float _t;
        private float _t2;
        private float _burstTimer;
        
        private Noise _noise = new Noise();
        public Noise noise
        {
            get => _noise;
            set => _noise = value;
        }
        
        [SerializeField]
        private Vector2 _noiseOffset;
        
        public Vector2 noiseOffset
        {
            get => _noiseOffset;
            set => _noiseOffset = value;
        }
        
        [SerializeField]
        private float _noiseFrequency = 1f;
        public float noiseFrequency
        {
            get => _noiseFrequency;
            set
            {
                _noiseFrequency = value;
                _noise.SetFrequency(_noiseFrequency);
            }
        }
        
        [SerializeField]
        private float _noiseStrength = 1f;
        
        public float noiseStrength
        {
            get => _noiseStrength;
            set => _noiseStrength = value;
        }

        private bool _noiseDebug;
        
        public bool noiseDebug
        {
            get => _noiseDebug;
            set => _noiseDebug = value;
        }
        
        private Vector2Int _noiseViewSize = new Vector2Int(64,64);
        
        public Vector2Int noiseViewSize
        {
            get => _noiseViewSize;
            set => _noiseViewSize = value;
        }
        
        [SerializeField]
        private Module _multithreadModule = new Module(false);
        
        [SerializeField]
        private bool _multithreadEnabled = false;
        public bool multithreadEnabled
        {
            get
            {
                return _multithreadModule.enabled && _multithreadEnabled;
            }
            set
            {
                if (isMain && children != null)
                {
                    foreach (var particleImage in children)
                    {
                        particleImage._multithreadEnabled = value;
                        particleImage._multithreadModule.enabled = value;
                    }
                }
                else if(!isMain)
                {
                    main._multithreadEnabled = value;
                    main._multithreadModule.enabled = value;
                }
            }
        }
        
        
        private bool _emitting;
        
        /// <summary>
        /// Determines if the particle system is emitting.
        /// </summary>
        public bool isEmitting
        {
            get { return _emitting;}
            private set { _emitting = value; }
        }
        
        private bool _playing;
        
        /// <summary>
        /// Determines if the particle system is playing.
        /// </summary>
        public bool isPlaying
        {
            get { return _playing;}
            private set { _playing = value; }
        }
        
        private bool _stopped;
        
        /// <summary>
        /// Determines whether the Particle System is stopped.
        /// </summary>
        public bool isStopped
        {
            get { return _stopped;}
            private set { _stopped = value; }
        }

        private bool _paused;
        
        /// <summary>
        /// Determines whether the Particle System is paused.
        /// </summary>
        public bool isPaused
        {
            get { return _paused;}
            private set { _paused = value; }
        }

        [SerializeField]
        private UnityEvent _onStart = new UnityEvent();
        
        /// <summary>
        /// Called when the particle system starts.
        /// </summary>
        public UnityEvent onParticleStarted => _onStart;

        [SerializeField]
        private UnityEvent _onFirstParticleFinish = new UnityEvent();
        
        /// <summary>
        /// Called when the first piece of a particle finishes.
        /// </summary>
        public UnityEvent onFirstParticleFinished => _onFirstParticleFinish;
        
        [SerializeField]
        private UnityEvent _onParticleFinish = new UnityEvent();
        
        /// <summary>
        /// Called when any piece of a particle finishes.
        /// </summary>
        public UnityEvent onAnyParticleFinished => _onParticleFinish;
        
        [SerializeField]
        private UnityEvent _onLastParticleFinish = new UnityEvent();
        
        /// <summary>
        /// Called when the last piece of a particle finishes.
        /// </summary>
        public UnityEvent onLastParticleFinished => _onLastParticleFinish;
        
        [SerializeField]
        private UnityEvent _onStop = new UnityEvent();
        
        /// <summary>
        /// Called when the particle system is stopped.
        /// </summary>
        public UnityEvent onParticleStop => _onStop;

        private Vector3 _lastPosition;
        private Vector3 _deltaPosition;
        /// <summary>
        /// Delta position of the particle system.
        /// </summary>
        public Vector3 deltaPosition => _deltaPosition;

        private Camera _camera;
        private Camera mainCamera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = Camera.main;
                }

                return _camera;
            }
        }
        
        private bool _firstParticleFinished;
        
        private int _orderPerSec;
        private int _orderOverLife;
        private int _orderOverDistance;

        public bool moduleEmitterFoldout;
        public bool moduleParticleFoldout;
        public bool moduleMovementFoldout;
        public bool moduleEventsFoldout;
        public bool moduleAdvancedFoldout;

        protected override void Awake()
        {
            base.Awake();
            if (isMain)
            {
                children = GetChildren();
            }
            
            if (fitRect)
            {
                FitRect();
            }
            
            main = GetMain();
            main.children = main.GetChildren();
            
            _playMode = main.PlayMode;
            
            _multithreadEnabled = main._multithreadEnabled;
            _multithreadModule.enabled = main._multithreadModule.enabled;
            
            _lastPosition = transform.position;
            
            if (canvas)
            {
                canvasRect = canvas.gameObject.GetComponent<RectTransform>();
            }
            
            RecalculateMasking();
            RecalculateClipping();
            
            Clear();
            if (PlayMode == PlayMode.OnAwake && Application.isPlaying)
            {
                Play();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (isMain)
            {
                children = GetChildren();
            }
            
            if (fitRect)
            {
                FitRect();
            }
            
            main = GetMain();
            main.children = main.GetChildren();
            
            _playMode = main.PlayMode;
            
            _multithreadEnabled = main._multithreadEnabled;
            _multithreadModule.enabled = main._multithreadModule.enabled;
            
            _lastPosition = transform.position;
            
            if (canvas && canvasRect == null)
            {
                canvasRect = canvas.gameObject.GetComponent<RectTransform>();
            }
            
            _noise.SetNoiseType(Noise.NoiseType.OpenSimplex2);
            _noise.SetFrequency(_noiseFrequency / 100f);
            
            if (PlayMode == PlayMode.OnEnable && Application.isPlaying)
            {
                Stop(true);
                Clear();
                Play();
            }
            
            RecalculateMasking();
            RecalculateClipping();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
#if PARTICLE_IMAGE_JOBS
            if(jobParticles.IsCreated)
                jobParticles.Dispose();
                
            if(trailData.IsCreated)
                trailData.Dispose();
#endif
        }

        public ParticleImage GetMain()
        {
            if (transform.parent)
            {
                if (transform.parent.TryGetComponent<ParticleImage>(out ParticleImage p))
                {
                    return p.GetMain();
                }
            }

            return this;
        }
        
        /// <summary>
        /// Get all children of this Particle Image.
        /// </summary>
        /// <returns>
        /// A list of all children ParticleImage.
        /// </returns>
        public ParticleImage[] GetChildren()
        {
            if (transform.childCount <= 0) return null;

            var ch = GetComponentsInChildren<ParticleImage>().Where(t => t != this);

            if (ch.Any())
            {
                return ch.ToArray();
            }

            return null;
        }

        private void OnTransformChildrenChanged()
        {
            main = GetMain();
            if(isMain)
                children = GetChildren();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            main = GetMain();
            if(isMain)
                children = GetChildren();
        }

        /// <summary>
        /// Starts the particle system.
        /// </summary>
        public void Play()
        {
            main.DoPlay();
        }

        private void DoPlay()
        {
            if (isMain && children != null)
            {
                foreach (var child in children)
                {
                    child.DoPlay();
                }
            }
            _t = 1f / _rate;
            _time = 0f;
            _burstTimer = 0f;
            for (int i = 0; i < _bursts.Count; i++)
            {
                _bursts[i].used = false;
            }
            
            isEmitting = true;
            isPlaying = true;
            isPaused = false;
            isStopped = false;

            if (prewarm)
            {
                Prewarm();
            }
            
            Simulate(_timeScale == TimeScale.Normal ? Time.deltaTime : Time.unscaledDeltaTime, prewarm);

            OnParticleStart();
        }

        /// <summary>
        /// Pauses the particle system.
        /// </summary>
        public void Pause()
        {
            main.DoPause();
        }

        private void DoPause()
        {
            if (isMain && children != null)
            {
                foreach (var particleImage in children)
                {
                    particleImage.DoPause();
                }
            }
            
            isEmitting = false;
            isPlaying = false;
            isPaused = true;
        }
        
        /// <summary>
        /// Stops playing the Particle System.
        /// </summary>
        public void Stop()
        {
            Stop(false);
        }

        /// <summary>
        /// Stops playing the Particle System using the supplied stop behaviour.
        /// </summary>
        /// <param name="stopAndClear">
        /// If true, the particle system will be cleared and all emitted particles will be destroyed.
        /// </param>
        public void Stop(bool stopAndClear)
        {
            main.DoStop(stopAndClear);
        }

        private void DoStop(bool stopAndClear)
        {
            if (isMain && children != null)
            {
                foreach (var particleImage in children)
                {
                    particleImage.DoStop(stopAndClear);
                }
            }
            
            _orderPerSec = 0;
            _orderOverLife = 0;
            _orderOverDistance = 0;
            for (int i = 0; i < _bursts.Count; i++)
            {
                _bursts[i].used = false;
            }
            
            if (stopAndClear)
            {
                isStopped = true;
                isPlaying = false;
                Clear();
            }
            isEmitting = false;
            if (isPaused)
            {
                isPaused = false;
                isStopped = true;
                isPlaying = false;
                Clear();
            }
            for (int i = 0; i < _bursts.Count; i++)
            {
                _bursts[i].used = false;
            }
            _firstParticleFinished = false;

            OnParticleStop();
        }

        /// <summary>
        /// Remove all particles from the Particle System.
        /// </summary>
        public void Clear()
        {
            main.DoClear();
        }

        private void DoClear()
        {
            if (isMain && children != null)
            {
                foreach (var particleImage in children)
                {
                    particleImage.DoClear();
                }
            }
            for (int i = 0; i < _bursts.Count; i++)
            {
                _bursts[i].used = false;
            }
            _time = 0;
            _playback = 0;
            _burstTimer = 0;
            _particles.Clear();
#if PARTICLE_IMAGE_JOBS
            if (multithreadEnabled)
            {
                if(jobParticles.IsCreated)
                    jobParticles.Dispose(); 
                if(trailData.IsCreated)
                    trailData.Dispose();
            }
#endif
            mesh.Clear();
            canvasRenderer.SetMesh(mesh);
            SetMaterialDirty();
            if(particleTrailRenderer)
                particleTrailRenderer.Clear();
        }

        void Update()
        {
            Simulate(_timeScale == TimeScale.Normal ? Time.deltaTime : Time.unscaledDeltaTime);
            
            //Draw Noise Visualizer
            if (noiseEnabled && _noiseDebug)
            {
                #if PARTICLE_IMAGE_JOBS
                if (multithreadEnabled)
                {
                    var w1 = _noiseViewSize.x / 2;
                    var h1 = _noiseViewSize.y / 2;
                    for (int i = -w1; i < w1; i++)
                    {
                        for (int j = -h1; j < h1; j++)
                        {
                            
                            var p = new Vector3(i * 10, j * 10, 0);
                            float n = 0f;
                
                            if (space == Simulation.Local)
                            {
                                n = Unity.Mathematics.noise.snoise(new float3(p + new Vector3(noiseOffset.x, noiseOffset.y, 0)).xy * noiseFrequency / 100f);
                            }
                            else
                            {
                                var po = p + transform.localPosition;
                                n = Unity.Mathematics.noise.snoise(new float3(po + transform.localPosition).xy * noiseFrequency / 100f);
                            }
                            
                            Debug.DrawLine(RotatePointAroundCenter((p + transform.localPosition) * canvasRect.localScale.x, canvasRect.eulerAngles) + canvasRect.position, RotatePointAroundCenter((p + transform.localPosition + new Vector3(
                                math.cos(n * math.PI), 
                                math.sin(n * math.PI), 0) * 10) * canvasRect.localScale.x, canvasRect.eulerAngles) + canvasRect.position, new Color(n, n.Remap(-1f, 1f, 1f, 0f), 0f));
                        }
                    }
                    return;
                }
                
                #endif
                var w = _noiseViewSize.x / 2;
                var h = _noiseViewSize.y / 2;
                for (int i = -w; i < w; i++)
                {
                    for (int j = -h; j < h; j++)
                    {
                        var pos = new Vector3(i * 10, j * 10, 0);
                        var no = _noise.GetNoise(pos.x + noiseOffset.x, pos.y + noiseOffset.y);
                        if(space == Simulation.World)
                            no = _noise.GetNoise(pos.x + transform.localPosition.x + noiseOffset.x, pos.y + transform.localPosition.y + noiseOffset.y);
                        
                        Debug.DrawLine(RotatePointAroundCenter((pos + transform.localPosition) * canvasRect.localScale.x, canvasRect.eulerAngles) + canvasRect.position, RotatePointAroundCenter((pos + transform.localPosition + new Vector3(
                            Mathf.Cos(no * Mathf.PI), 
                            Mathf.Sin(no * Mathf.PI)) * 10) * canvasRect.localScale.x, canvasRect.eulerAngles) + canvasRect.position, new Color(no, no.Remap(-1f, 1f, 1f, 0f), 0f));
                    }
                }
            }
        }
        
        private Vector3 RotatePointAroundCenter(Vector3 point, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point);
        }

        private void UpdateSheets()
        {
            sheetsArray = new NativeArray<SpriteSheet>(textureTile.x * textureTile.y, multithreadEnabled ? Allocator.TempJob : Allocator.Temp);
            
            if (textureSheetEnabled)
            {
                for (int i = textureTile.y-1; i > -1; i--)
                {
                    for (int j = 0; j < textureTile.x; j++)
                    {
                        
                        sheetsArray[(textureTile.y - 1 - i) * textureTile.x + j] = new SpriteSheet(new Vector2(1f/textureTile.x * (j+1),1f/textureTile.y * (i+1)), new Vector2(1f/textureTile.x * j,1f/textureTile.y * i));
                    }
                }
            }
            else
            {
                if (sprite != null)
                {
                    var rect = DataUtility.GetOuterUV(sprite);
                    sheetsArray[0] = new SpriteSheet(new Vector2(rect[2],rect[3]), new Vector2(rect[0],rect[1]));
                }
                else
                    sheetsArray[0] = new SpriteSheet(new Vector2(1f,1f), new Vector2(0f,0f));
            }
        }

        private void Prewarm()
        {
            int numFrames = Mathf.FloorToInt(duration * 2 / 0.01f);
            
            for (int i = 0; i < numFrames; i++)
            {
                Simulate(0.01f, true);
            }
        }

        private void Simulate(float deltaTime, bool prewarming = false)
        {
            //<Generate Particles>
            if (isEmitting && !canvasRenderer.cull)
            {
                //Emit per second
                if(_rate > 0)
                {
                    if ((_time < (_duration + _startDelay) || _loop) && _time > _startDelay)
                    {
                        float dur = 1f / _rate;
                        _t += deltaTime;
                        while(_t >= dur)
                        {
                            _t -= dur;
                            _orderPerSec++;
                            if (multithreadEnabled)
                            {
#if PARTICLE_IMAGE_JOBS
                                if (JobParticles.Length > 0)
                                {
                                    JobParticles.InsertRangeWithBeginEnd(0, 1);
                                    JobParticles[0] = GenerateJobParticle(_orderPerSec,1, null);
                                }
                                else
                                {
                                    JobParticles.Add(GenerateJobParticle(_orderPerSec,1, null));
                                }
#endif
                            }
                            else
                            {
                                _particles.Add(GenerateParticle(_orderPerSec,1, null, _t));
                            }
                        }
                    }
                }

                //Emit over lifetime
                if (_rateOverLifetime > 0 && _duration > 0)
                {
                    if ((_time < (_duration + _startDelay) || _loop) && _time >= _startDelay)
                    {
                        float dur = _duration / _rateOverLifetime;
                        _t2 += deltaTime;
                        while(_t2 >= dur)
                        {
                            _t2 -= dur;
                            _orderOverLife++;
                            
                            if (multithreadEnabled)
                            {
#if PARTICLE_IMAGE_JOBS
                                if (JobParticles.Length > 0)
                                {
                                    JobParticles.InsertRangeWithBeginEnd(0, 1);
                                    JobParticles[0] = GenerateJobParticle(_orderOverLife,2, null);
                                }
                                else
                                {
                                    JobParticles.Add(GenerateJobParticle(_orderOverLife,2, null));
                                }
#endif
                            }
                            else
                            {
                                _particles.Add(GenerateParticle(_orderOverLife,2, null, _t2));
                            }
                        }
                    }
                }

                //Emit over distance
                if (_rateOverDistance > 0)
                {
                    if (_deltaPosition.magnitude > 1f / _rateOverDistance)
                    {
                        _orderOverDistance++;
                        if (multithreadEnabled)
                        {
#if PARTICLE_IMAGE_JOBS
                            if (JobParticles.Length > 0)
                            {
                                JobParticles.InsertRangeWithBeginEnd(0, 1);
                                JobParticles[0] = GenerateJobParticle(_orderOverDistance,3, null);
                            }
                            else
                            {
                                JobParticles.Add(GenerateJobParticle(_orderOverDistance,3, null));
                            }
#endif
                        }
                        else
                        {
                            _particles.Add(GenerateParticle(_orderOverDistance,3, null, 0));
                        }
                        _lastPosition = transform.position;
                    }
                }

                //Emit bursts
                if (_bursts != null)
                {
                    for (int i = 0; i < _bursts.Count; i++)
                    {
                        if (_burstTimer >= _bursts[i].time + _startDelay && _bursts[i].used == false)
                        {
                            for (int j = 0; j < _bursts[i].count; j++)
                            {
                                if (multithreadEnabled)
                                {
#if PARTICLE_IMAGE_JOBS
                                    if (JobParticles.Length > 0)
                                    {
                                        JobParticles.InsertRangeWithBeginEnd(0, 1);
                                        JobParticles[0] = GenerateJobParticle(j,0, _bursts[i]);
                                    }
                                    else
                                    {
                                        JobParticles.Add(GenerateJobParticle(j,0, _bursts[i]));
                                    }
#endif
                                }else{
                                    _particles.Add(GenerateParticle(j,0, _bursts[i], 0));
                                }
                            }
                            
                            _bursts[i].used = true;
                        }
                    }
                }
                
                if (_loop && _burstTimer >= _duration)
                {
                    _burstTimer = 0;
                    for (int i = 0; i < _bursts.Count; i++)
                    {
                        _bursts[i].used = false;
                    }
                }

                if (_time >= _duration + _startDelay && !_loop)
                {
                    isEmitting = false;
                }
                
                if(_loop && _loopTimer >= _duration + _startDelay)
                {
                    _loopTimer = 0;
                    _orderPerSec = 0;
                    _orderOverLife = 0;
                    _orderOverDistance = 0;
                }
            }
            //</Generate Particles>
            
            if (isPlaying && particleCount <= 0 && !isEmitting && isMain && CanStop)
            {
                Stop(true);
            }

            if (isPlaying)
            {
                _deltaPosition = transform.position - _lastPosition;
           
                if (_emitterConstraintTransform && _emitterConstraintEnabled.enabled)
                {
                    if (_emitterConstraintTransform is RectTransform)
                    {
                        transform.position = _emitterConstraintTransform.position;
                    }
                    else
                    {
                        Vector3 canPos;
                        Vector3 viewportPos = mainCamera.WorldToViewportPoint(_emitterConstraintTransform.position);

                        canPos = new Vector3(viewportPos.x.Remap(0.5f, 1.5f, 0f, canvasRect.rect.width),
                            viewportPos.y.Remap(0.5f, 1.5f, 0f, canvasRect.rect.height), 0);
                        
                        canPos = canvasRect.transform.TransformPoint(canPos);

                        canPos = transform.parent.InverseTransformPoint(canPos);

                        transform.localPosition = canPos;
                    }
                }

                if (!prewarming)
                {
                    _playback += deltaTime;
                }

                _time += deltaTime;
                _loopTimer += deltaTime;
                _burstTimer += deltaTime;
                
                UpdateSheets();
                
#if PARTICLE_IMAGE_JOBS

                #region Jobs

                if (multithreadEnabled)
                {
                    _meshDataArray = Mesh.AllocateWritableMeshData(1);
                    _meshData = _meshDataArray[0];
                    
                    _meshData.SetVertexBufferParams(JobParticles.Length * 4,
                        new VertexAttributeDescriptor(VertexAttribute.Position),
                        new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 4, stream: 1),
                        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2, stream: 2));
                    
                    _meshData.SetIndexBufferParams(JobParticles.Length * 6, IndexFormat.UInt16);

                    //Attractor
                    float3 targetPos = float3.zero;
                    float2 targetRect = float2.zero;
                    
                    if (attractorEnabled && attractorTarget)
                    {
                        if (attractorTarget is RectTransform)
                        {
                            targetPos = transform.InverseTransformPoint(attractorTarget.position);
                            var rt = attractorTarget as RectTransform;
                            var rect = rt.rect;
                            targetRect = new float2(rect.width, rect.height);
                        }
                        else
                        {
                            float3 viewportPos = WorldToViewportPoint(attractorTarget.position);
                            attractorType = AttractorType.Pivot;

                            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                            {
                                targetPos = new Vector3(
                                    ((viewportPos.x.Remap(0.5f, 1.5f,0f,canvasRect.rect.width) - canvasRect.InverseTransformPoint(transform.position).x + canvasRect.localPosition.x) / transform.lossyScale.x) * canvasRect.localScale.x, 
                                    ((viewportPos.y.Remap(0.5f, 1.5f,0f,canvasRect.rect.height) - canvasRect.InverseTransformPoint(transform.position).y + canvasRect.localPosition.y) / transform.lossyScale.y) * canvasRect.localScale.y, 
                                    0);
                            }
                            else
                            {
                                targetPos = new Vector3(
                                    (viewportPos.x.Remap(0.5f, 1.5f, 0f, canvasRect.rect.width) -
                                     canvasRect.InverseTransformPoint(transform.position).x) / transform.lossyScale.x * canvasRect.localScale.x,
                                    (viewportPos.y.Remap(0.5f, 1.5f, 0f, canvasRect.rect.height) -
                                     canvasRect.InverseTransformPoint(transform.position).y) / transform.lossyScale.y * canvasRect.localScale.y,
                                    0);
                            }
                        }
                    }
                    
                    var worldSpaceCounterPos = Vector3.zero;
                    
                    //World Space Calculation
                    if (space == Simulation.World)
                    {
                        worldSpaceCounterPos = transform.InverseTransformPoint(_lastTransformPosition);
                
                        _transformDeltaRotation = Quaternion.Inverse(transform.rotation).eulerAngles-Quaternion.Inverse(_lastTransformRotation).eulerAngles;
                
                        _lastTransformPosition = transform.position;
                        _lastTransformRotation = transform.rotation;
                    }
                    
                    //Schedule jobs
                    var cachedTransform = transform;
                    
                    _particleJob = new ParticleJob()
                    {
                        particles = JobParticles,
                        space = space,
                        meshData = _meshData,
                        deltaTime = deltaTime,
                        transformLocalPosition = cachedTransform.localPosition,
                        transformRotation = cachedTransform.rotation,
                        transformDeltaRotation = _transformDeltaRotation,
                        counterPos = worldSpaceCounterPos,
                        speedOverLifetime = JobUtils.ConvertMinMaxCurveToJobs(speedOverLifetime),
                        sizeOverLifetime = JobUtils.ConvertSeparatedMinMaxCurveJobData(sizeOverLifetime),
                        sizeBySpeed = JobUtils.ConvertSeparatedMinMaxCurveJobData(sizeBySpeed),
                        sizeSpeedRange = sizeSpeedRange.ToFloat2(),
                        colorOverLifetime = JobUtils.ConvertMinMaxColorJobData(colorOverLifetime),
                        colorBySpeed = JobUtils.ConvertMinMaxColorJobData(colorBySpeed),
                        colorSpeedRange = colorSpeedRange.ToFloat2(),
                        rotationOverLifetime = JobUtils.ConvertSeparatedMinMaxCurveJobData(rotationOverLifetime),
                        rotationBySpeed = JobUtils.ConvertSeparatedMinMaxCurveJobData(rotationBySpeed),
                        rotationSpeedRange = rotationSpeedRange.ToFloat2(),
                        alignToDirection = alignToDirection,
                        
                        velocityEnabled = velocityEnabled,
                        velocitySpace = velocitySpace,
                        velocityOverLifetime = JobUtils.ConvertSeparatedMinMaxCurveJobData(velocityOverLifetime),
                        
                        gravityEnabled = gravityEnabled,
                        gravity = JobUtils.ConvertMinMaxCurveToJobs(gravity),
                        
                        vortexEnabled = vortexEnabled,
                        vortex = JobUtils.ConvertMinMaxCurveToJobs(vortexStrength),
                        
                        noiseEnabled = noiseEnabled,
                        noiseStrength = noiseStrength,
                        noiseFrequency = noiseFrequency,
                        noiseOffset = noiseOffset,
                        
                        attractorEnabled = attractorEnabled,
                        attractorTarget = targetPos,
                        hasAttractorTarget = _attractorTarget,
                        attractorType = _targetMode,
                        attractorLerp = JobUtils.ConvertMinMaxCurveToJobs(attractorLerp),
                        attractorRect = targetRect,
                        
                        textureSheetEnabled = textureSheetEnabled,
                        spriteSheets = sheetsArray,
                        textureSheetSpeedRange = textureSheetFrameSpeedRange.ToFloat2(),
                        textureSheetType = _sheetType,
                        textureSheetFrameOverTime = JobUtils.ConvertMinMaxCurveToJobs(textureSheetFrameOverTime),
                        textureSheetStartFrame = JobUtils.ConvertMinMaxCurveToJobs(textureSheetStartFrame),
                        textureSheetFPS = _textureSheetFPS,
                        textureSheetCycleCount = _textureSheetCycles,
                        
                        trailsEnabled = trailsEnabled,
                        trailVertexDistance = _minimumVertexDistance,
                        trailPoints = TrailData
                    };
                    _particleJobHandle = _particleJob.Schedule(JobParticles.Length, 16);
                    
                    _particleJobHandle.Complete();
                    
                    sheetsArray.Dispose();
                    
                    //Trail data
                    var totalTrailPoints = 0;
                    var totalIndices = 0;

                    if (trailsEnabled && _particleTrailRenderer)
                    {
                        for (int i = 0; i < JobParticles.Length; i++)
                        {
                            var p = JobParticles[i];
                            p.trailVertexOffset = totalTrailPoints;
                            p.trailIndexOffset = totalIndices;
                            JobParticles[i] = p;
                        
                            if (TrailData[i].Length > 1)
                            {
                                totalTrailPoints += TrailData[i].Length * 2;
                                totalIndices += (TrailData[i].Length - 1) * 6;
                            }
                        }
                    
                        _trailMeshDataArray = Mesh.AllocateWritableMeshData(1);
                        _trailMeshData = _trailMeshDataArray[0];
                    
                        _trailMeshData.SetVertexBufferParams(totalTrailPoints,
                            new VertexAttributeDescriptor(VertexAttribute.Position),
                            new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 4, stream: 1));
                    
                        _trailMeshData.SetIndexBufferParams(totalIndices, IndexFormat.UInt16);
                        
                        _trailJob = new TrailJob()
                        {
                            particles = JobParticles,
                            trailMeshData = _trailMeshData,
                            trailPoints = TrailData,
                            trailsEnabled = trailsEnabled,
                            trailLifetime = _trailLifetime,
                            trailWidth = JobUtils.ConvertMinMaxCurveToJobs(_trailWidth),
                            trailInheritColor = _inheritParticleColor,
                            trailColorOverLifetime = JobUtils.ConvertMinMaxColorJobData(_trailColorOverLifetime),
                            trailColorOverTrail = JobUtils.ConvertMinMaxColorJobData(_trailColorOverTrail)
                        };

                        _trailJobHandle = _trailJob.Schedule(JobParticles.Length, 16, _particleJobHandle);
        
                        _trailJobHandle.Complete();
                    }

                    _meshData.subMeshCount = 1;
                    _meshData.SetSubMesh(0, new SubMeshDescriptor(0, JobParticles.Length * 6));

                    Mesh.ApplyAndDisposeWritableMeshData(_meshDataArray, mesh, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers);
                    
                    mesh.RecalculateBounds();
                    canvasRenderer.SetMesh(mesh);
                    SetMaterialDirty();
                    
                    //Apply trail mesh data
                    if(trailsEnabled && _particleTrailRenderer)
                        _particleTrailRenderer.SetMeshData(_trailMeshDataArray, _trailMeshData, totalIndices);
                    
                    //Remove old particles
                    if (JobParticles.Length > 0)
                    {
                        ref readonly ParticleData lastParticle = ref JobParticles.ElementAt(JobParticles.Length - 1);
                        while (lastParticle.time >= lastParticle.lifeTime && (!trailsEnabled || (TrailData.ElementAt(JobParticles.Length - 1).Length <= 1 || _dieWithParticle)))
                        {
                            OnAnyParticleFinished();
                            if (_firstParticleFinished == false)
                            {
                                _firstParticleFinished = true;
                               OnFirstParticleFinished();
                            }
                            if (JobParticles.Length <= 1)
                            {
                                OnLastParticleFinished();
                            }
                            if(trailsEnabled)
                                TrailData.RemoveAt(JobParticles.Length - 1);
                            JobParticles.RemoveAt(JobParticles.Length - 1);
                            if (JobParticles.Length <= 0)
                            {
                                break;
                            }
                            lastParticle = ref JobParticles.ElementAt(JobParticles.Length - 1);
                        }
                    }

                    return;
                }

                #endregion
                
#endif
                
                if (trailsEnabled)
                {
                    var sum = 0;
                    for (int i = 0; i < _particles.Count; i++)
                    {
                        sum += _particles[i].trailPoints.Count;
                    }
                    particleTrailRenderer.PrepareMeshData(sum, _particles.Count);
                }
                
                var vertices = new NativeArray<Vector3>(particles.Count * 4, Allocator.Temp);
                var triangles = new NativeArray<int>(particles.Count * 6, Allocator.Temp);
                var uvs = new NativeArray<Vector2>(particles.Count * 4, Allocator.Temp);
                var colors = new NativeArray<Color>(particles.Count * 4, Allocator.Temp);

                int vert = 0;
                int tris = 0;

                for (int i = particles.Count - 1; i >= 0; i--)
                {
                    _particles[i].Simulate(deltaTime);
                    
                    if (_particles[i].TimeSinceBorn > _particles[i].Lifetime || prewarming)
                    {
                        continue;
                    }
                    vertices[vert] = particles[i].points[0];
                    vertices[vert + 1] = particles[i].points[1];
                    vertices[vert + 2] = particles[i].points[2];
                    vertices[vert + 3] = particles[i].points[3];
            
                    triangles[tris] = vert;
                    triangles[tris + 1] = vert + 2;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert;
                    triangles[tris + 4] = vert + 3;
                    triangles[tris + 5] = vert + 2;

                    uvs[vert] = sheetsArray[particles[i].GetSheetId].size;
                    uvs[vert + 1] = new Vector2(sheetsArray[particles[i].GetSheetId].pos.x, sheetsArray[particles[i].GetSheetId].size.y);
                    uvs[vert + 2] = sheetsArray[particles[i].GetSheetId].pos;
                    uvs[vert + 3] = new Vector2(sheetsArray[particles[i].GetSheetId].size.x, sheetsArray[particles[i].GetSheetId].pos.y);

                    colors[vert] = particles[i].Color;
                    colors[vert + 1] = particles[i].Color;
                    colors[vert + 2] = particles[i].Color;
                    colors[vert + 3] = particles[i].Color;
            
                    vert += 4;
                    tris += 6;
                }

                if (particles.Count > 0)
                {
                    mesh.Clear();
                    if (vertices.Length > 0)
                    {
                        mesh.SetVertices(vertices);
                        mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
                        mesh.SetUVs(0, uvs);
                        mesh.SetColors(colors);
                    }
                }

                mesh.RecalculateBounds();
                canvasRenderer.SetMesh(mesh);
                
                if (trailsEnabled)
                {
                    particleTrailRenderer.SetMeshData();
                }

                //Remove dead particles
                for (int i = particles.Count - 1; i >= 0; i--)
                {
                    if (_particles[i].TimeSinceBorn > _particles[i].Lifetime && (_particles[i].trailPoints.Count <= 1 || _dieWithParticle))
                    {
                        OnAnyParticleFinished();
                        pool.Release(_particles[i]);
                        _particles.RemoveAt(i);
                        if (_firstParticleFinished == false)
                        {
                            _firstParticleFinished = true;
                           OnFirstParticleFinished();
                        }
                        if (particleCount < 1)
                        {
                            OnLastParticleFinished();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add a burst to the particle system
        /// </summary>
        public void AddBurst(float time, int count)
        {
            _bursts.Add(new Burst(time, count));
        }
        
        /// <summary>
        /// Remove burst at index
        /// </summary>
        public void RemoveBurst(int index)
        {
            _bursts.RemoveAt(index);
        }
        
        /// <summary>
        /// Set particle burst at index
        /// </summary>
        public void SetBurst(int index, float time, int count)
        {
            if (_bursts.Count > index)
            {
                _bursts[index] = new Burst(time, count);
            }
        }

        private bool CanStop
        {
            get
            {
                if (children != null)
                {
                    bool allChildrenInactive = true;

                    foreach (var child in children)
                    {
                        if (child.isEmitting == true || child.particleCount > 0)
                        {
                            allChildrenInactive = false;
                            break;
                        }
                    }

                    return allChildrenInactive;
                }
            
                return true;
            }
        }
        
        private Vector2 GetPointOnRect(float angle, float w, float h)
        {
            // Calculate the sine and cosine of the angle
            var sine = Mathf.Sin(angle);
            var cosine = Mathf.Cos(angle);

            // Calculate the x and y coordinates of the point
            // based on the sign of sine and cosine.
            // If sine is positive, the y coordinate is half the height of the rectangle.
            // If sine is negative, the y coordinate is negative half the height of the rectangle.
            // Similarly, if cosine is positive, the x coordinate is half the width of the rectangle.
            // If cosine is negative, the x coordinate is negative half the width of the rectangle.
            float dy = sine > 0 ? h / 2 : h / -2;
            float dx = cosine > 0 ? w / 2 : w / -2;

            // Check if the slope of the line between the origin and the point is steeper in
            // the x direction or in the y direction. If it is steeper in the x direction,
            // adjust the y coordinate so that the point is on the edge of the rectangle.
            // If it is steeper in the y direction, adjust the x coordinate instead.
            if (Mathf.Abs(dx * sine) < Mathf.Abs(dy * cosine))
            {
                dy = (dx * sine) / cosine;
            }
            else
            {
                dx = (dy * cosine) / sine;
            }

            // Return the point as a Vector2 object
            return new Vector2(dx, dy);
        }
        
#if PARTICLE_IMAGE_JOBS
        
        private float2 GetPointOnRectFloat2(float angle, float w, float h)
        {
            var sine = math.sin(angle);
            var cosine = math.cos(angle);
            float dy = sine > 0 ? h / 2 : h / -2;
            float dx = cosine > 0 ? w / 2 : w / -2;

            if (math.abs(dx * sine) < math.abs(dy * cosine))
            {
                dy = (dx * sine) / cosine;
            }
            else
            {
                dx = (dy * cosine) / sine;
            }

            // Return the point as a Vector2 object
            return new float2(dx, dy);
        }

        private ParticleData GenerateJobParticle(int order, int source, Burst burst)
        {
            float angle = 0;
            if (source == 0)//Burst
            {
                angle = order * (360f / burst.count) * _spreadLoop;
            }
            else if(source == 1)//Rate per Sec
            {
                angle = order * (360f / (_rate)) / _duration * _spreadLoop;
            }
            else if(source == 2)//Rate over Life
            {
                angle = order * (360f / (_rateOverLifetime)) * _spreadLoop;
            }
            else if(source == 3)//Rate over Distance
            {
                angle = order * (360f / (_rateOverDistance)) / _duration * _spreadLoop;
            }
            
            // Create new particle at system's starting position
            float2 startPosition = new float2();
            switch (_shape)
            {
                case EmitterShape.Point:
                    startPosition = float2.zero;
                    break;
                case EmitterShape.Circle:
                    var rp = Random.insideUnitCircle;
                    if (_emitOnSurface)
                    {
                        if (_spread == SpreadType.Random)
                        {
                            startPosition = new float2((rp * _radius).x, (rp * _radius).y);
                        }
                        else
                        {
                            startPosition = RotateOnAngleFloat2(new float2(0,Random.Range(0f,1f)), angle) * _radius;
                        }
                    }
                    else
                    {
                        if (_spread == SpreadType.Random)
                        {
                            Vector2 r = Random.insideUnitCircle.normalized;
                            startPosition = math.lerp(r * _radius, r * (_radius - _emitterThickness), Random.value);
                        }
                        else
                        {
                            startPosition = RotateOnAngleFloat2(new float2(0,1f), angle) * Random.Range(_radius, _radius - _emitterThickness);
                        }
                    }
                    break;
                case EmitterShape.Rectangle:
                    if (_emitOnSurface)
                    {
                        if(_spread == SpreadType.Uniform)
                        {
                            startPosition = math.lerp(GetPointOnRectFloat2(math.radians(angle), _width, _height), new float2(1f), Random.value);
                        }
                        else
                        {
                            startPosition = new float2(Random.Range(-_width / 2, _width / 2),
                                Random.Range(-_height / 2, _height / 2));
                        }
                    }
                    else
                    {
                        float a = Random.Range(0f, 360f);
                        
                        if(_spread == SpreadType.Uniform)
                        {
                            a = angle;
                        }
                        
                        startPosition = math.lerp(GetPointOnRectFloat2(math.radians(a), _width, _height), GetPointOnRectFloat2(math.radians(a), _width-_emitterThickness, _height-_emitterThickness), Random.value);
                    }
                    break;
                case EmitterShape.Line:
                    if(_spread == SpreadType.Uniform)
                    {
                        startPosition = new float2(Mathf.Repeat(angle, 361).Remap(0,360,-_length/2, _length/2), 0);
                    }
                    else
                    {
                        startPosition = new float2(Random.Range(-_length/2, _length/2), 0);
                    }
                    
                    break;
                case EmitterShape.Directional:
                    startPosition = float2.zero;
                    break;
            }
            
            if (space == Simulation.World)
            {
                //startPosition = math.mul(quaternion.Euler(transform.eulerAngles).value.xy, startPosition);
            }

            float2 startVelocity = float2.zero;
            switch (_shape)
            {
                case EmitterShape.Point:
                    var rp = Random.insideUnitCircle;
                    if (_spread == SpreadType.Uniform)
                    {
                        startVelocity = RotateOnAngleFloat2(new float2(0,1f), angle) * _startSpeed.Evaluate(Random.value, Random.value);;
                    }
                    else
                    {
                        startVelocity = math.normalize(new float2(rp.x, rp.y)) * _startSpeed.Evaluate(Random.value, Random.value);
                    }
                    break;
                case EmitterShape.Circle:
                    startVelocity = math.normalize(startPosition) * _startSpeed.Evaluate(Random.value, Random.value);
                    break;
                case EmitterShape.Rectangle:
                    startVelocity = math.normalize(startPosition) * _startSpeed.Evaluate(Random.value, Random.value);
                    break;
                case EmitterShape.Line:
                    startVelocity = (space == Simulation.World ? new float2(transform.up.x, transform.up.y) : new float2(Vector3.up.x, Vector3.up.y)) * _startSpeed.Evaluate(Random.value, Random.value);
                    break;
                case EmitterShape.Directional:
                    float a = 0;
                    if (space == Simulation.World)
                    {
                        if (_spread == SpreadType.Uniform)
                        {
                            a = Mathf.Repeat(angle, 361).Remap(0,360, -_angle / 2, _angle / 2) - transform.eulerAngles.z;
                        }
                        else
                        {
                            a = Random.Range(-_angle / 2, _angle / 2) - transform.eulerAngles.z;
                        }
                    }
                    else
                    {
                        if (_spread == SpreadType.Uniform)
                        {
                            a = Mathf.Repeat(angle, 361).Remap(0, 360, -_angle / 2, _angle / 2);
                        }
                        else
                        {
                            a = Random.Range(-_angle/2, _angle/2);
                        }
                    }
                    startVelocity = RotateOnAngleFloat2(a) * _startSpeed.Evaluate(Random.value, Random.value);
                    break;
            }

            if (trailsEnabled)
            {
                if (TrailData.Length == 0)
                {
                    var initialTrailPoint = new FixedList4096Bytes<TrailPointData>();
                    initialTrailPoint.Add(new TrailPointData()
                    {
                        position = (half2)startPosition,
                        time = (half)0,
                    });
                    TrailData.Add(initialTrailPoint);
                }
                else
                {
                    TrailData.InsertRangeWithBeginEnd(0,1);
                    var initialTrailPoint = new FixedList4096Bytes<TrailPointData>();
                    initialTrailPoint.Add(new TrailPointData()
                    {
                        position = (half2)startPosition,
                        time = (half)0,
                    });
                    trailData[0] = initialTrailPoint;
                }
            }

            var particle = new ParticleData();
            
            //start values
            particle.position = startPosition;
            particle.modifiedPosition = startPosition;
            particle.startSize = _startSize.EvaluateFloat3(Random.value, Random.value);
            particle.startVelocity = startVelocity;
            
            particle.lifeTime = _lifetime.Evaluate(Random.value, Random.value);
            var c = _startColor.Evaluate(Random.value, Random.value);
            particle.startColor = new float4(c.r, c.g, c.b, c.a);
            particle.startRotation = _startRotation.EvaluateZFloat3(Random.value, Random.value);
            
            particle.speedLerp = Random.value;
            particle.sizeLerp = Random.value;
            particle.colorLerp = Random.value;
            particle.rotationLerp = Random.value;
            particle.velocityLerp = Random.value;
            particle.gravityLerp = Random.value;
            particle.vortexLerp = Random.value;
            particle.attractorLerp = Random.value;
            
            particle.randomLerp1 = Random.value;
            particle.randomLerp2 = Random.value;
            particle.randomLerp3 = Random.value;
            
            particle.frameId = (int)textureSheetStartFrame.Evaluate(_time.Remap(0f, particle.lifeTime, 0f, 1f), Random.value);
            
            particle.trailLerpPosition = startPosition;
            particle.hasTrail = Random.value < trailRatio;
            
            return particle;
        }
        
#endif
        
        private Particle GenerateParticle(int order, int source, Burst burst, float startTime)
        {
            float angle = 0;
            if (source == 0)//Burst
            {
                angle = order * (360f / burst.count) * _spreadLoop;
            }
            else if(source == 1)//Rate per Sec
            {
                angle = order * (360f / (_rate)) / _duration * _spreadLoop;
            }
            else if(source == 2)//Rate over Life
            {
                angle = order * (360f / (_rateOverLifetime)) * _spreadLoop;
            }
            else if(source == 3)//Rate over Distance
            {
                angle = order * (360f / (_rateOverDistance)) / _duration * _spreadLoop;
            }
            
            // Create new particle at system's starting position
            Vector2 startPosition = Vector2.zero;
            switch (_shape)
            {
                case EmitterShape.Point:
                    startPosition = Vector2.zero;
                    break;
                case EmitterShape.Circle:
                    if (_emitOnSurface)
                    {
                        if (_spread == SpreadType.Random)
                        {
                            startPosition =  Random.insideUnitCircle * _radius;
                        }
                        else
                        {
                            startPosition = RotateOnAngle(new Vector3(0,Random.Range(0f,1f),0), angle) * _radius;
                        }
                    }
                    else
                    {
                        if (_spread == SpreadType.Random)
                        {
                            Vector2 r = Random.insideUnitCircle.normalized;
                            startPosition =  Vector3.Lerp(r * _radius, r * (_radius - _emitterThickness), Random.value);
                        }
                        else
                        {
                            startPosition = RotateOnAngle(new Vector3(0,1f,0), angle) * (UnityEngine.Random.Range(_radius, _radius - _emitterThickness));
                        }
                    }
                    break;
                case EmitterShape.Rectangle:
                    if (_emitOnSurface)
                    {
                        if(_spread == SpreadType.Uniform)
                        {
                            startPosition = Vector2.Lerp(GetPointOnRect(angle*Mathf.Deg2Rad, _width, _height), Vector2.one, Random.value);
                        }
                        else
                        {
                            startPosition = new Vector3(Random.Range(-_width / 2, _width / 2),
                                Random.Range(-_height / 2, _height / 2));
                        }
                    }
                    else
                    {
                        float a = Random.Range(0f, 360f);
                        
                        if(_spread == SpreadType.Uniform)
                        {
                            a = angle;
                        }
                        
                        startPosition = Vector2.Lerp(GetPointOnRect(a*Mathf.Deg2Rad, _width, _height), GetPointOnRect(a*Mathf.Deg2Rad, _width-_emitterThickness, _height-_emitterThickness), Random.value);
                    }
                    break;
                case EmitterShape.Line:
                    if(_spread == SpreadType.Uniform)
                    {
                        startPosition = new Vector3(Mathf.Repeat(angle, 361).Remap(0,360,-_length/2, _length/2), 0);
                    }
                    else
                    {
                        startPosition = new Vector3(Random.Range(-_length/2, _length/2), 0);
                    }
                    
                    break;
                case EmitterShape.Directional:
                    startPosition = Vector3.zero;
                    break;
            }

            if (space == Simulation.World)
            {
                //startPosition = Quaternion.Euler(transform.eulerAngles) * (startPosition);
            }
            
            Vector3 startVelocity = Vector3.zero;
            switch (_shape)
            {
                case EmitterShape.Point:
                    if (_spread == SpreadType.Uniform)
                    {
                        startVelocity = RotateOnAngle(new Vector3(0,1f,0), angle) * _startSpeed.Evaluate(Random.value, Random.value);;
                    }
                    else
                    {
                        startVelocity = Random.insideUnitCircle.normalized * _startSpeed.Evaluate(Random.value, Random.value);
                    }
                    break;
                case EmitterShape.Circle:
                    startVelocity = startPosition.normalized * _startSpeed.Evaluate(Random.value, Random.value);
                    break;
                case EmitterShape.Rectangle:
                    startVelocity = startPosition.normalized * _startSpeed.Evaluate(Random.value, Random.value);
                    break;
                case EmitterShape.Line:
                    startVelocity = (space == Simulation.World ? transform.up : Vector3.up) * _startSpeed.Evaluate(Random.value, Random.value);
                    break;
                case EmitterShape.Directional:
                    float a = 0;
                    if (space == Simulation.World)
                    {
                        if (_spread == SpreadType.Uniform)
                        {
                            a = Mathf.Repeat(angle, 361).Remap(0,360, -_angle / 2, _angle / 2) - transform.eulerAngles.z;
                        }
                        else
                        {
                            a = Random.Range(-_angle / 2, _angle / 2) - transform.eulerAngles.z;
                        }
                    }
                    else
                    {
                        if (_spread == SpreadType.Uniform)
                        {
                            a = Mathf.Repeat(angle, 361).Remap(0, 360, -_angle / 2, _angle / 2);
                        }
                        else
                        {
                            a = Random.Range(-_angle/2, _angle/2);
                        }
                    }
                    startVelocity = RotateOnAngle(a) * _startSpeed.Evaluate(Random.value, Random.value);
                    break;
            }

            Particle part = pool.Get();
            
            part.Initialize(
                    startPosition,
                    startVelocity,
                    _startRotation.EvaluateZ(Random.value, Random.value), 
                    _startColor.Evaluate(Random.value, Random.value),
                    _startSize.Evaluate(Random.value, Random.value),
                    _lifetime.Evaluate(Random.value, Random.value), startTime);
            
            return part;
        }
        
        public override Material material
        {
            get
            {
                return base.material;
            }
            set
            {
                if (m_Material == value)
                {
                    return;
                }

                m_Material = value;
                SetMaterialDirty();
            }
        }

        [SerializeField] private Sprite m_Sprite;
        
        public Sprite sprite
        {
            get
            {
                return m_Sprite;
            }
            set
            {
                if (m_Sprite == value)
                {
                    return;
                }
                m_Sprite = value;
                SetMaterialDirty();
            }
        }

        [SerializeField][Obsolete("Use sprite instead")]
        private Texture m_Texture;

        [Obsolete("Use sprite instead")]
        public Texture texture
        {
            get
            {
                return m_Texture;
            }
            set
            {
                if (m_Texture == value)
                {
                    return;
                }

                m_Texture = value;
                SetMaterialDirty();
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            if (fitRect)
            {
                FitRect();
            }

            if (!_emitOnSurface)
            {
                switch (_shape)
                {
                    case EmitterShape.Circle:
                        _emitterThickness = Mathf.Clamp(_emitterThickness, 0f, _radius);
                        break;
                    case EmitterShape.Rectangle:
                        _emitterThickness = Mathf.Clamp(_emitterThickness, 0f, rectTransform.sizeDelta.x<rectTransform.sizeDelta.y?_width:_height);
                        break;
                    case EmitterShape.Line:
                        _emitterThickness = Mathf.Clamp(_emitterThickness, 0f, _radius);
                        break;
                }
            }
        }

        private void FitRect()
        {
            switch (_shape)
            {
                // If the emitter has a circle shape, set the radius to half of the smaller
                // of the width and height of the emitter's RectTransform.
                case EmitterShape.Circle:
                    if (rectTransform.rect.width > rectTransform.rect.height)
                    {
                        _radius = rectTransform.rect.height/2;
                    }
                    else
                    {
                        _radius = rectTransform.rect.width/2;
                    }
                    break;
        
                // If the emitter has a rectangle shape, set the width and height of the emitter
                // to the width and height of the RectTransform.
                case EmitterShape.Rectangle:
                    _width = rectTransform.rect.width;
                    _height = rectTransform.rect.height;
                    break;

                // If the emitter has a line shape, set the length of the emitter to the width
                // of the RectTransform.
                case EmitterShape.Line:
                    _length = rectTransform.rect.width;
                    break;
            }
        }

        public override Texture mainTexture
        {
            get
            {
                if(sprite != null)
                    return sprite.texture;
                
                return m_Texture == null ? s_WhiteTexture : m_Texture;
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh) { }

        protected override void UpdateGeometry() { }
        
        private Vector3 RotateOnAngle(float angle){
            float rad = angle * Mathf.Deg2Rad;
            Vector3 position = new Vector3(Mathf.Sin( rad ), Mathf.Cos( rad ), 0);
            return position * 1f;
        }
        
        private Vector3 RotateOnAngle(Vector3 p, float angle){
            return Quaternion.Euler(new Vector3(0,0,angle)) * p;
        }
        
#if PARTICLE_IMAGE_JOBS
        
        private float2 RotateOnAngleFloat2(float angle)
        {
            float rad = math.radians(angle);
            float2 position = new float2(math.sin( rad ), math.cos( rad ));
            return position * 1f;
        }
        
        private float2 RotateOnAngleFloat2(float2 p, float angle){
            return math.mul(float2x2.Rotate(math.radians(angle)), p);
        }
        
#endif
        
        /// <summary>
        /// Converts world position to viewport position using the current camera
        /// </summary>
        /// <param name="position">World position</param>
        /// <returns>Viewport position</returns>
        public Vector3 WorldToViewportPoint(Vector3 position)
        {
            Vector3 pos = mainCamera.WorldToViewportPoint(position);
            return pos;
        }

        private void OnParticleStart()
        {
            onParticleStarted.Invoke();
        }

        private void OnFirstParticleFinished()
        {
            onFirstParticleFinished.Invoke();
        }
        
        private void OnAnyParticleFinished()
        {
            onAnyParticleFinished.Invoke();
        }
        
        private void OnLastParticleFinished()
        {
            onLastParticleFinished.Invoke();
        }
        
        private void OnParticleStop()
        {
            onParticleStop.Invoke();
        }
    }
    
    [Serializable]
    public class Burst
    {
        public float time = 0;
        public int count = 1;
        public bool used = false;
        
        public Burst(float time, int count)
        {
            this.time = time;
            this.count = count;
        }
    }

    [Serializable]
    public struct SpeedRange
    {
        public float from;
        public float to;
        public SpeedRange(float from, float to)
        {
            this.from = from;
            this.to = to;
        }
        
        #if PARTICLE_IMAGE_JOBS
        
        public float2 ToFloat2()
        {
            return new float2(from, to);
        }
        
        #endif
    }

    [Serializable]
    public struct Module
    {
        public bool enabled;

        public Module(bool enabled)
        {
            this.enabled = enabled;
        }
    }

    [Serializable]
    public struct SeparatedMinMaxCurve
    {
        [SerializeField]
        private bool separable;
        public bool separated;
        public ParticleSystem.MinMaxCurve mainCurve;
        public ParticleSystem.MinMaxCurve xCurve;
        public ParticleSystem.MinMaxCurve yCurve;
        public ParticleSystem.MinMaxCurve zCurve;

        public SeparatedMinMaxCurve(float startValue, bool separated = false, bool separable = true)
        {
            mainCurve = new ParticleSystem.MinMaxCurve(startValue);
            xCurve = new ParticleSystem.MinMaxCurve(startValue);
            yCurve = new ParticleSystem.MinMaxCurve(startValue);
            zCurve = new ParticleSystem.MinMaxCurve(startValue);
            this.separated = separated;
            this.separable = separable;
        }
        
        public SeparatedMinMaxCurve(AnimationCurve startValue, bool separated = false, bool separable = true)
        {
            mainCurve = new ParticleSystem.MinMaxCurve(1f,new AnimationCurve(startValue.keys));
            xCurve = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(startValue.keys));
            yCurve = new ParticleSystem.MinMaxCurve(1f,new AnimationCurve(startValue.keys));
            zCurve = new ParticleSystem.MinMaxCurve(1f,new AnimationCurve(startValue.keys));
            this.separated = separated;
            this.separable = separable;
        }

        public Vector3 Evaluate(float time, float lerp)
        {
            if (separated)
            {
                return new Vector3(xCurve.Evaluate(time, lerp), yCurve.Evaluate(time, lerp), zCurve.Evaluate(time, lerp));
            }

            return new Vector3(mainCurve.Evaluate(time, lerp), mainCurve.Evaluate(time, lerp), mainCurve.Evaluate(time, lerp));
        }
        
        public Vector3 EvaluateZ(float time, float lerp)
        {
            if (separated)
            {
                return new Vector3(0, 0, zCurve.Evaluate(time, lerp));
            }

            return new Vector3(0, 0, mainCurve.Evaluate(time, lerp));
        }
        
        public Vector2 EvaluateXY(float time, float lerp)
        {
            if (separated)
            {
                return new Vector2(xCurve.Evaluate(time, lerp), yCurve.Evaluate(time, lerp));
            }

            return new Vector2(mainCurve.Evaluate(time, lerp), mainCurve.Evaluate(time, lerp));
        }
        
        #if PARTICLE_IMAGE_JOBS

        public float3 EvaluateFloat3(float time, float lerp)
        {
            if (separated)
            {
                return new float3(xCurve.Evaluate(time, lerp), yCurve.Evaluate(time, lerp), zCurve.Evaluate(time, lerp));
            }

            return new float3(mainCurve.Evaluate(time, lerp), mainCurve.Evaluate(time, lerp), mainCurve.Evaluate(time, lerp));
        }
        
        public float3 EvaluateZFloat3(float time, float lerp)
        {
            if (separated)
            {
                return new float3(0, 0, zCurve.Evaluate(time, lerp));
            }

            return new float3(0, 0, mainCurve.Evaluate(time, lerp));
        }
        
        public float2 EvaluateXYFloat2(float time, float lerp)
        {
            if (separated)
            {
                return new float2(xCurve.Evaluate(time, lerp), yCurve.Evaluate(time, lerp));
            }

            return new float2(mainCurve.Evaluate(time, lerp), mainCurve.Evaluate(time, lerp));
        }

        #endif
    }
    
    public static class Extensions {
        public static float Remap (this float value, float from1, float to1, float from2, float to2) {
            float v = (value - from1) / (to1 - from1) * (to2 - from2) + from2;
#if PARTICLE_IMAGE_JOBS
            //Mathf.Approximately alternative for Burst compiler
            if (math.abs(to1 - from1) < math.max(0.000001f *  math.max(math.abs(from1), math.abs(to1)), math.EPSILON * 8))
            {
                return to1;
            }
#else
            if (Mathf.Approximately(from1, to1))
            {
                return to1;
            }
#endif
            return v;
        }
    }
}

namespace AssetKits.ParticleImage.Enumerations
{
    public enum EmitterShape
    {
        Point, Circle, Rectangle, Line, Directional
    }

    public enum SpreadType
    {
        Random, Uniform
    }

    public enum Simulation
    {
        Local, World
    }

    public enum AttractorType
    {
        Pivot, Surface
    }

    public enum PlayMode
    {
        None, OnEnable, OnAwake
    }
    
    public enum SheetType
    {
        Lifetime, Speed, FPS
    }

    public enum TimeScale
    {
        Unscaled, Normal
    }
}
