// Route and collection UI for the native GDI+ executable.
public static partial class Game
{
    static int RouteTypeColor(int kind)
    {
        if (kind == 1) return 0xE29A78;
        if (kind == 2) return 0xB6A0EC;
        if (kind == 3) return 0xF5D474;
        if (kind == 4) return 0x79E2D4;
        if (kind == 5) return 0xFF8575;
        return 0xB4F98A;
    }


    static void DrawRouteBackdrop()
    {
        Rect(0,80,1250,820,0x0B1319);
        for (int x = 0; x < 1250; x = x + 48) Line(x,80,x,900,0x15211F);
        for (int y = 80; y < 900; y = y + 48) Line(0,y,1250,y,0x15211F);
    }

    static void DrawRouteMap()
    {
        DrawRouteBackdrop();
        Text("PLAN YOUR WAY HOME / ROUTE MAP",70,105,11,0xB4F98A);
        Text("第 " + N(RouteAct) + " 幕 · " + RouteActName(),68,127,29,0xF4F0DF);
        Text("从高亮节点选择下一站。只有相邻路线可以连接，最后汇入区域首领。",70,169,12,0x81918F);
        int offset = (RouteAct-1)*18;
        for (int row = 0; row < 5; row = row + 1)
        {
            for (int col = 0; col < 3; col = col + 1)
            {
                int from = offset+row*3+col;
                if (NodeTypes[from] >= 0)
                {
                    for (int next = 0; next < 3; next = next + 1)
                    {
                        int to = offset+(row+1)*3+next;
                        if (NodeTypes[to] >= 0 && Abs((float)(next-col)) <= 1)
                        {
                            int lineColor = 0x29363A;
                            if (NodeVisited[from] > 0 && NodeVisited[to] > 0) lineColor = 0x8DB977;
                            Line(330+col*295,620-row*78-25,330+next*295,620-(row+1)*78+25,lineColor);
                        }
                    }
                }
            }
        }
        for (int row = 0; row < 6; row = row + 1)
        {
            float cy = 620-row*78;
            Text(N(row+1) + " / 站",93,cy-7,11,0x81918F);
            for (int col = 0; col < 3; col = col + 1)
            {
                int index = offset+row*3+col;
                int kind = NodeTypes[index];
                if (kind >= 0)
                {
                    float cx = 330+col*295;
                    int color = RouteTypeColor(kind);
                    int fill = 0x182127;
                    int border = 0x303B41;
                    string hint = "未到达";
                    if (row < RouteRow) { color = 0x60716F;hint = "未走这条路线"; }
                    if (NodeVisited[index] > 0) { border = 0x8DB977;color = 0xB4F98A;hint = "已经通过"; }
                    if (row == RouteRow)
                    {
                        if (RouteCanChoose(col))
                        {
                            border = color;
                            fill = 0x21312A;
                            hint = N(col+1) + " / 可前往";
                            if (SelectedChoice == col) { fill = 0x30442E;hint = "ENTER / 前往"; }
                        }
                        else { color = 0x60716F;hint = "路线不相连"; }
                    }
                    Rect(cx-105,cy-25,210,50,fill);
                    Outline(cx-105,cy-25,210,50,border);
                    if (row == RouteRow && RouteCanChoose(col) && SelectedChoice == col)
                        Outline(cx-109,cy-29,218,58,color);
                    Rect(cx-94,cy-15,6,28,color);
                    Text(RouteNodeName(kind),cx-75,cy-16,14,color);
                    Text(hint,cx-75,cy+5,10,0xA6B2A3);
                }
            }
        }
        Rect(1070,206,160,445,0x182127);
        Outline(1070,206,160,445,0x303B41);
        Text("路线图例",1087,226,14,0xF4F0DF);
        for (int i = 0; i < 6; i = i + 1)
        {
            int color = RouteTypeColor(i);
            Circle(1086,269+i*43,10,10,color);
            Text(RouteNodeName(i),1104,265+i*43,11,color);
        }
        Line(1086,540,1215,540,0x303B41);
        Text("亮框：当前可走",1086,561,11,0x81918F);
        Text("绿线：已经走过",1086,589,11,0x81918F);
        Text("灰线：后续路线",1086,617,11,0x81918F);
        Rect(70,706,960,69,0x182127);
        Text("1 / 2 / 3 直接前往 · ← / → 选节点 · ENTER 前往 · 点击高亮节点",87,722,13,0xB4F98A);
        Text("B 查看道具搭配图鉴 · 装备保留到本次旅途结束 · 零钱可在商店购买新装备",87,747,11,0x81918F);
        DrawBagSummary(78,806);
        DrawComboLine(78,836);
        Text("生命 " + N(Lives) + "    零钱 " + N(Coins) + " ¥",884,806,14,0xF5D474);
        Text("每幕六站 · 三幕送到家",997,853,11,0x81918F);
    }

    static void DrawRouteStop()
    {
        DrawRouteBackdrop();
        int color = 0xB6A0EC;
        string label = "STREET EVENT / 随机事件";
        if (Phase == 7) { color = 0xF5D474;label = "CORNER SHOP / 便利商店"; }
        if (Phase == 8) { color = 0x79E2D4;label = "TAKE A BREAK / 安全休息"; }
        Text(label,150,130,11,color);
        Text(NodeTitle,145,170,31,0xF4F0DF);
        Text(NodeText,150,232,13,0x81918F);
        if (Phase == 7) Text("钱包 " + N(Coins) + " ¥ · 买到的装备立即生效，可连续购买。",150,281,13,color);
        else Text("选项会立即生效，请留意收益和代价。",150,281,12,color);
        for (int i = 0; i < 3; i = i + 1)
        {
            float x = 150+i*315;
            int border = 0x303B41;
            if (SelectedChoice == i) border = color;
            int titleColor = 0xF4F0DF;
            if (Phase == 7 && (ShopBought[i] > 0 || ShopItems[i] < 0)) titleColor = 0x60716F;
            Rect(x,330,290,238,0x182127);
            Outline(x,330,290,238,border);
            Rect(x+18,350,35,35,color);
            Text(N(i+1),x+31,356,18,0x10171D);
            Text(ChoiceTags[i],x+69,357,11,color);
            Text(ChoiceNames[i],x+18,414,21,titleColor);
            Text(ChoiceDescriptions[i],x+18,453,11,0x81918F);
            Line(x+18,510,x+272,510,0x303B41);
            if (Phase == 7)
            {
                if (ShopBought[i] > 0) Text("已购买",x+18,530,13,0x60716F);
                else if (ShopItems[i] < 0) Text("暂无货品",x+18,530,13,0x60716F);
                else
                {
                    int priceColor = color;
                    if (ShopPrices[i] > Coins) priceColor = 0xFF8575;
                    Text(N(ShopPrices[i]) + " ¥",x+18,528,17,priceColor);
                    if (ShopPrices[i] > Coins) Text("零钱不足",x+172,532,11,0xFF8575);
                    else Text("ENTER / 购买",x+151,532,11,color);
                }
            }
            else Text(ChoiceCombos[i],x+18,530,11,color);
        }
        DrawBagSummary(150,604);
        if (Phase == 7)
        {
            Rect(435,640,380,56,0xF5D474);
            Text("0 / 离开商店，继续旅途 →",492,657,17,0x17211A);
            Text("← / → 切换卡片，Enter 购买。1 / 2 / 3 可直接购买对应货品。",150,727,12,0x81918F);
        }
        else Text("1 / 2 / 3 选择 · ← / → 切换 · ENTER 确认 · 点击卡片也可选择",150,657,12,0x81918F);
        DrawComboLine(150,779);
        Text("B / 查看搭配图鉴",150,818,12,0x79E2D4);
    }

    static void DrawCollection()
    {
        DrawRouteBackdrop();
        Rect(1060,90,160,45,0x182127);Outline(1060,90,160,45,0x79E2D4);Text("B / 返回旅途",1082,104,14,0x79E2D4);
        int pages = (ComboCount(0)+8)/9;
        if (CollectionView == 0)
        {
            Text("BUILD YOUR ESCORT / 搭配图鉴",65,112,11,0x79E2D4);
            Text("两件装备，一种新的护送方式。",60,147,30,0xF4F0DF);
            Text("亮色配方已经生效，暗色配方可用于规划奖励和商店。所有装备仅获得一次。",65,203,12,0x81918F);
            Text("TAB / 切换背包",1065,205,11,0x79E2D4);
            int active = 0;
            for (int i = 0; i < ComboCount(0); i = i + 1)
            {
                if (ComboActive(0,i)) active = active + 1;
            }
            Text("已激活 " + N(active) + " / " + N(ComboCount(0)) + "   第 " + N(CollectionPage+1) + " / " + N(pages) + " 页",871,149,15,0xB4F98A);
            for (int slot = 0; slot < 9; slot = slot + 1)
            {
                int index = CollectionPage*9+slot;
                if (index < ComboCount(0))
                {
                    int col = slot%3;
                    int row = slot/3;
                    float x = 65+380*col;
                    float y = 252+180*row;
                    int color = 0x81918F;
                    int border = 0x303B41;
                    string state = "待集齐";
                    if (ComboActive(0,index)) { color = 0xB4F98A;border = 0x526F61;state = "已激活"; }
                    Rect(x,y,350,160,0x182127);
                    Outline(x,y,350,160,border);
                    Text(ComboName(0,index),x+16,y+15,18,color);
                    Text(state,x+286,y+19,10,color);
                    Text(ComboRecipe(0,index),x+16,y+50,11,0xF5D474);
                    Line(x+16,y+76,x+334,y+76,0x303B41);
                    Text(ComboEffect(0,index),x+16,y+92,11,0xA6B2A3);
                }
            }
        }
        else
        {
            pages = 2;
            Text("PACK YOUR WAY HOME / 装备背包",65,103,11,0x79E2D4);
            Text("把旅途上的装备，都带在身上。",60,134,30,0xF4F0DF);
            Text("亮色表示已经拥有。奖励、商店和街头事件都可能补齐你的背包。",65,187,12,0x81918F);
            Text("TAB / 搭配图鉴",1065,190,11,0x79E2D4);
            int owned = 0;
            for (int i = 0; i < 24; i = i + 1)
            {
                if (P1Items[i] > 0) owned = owned + 1;
            }
            Text("已收集 " + N(owned) + " / 24   第 " + N(CollectionPage+1) + " / 2 页",871,137,15,0xB4F98A);
            for (int slot = 0; slot < 12; slot = slot + 1)
            {
                int index = CollectionPage*12+slot;
                if (index < 24)
                {
                    int col = slot%3;
                    int row = slot/3;
                    float x = 65+380*col;
                    float y = 225+145*row;
                    int color = 0x81918F;
                    int border = 0x303B41;
                    int descriptionColor = 0x71817E;
                    string state = "未收集";
                    if (P1Items[index] > 0)
                    {
                        color = 0xB4F98A;border = 0x526F61;
                        descriptionColor = 0xD3DCCE;state = "已拥有";
                    }
                    Rect(x,y,350,130,0x182127);
                    Outline(x,y,350,130,border);
                    Text(ItemName(0,index),x+16,y+13,18,color);
                    Text(state,x+286,y+18,10,color);
                    Line(x+16,y+44,x+334,y+44,0x303B41);
                    Text(ItemDescription(0,index),x+16,y+57,11,descriptionColor);
                }
            }
        }
        for (int page = 0; page < pages; page = page + 1)
        {
            float x = 471+page*99;
            int color = 0x303B41;
            if (page == CollectionPage) color = 0x79E2D4;
            Rect(x,813,79,37,0x182127);
            Outline(x,813,79,37,color);
            Text(N(page+1),x+33,821,15,color);
        }
        Text("TAB 切换 · ← / → 翻页 · 数字键直达页面 · B / ESC 返回旅途",65,866,12,0x79E2D4);
        Text("查看背包和图鉴时，路段时间暂停。",905,866,11,0x81918F);
    }

    static void DrawRouteObjects()
    {
        if (RoadRule == 2)
        {
            float y = ObstacleY+80;
            Rect(ObstacleX,y,ObstacleW,ObstacleH,0xDAB668);
            for (float x = ObstacleX+7; x < ObstacleX+ObstacleW-15; x = x + 31)
                Line(x,y+4,x+15,y+ObstacleH-4,0x34444B);
            Outline(ObstacleX,y,ObstacleW,ObstacleH,0xF5D474);
            Rect(ObstacleX+33,y+6,ObstacleW-66,ObstacleH-12,0xDAB668);
            Text("← 绕行   施工隔离带   绕行 →",ObstacleX+39,y+9,11,0x34444B);
        }
        if (RoadblockTime > 0)
        {
            Rect(RoadblockX-37,RoadblockY+68,74,24,0xE29A78);
            Outline(RoadblockX-37,RoadblockY+68,74,24,0xF5D474);
            Text("路障",RoadblockX-13,RoadblockY+74,11,0x34444B);
        }
        if (BeaconTime > 0)
        {
            Circle(BeaconX-23,BeaconY+57,46,46,0x597E70);
            Circle(BeaconX-16,BeaconY+64,32,32,0xE29A78);
            Rect(BeaconX-6,BeaconY+65,12,19,0xF9EFD6);
            Rect(BeaconX-1,BeaconY+61,2,9,0xE29A78);
            Text("诱饵",BeaconX-14,BeaconY+107,11,0xE29A78);
        }
        if (SignalTime > 0)
        {
            Rect(451,124,178,50,0x21312A);
            Outline(451,124,178,50,0x79E2D4);
            Text("信号控场中",488,141,14,0x79E2D4);
        }
        if (Mode == 0 && RoadRule > 0)
        {
            Rect(20,702,434,32,0x182127);
            Text(RoadWarning,33,710,12,0xF5D474);
        }
    }
}
