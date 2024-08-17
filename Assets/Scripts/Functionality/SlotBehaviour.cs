using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
    [SerializeField]
    private RectTransform mainContainer_RT;

    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;
    [SerializeField]
    private List<SlotImage> Tempimages;

    [Header("Slots Objects")]
    [SerializeField]
    private GameObject[] Slot_Objects;
    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField] private Button BetPerLine;
    [SerializeField] private Button Bet_plus;
    [SerializeField] private Button Bet_minus;


    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] Symbol1;
    [SerializeField]
    private Sprite[] Symbol2;
    [SerializeField]
    private Sprite[] Symbol3;
    [SerializeField]
    private Sprite[] Symbol4;
    [SerializeField]
    private Sprite[] Symbol5;
    [SerializeField]
    private Sprite[] Symbol6;
    [SerializeField]
    private Sprite[] Symbol7;
    [SerializeField]
    private Sprite[] Symbol8;
    [SerializeField]
    private Sprite[] Symbol9;
    [SerializeField]
    private Sprite[] Symbol10;
    [SerializeField]
    private Sprite[] Symbol11;
    [SerializeField]
    private Sprite[] Symbol12;
    [SerializeField]
    private Sprite[] Symbol13;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text TotalWin_text;
    [SerializeField]
    private TMP_Text LineBet_text;

    private Dictionary<int, string> y_string = new Dictionary<int, string>();
        
    int tweenHeight = 0;

    [SerializeField]
    private GameObject Image_Prefab;

    [SerializeField]
    private PayoutCalculation PayCalculator;

    [SerializeField]
    private List<ImageAnimation> TempList;

    private List<Tweener> alltweens = new List<Tweener>();

    [SerializeField]
    private int IconSizeFactor = 100;
    [SerializeField]
    private int SpaceFactor = 0;

    private int numberOfSlots = 5;

    [SerializeField]
    int verticalVisibility = 3;

    [SerializeField]
    private SocketIOManager SocketManager;

    [SerializeField]
    private GambleController gambleController;

    [SerializeField]
    private Sprite[] Box_Sprites;

    [SerializeField]
    private AudioController audioController;

    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private BonusController _bonusManager;

    protected int Lines = 20;

    Coroutine AutoSpinRoutine = null;
    Coroutine tweenroutine;
    bool IsAutoSpin = false;
    bool IsSpinning = false;
    bool SlotRunning = false;
    internal bool CheckPopups = false;
    private int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    internal bool IsHoldSpin = false;

    private void Start()
    {

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        if (Bet_plus) Bet_plus.onClick.RemoveAllListeners();
        if (Bet_plus) Bet_plus.onClick.AddListener(delegate { ChangeBet(true); });

        if (Bet_minus) Bet_minus.onClick.RemoveAllListeners();
        if (Bet_minus) Bet_minus.onClick.AddListener(delegate { ChangeBet(false); });

        tweenHeight = (13 * IconSizeFactor) - 280;

        ToggleBetButton(0);
        //CheckBetButton();
    }

    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {

            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());
        }
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            if (BetCounter < SocketManager.initialData.Bets.Count - 1)
            {
                BetCounter++;
                if(BetCounter >= SocketManager.initialData.Bets.Count - 1)
                {
                    ToggleBetButton(0);
                }
                else
                {
                    ToggleBetButton(2);
                }
            }
        }
        else
        {
            if (BetCounter > 0)
            {
                BetCounter--;
                if(BetCounter <= 0)
                {
                    ToggleBetButton(1);
                }
                else
                {
                    ToggleBetButton(2);
                }
            }
        }

        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    private void ToggleBetButton(int toggle)
    {
        switch(toggle)
        {
            case 0:
                //When Reached To The Highest Of The Limit
                Bet_plus.interactable = false;
                Bet_minus.interactable = true;
                break;
            case 1:
                //When Reached To The Lowest Of The Limit
                Bet_plus.interactable = true;
                Bet_minus.interactable = false;
                break;
            case 2:
                //When Reached In The Middle Of The Limit
                Bet_plus.interactable = true;
                Bet_minus.interactable = true;
                break;
        }
    }

    private void CheckBetButton()
    {
        Debug.Log(string.Concat("<color=blue><b>", SocketManager.initialData.Bets.Count, "</b></color>"));
        if(BetCounter >= SocketManager.initialData.Bets.Count -1)
        {
            ToggleBetButton(0);
        }
        else if(BetCounter <= 0)
        {
            ToggleBetButton(1);
        }
        else
        {
            ToggleBetButton(2);
        }
    }

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
            if (AutoSpin_Button) AutoSpin_Button.interactable = false;
            if (SlotStart_Button) SlotStart_Button.interactable = false;
        }
        else
        {
            if (AutoSpin_Button) AutoSpin_Button.interactable = true;
            if (SlotStart_Button) SlotStart_Button.interactable = true;
        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }

    }

    private IEnumerator AutoSpinCoroutine()
    {

        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
        }
    }

    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count, LineVal);
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }

    //Start Auto Spin on Button Hold
    #region Hold to auto spin
    internal void StartSpinRoutine()
    {
        IsHoldSpin = false;
        Invoke("AutoSpinHold", 2f);
    }

    internal void StopSpinRoutine()
    {
        CancelInvoke("AutoSpinHold");
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            //if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private void AutoSpinHold()
    {
        Debug.Log("Auto Spin Started");
        IsHoldSpin = true;
        AutoSpin();
    }
    #endregion

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();

        if (TotalBet_text) TotalBet_text.text = "99999";
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space) && SlotStart_Button.interactable)
    //    {
    //        StartSlots();
    //    }
    //}

    //internal void PopulateInitalSlots(int number, List<int> myvalues)
    //{
    //    PopulateSlot(myvalues, number);
    //}

    internal void LayoutReset(int number)
    {
        if (Slot_Elements[number]) Slot_Elements[number].ignoreLayout = true;
        if (SlotStart_Button) SlotStart_Button.interactable = true;
    }

    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, myImages.Length);
                Tempimages[i].slotImages[j].transform.GetChild(0).GetComponent<Image>().sprite = myImages[randomIndex];
            }
        }
    }

    //private void PopulateSlot(List<int> values , int number)
    //{
    //    if (Slot_Objects[number]) Slot_Objects[number].SetActive(true);
    //    for(int i = 0; i<values.Count; i++)
    //    {
    //        GameObject myImg = Instantiate(Image_Prefab, Slot_Transform[number]);
    //        images[number].slotImages.Add(myImg.transform.GetChild(0).GetComponent<Image>());
    //        images[number].slotImages[i].sprite = myImages[values[i]];
    //    }
    //    for (int k = 0; k < 2; k++)
    //    {
    //        GameObject mylastImg = Instantiate(Image_Prefab, Slot_Transform[number]);
    //        images[number].slotImages.Add(mylastImg.transform.GetChild(0).GetComponent<Image>());
    //        images[number].slotImages[images[number].slotImages.Count - 1].sprite = myImages[values[k]];
    //    }
    //    if (mainContainer_RT) LayoutRebuilder.ForceRebuildLayoutImmediate(mainContainer_RT);
    //    tweenHeight = (values.Count * IconSizeFactor) - 280;
    //    GenerateMatrix(number);
    //}

    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 0:
                for (int i = 0; i < Symbol1.Length; i++)
                {
                    animScript.textureArray.Add(Symbol1[i]);
                }
                break;
            case 1:
                for (int i = 0; i < Symbol2.Length; i++)
                {
                    animScript.textureArray.Add(Symbol2[i]);
                }
                break;
            case 2:
                for (int i = 0; i < Symbol3.Length; i++)
                {
                    animScript.textureArray.Add(Symbol3[i]);
                }
                break;
            case 3:
                for (int i = 0; i < Symbol4.Length; i++)
                {
                    animScript.textureArray.Add(Symbol4[i]);
                }
                break;
            case 4:
                for (int i = 0; i < Symbol5.Length; i++)
                {
                    animScript.textureArray.Add(Symbol5[i]);
                }
                break;
            case 5:
                for (int i = 0; i < Symbol6.Length; i++)
                {
                    animScript.textureArray.Add(Symbol6[i]);
                }
                break;
            case 6:
                for (int i = 0; i < Symbol7.Length; i++)
                {
                    animScript.textureArray.Add(Symbol7[i]);
                }
                break;
            case 7:
                for (int i = 0; i < Symbol8.Length; i++)
                {
                    animScript.textureArray.Add(Symbol8[i]);
                }
                break;
            case 8:
                for (int i = 0; i < Symbol9.Length; i++)
                {
                    animScript.textureArray.Add(Symbol9[i]);
                }
                break;
            case 9:
                for (int i = 0; i < Symbol10.Length; i++)
                {
                    animScript.textureArray.Add(Symbol10[i]);
                }
                break;
            case 10:
                for (int i = 0; i < Symbol11.Length; i++)
                {
                    animScript.textureArray.Add(Symbol11[i]);
                }
                break;
            case 11:
                for (int i = 0; i < Symbol12.Length; i++)
                {
                    animScript.textureArray.Add(Symbol12[i]);
                }
                break;
            case 12:
                for (int i = 0; i < Symbol13.Length; i++)
                {
                    animScript.textureArray.Add(Symbol13[i]);
                }
                break;
        }
    }

    private void StartSlots(bool autoSpin=false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();
        if (gambleController) gambleController.toggleDoubleButton(false);
        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }

        if (SlotAnimRoutine != null)
        {
            StopCoroutine(SlotAnimRoutine);
            SlotAnimRoutine = null;
        }

        AnimStoppedProcess();
        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0) 
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        tweenroutine=StartCoroutine(TweenRoutine());
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            if (!IsSpinning)
            {
                if (audioController) audioController.StopWLAaudio();
            }
        }
    }

    [SerializeField]
    private List<int> TempLineIds;
    [SerializeField]
    private List<string> x_animationString;
    [SerializeField]
    private List<string> y_animationString;
    private IEnumerator TweenRoutine()
    {
        gambleController.GambleTweeningAnim(false);
        if (currentBalance < currentTotalBet)
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            yield break;
        }
        if (audioController) audioController.PlayWLAudio("spin");
        IsSpinning = true;
        ToggleButtonGrp(false);
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
            yield return new WaitForSeconds(0.1f);
        }

        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;

        balance = balance - bet;

        DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f2");
        });

        SocketManager.AccumulateResult(BetCounter);

        yield return new WaitUntil(() => SocketManager.isResultdone);

        for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++)
        {
            List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
            for (int i = 0; i < 5; i++)
            {
                if (images[i].slotImages[images[i].slotImages.Count - 5 + j].transform.GetChild(0)) images[i].slotImages[images[i].slotImages.Count - 5 + j].transform.GetChild(0).GetComponent<Image>().sprite = myImages[resultnum[i]];
                PopulateAnimationSprites(images[i].slotImages[images[i].slotImages.Count - 5 + j].transform.GetChild(0).gameObject.GetComponent<ImageAnimation>(), resultnum[i]);
            }
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i);
        }

        yield return new WaitForSeconds(0.3f);
        CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.jackpot);
        KillAllTweens();


        CheckPopups = true;

        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("f2");

        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");

        if (SocketManager.resultData.isBonus)
        {
            CheckBonusGame();
        }
        else
        {
            CheckWinPopups();
        }

        yield return new WaitUntil(() => !CheckPopups);
        if (!IsAutoSpin)
        {
            ActivateGamble();
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            ActivateGamble();
            yield return new WaitForSeconds(2f);
            //IsSpinning = false;
        }
    }

    private void ActivateGamble()
    {
        Debug.Log("run this line");
        if (SocketManager.playerdata.currentWining > 0 && SocketManager.playerdata.currentWining <= SocketManager.GambleLimit)
        {
            Debug.Log("run this line 1 ");
            gambleController.toggleDoubleButton(true);
            gambleController.GambleTweeningAnim(true);
        }
        else
        {
            Debug.Log("run this line exception " + SocketManager.playerdata.currentWining + "  " + SocketManager.GambleLimit);
        }
    }

    internal void DeactivateGamble()
    {
        StopAutoSpin();
        ToggleButtonGrp(true);
    }

    internal void CheckWinPopups()
    {
        if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15 && SocketManager.resultData.WinAmout < currentTotalBet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.resultData.WinAmout);
        }
        else
        {
            CheckPopups = false;
        }
    }

    internal void updateBalance()
    {
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("f2");
    }

    internal void CheckBonusGame()
    {
        _bonusManager.GetSuitCaseList(SocketManager.resultData.BonusResult);
    }

    void ToggleButtonGrp(bool toggle)
    {

        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (BetPerLine) BetPerLine.interactable = toggle;
        //if (Gamble_button) Gamble_button.interactable = toggle;
        if (toggle)
        {
            CheckBetButton();
        }
        else
        {
            if (Bet_minus) Bet_minus.interactable = toggle;
            if (Bet_plus) Bet_plus.interactable = toggle;
        }


    }

    internal void SetInitialUI()
    {
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString("f2");
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString("f2");
        if (TotalWin_text) TotalWin_text.text = "0.00";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }

    private IEnumerator slotLineAnim()
    {
        int n = 0;
        PayCalculator.ResetLines();
        while(n < 5)
        {
            List<int> y_anim = null;
            for (int i = 0; i < TempLineIds.Count; i++)
            {
                y_anim = y_string[TempLineIds[i]]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(TempLineIds[i], true);
                for (int k = 0; k < y_anim.Count; k++)
                {
                    if (Tempimages[k].slotImages[y_anim[k]].transform.GetChild(0).gameObject.GetComponent<ImageAnimation>().currentAnimationState == ImageAnimation.ImageState.PLAYING)
                    {
                        Tempimages[k].slotImages[y_anim[k]].transform.GetChild(0).gameObject.GetComponent<SlotScript>().SetBg(Box_Sprites[TempLineIds[i]]);
                    }
                }
                yield return new WaitForSeconds(3);
                for (int k = 0; k < y_anim.Count; k++)
                {
                    if (Tempimages[k].slotImages[y_anim[k]].transform.GetChild(0).gameObject.GetComponent<ImageAnimation>().currentAnimationState == ImageAnimation.ImageState.PLAYING)
                    {
                        Tempimages[k].slotImages[y_anim[k]].transform.GetChild(0).gameObject.GetComponent<SlotScript>().ResetBG();
                    }
                }
                PayCalculator.ResetStaticLine();
            }
            for (int i = 0; i < TempLineIds.Count; i++)
            {
                PayCalculator.GeneratePayoutLinesBackend(TempLineIds[i]);
            }
            yield return new WaitForSeconds(3);
            PayCalculator.ResetLines();
            yield return new WaitForSeconds(1);
            n++;
        }
        AnimStoppedProcess();
    }

    private Coroutine SlotAnimRoutine = null;


    private void AnimStoppedProcess()
    {
        StopGameAnimation();
        for (int i = 0; i < images.Count; i++)
        {
            foreach (Image child in images[i].slotImages)
            {
                child.transform.GetChild(0).gameObject.GetComponent<SlotScript>().ResetBG();
            }
        }
        PayCalculator.ResetLines();
        TempLineIds.Clear();
        TempLineIds.TrimExcess();
    }

    private void StartGameAnimation(GameObject animObjects) 
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        temp.StartAnimation();
        TempList.Add(temp);
    }

    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
        TempList.Clear();
        TempList.TrimExcess();
    }

    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, double jackpot = 0)
    {
        TempLineIds = LineId;
        List<int> points_anim = null;

        if (LineId.Count > 0)
        {
            if (audioController) audioController.PlayWLAudio("win");
            if (jackpot > 0)
            {
                for (int i = 0; i < Tempimages.Count; i++)
                {
                    for (int k = 0; k < Tempimages[i].slotImages.Count; k++)
                    {
                        StartGameAnimation(Tempimages[i].slotImages[k].transform.GetChild(0).gameObject);
                    }
                }
            }
            else
            {
                for (int i = 0; i < points_AnimString.Count; i++)
                {
                    points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                    for (int k = 0; k < points_anim.Count; k++)
                    {
                        if (points_anim[k] >= 10)
                        {
                            StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].transform.GetChild(0).gameObject);
                        }
                        else
                        {
                            StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].transform.GetChild(0).gameObject);
                        }
                    }
                }
            }
            PayCalculator.ResetStaticLine();
            if (SlotAnimRoutine != null)
            {
                StopCoroutine(SlotAnimRoutine);
                SlotAnimRoutine = null;
            }
            SlotAnimRoutine = StartCoroutine(slotLineAnim());

        }
        else
        {
            if (audioController) audioController.StopWLAaudio();
        }
    }

    private void GenerateMatrix(int value)
    {
        for (int j = 0; j < 3; j++)
        {
            Tempimages[value].slotImages.Add(images[value].slotImages[images[value].slotImages.Count - 5 + j]);
        }
    }

    internal void GambleCollect()
    {
        SocketManager.GambleCollectCall();
    }

    #region TweeningCode

    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }
    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index)
    {
        alltweens[index].Pause();
        int tweenpos = (reqpos * (IconSizeFactor + SpaceFactor)) - (IconSizeFactor + (2 * SpaceFactor));
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100 + (SpaceFactor > 0 ? SpaceFactor / 4 : 0), 0.5f).SetEase(Ease.OutElastic);
        yield return new WaitForSeconds(0.2f);
    }

    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}

