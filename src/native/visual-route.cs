// Polished native route, street-stop and collection screens.
// Keeps all original input coordinates and game state unchanged.
public static partial class Game
{
    static int RouteTypeColor(int kind)
    {
        if (kind == 1) return 0xFFB575;
        if (kind == 2) return 0xB9AAF1;
        if (kind == 3) return 0xF2CD83;
        if (kind == 4) return 0x63DFC4;
        if (kind == 5) return 0xFF8E8C;
        return 0x98CCED;
    }

    static void DrawRouteBackdrop()
    {
        Rect(0,80,1250,820,0x0B1426);
        Art(2,0,80,1250,820);
    }

    static int RouteActiveCombos()
    {
        int count = 0;
        for (int i = 0; i < ComboCount(0); i = i + 1)
        {
            if (ComboActive(0,i)) count = count + 1;
        }
        return count;
    }

    static void DrawRouteLoadout(float x,float y,float w)
    {
        Panel(x,y,w,96,0x15243A,0x2B425D);
        Text("随身装备",x+20,y+14,14,0xF6EFDF);
        int shown = 0;
        for (int i = 0; i < 24; i = i + 1)
        {
            if (P1Items[i] > 0 && shown < 8)
            {
                float ix = x+20+42*shown;
                Panel(ix,y+42,34,34,0x1B2D46,0x2B425D);
                Art(3+i,ix+3,y+45,28,28);
                shown = shown + 1;
            }
        }
        if (shown == 0) Text("探索街区，收集第一件装备",x+20,y+49,12,0xA0AEC2);
        int owned = ItemOwnedCount(0);
        Text("已收集 " + N(owned) + " / 24 件",x+392,y+16,14,0xF6EFDF);
        Text("已激活 " + N(RouteActiveCombos()) + " / 20 套搭配",x+392,y+47,13,0x63DFC4);
        Keycap("B",x+w-160,y+47,0x63DFC4);
        Text("背包与图鉴",x+w-116,y+54,12,0xA0AEC2);
    }

    static string RouteNodeAdvice(int kind)
    {
        if (kind == 1) return "挑战密集车流，赢取更多金币与装备。";
        if (kind == 2) return "遇见街头故事，在收益与代价间做选择。";
        if (kind == 3) return "用零钱购买装备，补齐想要的组合。";
        if (kind == 4) return "恢复生命、赚取零钱，或领取免费装备。";
        if (kind == 5) return "穿过本幕特殊路口，前往下一个街区。";
        return "护送两人到达对岸，获得金币与新装备。";
    }

    static void DrawRouteMap()
    {
        DrawRouteBackdrop();
        Text("THE WAY HOME  /  路线规划",65,105,11,0x63DFC4);
        Text("第 " + N(RouteAct) + " 幕 · " + RouteActName(),60,128,30,0xF6EFDF);
        Text("下一站 · " + RouteNodeAdvice(NodeTypes[(RouteAct-1)*18+RouteRow*3+SelectedChoice]),65,174,14,0xA0AEC2);
        Panel(908,105,282,79,0x15243A,0x2B425D);
        Text("回家进度",926,118,11,0xA0AEC2);
        Text(N(RouteCompletedCount()) + " / 18 站",926,138,21,0xF6EFDF);
        for (int act = 0; act < 3; act = act + 1)
        {
            int actColor = 0x2B425D;
            if (act < RouteAct) actColor = 0x63DFC4;
            Rect(1080+act*28,140,20,6,actColor);
        }
        Panel(65,201,980,466,0x101D31,0x2B425D);
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
                            int lineColor = 0x253A54;
                            if (NodeVisited[from] > 0 && NodeVisited[to] > 0) lineColor = 0x63DFC4;
                            if (NodeVisited[from] == 2 && row+1 == RouteRow && RouteCanChoose(next)) lineColor = 0x507A89;
                            Line(330+col*295,620-row*78-25,330+next*295,620-(row+1)*78+25,lineColor);
                        }
                    }
                }
            }
        }
        for (int row = 0; row < 6; row = row + 1)
        {
            float cy = 620-row*78;
            int rowColor = 0x61748E;
            if (row == RouteRow) rowColor = 0x63DFC4;
            Text("0" + N(row+1),87,cy-16,16,rowColor);
            Text("第 " + N(row+1) + " 站",87,cy+6,10,rowColor);
            for (int col = 0; col < 3; col = col + 1)
            {
                int index = offset+row*3+col;
                int kind = NodeTypes[index];
                if (kind >= 0)
                {
                    float cx = 330+col*295;
                    int color = RouteTypeColor(kind);
                    int fill = 0x15243A;
                    int border = 0x2B425D;
                    string hint = "后续路口";
                    if (row < RouteRow) { color = 0x61748E;fill = 0x101D31;hint = "未选择"; }
                    if (NodeVisited[index] > 0) { border = 0x365B60;color = 0x63DFC4;hint = "已通过"; }
                    if (row == RouteRow)
                    {
                        if (RouteCanChoose(col))
                        {
                            border = 0x457877;
                            fill = 0x1B2D46;
                            hint = N(col+1) + "  /  可前往";
                            if (SelectedChoice == col) { fill = 0x213D48;border = 0x63DFC4;hint = "ENTER  /  前往"; }
                        }
                        else { color = 0x61748E;hint = "路线不相连"; }
                    }
                    Panel(cx-105,cy-25,210,50,fill,border);
                    Art(27+kind,cx-96,cy-17,34,34);
                    Text(RouteNodeName(kind),cx-50,cy-18,14,color);
                    Text(hint,cx-50,cy+5,10,0xA0AEC2);
                    if (row == RouteRow && RouteCanChoose(col) && SelectedChoice == col)
                    {
                        Circle(cx+89,cy-17,6,6,0x63DFC4);
                    }
                }
            }
        }
        Panel(1070,201,160,466,0x15243A,0x2B425D);
        Text("街区路口",1087,218,14,0xF6EFDF);
        for (int i = 0; i < 6; i = i + 1)
        {
            Art(27+i,1087,261+i*43,25,25);
            Text(RouteNodeName(i),1120,267+i*43,12,RouteTypeColor(i));
        }
        Line(1086,530,1214,530,0x2B425D);
        Circle(1088,552,7,7,0x63DFC4);
        Text("亮框  当前可走",1104,547,11,0xA0AEC2);
        Line(1088,585,1097,585,0x63DFC4);
        Text("绿线  已走路线",1104,578,11,0xA0AEC2);
        Line(1088,616,1097,616,0x2B425D);
        Text("暗线  后续路线",1104,609,11,0xA0AEC2);
        Panel(65,689,1165,68,0x15243A,0x2B425D);
        Keycap("1",83,705,0x63DFC4);
        Keycap("2",131,705,0x63DFC4);
        Keycap("3",179,705,0x63DFC4);
        Text("选择路口",231,714,13,0xF6EFDF);
        Text("← / → 选节点 · ENTER 前往 · 也可点击高亮节点",336,714,13,0xA0AEC2);
        Text("每幕六站 · 三幕到家",1000,714,12,0x63DFC4);
        DrawRouteLoadout(65,773,1165);
    }

    static void DrawRouteStop()
    {
        DrawRouteBackdrop();
        int color = 0xB9AAF1;
        string label = "STREET STORIES  /  街头事件";
        int kind = 2;
        if (Phase == 7) { color = 0xF2CD83;label = "CORNER STORE  /  便利商店";kind = 3; }
        if (Phase == 8) { color = 0x63DFC4;label = "SAFE HAVEN  /  安全驿站";kind = 4; }
        Text(label,150,127,11,color);
        Text(NodeTitle,145,167,32,0xF6EFDF);
        TextBlock(NodeText,150,223,760,48,14,0xA0AEC2);
        Art(27+kind,953,132,112,112);
        if (Phase == 7) Text("钱包 " + N(Coins) + " ¥ · 装备拿到即生效，可以连续购买。",150,287,14,color);
        else Text("做出一个选择，让这一站成为旅途的一部分。",150,287,14,color);
        for (int i = 0; i < 3; i = i + 1)
        {
            float x = 150+i*315;
            int border = 0x2B425D;
            int fill = 0x15243A;
            if (SelectedChoice == i) { border = color;fill = 0x1B2D46; }
            int titleColor = 0xF6EFDF;
            if (Phase == 7 && (ShopBought[i] > 0 || ShopItems[i] < 0)) { titleColor = 0x71849C; }
            Panel(x,330,290,238,fill,border);
            Keycap(N(i+1),x+239,344,color);
            Panel(x+18,346,51,51,0x0F1D30,0x2B425D);
            int icon = 27+kind;
            if (Phase == 7 && ShopItems[i] >= 0) icon = 3+ShopItems[i];
            if (Phase == 6 && RouteEventItem >= 0 && i == 0) icon = 3+RouteEventItem;
            if (Phase == 8 && RouteEventItem >= 0 && i == 2) icon = 3+RouteEventItem;
            Art(icon,x+23,351,41,41);
            TextBlock(ChoiceTags[i],x+83,354,150,39,12,color);
            Text(ChoiceNames[i],x+18,415,21,titleColor);
            TextBlock(ChoiceDescriptions[i],x+18,451,254,52,13,0xA0AEC2);
            Line(x+18,514,x+272,514,0x2B425D);
            if (Phase == 7)
            {
                if (ShopBought[i] > 0) Text("已加入背包",x+18,533,13,0x63DFC4);
                else if (ShopItems[i] < 0) Text("货架暂空",x+18,533,13,0x71849C);
                else
                {
                    int priceColor = color;
                    if (ShopPrices[i] > Coins) priceColor = 0xFF8E8C;
                    Text(N(ShopPrices[i]) + " ¥",x+18,528,20,priceColor);
                    if (ShopPrices[i] > Coins) Text("零钱不足",x+204,534,11,0xFF8E8C);
                    else if (SelectedChoice == i) Text("ENTER / 购买",x+179,534,11,color);
                    else Text("点击购买",x+204,534,11,0xA0AEC2);
                }
            }
            else TextBlock(ChoiceCombos[i],x+18,526,254,34,11,color);
        }
        if (Phase == 7)
        {
            Panel(150,585,920,43,0x15243A,0x2B425D);
            TextBlock(ChoiceCombos[SelectedChoice],167,598,880,26,12,0xA0AEC2);
            Panel(435,640,380,56,0x63DFC4,0x63DFC4);
            Text("0  /  离开商店，继续旅途 →",488,657,17,0x0B1426);
            Text("← / → 切换 · ENTER 购买 · 1 / 2 / 3 直购 · 点击商品也可购买",150,725,13,0xA0AEC2);
        }
        else
        {
            Panel(150,608,920,89,0x15243A,0x2B425D);
            Keycap("1",170,627,color);
            Keycap("2",218,627,color);
            Keycap("3",266,627,color);
            Text("选择方案",321,636,14,0xF6EFDF);
            Text("← / → 切换 · ENTER 确认 · 也可点击卡片",486,636,13,0xA0AEC2);
            Text("装备整趟保留，多套组合可以同时生效。",170,672,12,color);
        }
        DrawRouteLoadout(150,775,920);
    }

    static void DrawCollection()
    {
        DrawRouteBackdrop();
        Panel(1060,90,160,55,0x1B2D46,0x63DFC4);
        Keycap("B",1074,102,0x63DFC4);
        Text("返回旅途",1117,110,14,0xF6EFDF);
        int pages = (ComboCount(0)+8)/9;
        if (CollectionView == 0)
        {
            Text("BUILD YOUR ESCORT  /  搭配图鉴",65,108,11,0x63DFC4);
            Text("两件装备，多一种护送方式。",60,140,30,0xF6EFDF);
            Text("亮色配方已经生效。查看缺少的组件，规划接下来的奖励和商店。",65,192,14,0xA0AEC2);
            Text("TAB / 切换背包",1072,194,12,0x63DFC4);
            Text("已激活 " + N(RouteActiveCombos()) + " / " + N(ComboCount(0)),843,152,17,0x63DFC4);
            for (int slot = 0; slot < 9; slot = slot + 1)
            {
                int index = CollectionPage*9+slot;
                if (index < ComboCount(0))
                {
                    int col = slot%3;
                    int row = slot/3;
                    float x = 65+380*col;
                    float y = 240+182*row;
                    int color = 0xA0AEC2;
                    int border = 0x2B425D;
                    int fill = 0x15243A;
                    int a = ComboPartA(0,index);
                    int b = ComboPartB(0,index);
                    int parts = P1Items[a]+P1Items[b];
                    string state = N(parts) + " / 2 组件";
                    if (parts == 1) color = 0xFFB575;
                    if (ComboActive(0,index)) { color = 0x63DFC4;border = 0x365B60;fill = 0x19313E;state = "已激活"; }
                    Panel(x,y,350,166,fill,border);
                    Text(ComboName(0,index),x+16,y+13,18,0xF6EFDF);
                    Text(state,x+271,y+18,11,color);
                    int borderA = 0x2B425D;
                    int borderB = 0x2B425D;
                    if (P1Items[a] > 0) borderA = 0x63DFC4;
                    if (P1Items[b] > 0) borderB = 0x63DFC4;
                    Panel(x+16,y+46,42,42,0x0F1D30,borderA);
                    Panel(x+79,y+46,42,42,0x0F1D30,borderB);
                    Art(3+a,x+21,y+51,32,32);
                    Art(3+b,x+84,y+51,32,32);
                    Text("＋",x+62,y+56,17,0xA0AEC2);
                    TextBlock(ComboRecipe(0,index),x+135,y+47,199,42,12,color);
                    Line(x+16,y+99,x+334,y+99,0x2B425D);
                    TextBlock(ComboEffect(0,index),x+16,y+111,318,46,12,0xA0AEC2);
                }
            }
            if (CollectionPage == 2)
            {
                Panel(825,240,350,166,0x101D31,0x2B425D);
                Text("你的构筑，由旅途决定",845,261,18,0x63DFC4);
                TextBlock("同一件装备可以参与多种组合。集齐组件后自动生效，多套搭配可以同时使用。",845,300,307,82,14,0xA0AEC2);
            }
        }
        else
        {
            pages = 2;
            Text("PACK FOR THE JOURNEY  /  装备背包",65,108,11,0x63DFC4);
            Text("一路收集，一起带回家。",60,140,30,0xF6EFDF);
            Text("每件装备只需获得一次。亮色名称表示已经拥有，奖励与商店都能补齐背包。",65,192,14,0xA0AEC2);
            Text("TAB / 搭配图鉴",1080,194,12,0x63DFC4);
            Text("已收集 " + N(ItemOwnedCount(0)) + " / 24",859,152,17,0x63DFC4);
            for (int slot = 0; slot < 12; slot = slot + 1)
            {
                int index = CollectionPage*12+slot;
                if (index < 24)
                {
                    int col = slot%3;
                    int row = slot/3;
                    float x = 65+380*col;
                    float y = 220+146*row;
                    int color = 0x8A9BB3;
                    int border = 0x2B425D;
                    int descriptionColor = 0x8A9BB3;
                    int fill = 0x15243A;
                    string state = "未收集";
                    if (P1Items[index] > 0)
                    {
                        color = 0xF6EFDF;border = 0x365B60;fill = 0x19313E;
                        descriptionColor = 0xB5C4D4;state = "已拥有";
                    }
                    Panel(x,y,350,132,fill,border);
                    Panel(x+16,y+12,49,49,0x0F1D30,border);
                    Art(3+index,x+21,y+17,39,39);
                    Text(ItemName(0,index),x+79,y+17,17,color);
                    int stateColor = 0x71849C;
                    if (P1Items[index] > 0) stateColor = 0x63DFC4;
                    Text(state,x+79,y+43,11,stateColor);
                    Line(x+16,y+71,x+334,y+71,0x2B425D);
                    TextBlock(ItemDescription(0,index),x+16,y+82,318,43,12,descriptionColor);
                }
            }
        }
        for (int page = 0; page < pages; page = page + 1)
        {
            float x = 471+page*99;
            int color = 0x2B425D;
            int fill = 0x15243A;
            int textColor = 0xA0AEC2;
            if (page == CollectionPage) { color = 0x63DFC4;fill = 0x213D48;textColor = 0x63DFC4; }
            Panel(x,813,79,37,fill,color);
            Text(N(page+1),x+34,821,15,textColor);
        }
        Text("TAB 切换 · ← / → 翻页 · 数字键直达 · B / ESC 返回旅途",65,866,13,0x63DFC4);
        Text("查看期间，路段时间暂停",995,866,12,0xA0AEC2);
    }
}
