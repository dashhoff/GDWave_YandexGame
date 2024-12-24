#if UNITY_BURST && UNITY_MATHEMATICS && UNITY_COLLECTIONS
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AssetKits.ParticleImage.Jobs
{
    [BurstCompile]
    public struct TrailJob : IJobParallelFor
    {
        public NativeArray<ParticleData> particles;
        [ReadOnly] public Mesh.MeshData trailMeshData;
        [ReadOnly] public bool trailsEnabled;
        public NativeArray<FixedList4096Bytes<TrailPointData>> trailPoints;
        [ReadOnly] public float trailLifetime;
        [ReadOnly] public MinMaxCurveJobData trailWidth;
        [ReadOnly] public bool trailInheritColor;
        [ReadOnly] public MinMaxColorJobData trailColorOverLifetime;
        [ReadOnly] public MinMaxColorJobData trailColorOverTrail;
        
        public void Execute(int index)
        {
            var particle = particles[index];
            //Update trails
            if(trailsEnabled)
            {
                var trailBuffer = trailMeshData.GetVertexData<float3>(0);
                var trailColorBuffer = trailMeshData.GetVertexData<float4>(1);
                var trailIndexBuffer = trailMeshData.GetIndexData<ushort>();
                
                NativeList<float3> tempPositions = new NativeList<float3>(Allocator.Temp);
                NativeList<ushort> tempIndices = new NativeList<ushort>(Allocator.Temp);

                var vertexOffset = particle.trailVertexOffset;
                var indexOffset = particle.trailIndexOffset;

                if (trailPoints[index].Length > 1)
                {
                    for (int j = 0; j < trailPoints[index].Length; j++)
                    {
                        var trailLength = math.distance(trailPoints[index][0].position, particle.position);
                        float pointDistance = math.clamp((j > 0) ? math.distance(trailPoints[index][0].position, trailPoints[index][j].position).Remap(0, trailLength, 1f, 0f) : 1f, 0f, 1f);
                        float pointWidth = particle.size.x * trailWidth.Evaluate(pointDistance, particle.randomLerp3);
                        
                        float3 dir;
                        
                        if (j < trailPoints[index].Length - 1)
                        {
                            dir = math.normalize(new float3(trailPoints[index][j].position, 0f) - new float3(trailPoints[index][j + 1].position, 0f));
                        }
                        else
                        {
                            dir = math.normalize(new float3(trailPoints[index][j].position, 0f) - new float3(trailPoints[index][j - 1].position, 0f));
                        }
                        
                        var perp = math.cross(dir, new float3(0, 0, 1));

                        if (j == trailPoints[index].Length - 1)
                        {
                            tempPositions.Add(new float3(trailPoints[index][j].position, 0) + perp * pointWidth / 2);
                            tempPositions.Add(new float3(trailPoints[index][j].position, 0) - perp * pointWidth / 2);
                        }
                        else
                        {
                            tempPositions.Add(new float3(trailPoints[index][j].position, 0) - perp * pointWidth / 2);
                            tempPositions.Add(new float3(trailPoints[index][j].position, 0) + perp * pointWidth / 2);
                        }
                        
                        //Apply color
                        float4 c = trailColorOverTrail.Evaluate(pointDistance, particle.colorLerp) * trailColorOverLifetime.Evaluate(particle.normalizedTime, particle.colorLerp);
                        
                        if (trailInheritColor)
                        { 
                            c *= particle.color;
                        }
                        
                        trailColorBuffer[j * 2 + vertexOffset] = c;
                        trailColorBuffer[j * 2 + 1 + vertexOffset] = c;
                    }
                    
                    for (int j = 0; j < tempPositions.Length - 2; j++)
                    {
                        if (j % 2 == 0)
                        {
                            tempIndices.Add((ushort)(j + 0));
                            tempIndices.Add((ushort)(j + 1));
                            tempIndices.Add((ushort)(j + 2));
                        }
                        else
                        {
                            tempIndices.Add((ushort)(j + 2));
                            tempIndices.Add((ushort)(j + 1));
                            tempIndices.Add((ushort)(j + 0));
                        }
                    }
                
                    for (int j = 0; j < tempPositions.Length; j++)
                    {
                        if(vertexOffset + j < trailBuffer.Length)
                            trailBuffer[vertexOffset + j] = tempPositions[j];
                    }
                
                    for (int j = 0; j < tempIndices.Length; j++)
                    {
                        if(indexOffset + j < trailIndexBuffer.Length)
                            trailIndexBuffer[indexOffset + j] = (ushort)(tempIndices[j] + vertexOffset);
                    }
                }
                
                if (trailPoints[index].Length > 0 && particle.time >= trailPoints[index][0].time + trailLifetime)
                {
                    var trail = trailPoints[index];
                    trail.RemoveAt(0);
                    trailPoints[index] = trail;
                    if (trailPoints[index].Length > 0)
                        particle.trailLerpPosition = trailPoints[index][0].position;
                }

                //First point lerp
                if (trailPoints[index].Length > 0 && particle.time < particle.lifeTime)
                {
                    var trail = trailPoints[index];
                    trail[trail.Length-1] = new TrailPointData()
                    {
                        position = (half2)particle.position,
                        time = (half)particle.time
                    };
                    trailPoints[index] = trail;
                }
                
                //Last point lerp
                // if (trails[index].Length > 1)
                // {
                //     var lastFollowingPoint = trails[index][0];
                //     lastFollowingPoint.position = (half2)math.lerp(trails[index][1].position, particle.trailLerpPosition, math.abs(particle.time.Remap(lastFollowingPoint.time+trailLifetime, trails[index][1].time + trailLifetime, 0f, 1f)));
                //     var fixedList4096Bytes = trails[index];
                //     fixedList4096Bytes[0] = lastFollowingPoint;
                //     trails[index] = fixedList4096Bytes;
                // }
            }
        }
    }
}
#endif