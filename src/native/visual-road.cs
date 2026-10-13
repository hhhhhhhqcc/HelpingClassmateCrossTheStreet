// Night city gameplay presentation. All positions and gameplay rules are unchanged.
public static partial class Game
{
    static void DrawRoad()
    {
        Rect(0,80,1080,660,0x17283E);
        Art(1,0,80,1080,660);
        for (int x=0;x<1080;x=x+72)
        {
            Line(x,80,x,197,0x233B52);
            Line(x,608,x,740,0x233B52);
        }
        for (int y=90;y<198;y=y+27) Line(0,y,1080,y,0x233B52);
        for (int y=612;y<740;y=y+27) Line(0,y,1080,y,0x233B52);
        Rect(0,194,1080,11,0x0B1426);
        Rect(0,596,1080,14,0x0B1426);
        Rect(0,197,1080,4,0x728596);
        Rect(0,601,1080,4,0x728596);
        Rect(0,201,1080,399,0x223347);
        for (int i=0;i<LaneCount;i=i+1)
        {
            float ly=LaneY[i]+80;
            float lh=LaneH[i];
            float lt=ly-lh/2;
            int asphalt=0x24364B;
            if (i%2==1) asphalt=0x293D52;
            Rect(0,lt,1080,lh,asphalt);
            if (i>0)
            {
                for (int x=4;x<1080;x=x+50) Rect(x,lt,26,2,0x74879A);
            }
            for (float sy=lt+8;sy<ly+lh/2-5;sy=sy+19)
            {
                Rect(477,sy+2,126,10,0x132338);
                Rect(477,sy,126,9,0xD7DDDA);
            }
            int direction=LaneDirection[i];
            for (int a=0;a<2;a=a+1)
            {
                float ax=216+a*624;
                Line(ax-direction*25,ly,ax+direction*23,ly,0x6B839B);
                Line(ax+direction*10,ly-10,ax+direction*23,ly,0x6B839B);
                Line(ax+direction*10,ly+10,ax+direction*23,ly,0x6B839B);
                Line(ax-direction*25,ly+2,ax+direction*23,ly+2,0x6B839B);
            }
        }
        for (int i=0;i<MedianCount;i=i+1)
        {
            float my=MedianY[i]+80;
            Rect(0,my-15,1080,31,0x0D1B2D);
            Rect(0,my-13,1080,26,0x305F60);
            Rect(0,my-13,1080,3,0x78B6A8);
            Rect(0,my+10,1080,3,0x78B6A8);
            for (int x=16;x<1080;x=x+38)
            {
                Rect(x,my-5,22,10,0x427B6B);
                Line(x+4,my-4,x+14,my+3,0x619987);
            }
            Rect(461,my-13,158,26,0x63DFC4);
            Text("安 全 岛",510,my-8,11,0x122B32);
        }
        // Street fronts, benches, and trees remain outside the traffic lanes.
        Panel(26,87,182,43,0x243B55,0x38556B);
        Rect(37,96,22,20,0xFFD38B);
        Rect(69,96,22,20,0x9DB8C8);
        Rect(101,96,22,20,0xFFD38B);
        Rect(133,96,22,20,0x9DB8C8);
        Rect(165,96,22,20,0xFFD38B);
        Panel(865,87,186,44,0x263E54,0x426075);
        Text("便利店  24H",910,97,15,0xFFB575);
        Rect(879,126,158,4,0x63DFC4);
        DrawTree(276,125);
        DrawTree(796,125);
        DrawTree(64,672);
        DrawTree(1001,680);
        Rect(128,651,124,13,0x466778);
        Rect(139,664,7,19,0x263C51);
        Rect(233,664,7,19,0x263C51);
        Line(131,657,248,657,0x8B9EAC);
        Rect(839,662,121,13,0x466778);
        Rect(851,675,7,18,0x263C51);
        Rect(942,675,7,18,0x263C51);
        Line(842,668,956,668,0x8B9EAC);
        Panel(447,126,187,55,0x173E44,0x63DFC4);
        Text("↑  对岸安全区",483,140,16,0xB7F7E7);
        Text("两人到达 · 护送成功",479,164,9,0x84BCAF);
        Text("从这里出发  ↑",490,715,12,0xC5D6E4);
        DrawLight(422,176);
        DrawLight(663,641);
        Panel(16,140,364,35,0x0F1F32,0x294057);
        if (Mode==0) Text("第 "+N(RouteAct)+" 幕  /  "+RouteNodeName(NodeKind)+"  /  "+Modifier,28,151,10,0xDCE8EE);
        else Text("对抗路段  /  "+Modifier,28,151,11,0xDCE8EE);
        if (Modifier=="雨天")
        {
            for (int x=90;x<1080;x=x+111)
            {
                Line(x,175,x+25,155,0x416079);
                Line(x+17,632,x+45,612,0x416079);
            }
        }
    }

    static void DrawTree(float x,float y)
    {
        Circle(x-29,y+14,61,19,0x0E1B2B);
        Rect(x-4,y-6,9,32,0x6B5E59);
        Circle(x-26,y-34,54,51,0x193E40);
        Circle(x-25,y-34,42,41,0x285F55);
        Circle(x-22,y-34,26,24,0x3B8070);
        Circle(x-10,y-18,23,22,0x316D60);
        Line(x-7,y+8,x+4,y-4,0x93A38E);
    }

    static void DrawLight(float x,float y)
    {
        Rect(x-3,y-8,6,33,0x7890A4);
        Circle(x-11,y+20,22,8,0x0A182A);
        Panel(x-11,y-42,22,37,0x0B1426,0x496779);
        Circle(x-5,y-36,10,10,0xFF7E86);
        Circle(x-5,y-20,10,10,0x63DFC4);
        Circle(x-2,y-33,3,3,0xFFD9CF);
        Circle(x-2,y-17,3,3,0xC5FFED);
    }

    static void DrawCar(int i)
    {
        float x=CarX[i];
        float y=CarY[i]+80;
        float w=CarW[i];
        float h=CarH[i];
        int direction=CarDirection[i];
        Circle(x-w/2-1,y-h/2+6,w+6,h+8,0x0D192A);
        if (CarType[i]==2)
        {
            Circle(x-w/2,y-8,15,16,0x0B1426);
            Circle(x+w/2-15,y-8,15,16,0x0B1426);
            Rect(x-w/2+4,y-2,w-8,4,0xA6BDCA);
            Panel(x-11,y-10,24,20,CarColor[i],0xF6EFDF);
            Circle(x-8,y-8,16,16,0xF6D3AD);
            Circle(x-8,y-8,16,10,0x416784);
            Rect(x-direction*7-9,y-6,10,12,0x567F9D);
            Line(x+direction*8,y-9,x+direction*15,y-9,0xF6EFDF);
            Circle(x+direction*(w/2)-5,y-5,10,10,0xFFDA8C);
        }
        else
        {
            Rect(x-w/2+9,y-h/2-3,14,7,0x091320);
            Rect(x-w/2+9,y+h/2-4,14,7,0x091320);
            Rect(x+w/2-24,y-h/2-3,14,7,0x091320);
            Rect(x+w/2-24,y+h/2-4,14,7,0x091320);
            Panel(x-w/2,y-h/2,w,h,CarColor[i],0xB6C6CD);
            Rect(x-w/2+21,y-h/2+4,w-43,h-8,0x35516A);
            Rect(x-w/2+25,y-h/2+7,w-51,h-14,CarColor[i]);
            Rect(x+direction*(w/2-19)-7,y-h/2+5,14,h-10,0x17364A);
            Line(x+direction*(w/2-19)-5,y-h/2+7,x+direction*(w/2-19)+5,y-h/2+13,0x729CA8);
            Rect(x-direction*(w/2-15)-4,y-h/2+7,8,h-14,0x274357);
            Line(x-w/2+16,y-h/2+4,x+w/2-16,y-h/2+4,0xEBE3D3);
            for (int a=0;a<2;a=a+1)
            {
                float ly=y-h/2+6+a*(h-18);
                Circle(x+direction*(w/2+1)-5,ly-3,10,12,0x846F51);
                Rect(x+direction*(w/2-3)-2,ly,4,7,0xFFE6A6);
                Rect(x-direction*(w/2-3)-2,ly,4,7,0xFF6E77);
            }
            if (CarType[i]==1)
            {
                for (float wx=-w/2+27;wx<w/2-26;wx=wx+18)
                {
                    Rect(x+wx,y-h/2+4,11,8,0x183449);
                    Rect(x+wx,y+h/2-12,11,8,0x183449);
                    Line(x+wx+2,y-h/2+6,x+wx+7,y-h/2+6,0x88B1BA);
                }
                Rect(x-20,y-4,41,8,0xE4DFCF);
                Text("校车",x-12,y-5,8,0x314858);
            }
            if (Slow>0) Outline(x-w/2-4,y-h/2-5,w+8,h+10,0x63DFC4);
        }
    }

    static void DrawPerson(float x,float y,int owner)
    {
        y=y+80;
        Circle(x-18,y+4,36,15,0x0A1525);
        Rect(x-9,y,7,14,0x122136);
        Rect(x+3,y,7,14,0x122136);
        Rect(x-11,y+10,10,5,0xF6EFDF);
        Rect(x+3,y+10,10,5,0xF6EFDF);
        int shirt=0x63DFC4;
        int edge=0x123E45;
        int hair=0x20283B;
        if (owner==1) { shirt=0xFFB575;edge=0x864C3E;hair=0x3A293D; }
        Circle(x-14,y-22,28,31,edge);
        Rect(x-12,y-18,24,24,shirt);
        Rect(x-16,y-12,5,18,0xF2CBA8);
        Rect(x+11,y-12,5,18,0xF2CBA8);
        Rect(x-16,y-15,5,8,shirt);
        Rect(x+11,y-15,5,8,shirt);
        Line(x-7,y-18,x-7,y+4,0xECF7E7);
        Line(x+7,y-18,x+7,y+4,0xECF7E7);
        Circle(x-12,y-38,25,24,0x172336);
        Circle(x-11,y-35,22,22,0xF6D3AF);
        Circle(x-12,y-40,25,17,hair);
        Rect(x-12,y-31,5,10,hair);
        Rect(x+8,y-31,5,8,hair);
        Rect(x-6,y-24,2,3,0x343249);
        Rect(x+4,y-24,2,3,0x343249);
        Line(x-2,y-19,x+2,y-19,0xC98775);
        if (owner==1)
        {
            Circle(x-9,y-13,19,25,0x4C6F91);
            Rect(x-7,y-12,15,22,0x132339);
            Rect(x-5,y-10,11,16,0x68CFFF);
            Rect(x-3,y-8,7,3,0xE5F9FF);
            Rect(x-3,y-2,5,2,0xD3F6FF);
            Circle(x-5,y+8,3,3,0xA9D7EF);
            Rect(x-11,y-7,5,8,0xF6D3AF);
            Rect(x+8,y-7,5,8,0xF6D3AF);
            Panel(x-25,y-66,50,21,0x452F35,0xFFB575);
            if (Mode==1) Text("2P",x-10,y-63,11,0xFFD8B1);
            else Text("同学",x-13,y-63,11,0xFFD8B1);
        }
        else
        {
            Rect(x-8,y-14,15,17,0x286F68);
            Rect(x-5,y-12,9,3,0xA5F4DA);
            Panel(x-22,y-66,44,21,0x153C43,0x63DFC4);
            if (Mode==1) Text("1P",x-10,y-63,11,0xA6F5DB);
            else Text("你",x-6,y-63,11,0xA6F5DB);
        }
        if (owner==0 && Arrived1==1) Text("✓",x+17,y-34,20,0x63DFC4);
        if (owner==1 && Arrived2==1) Text("✓",x+17,y-34,20,0x63DFC4);
    }

    static void DrawHud()
    {
        Rect(0,0,1250,80,0x0B1426);
        Line(0,79,1250,79,0x2A425B);
        Panel(18,17,42,47,0x163C44,0x63DFC4);
        Rect(30,25,19,30,0x63DFC4);
        Rect(33,29,13,19,0x0B1426);
        Circle(37,51,4,3,0x0B1426);
        Text("CROSSWALK CLUB",76,14,9,0x63DFC4);
        Text("同学，别刷了！",73,33,24,0xF6EFDF);
        Panel(346,14,174,52,0x15243A,0x294057);
        if (Mode==0)
        {
            Text("单人护送  /  分岔旅途",360,20,9,0xA0AEC2);
            int station=RouteRow+1;
            if (station<1) station=1;
            Text(N(RouteAct)+" 幕 · 第 "+N(station)+" 站",360,38,17,0x63DFC4);
        }
        else
        {
            Text("双人对抗  /  三局两胜",360,20,9,0xA0AEC2);
            Text("第 "+N(Round)+" 局",360,37,19,0x63DFC4);
        }
        Panel(534,14,126,52,0x15243A,0x294057);
        Text("护送员生命",548,20,9,0xA0AEC2);
        Text("♥  "+N(Lives),548,35,23,0xFF8C92);
        Panel(674,14,149,52,0x15243A,0x294057);
        Panel(837,14,179,52,0x15243A,0x294057);
        if (Mode==0)
        {
            Text("旅途得分",688,20,9,0xA0AEC2);
            Text(N(Score),688,35,23,0xF6EFDF);
            Text("零钱  /  路线补给",851,20,9,0xA0AEC2);
            Text(N(Coins)+" ¥",851,35,23,0xFFB575);
        }
        else
        {
            Text("比分   1P : 2P",688,20,9,0xA0AEC2);
            Text(N(Score1)+" : "+N(Score2),688,35,23,0xF6EFDF);
            Text("本局剩余时间",851,20,9,0xA0AEC2);
            int timeColor=0xFFB575;
            if (TimeLeft<15) timeColor=0xFF8C92;
            Text(N((int)TimeLeft)+" 秒",851,35,23,timeColor);
        }
        Keycap("P",1038,16,0xA0AEC2);
        Text("暂停",1088,19,11,0xA0AEC2);
        Keycap("M",1145,16,0xA0AEC2);
        Text("菜单",1195,19,11,0xA0AEC2);
        Text("抬头看路，一起回家。",1050,53,10,0x7E93AB);
    }

    static void DrawInventory()
    {
        Rect(1080,80,170,660,0x0B1426);
        Line(1080,80,1080,740,0x294057);
        int count1=ItemOwnedCount(0);
        if (Mode==1)
        {
            Panel(1091,91,148,353,0x15243A,0x294057);
            Text("1P  /  护送装备",1102,105,12,0x63DFC4);
            Line(1103,130,1227,130,0x294057);
            float y=139;
            for (int i=0;i<12;i=i+1)
            {
                if (P1Items[i]>0)
                {
                    Art(i+3,1103,y,19,19);
                    Text(ItemName(0,i),1128,y+2,10,0xF6EFDF);
                    y=y+23;
                }
            }
            if (count1==0)
            {
                Text("背包暂时为空",1103,150,11,0xA0AEC2);
                Text("赛前和局间选装",1103,178,10,0x7E93AB);
            }
            Text("独有装备  "+N(count1)+" / 12",1103,423,9,0xA0AEC2);
            Panel(1091,456,148,272,0x15243A,0x294057);
            Text("2P  /  捣蛋装备",1102,470,12,0xFFB575);
            Line(1103,494,1227,494,0x294057);
            y=504;
            for (int i=0;i<8;i=i+1)
            {
                if (P2Items[i]>0)
                {
                    Circle(1106,y+6,6,6,0xFFB575);
                    Text(ItemName(1,i),1120,y+2,10,0xF6EFDF);
                    y=y+23;
                }
            }
            if (ItemOwnedCount(1)==0) Text("局间选择捣蛋道具",1103,515,10,0xA0AEC2);
            Text("独有装备  "+N(ItemOwnedCount(1))+" / 8",1103,706,9,0xA0AEC2);
            return;
        }
        Panel(1091,91,148,637,0x15243A,0x294057);
        Text("护 送 背 包",1104,106,12,0x63DFC4);
        Text("已收集  "+N(count1)+" / 24",1104,132,10,0xA0AEC2);
        Line(1103,158,1227,158,0x294057);
        for (int i=0;i<3;i=i+1)
        {
            float y=171+i*48;
            int border=0x2A4059;
            if (P1Items[i]>0) border=0x3C716E;
            Panel(1100,y,131,42,0x0F1E32,border);
            Art(i+3,1107,y+7,27,27);
            string skill="慢时钟  Q";
            float cd=ClockCD;
            if (i==1) { skill="反光伞  F";cd=ShieldCD; }
            if (i==2) { skill="续命奶茶  R";cd=TeaCD; }
            Text(skill,1142,y+5,10,0xF6EFDF);
            if (P1Items[i]==0) Text("尚未获得",1142,y+23,9,0x7E93AB);
            else if (cd>0) Text(N((int)cd+1)+" 秒后就绪",1142,y+23,9,0xA0AEC2);
            else Text("可以使用",1142,y+23,9,0x63DFC4);
        }
        Panel(1100,326,131,178,0x0F1E32,0x365C66);
        Keycap("T",1108,337,0x63DFC4);
        Text("当前装置",1155,343,10,0xA0AEC2);
        if (ItemActiveT>=12 && ItemActiveT<=15)
        {
            Art(ItemActiveT+3,1139,371,52,52);
            Text(CurrentDeviceName(),1108,433,12,0xF6EFDF);
            if (GadgetCD>0) Text("冷却  "+N((int)GadgetCD+1)+" 秒",1108,455,10,0xA0AEC2);
            else Text("就绪 · 按 T 使用",1108,455,10,0x63DFC4);
        }
        else
        {
            Circle(1148,376,36,36,0x263D52);
            Text("?",1157,380,21,0x8094AC);
            Text("旅途上寻找装置",1108,434,10,0xA0AEC2);
            Text("路障 / 遥控 / 引路",1108,455,9,0x7E93AB);
        }
        Text("TAB  切换装置",1108,483,10,0xA0AEC2);
        int combos=0;
        for (int c=0;c<ComboCount(0);c=c+1) { if (ComboActive(0,c)) combos=combos+1; }
        Text("已激活 "+N(combos)+" 套组合",1104,531,11,0x63DFC4);
        Text("装备集齐 · 自动生效",1104,554,9,0xA0AEC2);
        Panel(1100,584,131,53,0x1E3B49,0x63DFC4);
        Keycap("B",1107,593,0x63DFC4);
        Text("图鉴 / 背包",1156,597,10,0xF6EFDF);
        Text("查看全部装备与组合配方",1107,622,9,0x9DCABD);
        Text("两个人都到对岸",1104,667,11,0xD0DFE8);
        Text("才算护送成功。",1104,691,11,0xA0AEC2);
    }

    static void DrawControls()
    {
        Rect(0,740,1250,160,0x0B1426);
        Line(0,740,1250,740,0x294057);
        Text(StatusText,22,752,12,0xF6EFDF);
        Text("1P 体力",769,751,10,0xA0AEC2);
        Text(N((int)Stamina)+" / "+N((int)MaxStamina),916,751,10,0xD0E8DE);
        Rect(769,773,213,6,0x223950);
        float sw=Stamina/MaxStamina*213;
        if (sw<0) sw=0;
        if (sw>213) sw=213;
        Rect(769,773,sw,6,0x63DFC4);
        if (Mode==1)
        {
            Text("2P 捣蛋能量",1013,751,10,0xA0AEC2);
            Text(N((int)Energy),1200,751,10,0xFFB575);
            Rect(1013,773,213,6,0x223950);
            float ew=Energy/MaxEnergy*213;
            if (ew<0) ew=0;
            if (ew>213) ew=213;
            Rect(1013,773,ew,6,0xFFB575);
        }
        else Text("牵手护送  /  两人同时到岸",1009,758,11,0x9DCABD);
        Text("1P   WASD 移动   /   按住 SPACE 牵手",22,789,11,0xBCECDC);
        if (Mode==1) Text("2P   方向键移动  /  Enter 挣脱  /  J K L 技能",681,789,11,0xFFD1A3);
        else Text("T 使用装置   /   TAB 切换   /   B 图鉴与背包",681,789,11,0x9DCABD);
        DrawSkillStatus("E  口哨",WhistleCD,1,22,812,0x63DFC4);
        DrawSkillStatus("Q  慢时钟",ClockCD,P1Items[0],175,812,0x63DFC4);
        DrawSkillStatus("F  反光伞",ShieldCD,P1Items[1],328,812,0x63DFC4);
        DrawSkillStatus("R  续命奶茶",TeaCD,P1Items[2],481,812,0x63DFC4);
        if (Mode==1)
        {
            DrawSkillStatus("Enter  挣脱",EscapeCD,1,681,812,0xFFB575);
            DrawSkillStatus("J  假通知",NotificationCD,P2Items[0],819,812,0xFFB575);
            DrawSkillStatus("K  横滑闪避",DashCD,P2Items[1],957,812,0xFFB575);
            DrawSkillStatus("L  沉浸耳机",FocusCD,P2Items[2],1095,812,0xFFB575);
        }
        else
        {
            int hasDevice=0;
            if (ItemActiveT>=12 && ItemActiveT<=15) hasDevice=1;
            DrawSkillStatus("T  "+CurrentDeviceName(),GadgetCD,hasDevice,681,812,0x63DFC4);
            Keycap("B",839,821,0x63DFC4);
            Text("图鉴与背包",890,828,11,0xC5D8E6);
            Panel(1028,812,198,43,0x15243A,0x294057);
            Text("SPACE",1040,820,11,0xF6EFDF);
            if (Linked==1) Text("已牵手 · 注意体力",1040,838,9,0x63DFC4);
            else Text("靠近同学，按住牵手",1040,838,9,0xA0AEC2);
        }
        DrawComboLine(22,866);
        Text("P / ESC 暂停    M 菜单",22,888,9,0x7E93AB);
        Text("LOOK UP  /  HOLD HANDS  /  GET HOME SAFE",884,888,9,0x7E93AB);
    }

    static void DrawSkillStatus(string name,float cooldown,int rank,float x,float y,int color)
    {
        int border=0x294057;
        int title=0xA0AEC2;
        if (rank>0 && cooldown<=0) { border=color;title=0xF6EFDF; }
        Panel(x,y,131,43,0x15243A,border);
        Text(name,x+10,y+6,11,title);
        if (rank==0) Text("尚未获得",x+10,y+26,9,0x7E93AB);
        else if (cooldown>0) Text("冷却  "+N((int)cooldown+1)+" 秒",x+10,y+26,9,0xA0AEC2);
        else Text("就绪",x+10,y+26,9,color);
        if (rank>0 && cooldown<=0) Circle(x+115,y+29,5,5,color);
    }

    static void DrawComboLine(float x,float y)
    {
        int count1=0;
        int count2=0;
        for (int i=0;i<ComboCount(0);i=i+1) { if (ComboActive(0,i)) count1=count1+1; }
        for (int i=0;i<ComboCount(1);i=i+1) { if (ComboActive(1,i)) count2=count2+1; }
        if (Mode==0) Text("已激活 "+N(count1)+" 套组合  ·  B 查看配方与效果",x,y,11,0x63DFC4);
        else Text("1P 已激活 "+N(count1)+" 套  /  2P 已激活 "+N(count2)+" 套组合",x,y,11,0x63DFC4);
    }

    static void DrawBagSummary(float x,float y)
    {
        Text("1P 已收集 "+N(ItemOwnedCount(0))+" 件独有装备",x,y,12,0x63DFC4);
        if (Mode==1) Text("2P 已收集 "+N(ItemOwnedCount(1))+" 件独有装备",x+460,y,12,0xFFB575);
    }

    static void DrawRouteObjects()
    {
        if (RoadRule==2)
        {
            float y=ObstacleY+80;
            Rect(ObstacleX+4,y+5,ObstacleW,ObstacleH,0x0A1425);
            Rect(ObstacleX,y,ObstacleW,ObstacleH,0xDBA365);
            for (float x=ObstacleX+7;x<ObstacleX+ObstacleW-15;x=x+31)
                Line(x,y+4,x+15,y+ObstacleH-4,0x15243A);
            Outline(ObstacleX,y,ObstacleW,ObstacleH,0xFFD193);
            Rect(ObstacleX+33,y+6,ObstacleW-66,ObstacleH-12,0xE7B77F);
            Text("← 绕行   施工隔离带   绕行 →",ObstacleX+39,y+9,11,0x25384B);
        }
        if (RoadblockTime>0)
        {
            Panel(RoadblockX-37,RoadblockY+68,74,24,0xFFB575,0xFFD59B);
            Line(RoadblockX-31,RoadblockY+74,RoadblockX-21,RoadblockY+87,0x26374A);
            Line(RoadblockX+20,RoadblockY+74,RoadblockX+30,RoadblockY+87,0x26374A);
            Text("路障",RoadblockX-13,RoadblockY+74,10,0x26374A);
        }
        if (BeaconTime>0)
        {
            Circle(BeaconX-26,BeaconY+54,52,52,0x1F645F);
            Circle(BeaconX-20,BeaconY+60,40,40,0xFFB575);
            Rect(BeaconX-7,BeaconY+67,14,21,0xF6EFDF);
            Rect(BeaconX-9,BeaconY+65,18,4,0xD99872);
            Rect(BeaconX-1,BeaconY+59,3,13,0x9B5C58);
            Rect(BeaconX-4,BeaconY+72,8,7,0xC88D6C);
            Text("引路点",BeaconX-19,BeaconY+110,11,0xFFCA9B);
        }
        if (SignalTime>0)
        {
            Panel(447,126,187,55,0x173E44,0x63DFC4);
            Text("信号控场中",489,140,15,0xB7F7E7);
            Text("抓紧带同学通过",493,163,10,0x93D3BC);
        }
        if (Mode==0 && RoadRule>0)
        {
            Panel(16,699,434,34,0x243346,0xD59B66);
            Text(RoadWarning,28,710,12,0xFFCC9F);
        }
    }
}
