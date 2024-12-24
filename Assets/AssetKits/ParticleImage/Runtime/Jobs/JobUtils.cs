#if UNITY_BURST && UNITY_MATHEMATICS && UNITY_COLLECTIONS

using Unity.Mathematics;
using UnityEngine;

namespace AssetKits.ParticleImage.Jobs
{
    public static class JobUtils
    {
        public static MinMaxColorJobData ConvertMinMaxColorJobData(ParticleSystem.MinMaxGradient gradient)
        {
            var data = new MinMaxColorJobData();
            
            data.mode = gradient.mode;
            
            switch (gradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    data.constantMax = gradient.color.ToFloat4();
                    break;
                case ParticleSystemGradientMode.Gradient:
                    for (int i = 0; i < 16; i++)
                    {
                        data.max.Add(gradient.gradient.Evaluate(i / 15f).ToFloat4());
                    }
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    data.constantMin = gradient.colorMin.ToFloat4();
                    data.constantMax = gradient.colorMax.ToFloat4();
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    for (int i = 0; i < 16; i++)
                    {
                        data.min.Add(gradient.gradientMin.Evaluate(i / 15f).ToFloat4());
                        data.max.Add(gradient.gradientMax.Evaluate(i / 15f).ToFloat4());
                    }
                    break;
                case ParticleSystemGradientMode.RandomColor:
                    for (int i = 0; i < 16; i++)
                    {
                        data.max.Add(gradient.gradient.Evaluate(i / 15f).ToFloat4());
                    }
                    break;
            }

            return data;
        }

        public static MinMaxCurveJobData ConvertMinMaxCurveToJobs(ParticleSystem.MinMaxCurve curve)
        {
            var data = new MinMaxCurveJobData();
            
            data.mode = curve.mode;
            
            switch (curve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    data.constantMax = curve.constant;
                    break;
                case ParticleSystemCurveMode.Curve:
                    for (int i = 0; i < 16; i++)
                    {
                        data.max.Add(curve.curve.Evaluate(i / 15f) * curve.curveMultiplier);
                    }
                    break;
                case ParticleSystemCurveMode.TwoCurves:
                    for (int i = 0; i < 16; i++)
                    {
                        data.min.Add(curve.curveMin.Evaluate(i / 15f) * curve.curveMultiplier);
                        data.max.Add(curve.curveMax.Evaluate(i / 15f) * curve.curveMultiplier);
                    }
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    data.constantMin = curve.constantMin;
                    data.constantMax = curve.constantMax;
                    break;
            }
            
            return data;
        }

        public static SeparatedMinMaxCurveJobData ConvertSeparatedMinMaxCurveJobData(SeparatedMinMaxCurve curve)
        {
            var data = new SeparatedMinMaxCurveJobData();
            
            data.separated = curve.separated;

            if (curve.separated)
            {
                data.x = ConvertMinMaxCurveToJobs(curve.xCurve);
                data.y = ConvertMinMaxCurveToJobs(curve.yCurve);
                data.z = ConvertMinMaxCurveToJobs(curve.zCurve);
            }
            else
            {
                data.x = ConvertMinMaxCurveToJobs(curve.mainCurve);
            }

            return data;
        }
        
        public static float4 ToFloat4(this Color color)
        {
            return new float4(color.r, color.g, color.b, color.a);
        }
    }
}

#endif