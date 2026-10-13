public static partial class Game {
 static void Render(){MotionDrawScene();DrawUiMotion();}
 static void MotionDrawScene(){
  Rect(0,0,1250,900,0x0B1426);
  if(Phase==0){DrawMenu();return;}
  if(Phase==2){DrawChoice();return;}
  if(Phase==4){DrawResult();return;}
  if(Phase==3){DrawPause();return;}
  if(Phase==5){DrawHud();DrawRouteMap();return;}
  if(Phase==6 || Phase==7 || Phase==8){DrawHud();DrawRouteStop();return;}
  if(Phase==9){DrawHud();DrawCollection();return;}
  float shakeX=0;float shakeY=0;
  if(MotionShake>0){float amount=MotionShake*23;shakeX=Sin(MotionRoadClock*91)*amount;shakeY=Cos(MotionRoadClock*117)*amount;}
  BeginWorld(shakeX,shakeY);
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
  DrawMotionEffects();EndWorld();
  DrawHud();DrawInventory();DrawControls();DrawMotionControls();
 }
 static void DrawMotionControls()
 {
  if(Mode==0)
  {
   Panel(1028,812,198,43,0x15243A,0x63DFC4);
   Text("SHIFT  短冲刺",1040,819,12,0xF6EFDF);
   if(P1DashCooldown>0)Text("冷却 "+FloatText(P1DashCooldown)+" 秒",1040,839,10,0xA0AEC2);
   else Text("18 体力 · 牵手时一起冲",1040,839,10,0x63DFC4);
   if(NearMissFlash>0)Text("惊险闪避！+15 分 / +2 零钱",681,866,12,0xFFB575);
   else Text("本路段惊险闪避  "+N(StageNearMisses)+" 次",681,866,11,0xA0AEC2);
  }
  else
  {
   Panel(627,812,45,43,0x15243A,0x63DFC4);Text("SHIFT",631,820,10,0xF6EFDF);
   if(P1DashCooldown>0)Text(FloatText(P1DashCooldown),635,839,9,0xA0AEC2);else Text("短冲",636,839,9,0x63DFC4);
  }
  string volume="V 声音开启";if(MotionMuted==1)volume="V 已静音";
  Text(volume,625,888,9,0xA0AEC2);
 }
 static void DrawPause(){
  DrawShade();PageBrand("暂停 / 歇一口气",0x63DFC4);
  Panel(180,145,890,610,0x15243A,0x2B425D);
  Text("抬头看路，稍等一下。",224,178,32,0xF6EFDF);
  Text("P / ESC / ENTER 继续旅途   ·   M 返回菜单",228,242,14,0x63DFC4);
  Line(226,289,1024,289,0x2B425D);
  Text("1P / 护送员",226,320,22,0x63DFC4);
  Text("WASD 移动 · 空格牵手 · E 口哨 · SHIFT 短冲刺",226,365,16,0xF6EFDF);
  Text("Q 慢时钟 · F 反光伞 · R 续命奶茶 · V 开关声音 · F11 全屏",226,404,14,0xA0AEC2);
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
}
