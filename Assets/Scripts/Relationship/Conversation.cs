using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using Unity.VisualScripting.FullSerializer;

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
    public Canvas C_canvas;
    public Volume V_dialogueVolume;

    public Image I_Speaker_L;
    public Image I_Speaker_R;
    public Animator A_speaker_L;
    public Animator A_speaker_R;


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
            int _num = Random.Range(0, _convo.strings.Count);
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
            HideDialogueChoices();
            StartCoroutine(Fade(CG_dialogueHolder, true));
            StartCoroutine(Fade(V_dialogueVolume, true));
            StartCoroutine(Fade(CG_banterHolder, false));
            A_speaker_L.PlayClip("Transition_Inactive");
            A_speaker_R.PlayClip("Transition_New");
            PlayerController.GameState = PlayerController.gameStateEnum.dialogue;
            curConversation = _convo;
            i_convoStep = 0;
            TypeMessage();
        }
    }
    public void NextLine()
    {
        if (C_typing != null)
            b_typing = false;
        else
        {
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
                    {
                        Vector2 _pos = RT_speakerMover.anchoredPosition;
                        _pos.y = 250;
                        RT_speakerMover.anchoredPosition = _pos;
                        for (int i = 0; i < TM_dialogueChoices.Length; i++)
                        {
                            if (i >= curConversation.response.Count)
                                TM_dialogueChoices[i].gameObject.SetActive(false);
                            else
                            {
                                TM_dialogueChoices[i].gameObject.SetActive(true);
                                TM_dialogueChoices[i].text = curConversation.response[i].GetString();
                            }
                            PlayerController.GameState = PlayerController.gameStateEnum.dialogueResponse;
                        }
                    }
                    else
                        EndConversation();
                }
            }
        }
    }

    void HideDialogueChoices()
    {
        for (int i = 0; i < TM_dialogueChoices.Length; i++)
            TM_dialogueChoices[i].gameObject.SetActive(false);
        Vector2 _pos = RT_speakerMover.anchoredPosition;
        _pos.y = 0;
        RT_speakerMover.anchoredPosition = _pos;
    }

    public void EndConversation()
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
        C_typing = StartCoroutine(TypeMessage_Coroutine());
    }

    IEnumerator TypeMessage_Coroutine()
    {
        float _timer = 0;
        float _delay = curConversation.strings[i_convoStep].speed;
        string _string = curConversation.strings[i_convoStep].GetString();
        int _stringLength = 0;
        b_typing = true;
        while (b_typing)
        {
            if (_timer <= 0)
            {
                _stringLength++;
                if (_stringLength < _string.Length)
                {
                    TM_dialogue.text = _string.Substring(0, _stringLength);
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
        TM_dialogue.text = _string;
        C_typing = null;
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
