using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;




namespace com.playbux.tool
{
    [System.Serializable]
    public class QuestAssets : ScriptableObject
    {
        [SerializeField]
        private string questName;
        [SerializeField]
        private string questID;
        [SerializeField]
        private string position;
        [SerializeField]
        private List<NonPlayerCharacter> nonPlayerCharacterNodes;
        [SerializeField]
        private List<NonPlayerCharacter> serializableNonPlayerCharacterNodes;
        [SerializeField]
        private List<Dialog> serializableQuestNodes;

        public string QuestName { get => questName; set => questName = value; }
        public string QuestID { get => questID; set => questID = value; }
        public string Position { get => position; set => position = value; }
        public List<NonPlayerCharacter> NonPlayerCharacterNodes { get => nonPlayerCharacterNodes; set => nonPlayerCharacterNodes = value; }
        public List<NonPlayerCharacter> SerializableNonPlayerCharacterNodes { get => serializableNonPlayerCharacterNodes; set => serializableNonPlayerCharacterNodes = value; }
        public List<Dialog> SerializableQuestNodes { get => serializableQuestNodes; set => serializableQuestNodes = value; }
        

        public void StampedFrom(Information source)
        {
            
            Type sourceType = source.GetType();
            Type destinationType = this.GetType();

            foreach (PropertyInfo sourceProperty in sourceType.GetProperties())
            {
                PropertyInfo destinationProperty = destinationType.GetProperty(sourceProperty.Name);

                if (destinationProperty != null && destinationProperty.CanWrite)
                {
                    //   Debug.Log(sourceProperty.GetValue(source));
                    destinationProperty.SetValue(this, sourceProperty.GetValue(source));
                }
            }

           

        }

    }
}
