require('Lua.ExportedGlobalVariables')
local ScrollItem = require('Test.ScrollItem')
---@type ScrollEnhancement.ScrollEnhancement
local scrollEnhancement = UnityEngine_GameObject.Find('/Canvas/Scroll'):GetComponent(typeof(ScrollEnhancement_ScrollEnhancement))

---@type ScrollItem[]
local allItems = {}

---@param newItemRectTransform UnityEngine.RectTransform
---@param dataIndex number
---@param scrollItem ScrollItem
scrollEnhancement:BindCallbacks(
        function(newItemRectTransform)
            local newScrollItem = ScrollItem.New(newItemRectTransform)
            table.insert(allItems, newScrollItem)
            return newScrollItem
        end,
        function(dataIndex,scrollItem)
            dataIndex = dataIndex + 1
            scrollItem:SetText(dataIndex)
        end)
scrollEnhancement:RefreshDataAmount(1000)