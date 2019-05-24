using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;
using UnityEngine.UI;

namespace ScrollEnhancement
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollEnhancement : MonoBehaviour
    {
        [System.Serializable]
        public class Padding
        {
            public float left;
            public float right;
            public float top;
            public float bottom;
        }

        public enum Corner
        {
            UpperLeft,
            UpperRight,
            LowerLeft,
            LowerRight
        }

        public enum Axis
        {
            Horizontal,
            Vertical
        }

        public enum Alignment
        {
            UpperLeft,
            UpperCenter,
            UpperRight,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            LowerLeft,
            LowerCenter,
            LowerRight
        }

        public Padding padding = new Padding();
        [SerializeField] private Vector2 cellSize = new Vector2(100, 100);
        [SerializeField] private Vector2 spacing;
        [SerializeField] private Corner startCorner;
        [SerializeField] private Axis startAxis;
        [SerializeField] private Alignment childAlignment;

        [SerializeField] private GameObject prefab;

        List<ScrollItem> itemList = new List<ScrollItem>();
        private int dataAmount;
        private int maxColumnAmount;
        private int maxRowAmount;
        private LuaFunction onCreateNewItemPrefabCallback;
        private LuaFunction onItemNeedRefreshContentCallback;

        private ScrollRect _scrollRect;

        public ScrollRect ScrollRect
        {
            get
            {
                if (_scrollRect == null)
                {
                    _scrollRect = GetComponent<ScrollRect>();
                }

                return _scrollRect;
            }
        }

        int VisibleRowStartIndex
        {
            get
            {
                if (startAxis == Axis.Vertical)
                {
                    return 0;
                }

                float currentVisibleStartHeight = ScrollRect.content.anchoredPosition.y;
                return Mathf.FloorToInt(currentVisibleStartHeight / (padding.top + cellSize.y + spacing.y));
            }
        }

        int VisibleRowEndIndex
        {
            get
            {
                if (startAxis == Axis.Vertical)
                {
                    return maxRowAmount;
                }
                
                float currentVisibleEndHeight = ScrollRect.content.anchoredPosition.y + ScrollRect.viewport.rect.height;
                return Mathf.CeilToInt(currentVisibleEndHeight / (padding.top + cellSize.y + spacing.y));
            }
        }

        int VisibleColumnStartIndex
        {
            get
            {
                if (startAxis == Axis.Horizontal)
                {
                    return 0;
                }
                
                float currentVisibleStartWidth = -ScrollRect.content.anchoredPosition.x;
                return Mathf.FloorToInt(currentVisibleStartWidth / (padding.left + cellSize.x + spacing.x));
            }
        }

        int VisibleColumnEndIndex
        {
            get
            {
                if (startAxis == Axis.Horizontal)
                {
                    return maxColumnAmount;
                }
                float currentVisibleEndWidth = -ScrollRect.content.anchoredPosition.x + ScrollRect.viewport.rect.width;
                return Mathf.CeilToInt(currentVisibleEndWidth / (padding.left + cellSize.x + spacing.x));
            }
        }

        public void Dispose()
        {
            dataAmount = 0;
            if (onCreateNewItemPrefabCallback != null)
            {
                onCreateNewItemPrefabCallback.Dispose();
                onCreateNewItemPrefabCallback = null;
            }

            if (onItemNeedRefreshContentCallback != null)
            {
                onItemNeedRefreshContentCallback.Dispose();
                onItemNeedRefreshContentCallback = null;
            }

            for (int i = 0; i < itemList.Count; i++)
            {
                itemList[i].Dispose();
            }

            itemList.Clear();
            for (int i = 0; i < itemPoolList.Count; i++)
            {
                itemPoolList[i].Dispose();
            }

            itemPoolList.Clear();
//            if (scrollTween != null)
//            {
//                LeanTween.cancel(scrollTween.uniqueId);
//                scrollTween = null;
//            }

            ClearAllChildItems();
        }

        private void Start()
        {
            if (prefab != null)
            {
                prefab.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            UpdateItemsVisibility();
        }

        public void ClearAllChildItems()
        {
            dataAmount = 0;
            int childCount = ScrollRect.content.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject childGameObject = ScrollRect.content.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(childGameObject);
                }
                else
                {
                    DestroyImmediate(childGameObject);
                }
            }

            itemList.Clear();
        }

        public void BindCallbacks(LuaFunction onCreateNewItemPrefabCallback, LuaFunction onItemNeedRefreshContentCallback)
        {
            this.onCreateNewItemPrefabCallback = onCreateNewItemPrefabCallback;
            this.onItemNeedRefreshContentCallback = onItemNeedRefreshContentCallback;
        }

        /// <summary>
        /// 更改数据的数量,并且刷新一波所有的item
        /// </summary>
        /// <param name="amount"></param>
        public void RefreshDataAmount(int amount)
        {
            dataAmount = amount;
            ResizeContentRect();
            RefreshItemsContent();
            UpdateItemsVisibility();
        }

        /// <summary>
        /// 刷新一次所有的item
        /// </summary>
        public void RefreshItemsContent()
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                ScrollItem item = itemList[i];
                if (item.dataIndex < 0 || item.dataIndex >= dataAmount)
                {
                    continue;
                }

                if (onItemNeedRefreshContentCallback != null)
                {
                    onItemNeedRefreshContentCallback.Call(item.dataIndex, item.itemLuaClass);
                }
            }
        }

//        private LTDescr scrollTween;

        public void ScrollToData(int dataIndex, float scrollTime)
        {
            Vector2 targetDataPosition = CalculateIndexItemAnchorPosition(dataIndex);

            if (scrollTime <= 0)
            {
                ScrollRect.content.anchoredPosition = new Vector2(0, -targetDataPosition.y);
            }
            else
            {
//                if (scrollTween != null)
//                {
//                    LeanTween.cancel(scrollTween.uniqueId);
//                    scrollTween = null;
//                }
//
//                scrollTween = LeanTween.moveY(ScrollRect.content, -targetDataPosition.y, scrollTime)
//                    .setEase(LeanTweenType.easeOutCirc);
            }
        }

        void ResizeContentRect(bool resetPosition = false)
        {
            RefreshMaxRowAndColumnAmount();
            RectTransform contentRectTransform = ScrollRect.content;
            contentRectTransform.anchorMin = new Vector2(0, 1);
            contentRectTransform.anchorMax = new Vector2(0, 1);
            contentRectTransform.pivot = new Vector2(0, 1);

            float width;
            float height;
            if (startAxis == Axis.Horizontal)
            {
                width = ScrollRect.viewport.rect.width;
                height = maxRowAmount * cellSize.y + (maxRowAmount - 1) * spacing.y + padding.top + padding.bottom;
            }
            else
            {
                height = ScrollRect.viewport.rect.height;
                width = maxColumnAmount * cellSize.x + (maxColumnAmount - 1) * spacing.x + padding.left + padding.right;
            }

            ScrollRect.content.sizeDelta = new Vector2(width, height);

            if (resetPosition)
            {
                contentRectTransform.anchoredPosition = Vector2.zero;
            }
        }

        public void UpdateItemsVisibility()
        {
            //hide items
            int itemNeedStartDataIndex;
            int itemNeedEndDataIndex;

            if (startAxis == Axis.Horizontal)
            {
                itemNeedStartDataIndex = VisibleRowStartIndex * maxColumnAmount;
                itemNeedEndDataIndex = VisibleRowEndIndex * maxColumnAmount;
            }
            else
            {
                itemNeedStartDataIndex = VisibleColumnStartIndex * maxRowAmount;
                itemNeedEndDataIndex = VisibleColumnEndIndex * maxRowAmount;
            }

            for (int i = itemList.Count - 1; i >= 0; i--)
            {
                ScrollItem item = itemList[i];

                if (item.dataIndex >= dataAmount)
                {
                    PutItemBackToPool(item);
                    itemList.RemoveAt(i);
                    continue;
                }

                if (item.dataIndex < itemNeedStartDataIndex)
                {
                    PutItemBackToPool(item);
                    itemList.RemoveAt(i);
                    continue;
                }

                if (item.dataIndex > itemNeedEndDataIndex)
                {
                    PutItemBackToPool(item);
                    itemList.RemoveAt(i);
                }
            }

            //new items
            for (int i = itemNeedStartDataIndex; i <= itemNeedEndDataIndex; i++)
            {
                if (i < 0 || i >= dataAmount)
                {
                    continue;
                }

                ScrollItem item = GetItemAtDataIndex(i);
                if (item == null)
                {
                    item = GetItemFromPool();
                    itemList.Add(item);
                    if (onItemNeedRefreshContentCallback != null)
                    {
                        onItemNeedRefreshContentCallback.Call(i, item.itemLuaClass);
                    }
                }

                item.SetSize(cellSize);
                item.SetDataIndex(i);
                Vector2 itemPosition = CalculateIndexItemAnchorPosition(i);
                item.SetAnchorPosition(itemPosition);
            }
        }

        private ScrollItem GetItemAtDataIndex(int index)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                ScrollItem item = itemList[i];
                if (item.dataIndex == index)
                {
                    return item;
                }
            }

            return null;
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            //横着排布则必须竖直方向滚动,竖着排布则必须横方向滚动
            if (startAxis == Axis.Horizontal)
            {
                ScrollRect.horizontal = false;
                ScrollRect.vertical = true;
            }
            else
            {
                ScrollRect.horizontal = true;
                ScrollRect.vertical = false;
            }

            RefreshItemsLayoutInEditor();
        }

        /// <summary>
        /// 这个功能只用来在编辑器中看效果
        /// </summary>
        /// <param name="itemAmount"></param>
        [NoToLua]
        public void RefreshItemAmountInEditor(int itemAmount)
        {
            if (Application.isPlaying)
            {
                return;
            }

            dataAmount = itemAmount;

            RefreshMaxRowAndColumnAmount();
            int currentItemAmount = itemList.Count;
            if (currentItemAmount > itemAmount)
            {
                for (int i = currentItemAmount - itemAmount - 1; i >= 0; i--)
                {
                    ScrollItem item = itemList[itemList.Count - 1];
                    DestroyImmediate(item.gameObject);
                    itemList.RemoveAt(itemList.Count - 1);
                }
            }
            else if (currentItemAmount < itemAmount)
            {
                int newAmount = itemAmount - currentItemAmount;
                for (int i = 0; i < newAmount; i++)
                {
                    GameObject newItemObject = Instantiate(prefab, ScrollRect.content);
                    ScrollItem newItem = new ScrollItem(newItemObject);
                    newItem.SetSize(cellSize);
                    newItem.gameObject.SetActive(true);
                    itemList.Add(newItem);
                }
            }
        }

        [NoToLua]
        public void RefreshItemsLayoutInEditor()
        {
            if (Application.isPlaying)
            {
                return;
            }

            RefreshMaxRowAndColumnAmount();
            for (int i = 0; i < itemList.Count; i++)
            {
                ScrollItem item = itemList[i];
                item.SetSize(cellSize);
                Vector2 itemPos = CalculateIndexItemAnchorPosition(i);
                item.SetAnchorPosition(itemPos);
                item.SetSize(cellSize);
            }
        }
#endif

        private void RefreshMaxRowAndColumnAmount()
        {
            Rect contentRect = ScrollRect.content.rect;
            float contentWidth;
            float contentHeight;
            maxColumnAmount = 1;
            maxRowAmount = 1;
            if (startAxis == Axis.Horizontal)
            {
                contentWidth = contentRect.width - padding.left - padding.right;
                contentHeight = contentRect.height - padding.top;

                while (maxColumnAmount * cellSize.x + (maxColumnAmount - 1) * spacing.x < contentWidth)
                {
                    maxColumnAmount++;
                }

                maxColumnAmount = Mathf.Max(1, maxColumnAmount - 1);
                maxRowAmount = Mathf.CeilToInt((float) dataAmount / maxColumnAmount);
            }
            else
            {
                contentWidth = contentRect.width - padding.left;
                contentHeight = contentRect.height - padding.top - padding.bottom;

                while (maxRowAmount * cellSize.y + (maxRowAmount - 1) * spacing.y < contentHeight)
                {
                    maxRowAmount++;
                }

                maxRowAmount = Mathf.Max(1, maxRowAmount - 1);
                maxColumnAmount = Mathf.CeilToInt((float) dataAmount / maxRowAmount);
            }
        }

        private Vector2 CalculateIndexItemAnchorPosition(int dataIndex)
        {
            if (startAxis == Axis.Horizontal)
            {
                int verticalIndex = Mathf.FloorToInt((float) dataIndex / maxColumnAmount);
                int horizontalIndex = dataIndex - verticalIndex * maxColumnAmount;
                float verticalPosition = padding.top + verticalIndex * (cellSize.y + spacing.y);
                verticalPosition *= -1;
                float horizontalPosition = padding.left + horizontalIndex * (cellSize.x + spacing.x);
                return new Vector2(horizontalPosition, verticalPosition);
            }
            else
            {
                int horizontalIndex = Mathf.FloorToInt((float) dataIndex / maxRowAmount);
                int verticalIndex = dataIndex - horizontalIndex * maxRowAmount;
                float horizontalPosition = padding.left + horizontalIndex * cellSize.x + spacing.x * horizontalIndex;
                float verticalPosition = padding.top + verticalIndex * cellSize.y + spacing.y * verticalIndex;
                verticalPosition *= -1;
                return new Vector2(horizontalPosition, verticalPosition);
            }
        }

        private List<ScrollItem> itemPoolList = new List<ScrollItem>();

        private ScrollItem GetItemFromPool()
        {
            if (itemPoolList.Count > 0)
            {
                ScrollItem item = itemPoolList[itemPoolList.Count - 1];
                itemPoolList.RemoveAt(itemPoolList.Count - 1);
                item.gameObject.SetActive(true);
                return item;
            }

            GameObject newItemObject = Instantiate(prefab);
            ScrollItem newItem = new ScrollItem(newItemObject);
            newItem.gameObject.SetActive(true);
            newItem.rectTransform.SetParent(ScrollRect.content);
            newItem.rectTransform.localScale = Vector3.one;
            if (onCreateNewItemPrefabCallback != null)
            {
                newItem.itemLuaClass = onCreateNewItemPrefabCallback.Invoke<RectTransform, LuaTable>(newItem.rectTransform);
            }

            return newItem;
        }

        private void PutItemBackToPool(ScrollItem item)
        {
#if UNITY_EDITOR
            if (itemPoolList.Contains(item))
            {
                Debug.LogError("Scroll Enhancement 同一个item回收了两次");
                return;
            }
#endif

            item.gameObject.SetActive(false);
            itemPoolList.Add(item);
        }
    }
}