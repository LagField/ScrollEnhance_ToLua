using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;

namespace ScrollEnhancement
{
    public class ScrollItem
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public int dataIndex;
        /// <summary>
        /// 在tolua侧对于该item的引用
        /// </summary>
        public LuaTable itemLuaClass;

        public ScrollItem(GameObject gameObject)
        {
            this.gameObject = gameObject;
            rectTransform = gameObject.GetComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
        }

        public void SetAnchorPosition(Vector2 pos)
        {
            rectTransform.anchoredPosition = pos;
        }

        public void SetSize(Vector2 size)
        {
            rectTransform.sizeDelta = size;
        }

        public void SetDataIndex(int index)
        {
            dataIndex = index;
        }

        public void Dispose()
        {
            if (itemLuaClass != null)
            {
                itemLuaClass.Dispose();
                itemLuaClass = null;
            }
        }
    }
}