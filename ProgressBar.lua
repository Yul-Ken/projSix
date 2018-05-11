local ProgressBar = class("ProgressBar")
-- ProgressBar = {}

ProgressBar.width = 450
ProgressBar.height = 30
ProgressBar.value = 0
ProgressBar.processing = false
local navigation = {"Horizontal", "Vertical"}
local lablePos = {"up", "down", "left", "right"}
local currentTime = 0
function ProgressBar:Awake(component)
    self.panel = component
    self.background = component.transform:Find("BackGround"):FullGetComponent("KFrameWork.ImageExpand")
    self.filler = component.transform:Find("BackGround/filler"):FullGetComponent("KFrameWork.ImageExpand")
    self.percentLable = component.transform:Find("percent"):FullGetComponent("KFrameWork.TextExpand")
end

function ProgressBar:OnEnter(uiparams)
    -- self.init(450,30,"Horzontal",0,"left")
    -- printError("OnEnter")
    -- self:Refresh(uiparams)
end

function ProgressBar:Refresh(uiparams)
    local num = uiparams:ReadFloat()

    self:setValue(num)
end

function ProgressBar:OnExit(uiparams)
end

function ProgressBar:create(...) --宽，高，填充方式，填充方向,lable位置
    local w, h, fillMeth, fillOrigin, labelPos = ...
    self.setSize(w, h)
    self.panel.RectTransform.offsetMin = Vector2.new(-self.width / 2, -self.hight / 2)
    self.panel.RectTransform.offsetMax = Vector2.New(width / 2, hight / 2)
    self.setNavigation(fillMeth)
    self.setFillOrigin(fillOrigin)
    self.setLablePos(labelPos)
end

function ProgressBar:setValue(v, full)
    if v == nil or v < 0 then --注意不能交换"or"操作符两边的内容
        printError("the value of percent can't be null or negative!")
        return
    elseif full == nil then
        if v < 1 then
            self.value = math.Clamp(v, 0, 1)
        elseif v <= 100 then
            self.value = math.Clamp(v / 100, 0, 1)
        else
            printError("percent more than 100,Invalid number!")
        end
    elseif v > full then
        printError("percent out of range!")
    else
        self.value = math.Clamp(v / full, 0, 1)
    end

    self.filler.fillAmount = self.value
    self.percentLable.text = string.format("%d", tostring(self.value * 100)) .. "%"
end

function ProgressBar:setLablePos(n) --0，1，2，3对应上下左右
    if self.labelPos[n] == nil then
        print("no such labelPos")
        return
    end
    local lable_w = self.percentBG.RectTransform.rect.width
    local lable_h = self.percentBG.RectTransform.rect.hight
    if n == self.labelPos[0] then
        --根据进度条和百分比的rect宽高计算lable应在的位置
        self.percentBG.RectTransform.localPosition = Vector3.New(0, (self.height + lable_h) / 2, 0)
    elseif n == self.labelPos[1] then
        self.percentBG.RectTransform.localPosition = Vector3.New(0, -(self.height + lable_h) / 2, 0)
    elseif n == self.labelPos[2] then
        self.percentBG.RectTransform.localPosition = Vector3.New(0, -(self.width + lable_w) / 2, 0)
    elseif n == self.labelPos[3] then
        self.percentBG.RectTransform.localPosition = Vector3.New(0, (self.width + lable_w) / 2, 0)
    end
end

function ProgressBar:setNavigation(nv) --传入字符串Horizontal或者Vertical
    if navigation[nv] == nil then
        printError("no such navigation")
        return
    end
    nv = nv or "Horzontical"
    --默认水平
    if nv == navigation[0] then
        self.filler.FillMethod = UnityEngine.UI.Image.FillMethod.Horizontal
    end
    if nv == navigation[1] then
        self.filler.FillMethod = UnityEngine.UI.image.FillMethod.Vertical
    end
end

function ProgressBar:setFillOrigin(o) --传入0或1，当进度条水平的时候，0代表从左往右，垂直的时候，0代表从下往上
    if o ~= 0 or o ~= 1 then
        print("no such fillOrigin")
    end
    o = o or 0 --默认从左往右
    self.filler.fillOrigin = o
    --直接赋值0或1？
end

function ProgressBar:ProgressTo(to, time)
    if not self.processing then
        UpdateBeat:Add()
    end
    currentTime = Time.timeSinceLevelLoad
end

function ProgressBar:update(...)
    -- body
end

function ProgressBar:setBackImage(imgSprite)
    self.background.sprite = imgSprite
end

function ProgressBar:setSize(w, h)
    self.width = w or 450
    self.height = h or 30
    self.panel.RectTransform.rect.width = w
    self.panel.RectTransform.rect.height = h
end

function ProgressBar:testFunc(o)
    print("testFunc poi!")
    print(o.name)
    -- o.fillOrigin = 0
    print(o.transform.localPosition)
end
function ProgressBar:testFunc1()
    printError("Timer reached!")
end

return ProgressBar
