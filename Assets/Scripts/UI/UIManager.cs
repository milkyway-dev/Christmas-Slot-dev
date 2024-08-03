using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;


public class UIManager : MonoBehaviour
{

    [Header("Menu UI")]
    [SerializeField]
    private Button Info_Button;

    [Header("Popus UI")]
    [SerializeField]
    private GameObject MainPopup_Object;

    [Header("info Popup")]
    [SerializeField]
    private GameObject PaytablePopup_Object;
    [SerializeField]
    private Button PaytableExit_Button;
    [SerializeField]
    private Button Next_Button;
    [SerializeField]
    private Button Previous_Button;
    private int paginationCounter = 1;
    [SerializeField] private GameObject[] PageList;
    [SerializeField] private Button[] paginationButtonGrp;
    [SerializeField] private Button Infoback_button;
    [SerializeField]
    private TMP_Text[] SymbolsText;


    [Header("Settings Popup")]
    [SerializeField] private Button Setting_button;
    [SerializeField] private Button SettingExit_button;
    [SerializeField] private Button Setting_back_button;
    [SerializeField] private GameObject Setting_panel;
    [SerializeField] private Slider Sound_slider;
    [SerializeField] private Slider Music_slider;

    [Header("Splash Screen")]
    [SerializeField]
    private GameObject Loading_Object;
    [SerializeField]
    private Image Loading_Image;
    [SerializeField]
    private TMP_Text LoadPercent_Text;
    [SerializeField]
    private TMP_Text Loading_Text;

    [Header("LowBalance Popup")]
    [SerializeField]
    private Button LBExit_Button;
    [SerializeField]
    private Button LBBack_Button;
    [SerializeField]
    private GameObject LBPopup_Object;

    [Header("Disconnection Popup")]
    [SerializeField]
    private Button CloseDisconnect_Button;
    [SerializeField]
    private GameObject DisconnectPopup_Object;

    [Header("AnotherDevice Popup")]
    [SerializeField]
    private Button CloseAD_Button;
    [SerializeField]
    private GameObject ADPopup_Object;

    [Header("Quit Popup")]
    [SerializeField]
    private GameObject QuitPopup_Object;
    [SerializeField]
    private Button YesQuit_Button;
    [SerializeField]
    private Button NoQuit_Button;
    [SerializeField]
    private Button CrossQuit_Button;
    [SerializeField]
    private Button BackQuit_Button;

    [Header("Megawin Popup")]
    [SerializeField] private GameObject megawin;
    [SerializeField] private TMP_Text megawin_text;
    [SerializeField] private Image Win_Image;
    [SerializeField] private Sprite HugeWin_Sprite;
    [SerializeField] private Sprite BigWin_Sprite;
    [SerializeField] private Sprite MegaWin_Sprite;

    //[Header("gamble game")]
    //[SerializeField] private Button Gamble_button;
    //[SerializeField] private Button GambleExit_button;
    //[SerializeField] private GameObject Gamble_game;

    [Header("Audio")]
    [SerializeField] private AudioController audioController;

    [SerializeField]
    private Button GameExit_Button;

    [SerializeField]
    private SlotBehaviour slotManager;

    [SerializeField]
    private SocketIOManager socketManager;

    private bool isExit = false;


    private void Awake()
    {
        if (Loading_Object) Loading_Object.SetActive(true);
        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        float imageFill = 0f;
        DOTween.To(() => imageFill, (val) => imageFill = val, 0.7f, 2f).OnUpdate(() =>
        {
            if (Loading_Image) Loading_Image.fillAmount = imageFill;
            if (LoadPercent_Text) LoadPercent_Text.text = (100 * imageFill).ToString("f0") + "%";
        });
        yield return new WaitForSecondsRealtime(2);
        yield return new WaitUntil(() => socketManager.isLoaded);
        DOTween.To(() => imageFill, (val) => imageFill = val, 1, 1f).OnUpdate(() =>
        {
            if (Loading_Image) Loading_Image.fillAmount = imageFill;
            if (LoadPercent_Text) LoadPercent_Text.text = (100 * imageFill).ToString("f0") + "%";
        });
        yield return new WaitForSecondsRealtime(1f);
        if (Loading_Object) Loading_Object.SetActive(false);
    }

    private IEnumerator LoadingTextAnimate()
    {
        while (true)
        {
            if (Loading_Text) Loading_Text.text = "Loading.";
            yield return new WaitForSeconds(1f);
            if (Loading_Text) Loading_Text.text = "Loading..";
            yield return new WaitForSeconds(1f);
            if (Loading_Text) Loading_Text.text = "Loading...";
            yield return new WaitForSeconds(1f);
        }
    }

    private void Start()
    {
        if (Info_Button) Info_Button.onClick.RemoveAllListeners();
        if (Info_Button) Info_Button.onClick.AddListener(delegate { OpenPopup(PaytablePopup_Object); });

        if (PaytableExit_Button) PaytableExit_Button.onClick.RemoveAllListeners();
        if (PaytableExit_Button) PaytableExit_Button.onClick.AddListener(delegate { ClosePopup(PaytablePopup_Object); });

        if (Next_Button) Next_Button.onClick.RemoveAllListeners();
        if (Next_Button) Next_Button.onClick.AddListener(delegate { TurnPage(true); });

        if (Previous_Button) Previous_Button.onClick.RemoveAllListeners();
        if (Previous_Button) Previous_Button.onClick.AddListener(delegate { TurnPage(false); });

        if (Previous_Button) Previous_Button.interactable = false;

        if (paginationButtonGrp[0]) paginationButtonGrp[0].onClick.RemoveAllListeners();
        if (paginationButtonGrp[0]) paginationButtonGrp[0].onClick.AddListener(delegate { GoToPage(0); });

        if (paginationButtonGrp[1]) paginationButtonGrp[1].onClick.RemoveAllListeners();
        if (paginationButtonGrp[1]) paginationButtonGrp[1].onClick.AddListener(delegate { GoToPage(1); });

        if (paginationButtonGrp[2]) paginationButtonGrp[2].onClick.RemoveAllListeners();
        if (paginationButtonGrp[2]) paginationButtonGrp[2].onClick.AddListener(delegate { GoToPage(2); });

        if (paginationButtonGrp[3]) paginationButtonGrp[3].onClick.RemoveAllListeners();
        if (paginationButtonGrp[3]) paginationButtonGrp[3].onClick.AddListener(delegate { GoToPage(3); });

        if (Infoback_button) Infoback_button.onClick.RemoveAllListeners();
        if (Infoback_button) Infoback_button.onClick.AddListener(delegate { ClosePopup(PaytablePopup_Object); });


        if (Setting_button) Setting_button.onClick.RemoveAllListeners();
        if (Setting_button) Setting_button.onClick.AddListener(delegate { OpenPopup(Setting_panel); });

        if (Sound_slider) Sound_slider.onValueChanged.RemoveAllListeners();
        if (Sound_slider) Sound_slider.onValueChanged.AddListener(delegate { ChangeSound(); });

        if (Music_slider) Music_slider.onValueChanged.RemoveAllListeners();
        if (Music_slider) Music_slider.onValueChanged.AddListener(delegate { ChangeMusic(); }); 

        if (SettingExit_button) SettingExit_button.onClick.RemoveAllListeners();
        if (SettingExit_button) SettingExit_button.onClick.AddListener(delegate { ClosePopup(Setting_panel); });

        if (Setting_back_button) Setting_back_button.onClick.RemoveAllListeners();
        if (Setting_back_button) Setting_back_button.onClick.AddListener(delegate { ClosePopup(Setting_panel); });

        //if (Gamble_button) Gamble_button.onClick.RemoveAllListeners();
        //if (Gamble_button) Gamble_button.onClick.AddListener(delegate { OpenPopup(Gamble_game); });

        //if (GambleExit_button) GambleExit_button.onClick.RemoveAllListeners();
        //if (GambleExit_button) GambleExit_button.onClick.AddListener(delegate { ClosePopup(Gamble_game); });

        if (GameExit_Button) GameExit_Button.onClick.RemoveAllListeners();
        if (GameExit_Button) GameExit_Button.onClick.AddListener(delegate { OpenPopup(QuitPopup_Object); });

        if (NoQuit_Button) NoQuit_Button.onClick.RemoveAllListeners();
        if (NoQuit_Button) NoQuit_Button.onClick.AddListener(delegate { ClosePopup(QuitPopup_Object); });

        if (CrossQuit_Button) CrossQuit_Button.onClick.RemoveAllListeners();
        if (CrossQuit_Button) CrossQuit_Button.onClick.AddListener(delegate { ClosePopup(QuitPopup_Object); });

        if (BackQuit_Button) BackQuit_Button.onClick.RemoveAllListeners();
        if (BackQuit_Button) BackQuit_Button.onClick.AddListener(delegate { ClosePopup(QuitPopup_Object); });

        if (LBExit_Button) LBExit_Button.onClick.RemoveAllListeners();
        if (LBExit_Button) LBExit_Button.onClick.AddListener(delegate { ClosePopup(LBPopup_Object); });

        if (LBBack_Button) LBBack_Button.onClick.RemoveAllListeners();
        if (LBBack_Button) LBBack_Button.onClick.AddListener(delegate { ClosePopup(LBPopup_Object); });

        if (YesQuit_Button) YesQuit_Button.onClick.RemoveAllListeners();
        if (YesQuit_Button) YesQuit_Button.onClick.AddListener(CallOnExitFunction);

        if (CloseAD_Button) CloseAD_Button.onClick.RemoveAllListeners();
        if (CloseAD_Button) CloseAD_Button.onClick.AddListener(CallOnExitFunction);

        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.RemoveAllListeners();
        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.AddListener(CallOnExitFunction);

    }

    internal void LowBalPopup()
    {
        OpenPopup(LBPopup_Object);
    }

    internal void DisconnectionPopup()
    {
        //if (isReconnection)
        //{
        //    OpenPopup(ReconnectPopup_Object);
        //}
        //else
        //{
        //    ClosePopup(ReconnectPopup_Object);
        if (!isExit)
        {
            OpenPopup(DisconnectPopup_Object);
        }
        //}
    }


    internal void PopulateWin(int type, double amount)
    {
        double initAmount = 0;
        double originalAmount = amount;
        switch (type)
        {
            case 1:
                if (Win_Image) Win_Image.sprite = BigWin_Sprite;
                break;
            case 2:
                if (Win_Image) Win_Image.sprite = HugeWin_Sprite;
                break;
            case 3:
                if (Win_Image) Win_Image.sprite = MegaWin_Sprite;
                break;
        }
        if (megawin) megawin.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);

        DOTween.To(() => initAmount, (val) => initAmount = val, amount, 1f).OnUpdate(() =>
        {
            if (megawin_text) megawin_text.text = initAmount.ToString("f2");
        });

        DOVirtual.DelayedCall(3.5f, () =>
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
            if (megawin) megawin.SetActive(false);
            if (megawin_text) megawin_text.text = "0";
            slotManager.CheckPopups = false;

        });
    }

    internal void ADfunction()
    {
        OpenPopup(ADPopup_Object);
    }

    private void CallOnExitFunction()
    {
        isExit = true;
        audioController.PlayButtonAudio();
        slotManager.CallCloseSocket();
        Application.ExternalCall("window.parent.postMessage", "onExit", "*");
    }

    internal void InitialiseUIData(string SupportUrl, string AbtImgUrl, string TermsUrl, string PrivacyUrl, Paylines symbolsText)
    {
        PopulateSymbolsPayout(symbolsText);
    }

    private void PopulateSymbolsPayout(Paylines paylines)
    {
        for (int i = 0; i < SymbolsText.Length; i++)
        {
            string text = null;
            if (paylines.symbols[i].Multiplier[0][0] != 0)
            {
                text += "<color=yellow>5x - </color>" + paylines.symbols[i].Multiplier[0][0];
            }
            if (paylines.symbols[i].Multiplier[1][0] != 0)
            {
                text += "\n<color=yellow>4x - </color>" + paylines.symbols[i].Multiplier[1][0];
            }
            if (paylines.symbols[i].Multiplier[2][0] != 0)
            {
                text += "\n<color=yellow>3x - </color>" + paylines.symbols[i].Multiplier[2][0];
            }
            if (SymbolsText[i]) SymbolsText[i].text = text;
        }
    }

    private void OpenPopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
    }

    private void ClosePopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (Popup) Popup.SetActive(false);
        if (!DisconnectPopup_Object.activeSelf)
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }
    }

    private void TurnPage(bool type)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (type)
            paginationCounter++;
        else
            paginationCounter--;


        GoToPage(paginationCounter - 1);


    }

    private void GoToPage(int index)
    {

        paginationCounter = index + 1;

        paginationCounter = Mathf.Clamp(paginationCounter, 1, 4);

        if (Next_Button) Next_Button.interactable = !(paginationCounter >= 4);

        if (Previous_Button) Previous_Button.interactable = !(paginationCounter <= 1);

        for (int i = 0; i < PageList.Length; i++)
        {
            PageList[i].SetActive(false);
        }

        for (int i = 0; i < paginationButtonGrp.Length; i++)
        {
            paginationButtonGrp[i].interactable = true;
            paginationButtonGrp[i].transform.GetChild(0).gameObject.SetActive(false);
        }

        PageList[paginationCounter - 1].SetActive(true);
        paginationButtonGrp[paginationCounter - 1].interactable = false;
        paginationButtonGrp[paginationCounter - 1].transform.GetChild(0).gameObject.SetActive(true);
    }

    private void ChangeSound() {
     audioController.ChangeVolume("wl", Sound_slider.value);
     audioController.ChangeVolume("button", Sound_slider.value);

    }

    private void ChangeMusic() {
     audioController.ChangeVolume("bg", Music_slider.value);

    }
}
