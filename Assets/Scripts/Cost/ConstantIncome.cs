using UnityEngine;

public class ConstantIncome : MonoBehaviour, IIncomeProvider {

    //TODO warn about zero
    public int CurrentIncome => (_income += _incomeChangePerTurn) - _incomeChangePerTurn;
    
    [SerializeField] private int _income;
    [SerializeField] private int _incomeChangePerTurn = 0;
}
