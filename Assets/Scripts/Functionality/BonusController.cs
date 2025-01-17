using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class BonusController : MonoBehaviour
{
    [SerializeField]
    private SocketIOManager SocketManager;
    [SerializeField]
    private SlotBehaviour slotBehaviour;

    [SerializeField]
    private GameObject Bonus_Object;
    [SerializeField]
    private SlotBehaviour slotManager;
    [SerializeField]
    private GameObject raycastPanel;
    [SerializeField]
    private List<SelectBonusGift> BonusCases;
    [SerializeField]
    private AudioController _audioManager;
    [SerializeField]
    private TMP_Text mainamount_Text;

    double amount = 0;

    [SerializeField]
    private List<int> CaseValues;

    internal bool isGameOver = false;

    int index = 0;

    internal void GetSuitCaseList(List<int> values)
    {
        amount = 0;
        index = 0;
        CaseValues.Clear();
        CaseValues.TrimExcess();
        CaseValues = values;
        isGameOver = false;
        if (mainamount_Text) mainamount_Text.text = "0";

        foreach (SelectBonusGift cases in BonusCases)
        {
            cases.ResetGift();
        }

        if (raycastPanel) raycastPanel.SetActive(false);
        StartBonus();
    }

    internal void enableRayCastPanel(bool choice)
    {
        if (raycastPanel) raycastPanel.SetActive(choice);
    }

    internal void GameOver()
    {
        if (slotManager) slotManager.CheckWinPopups();
        _audioManager.SwitchBGSound(false);
        if (Bonus_Object) Bonus_Object.SetActive(false);
    }

    internal double GetValue()
    {
        double value = 0;

        value = CaseValues[index] * SocketManager.initialData.Bets[slotBehaviour.BetCounter];

        index++;

        if (value > 0)
        {
            amount += value;
        }
        else
        {
            isGameOver = true;
        }

        if (mainamount_Text) mainamount_Text.text = amount.ToString("f3");

        return value;
    }

    internal void PlayWinLooseSound(bool isWin)
    {
        if (isWin)
        {
            _audioManager.PlayBonusAudio("win");
        }
        else
        {
            _audioManager.PlayBonusAudio("lose");
        }
    }

    private void StartBonus()
    {
        _audioManager.SwitchBGSound(true);
        if (Bonus_Object) Bonus_Object.SetActive(true);
    }
}
