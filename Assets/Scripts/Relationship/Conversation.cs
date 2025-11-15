using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Conversation : MonoBehaviour
{
    [Header("Important thingies")]
    public ConversationManager C_Manager;

    [Header("Conversation References")]
    public CanvasGroup CG_dialogueHolder;
    public RectTransform RT_speakerMover;
    public TextMeshProUGUI TM_speaker;
    public TextMeshProUGUI TM_dialogue;
    public TextMeshProUGUI[] TM_dialogueChoices;
    public Animator[] A_dialogueBouncers;
    public Canvas C_canvas;
    public Volume V_dialogueVolume;

    public RawImage I_Speaker_L;
    public RawImage I_Speaker_R;
    public Animator A_speaker_L;
    public Animator A_speaker_R;
    [Space(10)]
    public Animator A_bouncer;

    private int i_dialogueChoice = 0;
    public float F_choiceMoveSpeed = 0.2f;
    private float f_choiceMoveTimer = 0;
    [Header("Speaker Image References")]
    public Camera PF_speakerCamera;
    public speakerClass speakerL = new speakerClass();
    public speakerClass speakerR = new speakerClass();

    [System.Serializable]
    public class speakerClass
    {
        [HideInInspector] public Camera c_speakerCamera = null;
        [HideInInspector] public List<ConversationManager.characterClass> characters = new List<ConversationManager.characterClass>();
        [HideInInspector] public List<RagdollManager> RMs = new List<RagdollManager>();
        [HideInInspector] public List<Animator> animators = new List<Animator>();
        public RenderTexture RendTex;

        public void Setup(Camera _cam, bool _left, List<ConversationManager.characterClass> _char)
        {
            if (c_speakerCamera == null)
            {
                float _offset = 10f;
                if (_left) _offset = -_offset;
                c_speakerCamera = Instantiate(_cam, new Vector3(_offset, -10000, 0), _cam.transform.rotation);
            }
            c_speakerCamera.targetTexture = RendTex;
            ClearCharacters();
            characters = _char;
            int i = 0;
            foreach (ConversationManager.characterClass c in characters)
            {
                RagdollManager rm = Instantiate(c.PF_ragdoll, c_speakerCamera.transform);
                rm.transform.parent = c_speakerCamera.transform;
                rm.transform.localPosition = new Vector3(0,-1.5f,10f);
                rm.transform.localPosition += Vector3.forward * i;

                if (_left)
                {
                    rm.transform.localPosition -= Vector3.right * i;
                    rm.transform.localEulerAngles = new Vector3(0, 140, 0);
                }
                else
                {
                    rm.transform.localPosition += Vector3.right * i;
                    rm.transform.localEulerAngles = new Vector3(0, 180, 0);
                }
                RMs.Add(rm);
                Animator a = rm.GetComponent<Animator>();
                a.SetLayerWeight(1, 0);
                animators.Add(a);

                if(ConversationManager.Instance.GetCharacterID(c.id) == CharacterID.Player)
                    SaveData.equippedArmorSet.Equip(rm);
                else
                    c.Armor.Equip(rm);
            }
        }
        public void Close()
        {
            Destroy(c_speakerCamera.gameObject);
            c_speakerCamera = null;
            ClearCharacters();
        }
        void ClearCharacters()
        {
            RMs.Clear();
            characters.Clear();
            foreach (var item in animators)
                Destroy(item.gameObject);
            animators.Clear();
        }
        public void Play(string _clip)
        {
            if (animators.Count > 0)
                animators[0].Play(_clip);
        }
        public void Play(List<string> _clips)
        {
            for (int i = 0; i < _clips.Count && i < animators.Count; i++)
                animators[i].Play(_clips[i]);
        }
    }

    [Header("Banter References")]
    public CanvasGroup CG_banterHolder;
    public TextMeshProUGUI TM_messages;

    public RectTransform RT_messageHolder_All;
    public RectTransform RT_messageHolder_Foot;
    public RectTransform RT_messageHolder_Ship;
    public SS_MessageObject PF_messagePrefab;
    public Sprite S_messageSprite;
    public List<MessageClass> M_activeMessages = new List<MessageClass>();

    public class MessageClass
    {
        public ConversationManager.stringClass S_Message;
        public string S_speakerString;
        public string S_curString;

        public ConversationID _id;

        public Color C_speakerColor = Color.black;
        public Color C_curColor = Color.black;
        public Transform T_Hook;
        public float F_lifetime;
        public string GetString()
        {
            string _temp = "<color=#" + UnityEngine.ColorUtility.ToHtmlStringRGBA(C_speakerColor) + ">";
            _temp += S_speakerString;
            _temp += "</color>" + "<br>";

            _temp += "<color=#" + UnityEngine.ColorUtility.ToHtmlStringRGBA(C_curColor) +">";
            _temp += S_curString;
            _temp += "</color>" + "<br>";
            return _temp;
        }
    }
    [Header("Audio References")]
    public AudioSource AS_audioSource;
    public AudioSource AS_audioBabbleSource;
    public _audioClips audioClips = new _audioClips();

    [System.Serializable]
    public class _audioClips
    {
        public AudioClip showDialogue;
        public AudioClip skipDialogue;
        public AudioClip nextDialogue;
        [Space(10)]
        public AudioClip nextChoice;
        public AudioClip confirmChoice;
        [Space(10)]
        public AudioClip[] babble;
    }
    public enum audioEnum { 
        showDialogue, 
        skipDialogue, 
        nextDialogue, 
        nextChoice, 
        confirmChoice,
        babble
    };

    private ConversationManager.dialogueClass curConversation;
    private int i_convoStep = 0;

    private Coroutine C_typing;
    private bool b_typing = false;


    private void Update()
    {
        UpdateMessageText();

        ChoiceMoveTimer();
    }

    void ChoiceMoveTimer()
    {
        if (f_choiceMoveTimer >= 0)
            f_choiceMoveTimer -= Time.deltaTime;
    }

    void UpdateMessageText()
    {
        string _temp = "";
        foreach (var item in M_activeMessages)
            _temp += item.GetString();
        TM_messages.text = _temp;
    }

    public void StartMessage(ConversationID _dialogueID, Transform _hook)
    {
        ConversationManager.banterClass _convo;
        foreach (var item in M_activeMessages)
        {
            if (item.T_Hook == _hook && _dialogueID == item._id)
                return;
        }
        if (C_Manager.GetConversation(_dialogueID, out _convo))
        {
            MessageClass _temp = new MessageClass();
            int _num = UnityEngine.Random.Range(0, _convo.strings.Count);
            _temp.S_Message = _convo.strings[_num];
            _temp._id = _dialogueID;
            _temp.T_Hook = _hook;
            _temp.C_curColor = new Color(1, 1, 1, 0.8f);
            _temp.S_speakerString = _convo.strings[_num].GetName();
            _temp.C_speakerColor = new Color(0.75f, 0.75f, 1f, 0.8f);
            _temp.F_lifetime = 3f;
            M_activeMessages.Add(_temp);
            StartCoroutine(TypeMessage_Coroutine(_temp));
        }
    }

    public void StartDialogue(ConversationID _dialogueID)
    {
        ConversationManager.dialogueClass _convo;
        if (C_Manager.GetConversation(_dialogueID, out _convo))
        {
            if (PlayerManager.main.GameState != PlayerController.gameStateEnum.dialogue &&
                PlayerManager.main.GameState != PlayerController.gameStateEnum.dialogueResponse)
            {
                PlayAudio(audioEnum.showDialogue);
                StartCoroutine(CG_dialogueHolder.Fade(true));
                StartCoroutine(V_dialogueVolume.Fade(true));
                StartCoroutine(CG_banterHolder.Fade(false));
                HideDialogueChoices(true);
            }
            else
                HideDialogueChoices(false);
            PlayerManager.main.GameState_Change(PlayerController.gameStateEnum.dialogue);

            A_speaker_L.PlayClip("Transition_Inactive");
            A_speaker_R.PlayClip("Transition_New");
            curConversation = _convo;
            i_convoStep = 0;
            TypeMessage();
        }
    }
    public void NextLine()
    {
        if (C_typing != null)
        {
            PlayAudio(audioEnum.skipDialogue);
            b_typing = false;
        }
        else
        {
            PlayAudio(audioEnum.nextDialogue);
            if (curConversation != null)
            {
                if (i_convoStep < curConversation.strings.Count - 1)
                {
                    i_convoStep++;
                    A_speaker_R.PlayClip("Transition_New");
                    TypeMessage();
                }
                else
                {
                    if (curConversation.response.Count > 0)
                        ShowDialogueChoices();
                    else
                    {
                        ApplyFollowUp(curConversation.followUp);
                    }
                }
            }
        }
    }

    void ShowDialogueChoices()
    {
        PlayerManager.main.GameState_Change(PlayerController.gameStateEnum.dialogueResponse);
        A_speaker_L.PlayClip("Transition_New");
        A_speaker_R.PlayClip("Transition_Inactive");
        A_bouncer.Play("Idle_Hidden");
        RT_speakerMover.DOAnchorPosY(250, 0.4f);

        i_dialogueChoice = 0;
        for (int i = 0; i < TM_dialogueChoices.Length; i++)
        {
            if (i >= curConversation.response.Count)
                TM_dialogueChoices[i].gameObject.SetActive(false);
            else
            {
                TM_dialogueChoices[i].gameObject.SetActive(true);
                StartCoroutine(TypeMessage_Coroutine(TM_dialogueChoices[i], curConversation.response[i].GetString()));
            }

            A_dialogueBouncers[i].gameObject.SetActive(i == i_dialogueChoice);
        }
        UpdateDialogueChoices();
    }

    void HideDialogueChoices(bool _instant = false)
    {
        for (int i = 0; i < TM_dialogueChoices.Length; i++)
            TM_dialogueChoices[i].gameObject.SetActive(false);
        if (_instant)
            RT_speakerMover.DOAnchorPosY(0, 0f);
        else
            RT_speakerMover.DOAnchorPosY(0, 0.4f);
    }

    public void MoveDialogueChoice(int _dir)
    {
        if (f_choiceMoveTimer < 0)
        {
            f_choiceMoveTimer = F_choiceMoveSpeed;
            PlayAudio(audioEnum.nextChoice);

            i_dialogueChoice += _dir;
            i_dialogueChoice = Mathf.Clamp(i_dialogueChoice, 0, curConversation.response.Count - 1);
            UpdateDialogueChoices();
        }
    }

    void UpdateDialogueChoices()
    {
        int _min = Mathf.Min(TM_dialogueChoices.Length, curConversation.response.Count);
        Vector2 sizeMin = TM_dialogueChoices[0].rectTransform.sizeDelta;
            sizeMin.y = 40;
        Vector2 sizeMax = sizeMin;
            sizeMax.y = 60;
        for (int i = 0; i < _min; i++)
        {
            if (i == i_dialogueChoice)
                TM_dialogueChoices[i].rectTransform.DOSizeDelta(sizeMax,0.2f);
            else
                TM_dialogueChoices[i].rectTransform.DOSizeDelta(sizeMin, 0.2f);

            A_dialogueBouncers[i].gameObject.SetActive(i == i_dialogueChoice);
        }
    }
    public void ConfirmDialogueChoice()
    {
        PlayAudio(audioEnum.confirmChoice);
        ApplyFollowUp(curConversation.response[i_dialogueChoice].followUp);
    }
    public void ApplyFollowUp(ConversationManager.followUpClass _resp)
    {
        switch (_resp.type)
        {
            case ConversationManager.responseEnum.nothing:
                EndConversation();
                break;
            case ConversationManager.responseEnum.dialogue:
                StartDialogue(_resp._convo);
                break;
            case ConversationManager.responseEnum.unityEvent:
                ApplyUnityEvent(_resp._event);
                break;
            default:
                break;
        }
    }

    public void ApplyUnityEvent(string _event)
    {
        switch (_event)
        {
            case "NewObjectives":
                Objective.Instance.NewObjectives();
                EndConversation();
                break;
            case "openStore":
                EndConversation();
                MainMenu.Instance.Open(MainMenu.panelEnum.store);
                break;
            case "openCustomize":
                EndConversation();
                MainMenu.Instance.Open(MainMenu.panelEnum.customize);
                break;
            case "openCustomizeLayout":
                EndConversation();
                MainMenu.Instance.Open(MainMenu.panelEnum.customizeLayout);
                break;
            case "openCustomizeGraffiti":
                EndConversation();
                MainMenu.Instance.Open(MainMenu.panelEnum.customizeGraffiti);
                break;
            default:
                EndConversation();
                break;
        }
    }

    void EndConversation()
    {
        StartCoroutine(CG_dialogueHolder.Fade(false));
        StartCoroutine(V_dialogueVolume.Fade(false));
        StartCoroutine(CG_banterHolder.Fade(true));
        PlayerManager.main.GameState_Change(PlayerController.gameStateEnum.active);
    }
    void TypeMessage()
    {
        if (C_typing != null)
            StopCoroutine(C_typing);
        ConversationManager.dStringClass _string = curConversation.strings[i_convoStep];
        TM_speaker.text = _string.GetName();
        /*
        Image _speaker;
        Image _listener;
        if (_string.leftSide)   { _speaker = I_Speaker_L; _listener = I_Speaker_R; }
        else                    { _speaker = I_Speaker_R; _listener = I_Speaker_L; }
        _speaker.sprite = _string.GetFace(true);
        _listener.sprite = _string.GetFace(false);
        */
        if (_string.leftSide)
        {
            speakerL.Setup(PF_speakerCamera, true, new List<ConversationManager.characterClass>() { _string.GetSpeaker() });
            speakerR.Setup(PF_speakerCamera, false, new List<ConversationManager.characterClass>() { _string.GetListener() });
            speakerL.Play(ConversationManager.GetAnim(_string.emotion));
            speakerR.Play(ConversationManager.GetAnim(_string.otherEmotion));
        }
        else
        {
            speakerL.Setup(PF_speakerCamera, true, new List<ConversationManager.characterClass>() { _string.GetListener() });
            speakerR.Setup(PF_speakerCamera, false, new List<ConversationManager.characterClass>() { _string.GetSpeaker() });
            speakerL.Play(ConversationManager.GetAnim(_string.otherEmotion));
            speakerR.Play(ConversationManager.GetAnim(_string.emotion));
        }
            C_typing = StartCoroutine(TypeMessage_Coroutine(TM_dialogue, _string.GetString(), _string.speed, true));
    }

    IEnumerator TypeMessage_Coroutine(TextMeshProUGUI _TM, string _string, float _delay = 0.02f, bool mainMessage = false)
    {
        float _timer = 0;
        int _stringLength = 0;
        if (mainMessage)
        {
            b_typing = true;
            A_bouncer.Play("Transition_Skipper");
        }
        while (_stringLength < _string.Length)
        {
            if (_timer <= 0)
            {
                PlayAudio(audioEnum.babble);
                _stringLength++;
                    _TM.text = _string.Substring(0, _stringLength);
                _timer = _delay;
            }
            yield return new WaitForEndOfFrame();
            _timer -= Time.deltaTime;

            if (mainMessage && !b_typing)
                break;
        }
        _TM.text = _string;
        if (mainMessage)
        {
            b_typing = false;
            C_typing = null;
            A_bouncer.Play("Transition_Bouncer");
        }
    }
    IEnumerator TypeMessage_Coroutine(MessageClass _message)
    {
        Transform _target = _message.T_Hook;
        SS_MessageObject _holder = GameObject.Instantiate(PF_messagePrefab, RT_messageHolder_All);
        _holder.Setup(S_messageSprite, _target, C_canvas, "", false);
        float _lifetime = _message.F_lifetime;
        float _timer = 0;
        float _delay = _message.S_Message.speed;
        string _text = _message.S_Message.GetString();
        int _stringLength = 0;

        Color _startColorSpeaker = _message.C_speakerColor;
        Color _endColorSpeaker = _startColorSpeaker;
        _endColorSpeaker.a = 0;

        Color _startColor = _message.C_curColor;
        Color _endColor = _startColor;
        _endColor.a = 0;

        b_typing = true;
        while (b_typing)
        {
            if (_timer <= 0)
            {
                _stringLength++;
                if (_stringLength < _text.Length)
                {
                    _message.S_curString = _text.Substring(0, _stringLength);
                }
                else
                {
                    b_typing = false;
                    break;
                }
                _timer = _delay;
            }
            _timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _message.S_curString = _text;
        _timer = 0;
        while (_timer < 1)
        {
            _timer += Time.deltaTime / _lifetime;
            yield return new WaitForEndOfFrame();
        }
        _timer = 0;
        while (_timer < 1)
        {
            _message.C_speakerColor = Color.Lerp(_startColorSpeaker, _endColorSpeaker, _timer);
            _message.C_curColor = Color.Lerp(_startColor, _endColor, _timer);
            _timer += Time.deltaTime / 0.5f;
            yield return new WaitForEndOfFrame();
        }
        Message_RemoveWithTarget(_target);
        Destroy(_holder.gameObject);
    }
    void Message_RemoveWithTarget(Transform _target)
    {
        foreach (var item in M_activeMessages)
            if (item.T_Hook == _target)
            {
                M_activeMessages.Remove(item);
                break;
            }
    }
    
    
    void PlayAudio(audioEnum _enum)
    {
        AudioSource _audioSource = AS_audioSource;
        switch (_enum)
        {
            case audioEnum.showDialogue:
                _audioSource.clip = audioClips.showDialogue;
                break;
            case audioEnum.skipDialogue:
                _audioSource.clip = audioClips.skipDialogue;
                break;
            case audioEnum.nextDialogue:
                _audioSource.clip = audioClips.nextDialogue;
                break;
            case audioEnum.nextChoice:
                _audioSource.clip = audioClips.nextChoice;
                break;
            case audioEnum.confirmChoice:
                _audioSource.clip = audioClips.confirmChoice;
                break;
            case audioEnum.babble:
                _audioSource = AS_audioBabbleSource;
                _audioSource.clip = audioClips.babble.GetRandom();
                break;
            default:
                break;
        }
        _audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        _audioSource.Play();
    }
}
