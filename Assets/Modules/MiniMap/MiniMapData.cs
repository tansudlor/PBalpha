
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace com.playbux.minimap
{

    [CreateAssetMenu(menuName = "Playbux/MiniMap/MiniMap-Data", fileName = "MiniMapData")]
    public class MiniMapData : ScriptableObject
    {
       
        [SerializeField]
        private Texture texture;
        [SerializeField]
        private Vector2[] worldPoint;
        [SerializeField]
        private Vector2[] mapPoint;
        [SerializeField]
        private List<MiniMapIcon> icons;

        private HashSet<MiniMapIcon> filterIcon = null;

        public Texture Texture { get => texture; set => texture = value; }
        public Vector2[] WorldPoint { get => worldPoint; set => worldPoint = value; }
        public Vector2[] MapPoint { get => mapPoint; set => mapPoint = value; }

        public void RemoveIconByName(string name)
        {
            icons.RemoveAll(i => i.Name == name);
        }

        public void AddCustomIcon(string name, string displayName, Sprite icon, Vector3 position, bool editable, IconGroup group)
        {
            try
            {
                if (icons.First(i => i.Name == name) != null)
                {
                    return;
                }
            }
            catch (Exception e)
            {

            }

            icons.Add(new MiniMapIcon(name, displayName, icon, position, true, group));

        }

        public void ClearAllCustomIcons()
        {
            for (int i = icons.Count - 1; i >= 0; i--)
            {
                if (icons[i].Custom == true)
                {
                    icons.RemoveAt(i);
                }
            }
        }


        public void AddCustomIcon(MiniMapIcon icon)
        {
            if (icons.First(i => i.Name == icon.Name) != null)
            {
                return;
            };
            icons.Add(icon);
        }

        public List<MiniMapIcon> GetIconByName(string name)
        {
            return icons.FindAll(i => (i.Name == name));
        }

        public void AddFilter(Predicate<MiniMapIcon> group)
        {
            if (filterIcon == null)
            {
                filterIcon = new HashSet<MiniMapIcon>();
            }
            var items = icons.FindAll(group);
            foreach (var item in items)
            {
                filterIcon.Add(item);
            }
        }

        public MiniMapIcon GetFilterAt(int i)
        {
            if (filterIcon == null)
            {
                return icons[i];
            }
            return filterIcon.ElementAt(i);
        }

        public int GetFilterLenght()
        {
            if (filterIcon == null)
            {
                return icons.Count;
            }
            return filterIcon.Count;
        }

        public void ClearFileter()
        {
            filterIcon = null;
        }

    }


    public enum IconGroup : uint
    {
        NONE = 0,
        BUILDING = 1 << 0,
        QUEST = 1 << 1,
        NPC = 1 << 2,
        PLAYER = 1 << 3,
        PINPOINT = 1 << 4,
        QUIZ = 1 <<5
    }

    [System.Serializable]
    public class MiniMapIcon
    {
        [SerializeField]
        private string name;
        [SerializeField]
        private string displayName;
        [SerializeField]
        private IconGroup group = IconGroup.BUILDING;
        [SerializeField]
        private Sprite icon;
        [SerializeField]
        private Vector3 position;
        [SerializeField]
        private bool custom;


        public MiniMapIcon(string name, string displayName, Sprite icon, Vector3 position, bool custom, IconGroup group)
        {
            this.name = name;
            this.DisplayName = displayName;
            this.icon = icon;
            this.position = position;
            this.custom = custom;
            this.group = group;
        }

        public Sprite Icon { get => icon; set => icon = value; }
        public Vector3 Position { get => position; set => position = value; }
        public bool Custom { get => custom; }
        public string Name { get => name; }
        public IconGroup Group { get => group; set => group = value; }
        public string DisplayName { get => displayName; set => displayName = value; }
    }
}

