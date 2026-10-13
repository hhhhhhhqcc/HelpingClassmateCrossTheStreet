// Live native UI: motion is cosmetic; every original hit area stays fixed.
public static partial class Game
{
 static float UiMotionTime()
 {
  return MotionClock;
 }
 static bool UiMotionHit(float x,float y,float w,float h)
 {
  return HoverX>=x && HoverX<=x+w && HoverY>=y && HoverY<=y+h;
 }
 static void UiMotionFrame(float x,float y,float w,float h,int color,bool selected)
 {
  float t=UiMotionTime();
  float glow=.12;
  if(selected)glow=.16+.08*(1+Sin(t*2.5));
  SoftRect(x-3,y-3,w+6,2,color,glow);
  SoftRect(x-3,y+h+1,w+6,2,color,glow);
  SoftRect(x-3,y-1,2,h+2,color,glow);
  SoftRect(x+w+1,y-1,2,h+2,color,glow);
  if(selected)
  {
   Line(x-1,y-1,x+18,y-1,color);Line(x-1,y-1,x-1,y+15,color);
   Line(x+w-18,y+h+1,x+w+1,y+h+1,color);Line(x+w+1,y+h-15,x+w+1,y+h+1,color);
   float point=(t*.28)%1.0;
   SoftRect(x+point*(w-36),y-2,36,2,color,.76);
  }
 }
 static void UiMenuCar(float progress,int color,int direction)
 {
  float x=410+progress*930;
  float y=852-progress*155;
  if(direction<0)y=y-43;
  float scale=.68+.32*progress;
  float w=100*scale;
  float h=31*scale;
  SoftRect(x+7,y+h+4,w-5,4,0x050A13,.65);
  RoundFill(x,y,w,h,color);
  RoundFill(x+w*.24,y-h*.40,w*.46,h*.60,color);
  Rect(x+w*.28,y-h*.30,w*.15,h*.37,0xA7CCDA);
  Rect(x+w*.47,y-h*.30,w*.17,h*.37,0xA7CCDA);
  Circle(x+w*.16,y+h*.68,13*scale,13*scale,0x071020);
  Circle(x+w*.70,y+h*.68,13*scale,13*scale,0x071020);
  Circle(x+w*.19,y+h*.77,7*scale,7*scale,0x71849C);
  Circle(x+w*.73,y+h*.77,7*scale,7*scale,0x71849C);
  if(direction>0)
  {
   SoftRect(x+w-2,y+8,22*scale,7*scale,0xF9D591,.22);
   Rect(x+w-6,y+7,4,8,0xF9D591);Rect(x+2,y+7,3,7,0xF49793);
  }
  else
  {
   SoftRect(x-19*scale,y+8,22*scale,7*scale,0xF9D591,.22);
   Rect(x+2,y+7,4,8,0xF9D591);Rect(x+w-5,y+7,3,7,0xF49793);
  }
 }
 static void DrawMenuWorldMotion()
 {
  float t=UiMotionTime();
  float light=.14+.05*(1+Sin(t*1.6));
  SoftRect(978,205,104,8,0xFFB575,.35+light);
  SoftRect(976,213,108,5,0xFFB575,light);
  SoftRect(1008,240,28,90,0xFFCB90,light*.18);
  SoftRect(1018,355,29,7,0xFFCB90,.19+.10*(1+Sin(t*.9)));
  SoftRect(682,298,8,11,0xFFB575,.17+.12*(1+Sin(t*.6)));
  SoftRect(707,330,8,11,0x63DFC4,.15+.08*(1+Sin(t*.8)));
  float markerY=276+Sin(t*2.1)*4;
  Ring(836,markerY+12,10,2,0x63DFC4);
  Line(832,markerY+10,840,markerY+10,0x63DFC4);
  Line(836,markerY+6,836,markerY+14,0x63DFC4);
  Line(836,markerY+25,831,markerY+21,0x63DFC4);
  Line(836,markerY+25,841,markerY+21,0x63DFC4);
  float droneX=811+Sin(t*.55)*43;
  float droneY=181+Cos(t*.72)*12;
  SoftRect(droneX-6,droneY+3,33,6,0x63DFC4,.08);
  Line(droneX-6,droneY+5,droneX+9,droneY+8,0x98CCED);
  Line(droneX+9,droneY+8,droneX+24,droneY+5,0x98CCED);
  Circle(droneX+5,droneY+3,8,8,0x63DFC4);
  float rotor=11+Sin(t*24)*5;
  Line(droneX-6-rotor,droneY,droneX-6+rotor,droneY,0xA6F0E1);
  Line(droneX+24-rotor,droneY,droneX+24+rotor,droneY,0xA6F0E1);
  Ring(droneX-6,droneY,9,1,0x507A89);
  Ring(droneX+24,droneY,9,1,0x507A89);
  UiMenuCar((t*.085+.11)%1.0,0x98CCED,1);
  UiMenuCar((t*.085+.57)%1.0,0xFFB575,1);
  UiMenuCar(1.0-(t*.071+.25)%1.0,0x63DFC4,-1);
  float notice=(t*.7)%1.0;
  SoftRect(1097,372-notice*30,4,3,0x63DFC4,(1.0-notice)*.6);
  SoftRect(1106,374-notice*30,3,2,0x63DFC4,(1.0-notice)*.3);
 }
 static void DrawMenu()
 {
  Art(0,0,0,1250,900);
  DrawMenuWorldMotion();
  PageBrand("放学之后 / 走回家吧",0x63DFC4);
  Text("ROGUELIKE ESCORT",180,135,12,0x63DFC4);
  Text("同学，",176,169,53,0xF6EFDF);
  Text("别刷了！",172,229,72,0x63DFC4);
  Text("把手机同学，安全送到家。",180,327,19,0xF6EFDF);
  Text("选一条回家的路，让你的装备替你想办法。",180,364,13,0xA0AEC2);
  int soloBorder=0x2B425D;int duelBorder=0x2B425D;
  int soloFill=0x15243A;int duelFill=0x15243A;
  if(Mode==0){soloBorder=0x63DFC4;soloFill=0x1B3540;}
  if(Mode==1){duelBorder=0xFFB575;duelFill=0x352D36;}
  if(UiMotionHit(180,410,300,96)){soloBorder=0x63DFC4;soloFill=0x1B3540;}
  if(UiMotionHit(500,410,300,96)){duelBorder=0xFFB575;duelFill=0x352D36;}
  Panel(180,410,300,96,soloFill,soloBorder);
  Keycap("1",197,424,0x63DFC4);Text("单人护送",253,425,20,0xF6EFDF);
  Text("三幕分岔路线 · 24 装备 / 20 组合",198,469,12,0xA0AEC2);
  Panel(500,410,300,96,duelFill,duelBorder);
  Keycap("2",517,424,0xFFB575);Text("双人对抗",573,425,20,0xF6EFDF);
  Text("同屏斗智 · 三局两胜 / 每局 75 秒",518,469,12,0xA0AEC2);
  if(Mode==0 || UiMotionHit(180,410,300,96))UiMotionFrame(180,410,300,96,0x63DFC4,Mode==0);
  if(Mode==1 || UiMotionHit(500,410,300,96))UiMotionFrame(500,410,300,96,0xFFB575,Mode==1);
  Panel(180,532,620,52,0x63DFC4,0x63DFC4);
  Text("ENTER / 出发，过马路",341,544,19,0x0B1426);
  float arrowX=643+Sin(UiMotionTime()*3.1)*4;
  Line(arrowX,558,arrowX+15,558,0x0B1426);
  Line(arrowX+15,558,arrowX+9,552,0x0B1426);
  Line(arrowX+15,558,arrowX+9,564,0x0B1426);
  UiMotionFrame(180,532,620,52,0x63DFC4,UiMotionHit(180,532,620,52));
  Text("1P  WASD 移动 / 空格牵手 / E 口哨 / SHIFT 短冲刺",180,612,13,0xD2DEEA);
  Text("2P  方向键移动 / Enter 挣脱 / J K L 道具",180,641,13,0xA0AEC2);
  Text("规划路线。集齐装备。一起到对岸。",180,714,12,0x63DFC4);
  Panel(840,644,340,126,0x15243A,0x2B425D);
  Text("一边刷视频，一边过马路？",861,665,18,0xF6EFDF);
  TextBlock("别急着冲。牵好你的同学，等车流过去，再带他往前走。",861,698,293,54,13,0xA0AEC2);
  Text("同学，别刷了！ / 街头行动版",42,850,11,0x71859E);
  Text("F11 全屏 · V 声音",586,850,11,0x71859E);
  Text("单人构筑 · 本地双人",1020,850,11,0x71859E);
 }
 static void DrawChoice()
 {
  DrawShade();int color=0x63DFC4;if(ChoiceOwner==1)color=0xFFB575;
  PageBrand("装备选择 / "+N(ChoiceOwner+1)+"P",color);
  string title="给这趟旅途，选一个好开头。";
  if(Mode==0 && RewardContext==2)title="这一段，安全到达。";
  Text(title,144,169,34,0xF6EFDF);
  if(Mode==1)Text("双方依次挑选道具，再进入下一局对抗。",150,233,14,0xA0AEC2);
  else if(RewardContext==2)
  {
   TextBlock(NodeText,150,228,920,54,14,0xA0AEC2);
   Text("装备只获得一次 · 集齐两件，自动激活组合",150,294,12,color);
  }
  else Text("三选一 · 装备只获得一次 · 集齐两件，自动激活组合",150,233,14,0xA0AEC2);
  for(int i=0;i<3;i=i+1)
  {
   float x=150+315*i;int border=0x2B425D;int fill=0x15243A;
   bool hover=UiMotionHit(x,330,290,238);
   if(SelectedChoice==i){border=color;fill=0x1B2D46;}
   else if(hover){border=color;fill=0x1A2C43;}
   Panel(x,330,290,238,fill,border);
   Keycap(N(i+1),x+18,349,color);
   TextBlock(ChoiceTags[i],x+74,354,160,22,11,color);
   int artId=3+Choices[i];if(ChoiceOwner==1)artId=33+Choices[i];
   float age=MotionPhaseAge-i*.055;
   if(age<0)age=0;
   float appear=age/.33;
   if(appear>1)appear=1;
   float enter=1-(1-appear)*(1-appear)*(1-appear);
   float lift=-Sin(appear*3.14159)*6+Sin(UiMotionTime()*1.7+i*.9)*2;
   float size=74*(.62+.38*enter);
   if(Choices[i]>=0)Art(artId,x+227-size*.5,380-size*.5+lift,size,size);
   if(SelectedChoice==i || hover)UiMotionFrame(x,330,290,238,color,SelectedChoice==i);
   if(age<.4)SoftRect(x+2,332+(age/.4)*232,286,2,color,.22*(1-age/.4));
   TextBlock(ChoiceNames[i],x+18,401,254,40,20,0xF6EFDF);
   TextBlock(ChoiceDescriptions[i],x+18,444,254,56,12,0xA0AEC2);
   Line(x+18,510,x+272,510,0x2B425D);
   TextBlock(ChoiceCombos[i],x+18,521,254,40,11,color);
  }
  DrawBagSummary(150,615);
  Panel(150,663,920,58,0x15243A,0x2B425D);
  Text("1 / 2 / 3 直接选择   ·   ← / → 切换   ·   ENTER 确认",172,681,14,color);
  DrawComboLine(150,756);
  Text("挑一件趁手的，让接下来的路更好走。",150,822,12,0x71859E);
 }
 static void DrawRouteMotion()
 {
  float t=UiMotionTime();
  int offset=(RouteAct-1)*18;
  if(RouteRow>0)
  {
   for(int prev=0;prev<3;prev=prev+1)
   {
    if(NodeVisited[offset+(RouteRow-1)*3+prev]==2)
    {
     for(int col=0;col<3;col=col+1)
     {
      if(RouteCanChoose(col) && Abs((float)(col-prev))<=1)
      {
       float fromX=330+prev*295;
       float fromY=620-(RouteRow-1)*78-25;
       float toX=330+col*295;
       float toY=620-RouteRow*78+25;
       float flow=(t*.65+col*.19)%1.0;
       float px=fromX+(toX-fromX)*flow;
       float py=fromY+(toY-fromY)*flow;
       Circle(px-2,py-2,4,4,0x63DFC4);
      }
     }
    }
   }
  }
  for(int col=0;col<3;col=col+1)
  {
   if(RouteCanChoose(col))
   {
    float cx=330+col*295;
    float cy=620-RouteRow*78;
    if(SelectedChoice==col || UiMotionHit(cx-105,cy-25,210,50))
    {
     UiMotionFrame(cx-105,cy-25,210,50,0x63DFC4,SelectedChoice==col);
    }
    if(SelectedChoice==col)
    {
     float dotX=cx+94;
     float dotY=cy-14;
     Ring(dotX,dotY,6+Sin(t*2.4)*1.5,1,0x63DFC4);
    }
   }
  }
 }
 static void DrawStopMotion()
 {
  int color=0xB9AAF1;
  if(Phase==7)color=0xF2CD83;
  if(Phase==8)color=0x63DFC4;
  for(int i=0;i<3;i=i+1)
  {
   bool available=true;
   if(Phase==7 && (ShopBought[i]>0 || ShopItems[i]<0))available=false;
   float x=150+315*i;
   if(available && (SelectedChoice==i || UiMotionHit(x,330,290,238)))UiMotionFrame(x,330,290,238,color,SelectedChoice==i);
  }
  if(Phase==7 && UiMotionHit(435,640,380,56))UiMotionFrame(435,640,380,56,0x63DFC4,true);
 }
 static void DrawCollectionMotion()
 {
  if(UiMotionHit(1060,90,160,55))UiMotionFrame(1060,90,160,55,0x63DFC4,true);
  int pages=(ComboCount(0)+8)/9;
  if(CollectionView==1)pages=2;
  for(int page=0;page<pages;page=page+1)
  {
   float x=471+99*page;
   if(CollectionPage==page || UiMotionHit(x,813,79,37))UiMotionFrame(x,813,79,37,0x63DFC4,CollectionPage==page);
  }
 }
 static void DrawUiMotion()
 {
  if(Phase==5)DrawRouteMotion();
  if(Phase==6 || Phase==7 || Phase==8)DrawStopMotion();
  if(Phase==9)DrawCollectionMotion();
  if(Phase==4)
  {
   if(UiMotionHit(280,510,250,50))UiMotionFrame(280,510,250,50,0x63DFC4,true);
   if(UiMotionHit(570,510,250,50))UiMotionFrame(570,510,250,50,0x98CCED,true);
  }
  if(Phase!=1 && MotionPhaseAge<.20)
  {
   float age=MotionPhaseAge;
   if(age<0)age=0;
   SoftRect(0,80,1250,820,0x0B1426,.30*(1-age/.20));
  }
 }
}
