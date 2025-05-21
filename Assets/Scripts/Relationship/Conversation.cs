using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Conversation : MonoBehaviour
{
    [Header("Important thingies")]
    public static Conversation Instance;
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

    public Image I_Speaker_L;
    public Image I_Speaker_R;
    public Animator A_speaker_L;
    public Animator A_speaker_R;
    [Space(10)]
    public Animator A_bouncer;

    private int i_dialogueChoice = 0;
    public float F_choiceMoveSpeed = 0.2f;
    private float f_choiceMoveTimer = 0;

    [Header("Banter References")]
    public CanvasGroup CG_banterHolder;
    public TextMeshProUGUI TM_messages;

    public RectTransform RT_messageHolder;
    public RectTransform PF_messagePrefab;
    public List<MessageClass> M_activeMessages = new List<MessageClass>();

    public class MessageClass
    {
        public ConversationManager.stringClass S_Message;
        public string S_speakerString;
        public string S_curString;

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


    private void Awake()
    {
        Instance = this;
    }

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
            if (item.T_Hook == _hook)
                return;
        }
        if (C_Manager.GetConversation(_dialogueID, out _convo))
        {
            MessageClass _temp = new MessageClass();
            int _num = UnityEngine.Random.Range(0, _convo.strings.Count);
            _temp.S_Message = _convo.strings[_num];
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
            if (PlayerController.GameState != PlayerController.gameStateEnum.dialogue &&
                PlayerController.GameState != PlayerController.gameStateEnum.dialogueResponse)
            {
                PlayAudio(audioEnum.showDialogue);
                StartCoroutine(Fade(CG_dialogueHolder, true));
                StartCoroutine(Fade(V_dialogueVolume, true));
                StartCoroutine(Fade(CG_banterHolder, false));
                HideDialogueChoices(true);
            }
            else
                HideDialogueChoices(false);
            PlayerController.GameState = PlayerController.gameStateEnum.dialogue;
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
        PlayerController.GameState = PlayerController.gameStateEnum.dialogueResponse;
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
            default:
                EndConversation();
                break;
        }
    }

    void EndConversation()
    {
        StartCoroutine(Fade(CG_dialogueHolder, false));
        StartCoroutine(Fade(V_dialogueVolume, false));
        StartCoroutine(Fade(CG_banterHolder, true));
        PlayerController.GameState = PlayerController.gameStateEnum.active;
    }
    void TypeMessage()
    {
        if (C_typing != null)
            StopCoroutine(C_typing);
        ConversationManager.dStringClass _string = curConversation.strings[i_convoStep];

        Image _speaker;
        Image _listener;
        if (_string.leftSide)   { _speaker = I_Speaker_L; _listener = I_Speaker_R; }
        else                    { _speaker = I_Speaker_R; _listener = I_Speaker_L; }
        _speaker.sprite = _string.GetFace(true);
        _listener.sprite = _string.GetFace(false);
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
        RectTransform _holder = GameObject.Instantiate(PF_messagePrefab, RT_messageHolder);
        Transform _target = _message.T_Hook;
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
            FollowObject(_holder, _target);
            yield return new WaitForEndOfFrame();
        }
        _message.S_curString = _text;
        _timer = 0;
        while (_timer < 1)
        {
            _timer += Time.deltaTime / _lifetime;
            FollowObject(_holder, _target);
            yield return new WaitForEndOfFrame();
        }
        _timer = 0;
        while (_timer < 1)
        {
            _message.C_speakerColor = Color.Lerp(_startColorSpeaker, _endColorSpeaker, _timer);
            _message.C_curColor = Color.Lerp(_startColor, _endColor, _timer);
            _timer += Time.deltaTime / 0.5f;
            FollowObject(_holder, _target);
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
    void FollowObject(RectTransform _holder, Transform _target)
    {
        Vector3 _tarPos = Camera.main.WorldToScreenPoint(_target.position);

        _tarPos /= C_canvas.scaleFactor;

        Vector2 _size = _holder.sizeDelta / 2;
        Vector2 _xBounds = new Vector2(_size.x, (Screen.width / C_canvas.scaleFactor) - _size.x);
        Vector2 _yBounds = new Vector2(_size.y, (Screen.height / C_canvas.scaleFactor) - _size.y);

        _tarPos.x = Mathf.Clamp(_tarPos.x, _xBounds.x, _xBounds.y);
        _tarPos.y = Mathf.Clamp(_tarPos.y, _yBounds.x, _yBounds.y);
        _holder.anchoredPosition = _tarPos;
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
    IEnumerator Fade(CanvasGroup _canvas, bool _fadeIn = true, float _speed = 0.1f)
    {
        if (_fadeIn || _canvas.gameObject.activeSelf)
        {
            _canvas.gameObject.SetActive(true);
            float _timer = 0f;
            float _start = _fadeIn ? 0f : 1f;
            float _end = _fadeIn ? 1f : 0f;
            while (_timer < 1f)
            {
                _canvas.alpha = Mathf.Lerp(_start, _end, _timer);
                yield return new WaitForEndOfFrame();
                _timer += Time.deltaTime / _speed;
            }
            _canvas.alpha = _end;
            if (!_fadeIn)
                _canvas.gameObject.SetActive(false);
        }
    }
    IEnumerator Fade(Volume _volume, bool _fadeIn = true, float _speed = 0.1f)
    {
        if (_fadeIn || _volume.gameObject.activeSelf)
        {
            _volume.gameObject.SetActive(true);
            float _timer = 0f;
            float _start = _fadeIn ? 0f : 1f;
            float _end = _fadeIn ? 1f : 0f;
            while (_timer < 1f)
            {
                _volume.weight = Mathf.Lerp(_start, _end, _timer);
                yield return new WaitForEndOfFrame();
                _timer += Time.deltaTime / _speed;
            }
            _volume.weight = _end;
            if (!_fadeIn)
                _volume.gameObject.SetActive(false);
        }
    }
}
