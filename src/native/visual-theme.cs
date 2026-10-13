// Unified visual language for the packaged native game.
public static partial class Game
{
 static void RoundFill(float x,float y,float w,float h,int color)
 {
  float r=10;
  if(w<20 || h<20){Rect(x,y,w,h,color);return;}
  Rect(x+r,y,w-2*r,h,color);Rect(x,y+r,w,h-2*r,color);
  Circle(x,y,2*r,2*r,color);Circle(x+w-2*r,y,2*r,2*r,color);
  Circle(x,y+h-2*r,2*r,2*r,color);Circle(x+w-2*r,y+h-2*r,2*r,2*r,color);
 }
 static void Panel(float x,float y,float w,float h,int fill,int border)
 {
  RoundFill(x+2,y+4,w,h,0x070D18);
  RoundFill(x,y,w,h,border);
  RoundFill(x+1,y+1,w-2,h-2,fill);
 }
 static void Keycap(string key,float x,float y,int color)
 {
  Panel(x,y,42,24,0x1B2D46,0x34516B);
  TextBlock(key,x+3,y+3,36,20,12,color);
 }
 static void PageBrand(string label,int color)
 {
  Rect(0,0,1250,80,0x0B1426);
  Panel(42,21,36,36,color,color);
  Line(51,32,67,32,0x0B1426);Line(51,39,67,39,0x0B1426);Line(51,46,67,46,0x0B1426);
  Text("同学，别刷了！",92,22,21,0xF6EFDF);
  Text("都市漫游",265,31,11,0xA0AEC2);
  Text(label,915,30,12,color);
  Line(42,79,1208,79,0x2B425D);
 }
 static void DrawShade()
 {
  Rect(0,80,1250,820,0x0B1426);
  Art(2,0,80,1250,820);
 }
 static void Render()
 {
  Rect(0,0,1250,900,0x0B1426);
  if(Phase==0){DrawMenu();return;}
  if(Phase==2){DrawChoice();return;}
  if(Phase==4){DrawResult();return;}
  if(Phase==3){DrawPause();return;}
  if(Phase==5){DrawHud();DrawRouteMap();return;}
  if(Phase==6 || Phase==7 || Phase==8){DrawHud();DrawRouteStop();return;}
  if(Phase==9){DrawHud();DrawCollection();return;}
  DrawRoad();DrawRouteObjects();
  for(int i=0;i<96;i=i+1){if(CarActive[i]==1)DrawCar(i);}
  if(Linked==1){Line(P1X+11,P1Y+71,P2X-11,P2Y+71,0x63DFC4);Line(P1X+11,P1Y+73,P2X-11,P2Y+73,0x63DFC4);}
  if(P1Y<P2Y){DrawPerson(P1X,P1Y,0);DrawPerson(P2X,P2Y,1);}else{DrawPerson(P2X,P2Y,1);DrawPerson(P1X,P1Y,0);}
  if(Shield>0){Outline(P1X-34,P1Y+29,68,69,0x63DFC4);Text("反光伞",P1X-27,P1Y+9,12,0xF6EFDF);}
  if(BubbleTime>0){float bx=P2X-110;if(bx<8)bx=8;if(bx>852)bx=852;float by=P2Y-12;if(by<87)by=87;Panel(bx,by,220,30,0xF6EFDF,0xFFB575);TextBlock(Bubble,bx+10,by+4,200,24,12,0x23334B);}
  if(Slow>0)Text("慢时生效",29,145,13,0x63DFC4);
  if(Force>0)Text("听话中",944,145,13,0x63DFC4);
  if(EscapeTime>0)Text("2P 挣脱中",28,174,13,0xFFB575);
  if(Stun2>0)Text("晕！",P2X-12,P2Y+6,16,0xFFB575);
  if(FocusTime>0){Outline(P2X-25,P2Y+34,50,52,0xFFB575);Text("耳机 "+N(FocusCharges)+" 次",P2X+30,P2Y+46,11,0xF6EFDF);}
  if(Mood>0 && Mode==0)Text("分心！",P2X-18,P2Y+7,12,0xFFB575);
  DrawHud();DrawInventory();DrawControls();
 }
 static void DrawMenu()
 {
  Art(0,0,0,1250,900);
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
  Panel(180,410,300,96,soloFill,soloBorder);
  Keycap("1",197,424,0x63DFC4);Text("单人护送",253,425,20,0xF6EFDF);
  Text("三幕分岔路线 · 24 装备 / 20 组合",198,469,12,0xA0AEC2);
  Panel(500,410,300,96,duelFill,duelBorder);
  Keycap("2",517,424,0xFFB575);Text("双人对抗",573,425,20,0xF6EFDF);
  Text("同屏斗智 · 三局两胜 / 每局 75 秒",518,469,12,0xA0AEC2);
  Panel(180,532,620,52,0x63DFC4,0x63DFC4);
  Text("ENTER / 出发，过马路  →",354,544,19,0x0B1426);
  Text("1P  WASD 移动 / 空格牵手 / E 口哨",180,612,13,0xD2DEEA);
  Text("2P  方向键移动 / Enter 挣脱 / J K L 道具",180,641,13,0xA0AEC2);
  Text("规划路线。集齐装备。一起到对岸。",180,714,12,0x63DFC4);
  Panel(840,644,340,126,0x15243A,0x2B425D);
  Text("一边刷视频，一边过马路？",861,665,18,0xF6EFDF);
  TextBlock("别急着冲。牵好你的同学，等车流过去，再带他往前走。",861,698,293,54,13,0xA0AEC2);
  Text("同学，别刷了！ / 都市漫游版",42,850,11,0x71859E);
  Text("单人构筑 · 本地双人",1020,850,11,0x71859E);
 }
 static void DrawMenuArt(){}
 static void DrawChoice()
 {
  DrawShade();int color=0x63DFC4;if(ChoiceOwner==1)color=0xFFB575;
  PageBrand("装备选择 / "+N(ChoiceOwner+1)+"P",color);
  Text("给这趟旅途，选一个好开头。",144,169,34,0xF6EFDF);
  if(Mode==1)Text("双方依次挑选道具，再进入下一局对抗。",150,233,14,0xA0AEC2);
  else Text("三选一 · 装备只获得一次 · 集齐两件，自动激活组合",150,233,14,0xA0AEC2);
  for(int i=0;i<3;i=i+1)
  {
   float x=150+315*i;int border=0x2B425D;int fill=0x15243A;
   if(SelectedChoice==i){border=color;fill=0x1B2D46;}
   Panel(x,330,290,238,fill,border);
   Keycap(N(i+1),x+18,349,color);
   TextBlock(ChoiceTags[i],x+74,354,160,22,11,color);
   int artId=3+Choices[i];if(ChoiceOwner==1)artId=33+Choices[i];
   if(Choices[i]>=0)Art(artId,x+190,343,74,74);
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
 static void DrawPause()
 {
  DrawShade();PageBrand("暂停 / 歇一口气",0x63DFC4);
  Panel(180,145,890,610,0x15243A,0x2B425D);
  Text("抬头看路，稍等一下。",224,178,32,0xF6EFDF);
  Text("P / ESC / ENTER 继续旅途   ·   M 返回菜单",228,242,14,0x63DFC4);
  Line(226,289,1024,289,0x2B425D);
  Text("1P / 护送员",226,320,22,0x63DFC4);
  Text("WASD 移动   ·   按住空格牵手   ·   E 口哨",226,365,16,0xF6EFDF);
  Text("Q 慢时钟   ·   F 反光伞   ·   R 续命奶茶",226,404,14,0xA0AEC2);
  if(Mode==1)
  {
   Text("2P / 手机同学",226,478,22,0xFFB575);
   Text("方向键移动   ·   ENTER 挣脱   ·   J 假通知",226,523,15,0xF6EFDF);
   Text("K 横滑闪避   ·   L 沉浸耳机",226,559,14,0xA0AEC2);
   TextBlock("拖到倒计时结束，或者让 1P 耗尽生命，即可拿下这一局。",226,604,756,42,13,0xA0AEC2);
  }
  else
  {
   Text("T 使用装置   ·   TAB 轮换   ·   B 图鉴与背包",226,483,17,0x63DFC4);
   TextBlock("在地图规划路线，沿途收集装备。组合会自动生效，可以同时使用多套搭配。遇到乱走的同学，靠近牵住他，或者用口哨和装置引导。",226,531,756,85,14,0xA0AEC2);
  }
  DrawBagSummary(226,667);DrawComboLine(226,701);
  Text("点击面板也可以继续。",226,803,12,0x71859E);
 }
 static void DrawResult()
 {
  DrawShade();PageBrand("旅途记录 / 这一趟结束了",0x63DFC4);
  Panel(180,144,890,608,0x15243A,0x2B425D);
  int color=0x63DFC4;if(Winner!=1)color=0xFFB575;
  Text("这一趟，辛苦啦。",227,183,14,color);
  TextBlock(EndTitle,221,231,795,66,38,0xF6EFDF);
  TextBlock(EndText,226,315,780,58,15,0xA0AEC2);
  Panel(226,390,780,80,0x1B2D46,0x2B425D);
  if(Mode==0)Text("第 "+N(RouteAct)+" 幕      得分 "+N(Score)+"      零钱 "+N(Coins),255,414,23,color);
  else Text("最终比分      "+N(Score1)+" : "+N(Score2),407,414,26,color);
  Panel(280,510,250,50,0x63DFC4,0x63DFC4);Text("ENTER / 再来一局",313,523,17,0x0B1426);
  Panel(570,510,250,50,0x1B2D46,0x2B425D);Text("M / 返回菜单",626,523,17,0xF6EFDF);
  DrawComboLine(226,612);
  Text("ENTER 重试相同路线   ·   N 换一趟随机旅途",226,657,13,0xA0AEC2);
  Text("下次，把同学再往前带一步。",226,699,12,color);
 }
}
