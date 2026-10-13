// Native GDI+ drawing source. This intentionally uses only the supported C# subset.
// Host logical size: 1250 x 900. Original game world: 1080 x 660, at y=80.
// Required drawing wrappers: Rect, Outline, Circle, Text, Line, N(int).
// Circle takes bounding rectangle x,y,w,h (not center/radius). Colors are RGB int.
public static partial class Game
{
    static void Render()
    {
        Rect(0,0,1250,900,0x10171D);
        DrawRoad();
        DrawRouteObjects();
        for (int i = 0; i < 96; i = i + 1)
        {
            if (CarActive[i] == 1) DrawCar(i);
        }
        if (Linked == 1)
        {
            Line(P1X+11,P1Y+71,P2X-11,P2Y+71,0xB7F1D5);
            Line(P1X+11,P1Y+72,P2X-11,P2Y+72,0xB7F1D5);
            Line(P1X+11,P1Y+73,P2X-11,P2Y+73,0xB7F1D5);
        }
        if (P1Y < P2Y)
        {
            DrawPerson(P1X,P1Y,0);
            DrawPerson(P2X,P2Y,1);
        }
        else
        {
            DrawPerson(P2X,P2Y,1);
            DrawPerson(P1X,P1Y,0);
        }
        if (Shield > 0)
        {
            Outline(P1X-34,P1Y+29,68,69,0x79E2D4);
            Text("反光伞",P1X-27,P1Y+9,12,0x526F61);
        }
        if (BubbleTime > 0)
        {
            float bx = P2X - 110;
            if (bx < 8) bx = 8;
            if (bx > 852) bx = 852;
            float by = P2Y - 12;
            if (by < 87) by = 87;
            Rect(bx,by,220,29,0xF9EFD6);
            Outline(bx,by,220,29,0x91A297);
            Text(Bubble,bx+10,by+5,12,0x65504A);
            Rect(P2X-3,by+28,6,6,0xF9EFD6);
        }
        if (Slow > 0) Text("慢时生效",29,145,13,0x526F61);
        if (Force > 0) Text("听话中",944,145,13,0x526F61);
        if (EscapeTime > 0) Text("2P 挣脱中",28,174,13,0x975F47);
        if (Stun2 > 0) Text("晕！",P2X-12,P2Y+6,16,0x975F47);
        if (FocusTime > 0)
        {
            Outline(P2X-25,P2Y+34,50,52,0xE29A78);
            Text("耳机 " + N(FocusCharges) + " 次",P2X+30,P2Y+46,11,0x975F47);
        }
        if (Mood > 0 && Mode == 0) Text("分心！",P2X-18,P2Y+7,12,0x975F47);
        DrawHud();
        DrawInventory();
        DrawControls();
        if (Phase == 0) DrawMenu();
        if (Phase == 2) DrawChoice();
        if (Phase == 3) DrawPause();
        if (Phase == 4) DrawResult();
        if (Phase == 5) DrawRouteMap();
        if (Phase == 6 || Phase == 7 || Phase == 8) DrawRouteStop();
        if (Phase == 9) DrawCollection();
    }

    static void DrawRoad()
    {
        Rect(0,80,1080,660,0xD7DFD2);
        for (int x = 0; x < 1080; x = x + 58)
        {
            Line(x,80,x,200,0xC3CDC2);
            Line(x,600,x,740,0xC3CDC2);
        }
        for (int y = 80; y < 200; y = y + 30) Line(0,y,1080,y,0xC3CDC2);
        for (int y = 600; y < 740; y = y + 30) Line(0,y,1080,y,0xC3CDC2);
        Rect(22,80,202,44,0x889D93);
        Rect(30,80,186,35,0xA8B7AA);
        for (int x = 45; x < 200; x = x + 36) Rect(x,86,24,18,0xC7D8C3);
        Rect(854,80,204,45,0x9A9D9A);
        Rect(862,80,188,34,0xD2C7AE);
        Text("便利店",932,84,16,0x55645B);
        DrawTree(276,116);
        DrawTree(800,114);
        DrawTree(66,668);
        DrawTree(1002,673);
        Rect(125,648,131,17,0x597E70);
        Rect(138,665,8,19,0x6A7367);
        Rect(233,665,8,19,0x6A7367);
        Rect(841,656,120,14,0x738679);
        Rect(855,670,8,18,0x6A7367);
        Rect(940,670,8,18,0x6A7367);
        Rect(0,193,1080,9,0x64756E);
        Rect(0,600,1080,9,0x64756E);
        Rect(0,200,1080,400,0x34444B);
        for (int i = 0; i < LaneCount; i = i + 1)
        {
            float ly = LaneY[i] + 80;
            float lh = LaneH[i];
            float lt = ly - lh / 2;
            if (i > 0)
            {
                for (int x = 0; x < 1080; x = x + 44) Rect(x,lt,20,2,0x64756E);
            }
            for (float y = lt + 7; y < ly + lh / 2 - 4; y = y + 19)
            {
                Rect(477,y,126,10,0xDFE8D3);
            }
            int dir = 1;
            if (i == 1 || i == 3 || i == 5) dir = -1;
            for (int a = 0; a < 2; a = a + 1)
            {
                float ax = 230 + a * 584;
                Line(ax-dir*20,ly,ax+dir*20,ly,0x64756E);
                Line(ax+dir*8,ly-8,ax+dir*20,ly,0x64756E);
                Line(ax+dir*8,ly+8,ax+dir*20,ly,0x64756E);
            }
        }
        for (int i = 0; i < MedianCount; i = i + 1)
        {
            float my = MedianY[i] + 80;
            Rect(0,my-14,1080,28,0x91A297);
            for (int x = 10; x < 1080; x = x + 26) Rect(x,my-7,14,14,0x647B68);
            Rect(464,my-12,152,24,0xD6DDC9);
            Text("安全岛",522,my-7,10,0x718274);
        }
        Rect(464,129,152,46,0x526F61);
        Outline(449,119,182,62,0x779984);
        Text("对 岸 · 安 全",491,141,14,0xE0F1D1);
        Text("从这里出发 ↑",491,708,14,0x758C7F);
        Text("手机不能替你看马路",678,633,12,0x73897B);
        DrawLight(422,171);
        DrawLight(660,629);
        Rect(15,95,410,29,0x182127);
        if (Mode == 0) Text("第 " + N(RouteAct) + " 幕 / " + RouteNodeName(NodeKind) + " / " + Modifier,25,101,11,0xF4F0DF);
        else Text("对抗路段 / " + Modifier,25,101,11,0xF4F0DF);
    }

    static void DrawTree(float x, float y)
    {
        Circle(x-22,y+10,54,21,0xC3CDC2);
        Rect(x-4,y,8,26,0x83795D);
        Circle(x-26,y-29,52,52,0x759578);
        Circle(x-25,y-29,36,36,0x8CA987);
        Circle(x-18,y-23,16,16,0xA2B89A);
    }

    static void DrawLight(float x, float y)
    {
        Rect(x-3,y-11,6,30,0x67776A);
        Rect(x-9,y-34,18,32,0x253C34);
        Circle(x-4,y-29,8,8,0xDF7B6B);
        Circle(x-4,y-16,8,8,0x8FA873);
    }

    static void DrawCar(int i)
    {
        float x = CarX[i];
        float y = CarY[i] + 80;
        float w = CarW[i];
        float h = CarH[i];
        int dir = CarDirection[i];
        Rect(x-w/2+3,y-h/2+7,w,h,0x263C35);
        if (CarType[i] == 2)
        {
            Rect(x-15,y-7,28,14,0x34303B);
            Rect(x-10,y-10,15,20,0xE6C778);
            Rect(x-9,y-8,15,16,CarColor[i]);
            Circle(x+dir*3-6,y-6,12,12,0xF6D5AF);
            Rect(x+dir*8-6,y-6,12,12,0x83AB95);
        }
        else
        {
            Rect(x-w/2+5,y-h/2-3,15,6,0x263238);
            Rect(x-w/2+5,y+h/2-3,15,6,0x263238);
            Rect(x+w/2-22,y-h/2-3,15,6,0x263238);
            Rect(x+w/2-22,y+h/2-3,15,6,0x263238);
            Rect(x-w/2,y-h/2,w,h,CarColor[i]);
            Rect(x+dir*(w/2-22)-7,y-h/2+4,14,h-8,0x334F5D);
            Rect(x-dir*(w/2-15)-4,y-h/2+6,8,h-12,0x516778);
            for (int a = 0; a < 2; a = a + 1)
            {
                float ly = y-h/2+5+a*(h-18);
                Rect(x+dir*(w/2-4)-2,ly,4,8,0xFAF0BE);
                Rect(x-dir*(w/2-3)-1,ly,3,7,0xDC7869);
            }
            if (CarType[i] == 1)
            {
                for (float wx = -w/2+26; wx < w/2-27; wx = wx + 19)
                {
                    Rect(x+wx,y-h/2+4,12,8,0x4F6870);
                    Rect(x+wx,y+h/2-12,12,8,0x4F6870);
                }
            }
            if (Slow > 0) Outline(x-w/2-4,y-h/2-5,w+8,h+10,0xB7EEE4);
        }
    }

    static void DrawPerson(float x, float y, int owner)
    {
        y = y + 80;
        Circle(x-14,y+2,28,14,0x91A297);
        Rect(x-8,y+2,6,12,0x2E444B);
        Rect(x+2,y+2,6,12,0x2E444B);
        Rect(x-9,y+10,8,4,0xE8EAD9);
        Rect(x+2,y+10,8,4,0xE8EAD9);
        int shirt = 0x75C6B0;
        int hair = 0x344C42;
        if (owner == 1) { shirt = 0xE29A78; hair = 0x555263; }
        Rect(x-12,y-18,24,25,shirt);
        Rect(x-16,y-13,5,18,0xF0CAA6);
        Rect(x+11,y-13,5,18,0xF0CAA6);
        Circle(x-11,y-34,22,21,0xF3D2B1);
        Rect(x-12,y-37,24,11,hair);
        Rect(x-6,y-25,2,2,0x344B46);
        Rect(x+4,y-25,2,2,0x344B46);
        if (owner == 1)
        {
            Rect(x-6,y-9,13,17,0x354152);
            Rect(x-4,y-7,9,11,0xACF5E6);
            Rect(x-2,y-5,5,2,0xDDFCF0);
            Rect(x-2,y-1,3,2,0xDDFCF0);
            if (Mode == 1) Text("2P",x-9,y-58,11,0x865D45);
            else Text("同学",x-13,y-58,11,0x865D45);
        }
        else
        {
            Rect(x-8,y-14,15,13,0x4B9D84);
            if (Mode == 1) Text("1P",x-9,y-58,11,0x365F52);
            else Text("你",x-6,y-58,11,0x365F52);
        }
        if (owner == 0 && Arrived1 == 1) Text("✓",x+16,y-33,18,0x477A5A);
        if (owner == 1 && Arrived2 == 1) Text("✓",x+16,y-33,18,0x477A5A);
    }

    static void DrawHud()
    {
        Rect(0,0,1250,80,0x10171D);
        Rect(20,17,40,45,0xB4F98A);
        Rect(29,26,20,28,0x10171D);
        Rect(33,30,12,18,0xB4F98A);
        Circle(37,50,4,4,0xB4F98A);
        Text("CROSSWALK CLUB / START A RUN",76,13,10,0xB4F98A);
        Text("同学，别刷了！",74,33,25,0xF4F0DF);
        Line(357,15,357,66,0x303B41);
        if (Mode == 0)
        {
            Text("单人旅途 / 3 幕街区",379,13,10,0x81918F);
            if (Phase == 0) Text("3 幕 · 分岔路线",376,37,20,0xB4F98A);
            else Text(N(RouteAct) + " 幕 · " + N(RouteRow+1) + " 站",376,37,20,0xB4F98A);
        }
        else
        {
            Text("双人对抗 / 三局两胜",379,13,10,0x81918F);
            Text("第 " + N(Round) + " 局",376,35,25,0xB4F98A);
        }
        Text("生命 / 护送员",548,13,10,0x81918F);
        Text(N(Lives) + "  爱心",545,37,20,0xFF8575);
        if (Mode == 0)
        {
            Text("本轮得分",716,13,10,0x81918F);
            Text(N(Score),712,35,25,0xF4F0DF);
            Text("零钱 / 路线补给",886,13,10,0x81918F);
            Text(N(Coins) + " ¥",883,35,25,0xF5D474);
        }
        else
        {
            Text("比分 / 1P : 2P",716,13,10,0x81918F);
            Text(N(Score1) + " : " + N(Score2),712,35,25,0xF4F0DF);
            Text("本局倒计时",886,13,10,0x81918F);
            Text(N((int)TimeLeft) + " s",883,35,25,0xF5D474);
        }
        Text("P  暂停",1100,23,12,0x81918F);
        Text("M  菜单",1100,44,12,0x81918F);
    }

    static void DrawInventory()
    {
        Rect(1080,80,170,660,0x10171D);
        Rect(1090,80,150,660,0x182127);
        Outline(1090,80,150,660,0x303B41);
        Text("1P / 护送装备",1101,98,11,0xB4F98A);
        Line(1101,124,1229,124,0x303B41);
        float y = 137;
        int count = 0;
        int limit = 12;
        if (Mode == 0) limit = 8;
        for (int i = 0; i < 24; i = i + 1)
        {
            if (P1Items[i] > 0)
            {
                if (count < limit)
                {
                    Rect(1100,y,129,25,0x1C272E);
                    Text(ItemName(0,i),1107,y+5,11,0xF4F0DF);
                    y = y + 29;
                }
                count = count + 1;
            }
        }
        if (count == 0)
        {
            Text("背包还空着",1102,147,12,0x81918F);
            Text("旅途上收集装备",1102,173,10,0x81918F);
            Text("搭配自动生效",1102,195,10,0x81918F);
        }
        if (count > limit) Text("另有 " + N(count-limit) + " 件装备",1101,y+8,11,0x81918F);
        if (Mode == 1)
        {
            Text("2P / 捣蛋装备",1101,502,11,0xE29A78);
            Line(1101,526,1229,526,0x303B41);
            y = 538;
            for (int i = 0; i < 8; i = i + 1)
            {
                if (P2Items[i] > 0)
                {
                    Text(ItemName(1,i),1107,y+3,10,0xF4F0DF);
                    y = y + 23;
                }
            }
        }
        else
        {
            Text("当前装置 / T",1101,434,11,0xF5D474);
            Text(CurrentDeviceName(),1101,465,12,0xF4F0DF);
            if (GadgetCD > 0) Text("冷却 " + N((int)GadgetCD+1) + " s",1101,492,11,0x81918F);
            else Text("TAB 切换装置",1101,492,11,0x81918F);
            Line(1101,524,1229,524,0x303B41);
            Text("B / 图鉴与背包",1101,546,12,0x79E2D4);
            Text("独有装备，没有等级",1101,576,10,0x81918F);
            Text("获取两件即可组合",1101,600,10,0x81918F);
            Text("两人都到对岸",1101,650,11,0x81918F);
            Text("才算护送成功。",1101,673,11,0x81918F);
        }
    }

    static void DrawControls()
    {
        Rect(0,750,1250,150,0x182127);
        Line(0,750,1250,750,0x303B41);
        Text(StatusText,20,762,12,0xF4F0DF);
        Text("1P 体力",768,760,10,0x81918F);
        Text(N((int)Stamina) + " / " + N((int)MaxStamina),930,760,10,0xF4F0DF);
        Rect(768,782,213,5,0x303B41);
        float sw = Stamina / MaxStamina * 213;
        if (sw < 0) sw = 0;
        if (sw > 213) sw = 213;
        Rect(768,782,sw,5,0xB4F98A);
        if (Mode == 1)
        {
            Text("2P 捣蛋能量",1014,760,10,0x81918F);
            Text(N((int)Energy),1200,760,10,0xE29A78);
            Rect(1014,782,213,5,0x303B41);
            float ew = Energy / MaxEnergy * 213;
            if (ew < 0) ew = 0;
            if (ew > 213) ew = 213;
            Rect(1014,782,ew,5,0xE29A78);
        }
        else
        {
            Text("两个人都到对岸才算过关",1014,765,11,0x79E2D4);
        }
        Rect(20,802,633,34,0x1C272E);
        Text("1P  WASD 移动 / 按住空格牵手 / E 口哨 / Q 慢时 / F 反光伞 / R 奶茶",30,811,11,0xB4F98A);
        if (Mode == 1)
        {
            Rect(670,802,557,34,0x1C272E);
            Text("2P  ↑↓←→ 移动 / Enter 挣脱 / J 假通知 / K 滑步 / L 耳机",680,811,11,0xE29A78);
        }
        else Text("T 使用装置 / TAB 切换 / B 图鉴与背包",678,811,11,0x79E2D4);
        DrawSkillStatus("E 口哨",WhistleCD,1,30,839,0xB4F98A);
        DrawSkillStatus("Q 慢时",ClockCD,P1Items[0],165,839,0xB4F98A);
        DrawSkillStatus("F 反光伞",ShieldCD,P1Items[1],300,839,0xB4F98A);
        DrawSkillStatus("R 奶茶",TeaCD,P1Items[2],440,839,0xB4F98A);
        if (Mode == 1)
        {
            DrawSkillStatus("Enter 挣脱",EscapeCD,1,680,839,0xE29A78);
            DrawSkillStatus("J 通知",NotificationCD,P2Items[0],824,839,0xE29A78);
            DrawSkillStatus("K 滑步",DashCD,P2Items[1],956,839,0xE29A78);
            DrawSkillStatus("L 耳机",FocusCD,P2Items[2],1088,839,0xE29A78);
        }
        if (Mode == 0)
        {
            int hasDevice = 0;
            if (ItemActiveT >= 12 && ItemActiveT <= 15) hasDevice = 1;
            DrawSkillStatus("T " + CurrentDeviceName(),GadgetCD,hasDevice,680,839,0x79E2D4);
        }
        DrawComboLine(20,864);
        Text("P / ESC 暂停    M 菜单",20,886,9,0x81918F);
        Text("LOOK UP / HOLD HANDS / GET HOME SAFE",908,886,9,0x81918F);
    }

    static void DrawSkillStatus(string name, float cooldown, int rank, float x, float y, int color)
    {
        if (rank == 0) Text(name + " · 未获取",x,y,10,0x81918F);
        else if (cooldown > 0) Text(name + " · " + N((int)cooldown+1) + "s",x,y,10,0x81918F);
        else Text(name + " · 就绪",x,y,10,color);
    }

    static void DrawBagSummary(float x, float y)
    {
        int kinds1 = 0;
        int kinds2 = 0;
        for (int i = 0; i < 24; i = i + 1)
        {
            if (P1Items[i] > 0) kinds1 = kinds1 + 1;
        }
        for (int i = 0; i < 8; i = i + 1)
        {
            if (P2Items[i] > 0) kinds2 = kinds2 + 1;
        }
        Text("1P 已收集 " + N(kinds1) + " 件独有装备",x,y,12,0xB4F98A);
        if (Mode == 1) Text("2P 已收集 " + N(kinds2) + " 件独有装备",x+460,y,12,0xE29A78);
    }

    static void DrawComboLine(float x, float y)
    {
        int count1 = 0;
        int count2 = 0;
        for (int i = 0; i < ComboCount(0); i = i + 1)
        {
            if (ComboActive(0,i)) count1 = count1 + 1;
        }
        for (int i = 0; i < ComboCount(1); i = i + 1)
        {
            if (ComboActive(1,i)) count2 = count2 + 1;
        }
        if (Mode == 0) Text("已激活 " + N(count1) + " 套搭配 · B 查看配方与效果",x,y,11,0x79E2D4);
        else Text("1P 已激活 " + N(count1) + " 套 / 2P 已激活 " + N(count2) + " 套搭配",x,y,11,0x79E2D4);
    }

    static void DrawShade()
    {
        Rect(0,80,1250,660,0x0B1319);
        for (int x = 0; x < 1250; x = x + 36) Line(x,80,x,740,0x15211F);
        for (int y = 80; y < 740; y = y + 36) Line(0,y,1250,y,0x15211F);
    }

    static void DrawMenu()
    {
        DrawShade();
        Text("CROSSWALK CLUB / START A RUN",180,152,11,0xB4F98A);
        Text("这条视频，",176,197,38,0xF4F0DF);
        Text("能到对岸再刷吗？",176,249,38,0xB4F98A);
        Text("同学沉迷手机，偶尔停下、乱走，甚至倒退。",180,322,14,0x81918F);
        Text("你得牵着他穿过车流，把两个人都送到对岸。",180,348,14,0x81918F);
        int soloBorder = 0x303B41;
        int duelBorder = 0x303B41;
        int soloTitle = 0xF4F0DF;
        int duelTitle = 0xF4F0DF;
        if (Mode == 0) { soloBorder = 0xB4F98A;soloTitle = 0xB4F98A; }
        if (Mode == 1) { duelBorder = 0xE29A78;duelTitle = 0xE29A78; }
        Rect(180,410,300,96,0x182127);
        Outline(180,410,300,96,soloBorder);
        Text("[1]  单人护送",199,426,18,soloTitle);
        Text("3 幕分岔街区 · 事件 / 商店 / 休息",199,460,11,0x81918F);
        Text("24 件独有装备 · 自由组合护送流派",199,479,11,0x81918F);
        Rect(500,410,300,96,0x182127);
        Outline(500,410,300,96,duelBorder);
        Text("[2]  双人对抗",519,426,18,duelTitle);
        Text("同屏对抗 · 75 秒 · 三局两胜",519,460,11,0x81918F);
        Text("2P 操控手机同学，阻挠 1P 护送。",519,479,11,0x81918F);
        Rect(180,532,620,52,0xB4F98A);
        Text("ENTER / 开始游戏  →",384,544,19,0x17211A);
        Text("1P：WASD / 空格 / E / Q F R",180,611,12,0x81918F);
        Text("2P：方向键 / Enter / J K L",180,639,12,0x81918F);
        Text("规划放学路线 · 收集独有装备 · 搭配出你的护送方式",180,685,10,0x81918F);
        DrawMenuArt();
    }

    static void DrawMenuArt()
    {
        Rect(866,218,245,278,0x182127);
        Outline(866,218,245,278,0x303B41);
        for (int y = 238; y < 484; y = y + 35) Rect(908,y,143,18,0x34444B);
        Rect(878,421,216,61,0x26392C);
        DrawPerson(964,377,0);
        DrawPerson(1003,388,1);
        Line(976,449,991,460,0xB7F1D5);
        Text("一起过马路",927,523,19,0xF4F0DF);
        Text("车来之前，抬一下头。",900,561,12,0x81918F);
        Text("ROGUELIKE ESCORT",921,588,10,0xB4F98A);
    }

    static void DrawChoice()
    {
        DrawShade();
        int color = 0xB4F98A;
        if (ChoiceOwner == 1) color = 0xE29A78;
        Text("GEAR UP / " + N(ChoiceOwner+1) + "P SELECT",150,167,11,color);
        if (Mode == 0) Text("选一件独有装备，继续旅途。",144,213,31,0xF4F0DF);
        else Text(N(ChoiceOwner+1) + "P 选一道具，准备下一局。",144,213,31,0xF4F0DF);
        Text("按 1 / 2 / 3 选择，或点击卡片。装备不重复获取，集齐配方自动生效。",150,275,13,0x81918F);
        for (int i = 0; i < 3; i = i + 1)
        {
            float x = 150 + i * 315;
            Rect(x,330,290,238,0x182127);
            int border = 0x303B41;
            if (SelectedChoice == i) border = color;
            Outline(x,330,290,238,border);
            Rect(x+18,350,35,35,color);
            Text(N(i+1),x+31,356,18,0x10171D);
            Text(ChoiceTags[i],x+69,357,11,color);
            Text(ChoiceNames[i],x+18,414,22,0xF4F0DF);
            Text(ChoiceDescriptions[i],x+18,456,11,0x81918F);
            Line(x+18,510,x+272,510,0x303B41);
            Text(ChoiceCombos[i],x+18,530,11,color);
        }
        DrawBagSummary(150,604);
        Text("← / → 切换卡片，Enter 确认。装备保留到本次挑战结束。",150,641,12,0x81918F);
        DrawComboLine(150,678);
    }

    static void DrawPause()
    {
        DrawShade();
        Text("暂停一下，抬头看路。",420,194,31,0xF4F0DF);
        Text("P / ESC / Enter 继续    M 返回菜单",442,263,14,0xB4F98A);
        Text("1P / 护送员",220,338,20,0xB4F98A);
        Text("WASD 移动   按住空格牵手   E 口哨",220,381,14,0x81918F);
        Text("Q 慢时钟   F 反光伞   R 续命奶茶",220,412,14,0x81918F);
        Text("道具需先获取；同学停下或乱走时，靠近牵住他。",220,447,12,0x81918F);
        if (Mode == 1)
        {
            Text("2P / 手机同学",220,517,20,0xE29A78);
            Text("方向键移动   Enter 挣脱   J 假通知",220,560,14,0x81918F);
            Text("K 横滑闪避   L 沉浸耳机",220,591,14,0x81918F);
            Text("拖延到倒计时结束，或让 1P 耗尽生命，就能赢下本局。",220,626,12,0x81918F);
        }
        else
        {
            Text("在分岔路线选择路段、事件、商店和休息点。",220,517,14,0x81918F);
            Text("T 使用装置 / TAB 切换装置 / B 查看图鉴与背包",220,551,14,0x79E2D4);
            Text("装备每局只能获取一次，独有组合会改变护送方式。",220,587,12,0x81918F);
        }
        DrawBagSummary(220,674);
        DrawComboLine(220,704);
    }

    static void DrawResult()
    {
        DrawShade();
        if (Mode == 0) Text("RUN FINISHED / 护送记录",486,184,11,0xB4F98A);
        else Text("DUEL FINISHED / 三局两胜",472,184,11,0xB4F98A);
        Text(EndTitle,370,238,34,0xF4F0DF);
        Text(EndText,275,329,14,0x81918F);
        if (Mode == 0) Text("第 " + N(RouteAct) + " 幕     得分 " + N(Score) + "     零钱 " + N(Coins),350,420,22,0xB4F98A);
        else Text("最终比分    " + N(Score1) + " : " + N(Score2),431,420,24,0xB4F98A);
        Rect(280,510,250,50,0xB4F98A);
        Text("ENTER / 再来一局",322,523,17,0x17211A);
        Rect(570,510,250,50,0x1C272E);
        Outline(570,510,250,50,0x303B41);
        Text("M / 返回菜单",630,523,17,0xF4F0DF);
        DrawComboLine(125,619);
        Text("Enter 重玩相同关卡种子 / N 换一组随机关卡",390,653,12,0x81918F);
        Text("别刷了，下局该你看路了。",434,681,12,0x81918F);
    }
}
