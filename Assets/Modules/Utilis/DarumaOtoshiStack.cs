using System.Collections.Generic;
using System.Linq;

namespace com.playbux.utilis
{
    public interface IDarumaOtoshiStack<T>
    {
        int Count { get; }
        void Add(T identifier);
        void Hit(T identifier);
        void Swap(int position, T identifier);
        T[] GetAll();
    }

    public class DarumaListStack : IDarumaOtoshiStack<string>
    {
        public int Count => stack.Count;
        private List<string> stack = new List<string>();

        public void Add(string identifier)
        {
            if (!stack.Exists(s => s == identifier))
                stack.Add(identifier);
        }
        public void Hit(string identifier)
        {
            var index = stack.FindIndex(s => s == identifier);

            if (index > -1)
                stack.RemoveAt(index);
        }
        public void Swap(int position, string identifier)
        {
            if (stack.Count - 1 < position)
                return;

            if (identifier == stack[position])
                return;

            int aPosition = -1;
            string tempIdentifier = "";

            for (int i = 0; i < stack.Count; i++)
            {
                if (identifier == stack[i])
                    aPosition = i;

                if (i == position)
                    tempIdentifier = stack[i];
            }

            if (aPosition < 0)
                return;

            if (string.IsNullOrEmpty(tempIdentifier))
                return;

            if (aPosition == position)
                return;

            stack[position] = identifier;
            stack[aPosition] = tempIdentifier;
        }
        public string[] GetAll()
        {
            return stack.ToArray();
        }
    }

    public class DarumaHashStack<T> : IDarumaOtoshiStack<T>
    {
        public int Count => hashSet.Count;

        private HashSet<T> hashSet = new HashSet<T>();

        public void Add(T identifier) => hashSet.Add(identifier);
        public void Hit(T identifier) => hashSet.Remove(identifier);
        public void Swap(int position, T identifier)
        {
            var tempList = hashSet.ToList();

            if (tempList.Count - 1 < position)
                return;

            if (identifier.Equals(tempList[position]))
                return;

            int aPosition = -1;
            T tempIdentifier = default;

            for (int i = 0; i < tempList.Count; i++)
            {
                if (identifier.Equals(tempList[i]))
                    aPosition = i;

                if (i == position)
                    tempIdentifier = tempList[i];
            }

            if (aPosition < 0)
                return;

            if (tempIdentifier == null)
                return;

            if (aPosition == position)
                return;

            tempList[position] = identifier;
            tempList[aPosition] = tempIdentifier;
            hashSet = tempList.ToHashSet();
        }
        public T[] GetAll() => hashSet.ToArray();
    }
}