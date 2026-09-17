using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class LevelSelect_SpawnArea : MonoBehaviour
{
    public LevelSelect LS;
    public Image[] I_areaShapes = new Image[0];
    public Location location;
    public ButtonAdvanced PF_buttonPrefab;
    private List<ButtonAdvanced> missionButtons = new List<ButtonAdvanced>();
    private List<int> missionNum = new List<int>();

    public void Start()
    {
        foreach (Image item in I_areaShapes)
            item.enabled = false;
        SpawnMissions();
    }

    public void SpawnMissions()
    {
        int _selArea = Random.Range(0, I_areaShapes.Length);
        if (location.availableMissions.Count == 0)
            missionButtons.Add(CreateButton(I_areaShapes[_selArea].GetComponent<RectTransform>(), GetEvent(0)));
        else for (int i = 0; i < location.availableMissions.Count; i++)
        {
            missionButtons.Add(CreateButton(I_areaShapes[_selArea].GetComponent<RectTransform>(), GetEvent(i)));
            missionNum.Add(i);
            _selArea = (_selArea + 1) % I_areaShapes.Length;
        }
    }

    ButtonAdvanced CreateButton(RectTransform _RT, System.Action _onPress)
    {
        //Instantiate
        ButtonAdvanced ba = Instantiate(PF_buttonPrefab, transform);
        RectTransform rt = ba.GetComponent<RectTransform>();
        ba.Setup(_onPress, null, null, location.T2_icon, null, location.S_name);
        //Change Position
        Vector2 pos = _RT.anchoredPosition;
        Vector2 bound = (_RT.sizeDelta - rt.sizeDelta) / 2;
        pos.x += Random.Range(-bound.x, bound.x);
        pos.y += Random.Range(-bound.y, bound.y);
        rt.anchoredPosition = pos;
        //Return
        return ba;
    }
    System.Action GetEvent(int i)
    {
        switch (i)
        {
            case 0:
                return SelectLevel_1;
            case 1:
                return SelectLevel_2;
            case 2:
                return SelectLevel_3;
            case 3:
                return SelectLevel_4;
            default:
                return SelectLevel_1;
        }
    }
    public void SelectLevel_1() {
        if (missionNum.Count < 1) LS.SelectLevel(location);
        else LS.SelectLevel(location, missionNum[0]);
    }
    public void SelectLevel_2() {
        if (missionNum.Count < 2) LS.SelectLevel(location);
        else LS.SelectLevel(location, missionNum[1]);
    }
    public void SelectLevel_3() {
        if (missionNum.Count < 3) LS.SelectLevel(location);
        else LS.SelectLevel(location, missionNum[2]);
    }
    public void SelectLevel_4() {
        if (missionNum.Count < 4) LS.SelectLevel(location);
        else LS.SelectLevel(location, missionNum[3]);
    }
}
