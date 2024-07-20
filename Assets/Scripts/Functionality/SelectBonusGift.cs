using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectBonusGift : MonoBehaviour
{
    [SerializeField]
    private Button this_Button;
    [SerializeField]
    private Image this_GameObject;
    [SerializeField]
    private GameObject selected_GameObject;
    [SerializeField]
    private PlayTextAnimation text_AnimScript;
    [SerializeField]
    private BonusController _bonusManager;

    private void Start()
    {
        if (this_Button) this_Button.onClick.RemoveAllListeners();        
        if (this_Button) this_Button.onClick.AddListener(SelectGift);
    }

    private void SelectGift()
    {
        if (_bonusManager) _bonusManager.enableRayCastPanel(true);
        int value = 0;
        value = _bonusManager.GetValue();
        if(value > 0)
        {
            if (_bonusManager) _bonusManager.PlayWinLooseSound(true);
            if (text_AnimScript) text_AnimScript.SetText("+" + value.ToString());
        }
        else
        {
            if (_bonusManager) _bonusManager.PlayWinLooseSound(false);
            if (text_AnimScript) text_AnimScript.SetText("No Bonus");
        }
        if (this_GameObject) this_GameObject.enabled = false;
        if (selected_GameObject) selected_GameObject.SetActive(true);
    }

    internal void ResetGift()
    {
        if (selected_GameObject) selected_GameObject.SetActive(false);
        if (this_GameObject) this_GameObject.enabled = true;
    }
}
