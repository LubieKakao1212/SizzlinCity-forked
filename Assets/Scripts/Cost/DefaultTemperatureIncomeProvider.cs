using UnityEngine;

public class DefaultTemperatureIncomeProvider : MonoBehaviour, IIncomeProvider
{
    public int CurrentIncome
    {
        get
        {
            //https://www.desmos.com/calculator/mogrqfowxc?lang=pl
            // var heat = Mathf.Clamp01(obj.Heat) - offset;
            // heat = Mathf.Sign(heat) * Mathf.Pow(Mathf.Abs(heat), power);
            // return (int)Mathf.Max(heat * coef, min);
            var heat = obj.Heat - min;
            return (int)(Mathf.Max(heat, 0) * coef);
        }
    }

    [SerializeField]
    public float min = 0f;

    [SerializeField]
    public float coef = 1f;
    
    // [SerializeField]
    // public float offset = 0f;
    //
    // [SerializeField]
    // public float power = 1f;

    [SerializeField]
    private ObjectHeatProvider obj;
}