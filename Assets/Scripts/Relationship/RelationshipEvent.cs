using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipEvent : MonoBehaviour
{
    public List<effectClass> effectList = new List<effectClass>();
    [System.Serializable]
    public class effectClass
    {
        public Relationship.meterClass effect = new Relationship.meterClass();
        public List<string> S_affectedCharacters = new List<string>();
        public List<Relationship.groupEnum> G_affectedGroups = new List<Relationship.groupEnum>();
    }
    public void ApplyEffect()
    {
        Relationship _relationship = Relationship.Instance;
        foreach (var _effect in effectList)
        {
            foreach (var item in _effect.S_affectedCharacters)
            {
                Relationship.characterClass _character = _relationship.GetCharacterFromID(item);
                if (item != null)
                    _character.soloRelationship.Add(_effect.effect);
                else
                    Debug.LogError("Couldn't find Character from ID: " + item);
            }

            foreach (var item in _effect.G_affectedGroups)
                _relationship.GetGroupFromEnum(item).relationship.Add(_effect.effect);
        }
    }

}
