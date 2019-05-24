---@class ScrollItem
local ScrollItem = {}
ScrollItem.__index = ScrollItem

---@param rootTrans UnityEngine.RectTransform
---@return ScrollItem
function ScrollItem.New(rootTrans)
    ---@type ScrollItem
    local self = {}
    setmetatable(self,ScrollItem)
    self.rootTransform = rootTrans
    self.rootGameObject = rootTrans.gameObject
    ---@type UnityEngine.UI.Text
    self.text = self.rootTransform:Find('Text'):GetComponent(typeof(UnityEngine_UI_Text))
    
    return self
end

function ScrollItem:Dispose()
    self.isDisposed = true
end

---@param text number
function ScrollItem:SetText(text)
    self.text.text = text
end

return ScrollItem