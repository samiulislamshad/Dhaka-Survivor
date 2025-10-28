using System.Collections.Generic;
using Systems.LeaderBoardSystem.View;
using UnityEngine;

namespace Systems.LeaderBoardSystem.ObjectPool
{
    public class ScrollViewPool
    {
        private List<UserDataView> _pool = new();
        private UserDataView _prefab;
        private Transform _parent;
        private float _itemHeight;
        private int _poolSize;

        public int PoolSize => _poolSize;
        public List<UserDataView> Pool => _pool;

        public ScrollViewPool(UserDataView prefab, Transform parent, float itemHeight, int poolSize)
        {
            _prefab = prefab;
            _parent = parent;
            _itemHeight = itemHeight;
            _poolSize = poolSize;
        }

        public void Initialize()
        {
            ClearPool();
            CreatePool();
        }

        private void ClearPool()
        {
            foreach (var item in _pool)
            {
                if (item != null)
                {
                    Object.Destroy(item.gameObject);
                }
            }
            _pool.Clear();
        }

        private void CreatePool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                var item = Object.Instantiate(_prefab, _parent);
                var rectTransform = item.GetComponent<RectTransform>();

                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(0.5f, 1);
                rectTransform.sizeDelta = new Vector2(0, _itemHeight);

                item.gameObject.SetActive(false);
                _pool.Add(item);
            }
        }

        public UserDataView GetPoolItem(int index)
        {
            if (index >= 0 && index < _pool.Count)
            {
                return _pool[index];
            }
            return null;
        }

        public void SetItemActive(int index, bool active)
        {
            if (index >= 0 && index < _pool.Count)
            {
                _pool[index].gameObject.SetActive(active);
            }
        }

        public void SetAllItemsActive(bool active)
        {
            foreach (var item in _pool)
            {
                item.gameObject.SetActive(active);
            }
        }

        public void Dispose()
        {
            ClearPool();
        }
    }
}