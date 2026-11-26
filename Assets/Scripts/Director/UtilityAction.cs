using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UtilityAction {
    public string Name;
    public List<Consideration> Considerations = new();

    public float Evaluate(Dictionary<string, float> ctx) {
        float score = 0;
        float weightSum = 0;
        foreach (Consideration consideration in Considerations) {
            float value = ctx[consideration.Key];
            float evaluation = Mathf.Clamp01(consideration.Curve.Evaluate(value));
            score += evaluation * consideration.Weight;
            weightSum += consideration.Weight;
        }
        
        return score / weightSum;
    }
}