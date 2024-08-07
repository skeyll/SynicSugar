using System.Collections.Generic;
using SynicSugar.MatchMake;
using UnityEngine.UI;
using UnityEngine;

namespace SynicSugar.Samples.Tank {
    public class TankMatchMakeConditions : MonoBehaviour, IMatchmakingConditions {
        [SerializeField] InputField idField, nameField;
        [SerializeField] Text idText, nameText;

        public string[] GenerateBucket(){
            return new string[1]{"Tank"};
        }
        /// <summary>
        /// RoomID matchmaking.
        /// </summary>
        /// <returns></returns>
        public List<AttributeData> GenerateMatchmakingAttributes(){
            List<AttributeData> attributes = new List<AttributeData>();

            AttributeData attribute = new AttributeData();
            string formattedID = idField.text.PadLeft(4, '0');
            attribute.Key = "ROOM";
            attribute.SetValue(formattedID);
            attribute.ComparisonOperator = ComparisonOp.Equal;
            
            attributes.Add(attribute);

            idText.text = formattedID;

            return attributes;
        }
        public List<AttributeData> GenerateUserAttributes(){
            //We can set max 100 attributes.
            List<AttributeData> attributes = new();
            //Name
            AttributeData name = new (){
                Key = "NAME"
            };
            string Name = GetNameString();
            name.SetValue(Name);
            attributes.Add(name);

            //This is not actually used. Just example of adding more than one attribute.
            AttributeData level = new (){
                Key = "LEVEL"
            };
            int randomValue = UnityEngine.Random.Range(0, 31);
            level.SetValue(randomValue);
            attributes.Add(level);
            
            SynicSugarDebug.Instance.Log($"UserName: {Name} / Level: {randomValue}");


            nameText.text = Name;

            return attributes;
        }
        string GetNameString(){
            if(!string.IsNullOrEmpty(nameField.text)){
                return nameField.text;
            }
            var sample = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string name = string.Empty;
            var random = new System.Random();

            for (int i = 0; i < 6; i++){
                name += sample[random.Next(sample.Length)];
            }
            return name;
        }
        internal void SwitchInputfieldActive(bool isActivate){
            idField.gameObject.SetActive(isActivate);
            nameField.gameObject.SetActive(isActivate);

            idText.gameObject.SetActive(!isActivate);
            nameText.gameObject.SetActive(!isActivate);
        }
    }
}