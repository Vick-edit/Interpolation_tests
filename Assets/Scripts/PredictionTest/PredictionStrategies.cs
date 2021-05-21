using System;

namespace PredictionTest
{
    [Serializable]
    public enum PredictionStrategy
    {
        NoPrediction,
        InterpolationOnlyWithoutSkip,
        InterpolationOnlyWithSkip,
        ExtrapolationOnly,
        CombineInterpolationAndExtrapolation,
    }
}