using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relationship : MonoBehaviour
{
    public static Relationship Instance;
    public List<characterClass> characterList = new List<characterClass>();
    private static List<groupClass> groupList = new List<groupClass>();

    public enum groupEnum { player, police};

    public class groupClass
    {
        public string name = "";
        public meterClass relationship = new meterClass();
        public List<characterClass> characters = new List<characterClass>();
    }
    
    [System.Serializable]
    public class characterClass
    {
        public string name = "";
        public string id = "";
        public GameObject prefab;
        public List<groupEnum> groups = new List<groupEnum>();
        public meterClass soloRelationship = new meterClass();

        public meterClass getRelationship()
        {
            meterClass _temp = soloRelationship.Clone();
            foreach (var item in groups)
                _temp.Add(groupList[(int)item].relationship);
            return _temp;
        }
    }

    [System.Serializable]
    public class meterClass
    {
        public int sincere = 0;
        public int sarcastic = 0;

        public int friendly = 0;
        public int hostile = 0;

        public int lawful = 0;
        public int chaotic = 0;

        public int good = 0;
        public int evil = 0;

        public int loud = 0;
        public int quiet = 0;

        public meterClass Clone()
        {
            meterClass _temp = new meterClass();
            _temp.sincere = sincere;
            _temp.sarcastic = sarcastic;

            _temp.friendly = friendly;
            _temp.hostile = hostile;

            _temp.lawful = lawful;
            _temp.chaotic = chaotic;

            _temp.good = good;
            _temp.evil = evil;

            _temp.loud = loud;
            _temp.quiet = quiet;
            return _temp;
        }
        public void Add(meterClass _add)
        {
            sincere += _add.sincere;
            sarcastic += _add.sarcastic;

            friendly += _add.friendly;
            hostile += _add.hostile;

            lawful += _add.lawful;
            chaotic += _add.chaotic;

            good += _add.good;
            evil += _add.evil;

            loud += _add.loud;
            quiet += _add.quiet;
        }

    }
    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        CreateGroups();
    }

    void CreateGroups()
    {
        groupList.Clear();
        for (int i = 0; i < System.Enum.GetValues(typeof(groupEnum)).Length; i++)
        {
            groupList.Add(new groupClass());

            foreach (var item in characterList)
                if(item.groups.Contains((groupEnum)i))
                    groupList[i].characters.Add(item);
        }
    }

    public characterClass GetCharacterFromID(string _id)
    {
        if (_id != "")
        {
            foreach (var item in characterList)
                if (item.id == _id)
                    return item;
            Debug.LogError("Couldn't find character with ID: " + _id);
        }
        return null;
    }

    public groupClass GetGroupFromEnum(groupEnum _enum)
    {
        return groupList[(int)_enum];
    }
}
