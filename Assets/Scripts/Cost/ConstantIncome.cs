using UnityEngine;

public class ConstantIncome : MonoBehaviour, IIncomeProvider {

    //TODO warn about zero
    public int CurrentIncome => _income;
    
    [SerializeField] private int _income;
}
