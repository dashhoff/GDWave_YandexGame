#if UNITY_BURST && UNITY_MATHEMATICS && UNITY_COLLECTIONS
using AssetKits.ParticleImage.Enumerations;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AssetKits.ParticleImage.Jobs
{
    [BurstCompile]
    public struct ParticleJob : IJobParallelFor
    {
        //GENERAL
        public NativeArray<ParticleData> particles;
        [ReadOnly] public Simulation space;
        [ReadOnly] public float3 transformLocalPosition;
        [ReadOnly] public float3 counterPos;
        [ReadOnly] public quaternion transformRotation;
        [ReadOnly] public float3 transformDeltaRotation;
        [ReadOnly] public Mesh.MeshData meshData;
        [ReadOnly] public float deltaTime;
        
        [ReadOnly] public MinMaxCurveJobData speedOverLifetime;
        
        [ReadOnly] public SeparatedMinMaxCurveJobData sizeOverLifetime;
        [ReadOnly] public SeparatedMinMaxCurveJobData sizeBySpeed;
        [ReadOnly] public float2 sizeSpeedRange;
        
        [ReadOnly] public MinMaxColorJobData colorOverLifetime;
        [ReadOnly] public MinMaxColorJobData colorBySpeed;
        [ReadOnly] public float2 colorSpeedRange;
        
        [ReadOnly] public SeparatedMinMaxCurveJobData rotationOverLifetime;
        [ReadOnly] public SeparatedMinMaxCurveJobData rotationBySpeed;
        [ReadOnly] public float2 rotationSpeedRange;
        [ReadOnly] public bool alignToDirection;
        
        //TEXTURE ANIMATION
        [ReadOnly] public bool textureSheetEnabled;
        [ReadOnly] public NativeArray<SpriteSheet> spriteSheets;
        [ReadOnly] public SheetType textureSheetType;
        [ReadOnly] public MinMaxCurveJobData textureSheetFrameOverTime;
        [ReadOnly] public MinMaxCurveJobData textureSheetStartFrame;
        [ReadOnly] public float textureSheetFPS;
        [ReadOnly] public float2 textureSheetSpeedRange;
        [ReadOnly] public int textureSheetCycleCount;
        
        //TRAILS
        [ReadOnly] public bool trailsEnabled;
        [ReadOnly] public float trailVertexDistance;
        public NativeArray<FixedList4096Bytes<TrailPointData>> trailPoints;

        //MOVEMENT MODIFIERS
        //Velocity
        [ReadOnly] public bool velocityEnabled;
        [ReadOnly] public Simulation velocitySpace;
        [ReadOnly] public SeparatedMinMaxCurveJobData velocityOverLifetime;
        
        //Gravity
        [ReadOnly] public bool gravityEnabled;
        [ReadOnly] public MinMaxCurveJobData gravity;

        //Vortex
        [ReadOnly] public bool vortexEnabled;
        [ReadOnly] public MinMaxCurveJobData vortex;
        
        //Noise
        [ReadOnly] public bool noiseEnabled;
        [ReadOnly] public float noiseStrength;
        [ReadOnly] public float noiseFrequency;
        [ReadOnly] public float2 noiseOffset;
        
        //Attractor
        [ReadOnly] public bool attractorEnabled;
        [ReadOnly] public float3 attractorTarget;
        [ReadOnly] public bool hasAttractorTarget;
        [ReadOnly] public MinMaxCurveJobData attractorLerp;
        [ReadOnly] public AttractorType attractorType;
        [ReadOnly] public float2 attractorRect;

        public void Execute(int index)
        {
            var particle = particles[index];
            particle.time += deltaTime;
            particle.normalizedTime = particle.time.Remap(0f, particle.lifeTime, 0f, 1f);

            // Update particle position
            
            particle.velocity = particle.startVelocity * speedOverLifetime.Evaluate(particle.normalizedTime, particle.speedLerp);

            if (space == Simulation.World)
            {
                particle.modifiedPosition += counterPos.xy;
                particle.modifiedPosition = RotatePointAroundCenter(particle.modifiedPosition, transformDeltaRotation);
                particle.startVelocity = RotatePointAroundCenter(particle.startVelocity, transformDeltaRotation);
                
            }
            
            if (velocityEnabled)
            {
                if(velocitySpace == Simulation.World)
                {
                    particle.velocity += RotatePointAroundCenter(velocityOverLifetime.EvaluateXY(particle.normalizedTime,  particle.colorLerp), Quaternion.Inverse(transformRotation).eulerAngles);
                }
                else
                {
                    particle.velocity += velocityOverLifetime.EvaluateXY(particle.normalizedTime, particle.velocityLerp);
                }
            }
            
            if (gravityEnabled)
            {
                particle.gravityVelocity += RotatePointAroundCenter(new float2(0,gravity.Evaluate(particle.normalizedTime, particle.gravityLerp)), Quaternion.Inverse(transformRotation).eulerAngles) * deltaTime;
                particle.velocity += particle.gravityVelocity;
            }
            
            if (noiseEnabled)
            {
                float noise = 0f;
                
                if (space == Simulation.Local)
                {
                    noise = Unity.Mathematics.noise.snoise((particle.position + new float2(noiseOffset.x, noiseOffset.y)) * (noiseFrequency / 100f));
                }
                else
                {
                    var pos = particle.position + transformLocalPosition.xy;
                    noise = Unity.Mathematics.noise.snoise((pos + new float2(noiseOffset.x, noiseOffset.y)) * (noiseFrequency / 100f));
                }
                
                particle.velocity += new float2(
                    math.cos(noise * math.PI), 
                    math.sin(noise * math.PI)) * noiseStrength;
            }
            
            
            
            particle.modifiedPosition += particle.velocity * deltaTime * 100;
            
            if (vortexEnabled)
            {
                particle.modifiedPosition = RotatePointAroundCenter(particle.modifiedPosition, new float3(0,0, vortex.Evaluate(particle.normalizedTime, particle.vortexLerp) * deltaTime * 100));
            }

            if (attractorEnabled && hasAttractorTarget)
            {
                if(attractorType == AttractorType.Pivot)
                    particle.position = math.lerp(particle.modifiedPosition, attractorTarget.xy, attractorLerp.Evaluate(particle.normalizedTime, particle.attractorLerp));
                else
                {
                    particle.position = math.lerp(particle.modifiedPosition,
                        new float2(
                            attractorTarget.x + particle.attractorLerp.Remap(0f, 1f, -attractorRect.x/2, attractorRect.x/2),
                            attractorTarget.y + particle.colorLerp.Remap(0f, 1f, -attractorRect.y/2, attractorRect.y/2)),
                        attractorLerp.Evaluate(particle.normalizedTime, particle.attractorLerp));
                }
            }
            else
            {
                particle.position = particle.modifiedPosition;
            }

            particle.deltaPosition = particle.position - particle.lastPosition;
            particle.lastPosition = particle.position;

            var normalizedSpeed = math.length(particle.deltaPosition) * (1f / deltaTime) / 100f;

            // Update particle size
            particle.size = particle.startSize * sizeOverLifetime.Evaluate(particle.normalizedTime, particle.sizeLerp) * sizeBySpeed.Evaluate(normalizedSpeed.Remap(sizeSpeedRange.x, sizeSpeedRange.y, 0f, 1f), particle.sizeLerp);
            
            // Update particle color
            particle.color = particle.startColor * colorOverLifetime.Evaluate(particle.normalizedTime, particle.colorLerp) * colorBySpeed.Evaluate(normalizedSpeed.Remap(colorSpeedRange.x, colorSpeedRange.y, 0f, 1f), particle.colorLerp);
            
            // Update particle rotation
            particle.rotation = particle.startRotation;
            
            var rot = math.lerp(particle.rotation, particle.rotation + rotationOverLifetime.EvaluateZ(particle.normalizedTime, particle.rotationLerp), particle.normalizedTime);
            
            if (alignToDirection)
            {
                if (particle.deltaPosition.x == 0 && particle.deltaPosition.y == 0)
                {
                    particle.deltaPosition = new float2(0.0001f, 0.0001f);
                }
                var direction = math.normalize(particle.deltaPosition);
                
                var angle = math.degrees(math.atan2(direction.y, direction.x) - math.PI / 2f);
                rot += new float3(0,0,angle);
            }
            else
            {
                rot += (float3)Quaternion.Inverse(transformRotation).eulerAngles;
            }
            
            particle.rotation = rot + rotationBySpeed.EvaluateZ(normalizedSpeed.Remap(rotationSpeedRange.x, rotationSpeedRange.y, 0f, 1f), particle.rotationLerp);
            
            // Texture animation
            if (textureSheetEnabled)
            {
                switch (textureSheetType)
                {
                    case SheetType.Speed:
                        particle.frameId = (int)normalizedSpeed.Remap(textureSheetSpeedRange.x, textureSheetSpeedRange.y, 0f, spriteSheets.Length);
                        break;
                    case SheetType.Lifetime:
                        particle.frameId = (int)(textureSheetFrameOverTime.Evaluate(particle.normalizedTime,
                            particle.randomLerp1)*textureSheetCycleCount)+(int)textureSheetStartFrame.Evaluate(particle.normalizedTime, particle.randomLerp2);
                        break;
                    case SheetType.FPS:
                        float dur = 1f / textureSheetFPS;
                        particle.frameDelta += deltaTime;
                        while(particle.frameDelta >= dur)
                        {
                            particle.frameDelta -= dur;
                            particle.frameId ++;
                        }
                        break;
                }
                
                particle.sheetId = (int)Mathf.Repeat(particle.frameId, spriteSheets.Length);
            }
            else
            {
                particle.sheetId = 0;
            }

            // Update mesh data
            var vertexBuffer = meshData.GetVertexData<float3>();
            
            if (particle.time >= particle.lifeTime)
            {
                vertexBuffer[index * 4 + 0] = float3.zero;
                vertexBuffer[index * 4 + 1] = float3.zero;
                vertexBuffer[index * 4 + 2] = float3.zero;
                vertexBuffer[index * 4 + 3] = float3.zero;
            }
            else
            {
                vertexBuffer[index * 4 + 0] = new float3(particle.position + RotatePointAroundCenter(new float2(-particle.size.x/2, -particle.size.y/2), particle.rotation), 0f);
                vertexBuffer[index * 4 + 1] = new float3(particle.position + RotatePointAroundCenter(new float2(particle.size.x/2, -particle.size.y/2), particle.rotation), 0f);
                vertexBuffer[index * 4 + 2] = new float3(particle.position + RotatePointAroundCenter(new float2(particle.size.x/2, particle.size.y/2), particle.rotation), 0f);
                vertexBuffer[index * 4 + 3] = new float3(particle.position + RotatePointAroundCenter(new float2(-particle.size.x/2, particle.size.y/2), particle.rotation), 0f);
            }

            var colorBuffer = meshData.GetVertexData<float4>(1);
            
            colorBuffer[index * 4 + 0] = particle.color;
            colorBuffer[index * 4 + 1] = particle.color;
            colorBuffer[index * 4 + 2] = particle.color;
            colorBuffer[index * 4 + 3] = particle.color;
            
            var uvBuffer = meshData.GetVertexData<float2>(2);
            
            uvBuffer[index * 4 + 0] = spriteSheets[particle.sheetId].pos;
            uvBuffer[index * 4 + 1] = new float2(spriteSheets[particle.sheetId].size.x, spriteSheets[particle.sheetId].pos.y);
            uvBuffer[index * 4 + 2] = spriteSheets[particle.sheetId].size;
            uvBuffer[index * 4 + 3] = new float2(spriteSheets[particle.sheetId].pos.x, spriteSheets[particle.sheetId].size.y);

            var indexBuffer = meshData.GetIndexData<ushort>();
            
            indexBuffer[index * 6 + 5] = (ushort)(index * 4 + 0);
            indexBuffer[index * 6 + 4] = (ushort)(index * 4 + 1);
            indexBuffer[index * 6 + 3] = (ushort)(index * 4 + 2);
            indexBuffer[index * 6 + 2] = (ushort)(index * 4 + 0);
            indexBuffer[index * 6 + 1] = (ushort)(index * 4 + 2);
            indexBuffer[index * 6 + 0] = (ushort)(index * 4 + 3);
            
            //Trail
            if (trailsEnabled)
            {
                var ltp = particle.lastTrailPosition;
                if (particle.hasTrail && particle.time < particle.lifeTime && math.distance(ltp, particle.position) > trailVertexDistance)
                {
                    var trail = trailPoints[index];
                    trail.Add(new TrailPointData()
                    {
                        position = new half2((half)particle.position.x,(half)particle.position.y),
                        time = (half)particle.time
                    });
                    trailPoints[index] = trail;
                    ltp = particle.position;
                    
                    particle.lastTrailPosition = ltp;
                }
            }

            // Apply changes back
            particles[index] = particle;
        }
        
        private float2 RotatePointAroundCenter(float2 point, float3 angles)
        {
            quaternion rot = quaternion.Euler(math.radians(angles));
            return math.mul(rot, point.xyx).xy;
        }
    }
}
#endif
