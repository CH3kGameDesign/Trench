using System.Collections.Generic;
using UnityEngine;

public class MessageCreator : MonoBehaviour
{
    public string S_name;
    public SS_MessageObject PF_message;
    public Sprite S_sprite;
    private SS_MessageObject _activeMessage;

    public canvasEnum canvasType = canvasEnum.all;
    public enum canvasEnum { all, onFoot, inShip}

    protected void Start()
    {
        PlayerManager.Instance.CheckMain(SetupMessage);
    }
    void SetupMessage()
    {
        Canvas _canvas = PlayerManager.conversation.C_canvas;

        //Set Holder To Canvas Type
        RectTransform _holder;
        switch (canvasType)
        {
            case canvasEnum.onFoot: _holder = PlayerManager.conversation.RT_messageHolder_Foot; break;
            case canvasEnum.inShip: _holder = PlayerManager.conversation.RT_messageHolder_Ship; break;
            default: _holder = PlayerManager.conversation.RT_messageHolder_All; break;
        }

        _activeMessage = Instantiate(PF_message, _holder);
        _activeMessage.Setup(S_sprite, transform, _canvas, S_name, true);
    }
    protected void OnEnable()
    {
        if (_activeMessage != null)
            _activeMessage.gameObject.SetActive(true);
    }
    protected void OnDisable()
    {
        if (_activeMessage != null)
            _activeMessage.gameObject.SetActive(false);
    }
    protected void OnDestroy()
    {
        if (_activeMessage != null)
            Destroy(_activeMessage.gameObject);
    }
}
