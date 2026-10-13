// Live road presentation. Visual offsets do not move collision geometry.
public static partial class Game
{
    static void MotionLimb(float ax,float ay,float bx,float by,float width,int color)
    {
        for(int step=0;step<5;step=step+1)
        {
            float t=step*.25;
            Circle(ax+(bx-ax)*t-width/2,ay+(by-ay)*t-width/2,width,width,color);
        }
    }

    static void DrawRoad()
    {
        Rect(0,80,1080,660,0x17283E);
        Art(1,0,80,1080,660);
        for(int x=0;x<1080;x=x+72)
        {
            Line(x,80,x,197,0x233B52);
            Line(x,608,x,740,0x233B52);
        }
        for(int y=90;y<198;y=y+27)Line(0,y,1080,y,0x233B52);
        for(int y=612;y<740;y=y+27)Line(0,y,1080,y,0x233B52);
        Rect(0,194,1080,11,0x0B1426);Rect(0,596,1080,14,0x0B1426);
        Rect(0,197,1080,4,0x728596);Rect(0,601,1080,4,0x728596);
        Rect(0,201,1080,399,0x223347);
        for(int i=0;i<LaneCount;i=i+1)
        {
            float ly=LaneY[i]+80;float lh=LaneH[i];float lt=ly-lh/2;
            int asphalt=0x24364B;if(i%2==1)asphalt=0x293D52;
            Rect(0,lt,1080,lh,asphalt);
            if(i>0){for(int x=4;x<1080;x=x+50)Rect(x,lt,26,2,0x74879A);}
            for(float sy=lt+8;sy<ly+lh/2-5;sy=sy+19)
            {
                Rect(477,sy+2,126,10,0x132338);Rect(477,sy,126,9,0xD7DDDA);
            }
            int direction=LaneDirection[i];
            for(int a=0;a<2;a=a+1)
            {
                float ax=216+a*624;
                Line(ax-direction*25,ly,ax+direction*23,ly,0x6B839B);
                Line(ax+direction*10,ly-10,ax+direction*23,ly,0x6B839B);
                Line(ax+direction*10,ly+10,ax+direction*23,ly,0x6B839B);
                Line(ax-direction*25,ly+2,ax+direction*23,ly+2,0x6B839B);
            }
            // Reflections drift gently; road markings and crossing stay fixed.
            float light=(MotionRoadClock*22+i*163)%1210-60;
            if(direction<0)light=1080-light;
            SoftRect(light,lt+lh*.25,82,3,0xA7D7DB,.05);
            if(SignalTime>0 && SignalLane==i)
            {
                SoftRect(0,lt,1080,lh,0x63DFC4,.08+.025*Sin(MotionRoadClock*3));
                for(int x=36;x<1080;x=x+106)
                {
                    Rect(x,ly-8,3,16,0x72DCC4);Rect(x+7,ly-8,3,16,0x72DCC4);
                }
            }
        }
        for(int i=0;i<MedianCount;i=i+1)
        {
            float my=MedianY[i]+80;
            Rect(0,my-15,1080,31,0x0D1B2D);Rect(0,my-13,1080,26,0x305F60);
            Rect(0,my-13,1080,3,0x78B6A8);Rect(0,my+10,1080,3,0x78B6A8);
            for(int x=16;x<1080;x=x+38)
            {
                Rect(x,my-5,22,10,0x427B6B);Line(x+4,my-4,x+14,my+3,0x619987);
            }
            Rect(461,my-13,158,26,0x63DFC4);
            SoftRect(452,my-17,176,34,0xB1FFDE,.06+.05*Sin(MotionRoadClock*2.1));
            Text("安 全 岛",510,my-8,11,0x122B32);
            float mark=(MotionRoadClock*21+i*17)%158;
            Rect(461+mark,my-12,5,2,0xC6FFE7);
        }
        // City furniture is outside the playable traffic lanes.
        Panel(26,87,182,43,0x243B55,0x38556B);
        for(int window=0;window<5;window=window+1)
        {
            int tint=0x9DB8C8;if(window%2==0)tint=0xFFD38B;
            Rect(37+window*32,96,22,20,tint);
            SoftRect(35+window*32,94,26,24,0xFFDF9E,.04+.02*Sin(MotionClock*.7+window));
        }
        Panel(865,87,186,44,0x263E54,0x426075);
        Text("便利店  24H",910,97,15,0xFFB575);
        Rect(879,126,158,4,0x63DFC4);
        SoftRect(876,124,164,7,0x63DFC4,.09+.03*Sin(MotionClock*1.8));
        DrawTree(276,125);DrawTree(796,125);DrawTree(64,672);DrawTree(1001,680);
        Rect(128,651,124,13,0x466778);Rect(139,664,7,19,0x263C51);Rect(233,664,7,19,0x263C51);
        Line(131,657,248,657,0x8B9EAC);
        Rect(839,662,121,13,0x466778);Rect(851,675,7,18,0x263C51);Rect(942,675,7,18,0x263C51);
        Line(842,668,956,668,0x8B9EAC);
        Panel(447,126,187,55,0x173E44,0x63DFC4);
        Text("↑  对岸安全区",483,140,16,0xB7F7E7);Text("两人到达 · 护送成功",479,164,9,0x84BCAF);
        Text("从这里出发  ↑",490,715,12,0xC5D6E4);
        DrawLight(422,176);DrawLight(663,641);
        if(Mode==0)Text("第 "+N(RouteAct)+" 幕  /  "+RouteNodeName(NodeKind)+"  /  "+Modifier,23,153,10,0xDCE8EE);
        else Text("对抗路段  /  "+Modifier,23,153,11,0xDCE8EE);
        if(Modifier=="雨天")
        {
            // Continuous rain, deterministic visual-only clock, no gameplay RNG.
            for(int drop=0;drop<54;drop=drop+1)
            {
                float rx=(drop*197+MotionRoadClock*49)%1110-15;
                float ry=80+(drop*137+MotionRoadClock*(275+drop%5*19))%660;
                if(ry>96 && ry<719)Line(rx,ry,rx-6,ry+15,0x54718A);
            }
            for(int ripple=0;ripple<6;ripple=ripple+1)
            {
                float age=(MotionRoadClock+ripple*.39)%1.4;
                float rx=126+ripple*154;float ry=644+ripple%2*47;
                Ring(rx,ry,3+age*9,1,0x38566A);
            }
        }
    }

    static void DrawCar(int i)
    {
        float x=CarX[i];float y=CarY[i]+80;float w=CarW[i];float h=CarH[i];
        int direction=CarDirection[i];
        float rate=1;if(Slow>0)rate=.45;
        rate=rate*ItemTrafficRate(i)*RoadTrafficRate(i);
        if(Shield>0 && P1Combo(1,8))
        {
            float dx=CarX[i]-P1X;float dy=CarY[i]-P1Y;
            float ex=CarX[i]-P2X;float ey=CarY[i]-P2Y;
            if(Min(Sqrt(dx*dx+dy*dy),Sqrt(ex*ex+ey*ey))<145)rate=rate*.65;
        }
        float roll=x*.2;float glow=.11+.035*Sin(MotionRoadClock*3+i*.7);
        if(rate>0 && x>-w && x<1080+w)
        {
            float tail=x-direction*(w/2+8);
            float length=Min(42,CarSpeed[i]*.13)*rate;
            for(int streak=0;streak<3;streak=streak+1)
            {
                float yy=y-h*.28+streak*h*.28;
                Line(tail,yy,tail-direction*(length+streak*4),yy,0x496477);
            }
        }
        Circle(x-w/2-1,y-h/2+6,w+6,h+8,0x0D192A);
        if(CarType[i]==2)
        {
            Circle(x-w/2,y-8,15,16,0x0B1426);Circle(x+w/2-15,y-8,15,16,0x0B1426);
            Line(x-w/2+7-4*Cos(roll),y-4-4*Sin(roll),x-w/2+7+4*Cos(roll),y-4+4*Sin(roll),0x698397);
            Line(x+w/2-7-4*Cos(roll),y-4-4*Sin(roll),x+w/2-7+4*Cos(roll),y-4+4*Sin(roll),0x698397);
            Rect(x-w/2+4,y-2,w-8,4,0xA6BDCA);
            Panel(x-11,y-10,24,20,CarColor[i],0xF6EFDF);
            Circle(x-8,y-8,16,16,0xF6D3AD);Circle(x-8,y-8,16,10,0x416784);
            Rect(x-direction*7-9,y-6,10,12,0x567F9D);
            Line(x+direction*8,y-9,x+direction*15,y-9,0xF6EFDF);
            Circle(x+direction*(w/2)-5,y-5,10,10,0xFFDA8C);
        }
        else
        {
            for(int wheel=0;wheel<2;wheel=wheel+1)
            {
                float wx=x-w/2+16+wheel*(w-40);
                Rect(wx-7,y-h/2-3,14,7,0x091320);Rect(wx-7,y+h/2-4,14,7,0x091320);
                float tread=(x*.27+wheel*3)%9;
                if(tread<0)tread=tread+9;
                Rect(wx-5+tread,y-h/2-2,2,5,0x526776);Rect(wx-5+tread,y+h/2-3,2,5,0x526776);
            }
            // The solid car body remains precisely CarW by CarH at the collision center.
            Panel(x-w/2,y-h/2,w,h,CarColor[i],0xB6C6CD);
            Rect(x-w/2+21,y-h/2+4,w-43,h-8,0x35516A);
            Rect(x-w/2+25,y-h/2+7,w-51,h-14,CarColor[i]);
            Rect(x+direction*(w/2-19)-7,y-h/2+5,14,h-10,0x17364A);
            Line(x+direction*(w/2-19)-5,y-h/2+7,x+direction*(w/2-19)+5,y-h/2+13,0x729CA8);
            Rect(x-direction*(w/2-15)-4,y-h/2+7,8,h-14,0x274357);
            Line(x-w/2+16,y-h/2+4,x+w/2-16,y-h/2+4,0xEBE3D3);
            for(int lamp=0;lamp<2;lamp=lamp+1)
            {
                float ly=y-h/2+6+lamp*(h-18);
                float lx=x+direction*(w/2+1);
                Circle(lx-5,ly-3,10,12,0x846F51);
                Rect(x+direction*(w/2-3)-2,ly,4,7,0xFFE6A6);
                Rect(x-direction*(w/2-3)-2,ly,4,7,0xFF6E77);
                float beam=lx;if(direction<0)beam=lx-28;
                SoftRect(beam,ly-2,28,9,0xFFE4A6,glow);
                SoftRect(x-direction*(w/2)-6,ly-1,12,8,0xFF727B,.06);
            }
            if(CarType[i]==1)
            {
                for(float wx=-w/2+27;wx<w/2-26;wx=wx+18)
                {
                    Rect(x+wx,y-h/2+4,11,8,0x183449);Rect(x+wx,y+h/2-12,11,8,0x183449);
                    Line(x+wx+2,y-h/2+6,x+wx+7,y-h/2+6,0x88B1BA);
                }
                Rect(x-20,y-4,41,8,0xE4DFCF);Text("校车",x-12,y-5,8,0x314858);
            }
        }
        if(Slow>0)Outline(x-w/2-3,y-h/2-3,w+6,h+6,0x65B8B3);
    }

    static void DrawPerson(float x,float y,int owner)
    {
        y=y+80;
        float walk=Walk1;float speed=MoveSpeed1;float fx=Face1X;float fy=Face1Y;
        if(owner==1){walk=Walk2;speed=MoveSpeed2;fx=Face2X;fy=Face2Y;}
        float moving=Min(1,speed/85);
        float step=Sin(walk*.13);float swing=step*5*moving;
        float bounce=Abs(Sin(walk*.13))*2.7*moving;
        float breath=Sin(MotionRoadClock*2.6+owner)*.55*(1-moving);
        float by=y-bounce+breath;
        int shirt=0x63DFC4;int edge=0x123E45;int hair=0x20283B;
        if(owner==1){shirt=0xFFB575;edge=0x864C3E;hair=0x3A293D;}
        Circle(x-18,y+7,36,11,0x0A1525);
        // Alternating feet and arms are driven by actual distance, not a looping idle clip.
        MotionLimb(x-6,by+1,x-7+fx*swing*.35,y+11+swing,7,0x122136);
        MotionLimb(x+6,by+1,x+6-fx*swing*.35,y+11-swing,7,0x122136);
        Rect(x-12+fx*swing*.35,y+9+swing,10,5,0xF6EFDF);
        Rect(x+1-fx*swing*.35,y+9-swing,10,5,0xF6EFDF);
        Circle(x-14,by-23,28,31,edge);Rect(x-12,by-18,24,24,shirt);
        float leftHandX=x-15+fx*2;float rightHandX=x+15+fx*2;
        float leftHandY=by-2-swing;float rightHandY=by-2+swing;
        if(owner==1){leftHandX=x-8+fx*4;rightHandX=x+8+fx*4;leftHandY=by-5;rightHandY=by-5;}
        MotionLimb(x-13,by-12,leftHandX,leftHandY,6,0xF2CBA8);
        MotionLimb(x+13,by-12,rightHandX,rightHandY,6,0xF2CBA8);
        MotionLimb(x-13,by-15,x-14,by-9,7,shirt);MotionLimb(x+13,by-15,x+14,by-9,7,shirt);
        Line(x-7,by-18,x-7,by+4,0xECF7E7);Line(x+7,by-18,x+7,by+4,0xECF7E7);
        float hx=x+fx*1.7;float hy=by-27;
        Circle(hx-12,hy-11,25,24,0x172336);
        Circle(hx-11,hy-8,22,22,0xF6D3AF);Circle(hx-12,hy-13,25,17,hair);
        Rect(hx-12,hy-4,5,10,hair);Rect(hx+8,hy-4,5,8,hair);
        if(fy<-.48)
        {
            Circle(hx-11,hy-10,23,21,hair);
            Line(hx-7,hy-7,hx+2,hy-7,0x506076);
        }
        else
        {
            Rect(hx-6+fx*3,hy+3,2,3,0x343249);Rect(hx+4+fx*3,hy+3,2,3,0x343249);
            Line(hx-2+fx*2,hy+8,hx+2+fx*2,hy+8,0xC98775);
        }
        if(owner==1)
        {
            float px=x-7+fx*5;float py=by-12;
            Circle(px-2,py-1,19,25,0x4C6F91);Rect(px,py,15,22,0x132339);
            Rect(px+2,py+2,11,16,0x68CFFF);
            float scroll=(MotionRoadClock*8)%7;
            Rect(px+4,py+3+scroll,7,2,0xE5F9FF);
            Rect(px+4,py+10+scroll*.3,5,2,0xD3F6FF);
            Rect(px+4,py+3,7,1,0x95E2FF);Circle(px+5,py+19,3,2,0xA9D7EF);
            Rect(px-3,py+8+Sin(MotionRoadClock*5)*1.2,5,7,0xF6D3AF);
            Rect(px+13,py+7,4,7,0xF6D3AF);
            SoftRect(px-3,py-2,21,23,0x68CFFF,.04+.02*Sin(MotionRoadClock*2));
            if(Mode==1)Text("2P",x-8,by-60,10,0xFFD8B1);
            else Text("同学",x-12,by-60,10,0xFFD8B1);
        }
        else
        {
            Rect(x-8,by-14,15,17,0x286F68);Rect(x-5,by-12,9,3,0xA5F4DA);
            if(DashTime>0)SoftRect(x-15,by-22,30,26,0xB1FFF0,.18);
            if(Mode==1)Text("1P",x-8,by-60,10,0xA6F5DB);
            else Text("你",x-5,by-60,10,0xA6F5DB);
        }
        if((owner==0 && Arrived1==1)||(owner==1 && Arrived2==1))Text("✓",x+17,by-34,20,0x63DFC4);
        if((owner==0 && Invincible>0)||(owner==1 && BuddyInvincible>0))Ring(x,y-6,28,1,0x79B2B5);
    }

    static void DrawRouteObjects()
    {
        if(RoadRule==2)
        {
            float y=ObstacleY+80;
            Rect(ObstacleX+4,y+5,ObstacleW,ObstacleH,0x0A1425);
            Rect(ObstacleX,y,ObstacleW,ObstacleH,0xDBA365);
            for(float x=ObstacleX+7;x<ObstacleX+ObstacleW-15;x=x+31)Line(x,y+4,x+15,y+ObstacleH-4,0x15243A);
            Outline(ObstacleX,y,ObstacleW,ObstacleH,0xFFD193);
            Rect(ObstacleX+33,y+6,ObstacleW-66,ObstacleH-12,0xE7B77F);
            Text("← 绕行   施工隔离带   绕行 →",ObstacleX+39,y+9,11,0x25384B);
            for(int lamp=0;lamp<3;lamp=lamp+1)
            {
                float lx=ObstacleX+15+lamp*(ObstacleW-30)/2;
                Circle(lx-4,y-6,8,8,0xFFB575);
                SoftRect(lx-9,y-11,18,18,0xFFB575,.12+.05*Sin(MotionRoadClock*3+lamp));
            }
        }
        if(RoadblockTime>0)
        {
            float y=RoadblockY+80;
            float radius=160;if(P1Combo(12,17))radius=220;
            Ring(RoadblockX,y,radius,1,0x41536A);
            Panel(RoadblockX-37,y-12,74,24,0xFFB575,0xFFD59B);
            Line(RoadblockX-31,y-6,RoadblockX-21,y+7,0x26374A);
            Line(RoadblockX+20,y-6,RoadblockX+30,y+7,0x26374A);
            Text("路障",RoadblockX-13,y-6,10,0x26374A);
            for(int lamp=0;lamp<2;lamp=lamp+1)
            {
                float lx=RoadblockX-27+lamp*54;
                Circle(lx-3,y-16,6,6,0xFFE5A8);
                SoftRect(lx-8,y-21,16,16,0xFFC078,.16+.04*Sin(MotionRoadClock*4));
            }
        }
        if(BeaconTime>0)
        {
            float y=BeaconY+80;float pulse=(MotionRoadClock*.85)%1;
            Ring(BeaconX,y,24+pulse*24,1,0x5A9F96);
            Ring(BeaconX,y,24,2,0x63DFC4);
            if(ItemActiveT==15)
            {
                float fy=y-31+Sin(MotionRoadClock*4)*3;
                Circle(BeaconX-16,y-4,32,8,0x172A3A);
                Rect(BeaconX-8,fy-5,16,10,0xD6E4E6);Rect(BeaconX-5,fy-3,10,6,0x63DFC4);
                for(int side=0;side<2;side=side+1)
                {
                    float sx=BeaconX-19+side*38;
                    Line(sx,fy,BeaconX,fy,0xA9BEC9);
                    float spin=Sin(MotionRoadClock*36+side)*10;
                    Line(sx-spin,fy-3,sx+spin,fy-3,0xE1F2EB);
                    Circle(sx-3,fy-3,6,6,0x416978);
                }
                SoftRect(BeaconX-1,fy+5,2,y-fy-4,0x63DFC4,.2);
                Text("无人机引路",BeaconX-29,y+30,10,0xA5EDDA);
            }
            else
            {
                float fy=y+Sin(MotionRoadClock*3)*1.2;
                Circle(BeaconX-18,y-12,36,33,0x286763);
                Rect(BeaconX-7,fy-13,14,21,0xF6EFDF);Rect(BeaconX-9,fy-15,18,4,0xD99872);
                Rect(BeaconX-1,fy-21,3,13,0x9B5C58);Rect(BeaconX-4,fy-8,8,7,0xC88D6C);
                Circle(BeaconX-4,fy+1,3,3,0x805E57);Circle(BeaconX+1,fy+2,3,3,0x805E57);
                Text("奶茶引路",BeaconX-23,y+30,10,0xFFCA9B);
            }
            float dx=BeaconX-P2X;float dy=BeaconY-P2Y;float dist=Sqrt(dx*dx+dy*dy);
            if(dist>35)
            {
                for(int dot=0;dot<5;dot=dot+1)
                {
                    float t=(dot*.2+MotionRoadClock*.34)%1;
                    Circle(P2X+dx*t-2,P2Y+80+dy*t-2,4,4,0x76AE9E);
                }
            }
        }
        if(SignalTime>0)
        {
            Panel(447,126,187,55,0x173E44,0x63DFC4);
            Text("信号控场中",489,140,15,0xB7F7E7);Text("抓紧带同学通过",493,163,10,0x93D3BC);
        }
        if(Mode==0 && RoadRule>0)
        {
            Text(RoadWarning,28,711,12,0xFFCC9F);
        }
    }

    static void MotionWave(float x,float y,float remaining,float duration,int color)
    {
        float progress=1-Min(1,Max(0,remaining/duration));
        Ring(x,y+80,19+progress*72,2,color);
        if(progress>.22)Ring(x,y+80,10+(progress-.22)*67,1,color);
    }

    static void DrawMotionEffects()
    {
        // Trails and particles are simulation-owned; rendering never mutates them.
        for(int trail=0;trail<12;trail=trail+1)
        {
            if(TrailLife[trail]>0)
            {
                float alpha=Min(.23,TrailLife[trail]*.55);
                float tx=TrailX[trail];float ty=TrailY[trail]+80;
                SoftRect(tx-11,ty-24,22,29,0x63DFC4,alpha);
                SoftRect(tx-8,ty-38,16,14,0xB1FFF0,alpha*.6);
                SoftRect(tx-13,ty+10,26,3,0x63DFC4,alpha*.7);
            }
        }
        for(int particle=0;particle<64;particle=particle+1)
        {
            if(FXlife[particle]>0)
            {
                float alpha=Min(1,FXlife[particle]/Max(.001,FXmax[particle]));
                float size=2+alpha*3;
                SoftRect(FXx[particle]-size/2,FXy[particle]+80-size/2,size,size,FXcolor[particle],alpha*.8);
            }
        }
        if(EffectWhistle>0)MotionWave(P1X,P1Y,EffectWhistle,.8,0x91DABF);
        if(EffectShield>0)MotionWave(P1X,P1Y,EffectShield,.7,0x79C9CF);
        if(EffectTea>0)
        {
            MotionWave(P2X,P2Y,EffectTea,.7,0xD9AA7F);
            Text("抬头啦",P2X-21,P2Y+19-(1-Min(1,EffectTea/.7))*16,12,0xFFD4A5);
        }
        if(EffectClock>0)MotionWave(P1X,P1Y,EffectClock,.9,0x89AACB);
        if(EffectGadget>0)MotionWave(P1X,P1Y,EffectGadget,.75,0xC7B588);
        if(Shield>0)
        {
            float sy=P1Y+70;
            Ring(P1X,sy,34+Sin(MotionRoadClock*3)*1.2,2,0x63DFC4);
            for(int spoke=0;spoke<3;spoke=spoke+1)
            {
                float angle=MotionRoadClock*1.8+spoke*2.094;
                float sx=P1X+Cos(angle)*34;float yy=sy+Sin(angle)*34;
                Circle(sx-2,yy-2,4,4,0xBFFFF0);
            }
        }
        if(Slow>0)
        {
            for(int clock=0;clock<4;clock=clock+1)
            {
                float cx=42+clock*264;float cy=219+clock%2*313;
                Ring(cx,cy,10,1,0x58889C);
                Line(cx,cy,cx+Cos(MotionRoadClock*.55)*7,cy+Sin(MotionRoadClock*.55)*7,0x84BBC3);
                Line(cx,cy,cx,cy-5,0x84BBC3);
            }
        }
        if(NearMissFlash>0)
        {
            float progress=1-Min(1,NearMissFlash/.9);
            Text("擦肩而过",FeedbackX-31,FeedbackY+21-progress*18,12,0xA5EAD2);
            Ring(FeedbackX,FeedbackY+80,22+progress*13,1,0x517B75);
        }
        if(SaveFlash>0)
        {
            float progress=1-Min(1,SaveFlash/1.3);
            Text("安全岛 · 喘口气",FeedbackX-45,FeedbackY+24-progress*17,12,0xB5FFE0);
            Ring(FeedbackX,FeedbackY+80,24+progress*27,1,0x63AE9B);
        }
        if(HitFlash>0)
        {
            float progress=1-Min(1,HitFlash/.5);
            SoftRect(0,80,1080,660,0xFFA18A,Min(.085,HitFlash*.17));
            Ring(FeedbackX,FeedbackY+80,22+progress*29,2,0xC58A7C);
        }
    }
}
