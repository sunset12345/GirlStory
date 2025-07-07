using System;
using System.Collections.Generic;
using System.Linq;

namespace GSDev.Time
{
    public class ActionEvent
    {
        public int ID;
        public float TimeStamp;
        public Action Handler;

        public void Invoke()
        {
            Handler?.Invoke();
        }
    }
    public class DelayCallBack
    {
        private readonly Stack<LinkedListNode<ActionEvent>> _pool;
        private readonly LinkedList<ActionEvent> _actions;
        private int _counter = 0;

        public DelayCallBack(Stack<LinkedListNode<ActionEvent>> pool = null)
        {
            _pool = pool == null? new Stack<LinkedListNode<ActionEvent>>() : pool;
            _actions = new LinkedList<ActionEvent>();
        }

        public void Update()
        {
            if (_actions == null || _actions.Count == 0) return;
            var currentTime = UnityEngine.Time.realtimeSinceStartup;
            while (_actions.Count > 0)
            {
                var node = _actions.First;
                if (currentTime > node.Value.TimeStamp)
                {
                    _actions.Remove(node);
                    node.Value.Invoke();
                }
                else break;
            }
        }

        public int Add(float time, Action callback)
        {
            try
            {
                var node = _pool.Count == 0 ? new LinkedListNode<ActionEvent>(new ActionEvent()) : _pool.Pop();
                node.Value.ID = ++_counter;
                node.Value.Handler = callback;
                node.Value.TimeStamp = UnityEngine.Time.realtimeSinceStartup + time;
                AddNode(node);
                return node.Value.ID;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex.ToString());
                return 0;
            }
        }

        private void AddNode(LinkedListNode<ActionEvent> node)
        {
            // if (node == null) throw new NullReferenceException();
            var crawler = _actions.First;
            if (crawler == null)
            {
                _actions.AddFirst(node);
                return;
            }

            var timeStamp = node.Value.TimeStamp;
            while (true)
            {
                if (timeStamp > crawler.Value.TimeStamp)
                {
                    crawler = crawler.Next;
                    if (crawler != null) continue;
                    _actions.AddLast(node);
                    return;
                }

                _actions.AddBefore(crawler, node);
                return;
            }
        }

        public void Remove(int id)
        {
            if (_actions.Count == 0) return;
            var action = _actions.FirstOrDefault(n => n.ID == id);
            var node = _actions.Find(action);
            RemoveNode(node);
        }

        private void RemoveNode(LinkedListNode<ActionEvent> node)
        {
            if (node == null) return;
            _actions.Remove(node);
            _pool.Push(node);
        }
    }
}