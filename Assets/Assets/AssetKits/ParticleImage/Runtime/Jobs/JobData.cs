#if UNITY_BURST && UNITY_MATHEMATICS && UNITY_COLLECTIONS

using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace AssetKits.ParticleImage.Jobs
{
    public struct ParticleData
    {
        public float time;
        public float normalizedTime;
        public float lifeTime;
        
        public float2 position;
        public float2 modifiedPosition;
        public float2 deltaPosition;
        public float2 lastPosition;
        public float2 velocity;
        public float2 startVelocity;
        public float2 gravityVelocity;
        public float4 startColor;
        public float4 color;
        public float3 startSize;
        public float3 size;
        public float3 rotation;
        public float3 startRotation;

        public float randomLerp1;
        public float randomLerp2;
        public float randomLerp3;
        
        public float speedLerp;
        public float sizeLerp;
        public float colorLerp;
        public float rotationLerp;
        public float velocityLerp;
        public float gravityLerp;
        public float vortexLerp;
        public float attractorLerp;
        
        public float frameDelta;
        public int frameId;
        public int sheetId;

        public bool hasTrail;
        public int trailVertexOffset;
        public int trailIndexOffset;
        public float2 lastTrailPosition;
        public float2 trailLerpPosition;
    }

    public struct TrailPointData
    {
        public half2 position;
        public half time;
    }
    
    public struct MinMaxColorJobData
    {
        public FixedList512Bytes<float4> min;
        public FixedList512Bytes<float4> max;
        public float4 constantMin;
        public float4 constantMax;
        public ParticleSystemGradientMode mode;
        
        public float4 Evaluate(float time, float lerp)
        {
            if(mode == ParticleSystemGradientMode.TwoColors)
                return math.lerp(constantMin, constantMax, lerp);
            if (mode == ParticleSystemGradientMode.RandomColor)
            {
                var count2 = max.Length;
                var index2 = (int)math.floor(lerp * (count2 - 1));
                if(index2 >= count2)
                    index2 = count2 - 1;
                var nextIndex2 = index2 + 1;
                if(nextIndex2 >= count2)
                    nextIndex2 = count2 - 1;
                var nextT2 = lerp * (count2 - 1) - index2;
                var value2 = max[index2];
                var nextValue2 = max[nextIndex2];
                return math.lerp(value2, nextValue2, nextT2);
            }
            if (mode == ParticleSystemGradientMode.Gradient)
            {
                var count = max.Length;
                var index = (int)math.floor(time * (count - 1));
                if(index >= count)
                    index = count - 1;
                var nextIndex = index + 1;
                if(nextIndex >= count)
                    nextIndex = count - 1;
                var nextT = time * (count - 1) - index;
                var value = max[index];
                var nextValue = max[nextIndex];
                return math.lerp(value, nextValue, nextT);
            }
            if (mode == ParticleSystemGradientMode.TwoGradients)
            {
                var countMin = min.Length;
                var indexMin = (int)math.floor(time * (countMin - 1));
                if(indexMin >= countMin)
                    indexMin = countMin - 1;
                var nextIndexMin = indexMin + 1;
                if(nextIndexMin >= countMin)
                    nextIndexMin = countMin - 1;
                var nextTMin = time * (countMin - 1) - indexMin;
                var valueMin = min[indexMin];
                var nextValueMin = min[nextIndexMin];
                var colorMin = math.lerp(valueMin, nextValueMin, nextTMin);
                    
                var countMax = max.Length;
                var indexMax = (int)math.floor(time * (countMax - 1));
                if(indexMax >= countMax)
                    indexMax = countMax - 1;
                var nextIndexMax = indexMax + 1;
                if(nextIndexMax >= countMax)
                    nextIndexMax = countMax - 1;
                var nextTMax = time * (countMax - 1) - indexMax;
                var valueMax = max[indexMax];
                var nextValueMax = max[nextIndexMax];
                var colorMax = math.lerp(valueMax, nextValueMax, nextTMax);
                    
                return math.lerp(colorMin, colorMax, lerp);
            }
            return constantMax;
        }
    }
    
    public struct MinMaxCurveJobData
    {
        public FixedList128Bytes<float> min;
        public FixedList128Bytes<float> max;
        public float constantMin;
        public float constantMax;
        public ParticleSystemCurveMode mode;
        
        public float Evaluate(float time, float lerp)
        {
            if (mode == ParticleSystemCurveMode.TwoConstants)
                return math.lerp(constantMin, constantMax, lerp);
            if (mode == ParticleSystemCurveMode.TwoCurves)
            {
                var countMin = min.Length;
                var indexMin = (int)math.floor(time * (countMin - 1));
                if(indexMin >= countMin)
                    indexMin = countMin - 1;
                var nextIndexMin = indexMin + 1;
                if(nextIndexMin >= countMin)
                    nextIndexMin = countMin - 1;
                var nextTMin = time * (countMin - 1) - indexMin;
                var valueMin = min[indexMin];
                var nextValueMin = min[nextIndexMin];
                var minLerp = math.lerp(valueMin, nextValueMin, nextTMin);
                    
                var countMax = max.Length;
                var indexMax = (int)math.floor(time * (countMax - 1));
                if(indexMax >= countMax)
                    indexMax = countMax - 1;
                var nextIndexMax = indexMax + 1;
                if(nextIndexMax >= countMax)
                    nextIndexMax = countMax - 1;
                var nextTMax = time * (countMax - 1) - indexMax;
                var valueMax = max[indexMax];
                var nextValueMax = max[nextIndexMax];
                var maxLerp = math.lerp(valueMax, nextValueMax, nextTMax);
                    
                return math.lerp(minLerp, maxLerp, lerp);
            }
            if (mode == ParticleSystemCurveMode.Curve)
            {
                var count = max.Length;
                var index = (int)math.clamp(math.floor(time * (count - 1)), 0, count - 1);
                if(index >= count)
                    index = count - 1;
                var nextIndex = index + 1;
                if(nextIndex >= count)
                    nextIndex = count - 1;
                var nextT = time * (count - 1) - index;
                var value = max[index];
                var nextValue = max[nextIndex];
                return math.lerp(value, nextValue, nextT);
            }
            
            return constantMax;
        }
    }
    
    public struct SeparatedMinMaxCurveJobData
    {
        public bool separated;
        public MinMaxCurveJobData x;
        public MinMaxCurveJobData y;
        public MinMaxCurveJobData z;
        
        public float3 Evaluate(float time, float lerp)
        {
            if (separated)
            {
                return new float3(x.Evaluate(time, lerp), y.Evaluate(time, lerp), z.Evaluate(time, lerp));
            }
            else
            {
                var value = x.Evaluate(time, lerp);
                return new float3(value, value, value);
            }
        }
        
        public float3 EvaluateZ(float time, float lerp)
        {
            if(separated)
                return new float3(x.Evaluate(time, lerp), y.Evaluate(time, lerp), z.Evaluate(time, lerp));
            else
                return new float3(0, 0, x.Evaluate(time, lerp));
        }
        
        public float2 EvaluateXY(float time, float lerp)
        {
            if(separated)
                return new float2(x.Evaluate(time, lerp), y.Evaluate(time, lerp));
            else
            {
                var value = x.Evaluate(time, lerp);
                return new float2(value, value);
            }
        }
    }
}
#endif
