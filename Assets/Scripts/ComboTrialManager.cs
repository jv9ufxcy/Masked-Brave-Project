using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;

public class ComboTrialManager : MonoBehaviour
{
    [SerializeField] private CharacterObject comboDummyObject, playerCharacter;
    [SerializeField] private List<TrialCombo> trialComboList=new List<TrialCombo>();
    [SerializeField] private List<Transform> comboInputsTransformList = new List<Transform>();
    [SerializeField] private int comboStep = 0, trialStep = 0;
    [SerializeField] private Vector3 dummyPos;
    [SerializeField] private Transform controlsPanel,trialExitPanel;
    private AudioManager audioManager;
    private string[] trialStartButton = new string[] {"<sprite=32>", "<sprite=34>", "P" }, selectResetButton = new string[] { "<sprite=74>", "<sprite=72>", "<sprite=64>" };
    [SerializeField] private string resetButtonText = "Reset", progressText = "Finish", successComboAudio = "Props/Checkpoint", failedComboAudio = "CutsceneSkip/Radio Over";
    void OnEnable()
    {
        playerCharacter.OnStartPressed += StartPressed;
        playerCharacter.OnSelectPressed += SelectPressed;
        playerCharacter.OnTouchpadPressed += TouchpadPressed;
        comboDummyObject.Attack += AttackHit;
        comboDummyObject.ComboDrop += ComboDrop;
    }
    void OnDisable()
    {
        playerCharacter.OnStartPressed -= StartPressed;
        playerCharacter.OnSelectPressed -= SelectPressed;
        playerCharacter.OnTouchpadPressed -= TouchpadPressed;
        comboDummyObject.Attack -= AttackHit;
        comboDummyObject.ComboDrop -= ComboDrop;
    }
    private void Start()
    {
        audioManager = AudioManager.instance;
        ShowHideTextPanels(false);
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForFixedUpdate();
        OnStart?.Invoke();
    }

    public void BeginTrials()
    {
        trialActive = true;
        CreateTrialTransform();
        SetControlsText();
        RefillMeter();
        Debug.Log(gameObject.name);
    }
    public void ShowHideTextPanels(bool visible)
    {
        controlsPanel.gameObject.SetActive(visible);
        trialExitPanel.gameObject.SetActive(visible);
    }
    private void SetControlsText()
    {
        controlsPanel.GetComponent<TextMeshProUGUI>().SetText("<size=262.5%>" + selectResetButton[GameEngine.coreData.currentControllerIndex] + "<size=100%>" + resetButtonText);
        trialExitPanel.GetComponent<TextMeshProUGUI>().SetText("<size=262.5%>" + trialStartButton[GameEngine.coreData.currentControllerIndex] + "<size=100%>" + progressText);
        trialExitPanel.gameObject.SetActive(trialComplete);
    }
    private void CreateTrialTransform()
    {
        comboInputsTransformList = new List<Transform>();
        foreach (ComboMove move in trialComboList[trialStep].comboInputsMoveList)
        {
            CreateComboMoveEntryTransform(move, entryContainer, comboInputsTransformList);
        }
    }
    public void StartPressed()
    {
        if (trialActive)
        {
            if (trialComplete)
            {
                trialActive = false;
                OnFinishInput?.Invoke();
            }
        }
    }
    public void SelectPressed()
    {
        if (trialActive)
        {
            if (comboSuccess == false)//don't allow reset mid combo
            {
                ResetComboText();
                ResetPosition();
                RefillMeter();
            }
            if (trialComplete)
            {
                trialStep = 0;
                BeginTrials();
                trialComplete = false;
            }
        }
    }
    void TouchpadPressed()
    {
        if (trialActive) 
            SelectPressed();
    }
    public void ComboDrop()
    {
        comboStep = 0;
        if (comboSuccess)
        {
            NextTrial();
            comboSuccess = false;
        }
        else
        {
            ResetComboText();
        }
        RefillMeter();
    }

    private void RefillMeter()
    {
        playerCharacter.ChangeMeter(playerCharacter.specialMeterMax);
    }

    public void AttackHit(int attackStateIndex, AttackEvent attack)
    {
        // check array if state matches attack index and step
        ComboTrial(attackStateIndex);
    }
    private void ComboTrial(int attackState)
    {
        if (!comboSuccess && trialStep<trialComboList.Count)//ignore hits after combo completes and trial finishes
        {
            if (attackState == trialComboList[trialStep].comboInputsMoveList[comboStep].stateIndex)
            {
                comboInputsTransformList[comboStep].DOKill();
                comboInputsTransformList[comboStep].DOPunchPosition(Vector3.right, .125f);
                TextMeshProUGUI moveText = comboInputsTransformList[comboStep].gameObject.GetComponentInChildren<TextMeshProUGUI>();
                moveText.color = Color.yellow;
                comboStep++;
            }
            if (comboStep >= trialComboList[trialStep].comboInputsMoveList.Count)//correct inputs exceed
            {
                OnComboSuccess();
            }
        }
    }
    bool comboSuccess = false, trialComplete = false, trialActive = false;
    void OnComboSuccess()
    {
        comboSuccess = true;
        PlaySound(successComboAudio);
        foreach (Transform combo in comboInputsTransformList)
        {
            TextMeshProUGUI moveText = combo.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            moveText.DOKill();
            moveText.color = Color.green;
        }
    }
    void NextTrial()
    {
        foreach (Transform combo in comboInputsTransformList)
        {
            Destroy(combo.gameObject);
        }
        comboInputsTransformList.Clear();
        trialStep++;
        if (trialStep >= trialComboList.Count)
        {
            OnTrialComplete();
        }
        else
        {
            CreateTrialTransform();
        }
    }
    public UnityEvent OnTrialClear,OnFinishInput,OnStart;
    void OnTrialComplete()
    {
        trialComplete = true;
        OnTrialClear?.Invoke();
        trialExitPanel.gameObject.SetActive(trialComplete);
    }
    void ResetComboText()
    {
        if (comboInputsTransformList != null)
        {
            foreach (Transform combo in comboInputsTransformList)
            {
                TextMeshProUGUI moveText = combo.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                moveText.DOKill();
                moveText.DOColor(Color.red, 0f);
                moveText.DOColor(Color.white, 0.25f);
            }
        }
    }
    void ResetPosition()
    {
        comboDummyObject.StartStateFromScript(0);
        comboDummyObject.SetVelocity(Vector2.zero);
        comboDummyObject.transform.position = dummyPos;
    }
    void PlaySound(string audioName)
    {
        audioManager.PlaySound(audioName);
    }
    private void CreateComboMoveEntryTransform(ComboMove comboMove,Transform container, List<Transform>transformList)
    {
        float templateHeight = 16f;
        Transform entryTransform = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        entryTransform.gameObject.SetActive(true);
        entryTransform.GetComponentInChildren<TextMeshProUGUI>().SetText("<size=262.5%>"+ comboMove.consoleInputs[GameEngine.coreData.currentControllerIndex] + "<size=100%>"+comboMove.name);
        transformList.Add(entryTransform);
    }

    [SerializeField] private Transform entryContainer, entryTemplate;
    [System.Serializable]
    private class ComboMove
    {
        [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
        public int stateIndex;
        public string name;
        public string[] consoleInputs;
    }
    [System.Serializable]
    private class TrialCombo
    {
        public string comboTitle;
        public List<ComboMove> comboInputsMoveList = new List<ComboMove>();
    }
}

