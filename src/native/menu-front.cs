// Formal game front end. All visuals retain the menu-flow hit areas.
public static partial class Game
{
 static bool MenuFrontHit(float x,float y,float w,float h)
 {
  return HoverX>=x && HoverX<=x+w && HoverY>=y && HoverY<=y+h;
 }
 static string MenuFrontName(int index)
 {
  if(index==0)return "单人旅程";
  if(index==1)return "本地对抗";
  if(index==2)return "设置";
  if(index==3)return "操作说明";
  return "退出游戏";
 }
 static string MenuFrontHint(int index)
 {
  if(index==0)return "选一条回家的路，带上你的同学。";
  if(index==1)return "一人护送，一人捣蛋，同屏见招拆招。";
  if(index==2)return "调整声音与显示模式。";
  if(index==3)return "先看看怎么牵手、提醒和出招。";
  return "暂时离开这条街。";
 }
 static void DrawMenuTitle()
 {
  Line(100,119,131,119,0x63DFC4);
  Text("放学之后",144,111,11,0xB8CCC9);
  Text("同学，",98,145,55,0xF6EFDF);
  Text("别刷了！",94,215,76,0x7CE2C8);
 }
 static void DrawMenuFooter()
 {
  Text("街景主菜单版  /  0.5.0",100,852,11,0x839AA6);
  if(MenuPage==0)Text("↑ / ↓ 选择    ENTER 确认    鼠标点击",374,852,11,0xA8BDC3);
  else if(MenuPage==1)Text("↑ / ↓ 选择    ENTER 切换    ESC 返回",374,852,11,0xA8BDC3);
  else if(MenuPage==2)Text("ESC 返回    V 声音    F11 全屏",374,852,11,0xA8BDC3);
  else Text("← / → 选择    ENTER 确认    ESC 取消",374,852,11,0xA8BDC3);
 }
 static void DrawMenuNavigation()
 {
  Text("把手机同学，安全带到对岸。",102,329,16,0xBFD0CC);
  for(int i=0;i<5;i=i+1)
  {
   float y=415+60*i;
   bool focused=MenuFocus==i;
   bool hover=MenuFrontHit(100,y,360,48);
   int color=0xA4B8BB;
   if(focused || hover)color=0xF6EFDF;
   if(focused)
   {
    SoftRect(96,y-1,367,50,0x63DFC4,.035);
    float arrow=83+Sin(MotionClock*2.2)*2;
    Line(arrow,y+13,arrow+7,y+18,0x7CE2C8);
    Line(arrow+7,y+18,arrow,y+23,0x7CE2C8);
    Line(100,y+48,345,y+48,0x406B69);
    Text(MenuFrontHint(i),102,y+32,11,0x91C9BC);
   }
   else if(hover)SoftRect(100,y+43,220,1,0xB8E4D6,.55);
   Text(MenuFrontName(i),101,y+1,25,color);
  }
  TextBlock("再看一眼马路。\n再往前，一起回家。",878,775,285,67,18,0xDDE9E3);
 }
 static void DrawMenuPanel(string title,string note)
 {
  RoundFill(102,325,1010,455,0x050D17);
  RoundFill(100,320,1010,455,0x0E1A28);
  SoftRect(100,320,1010,1,0x9BC3BF,.34);
  Text(title,147,350,30,0xF6EFDF);
  Text(note,150,394,13,0xA8BDC3);
 }
 static void DrawMenuReturn()
 {
  bool hover=MenuFrontHit(150,685,160,50);
  int color=0xA8BDC3;
  if(hover)color=0x7CE2C8;
  if(hover)SoftRect(150,685,160,50,0x63DFC4,.07);
  Line(169,709,179,699,color);Line(169,709,179,719,color);
  Line(169,709,193,709,color);
  Text("返回",212,695,20,color);
 }
 static void DrawMenuSettings()
 {
  DrawMenuPanel("设置","让这段回家的路，更合你的习惯。");
  for(int i=0;i<2;i=i+1)
  {
   float y=415+75*i;
   bool active=MenuSettingFocus==i;
   bool hover=MenuFrontHit(160,y,880,60);
   if(active || hover)SoftRect(160,y,880,60,0x63DFC4,.06);
   if(active)Rect(159,y+10,2,40,0x7CE2C8);
  }
  Text("声音",180,431,22,0xF6EFDF);
  Text("动作与菜单的声音反馈",401,438,14,0xA8BDC3);
  string sound="开启";int soundColor=0x63DFC4;float knob=994;
  if(MotionMuted==1){sound="关闭";soundColor=0x45586A;knob=950;}
  Text(sound,864,438,14,0xCDDAD8);
  RoundFill(947,434,70,24,soundColor);
  Circle(knob,436,20,20,0xF6EFDF);
  Text("显示模式",180,506,22,0xF6EFDF);
  Text("F11 也可以随时切换",401,513,14,0xA8BDC3);
  string display="窗口";int displayColor=0x45586A;float displayKnob=950;
  if(MenuFullscreen==1){display="全屏";displayColor=0x63DFC4;displayKnob=994;}
  Text(display,864,513,14,0xCDDAD8);
  RoundFill(947,509,70,24,displayColor);
  Circle(displayKnob,511,20,20,0xF6EFDF);
  Line(180,588,1020,588,0x263B49);
  Text("V  开关声音",180,616,13,0x849FA6);
  Text("F11  窗口 / 全屏",401,616,13,0x849FA6);
  DrawMenuReturn();
  Text("↑ / ↓ 选择设置    ENTER 或 → 切换",401,704,13,0x8EABA7);
 }
 static void DrawMenuHelp()
 {
  DrawMenuPanel("操作说明","看准车流里的空隙。牵好同学，再一起往前走。");
  Line(585,439,585,651,0x263B49);
  Text("护送员",160,438,23,0x7CE2C8);
  Text("WASD  移动 · 按住空格 牵手",161,485,16,0xE0EAE6);
  Text("E  口哨 · Shift  短冲刺",161,518,16,0xE0EAE6);
  Text("Q / F / R  使用已获得的道具",161,551,16,0xE0EAE6);
  Text("单人 T 使用装置 · Tab 轮换",161,585,14,0xA8BDC3);
  Text("B  背包与组合图鉴",161,614,14,0xA8BDC3);
  Text("牵手时一起冲，冲刺仍会撞车。",161,647,12,0x91C9BC);
  Text("手机同学 · 双人",630,438,23,0xFFBE8D);
  Text("方向键  移动 · Enter  挣脱",631,485,16,0xE0EAE6);
  Text("J / K / L  使用已获得的捣蛋道具",631,518,16,0xE0EAE6);
  TextBlock("阻止护送员带双方到对岸。拖到时间结束，或让护送员耗尽生命，你就赢了。",631,556,418,58,14,0xA8BDC3);
  Text("每局 75 秒 · 三局两胜",631,626,13,0xD4B297);
  DrawMenuReturn();
  Text("V 声音    F11 全屏    P / ESC 暂停",401,704,13,0x8EABA7);
 }
 static void DrawMenuQuit()
 {
  RoundFill(327,369,600,285,0x050D17);
  RoundFill(325,364,600,285,0x0E1A28);
  SoftRect(325,364,600,1,0x9BC3BF,.34);
  Text("要离开这条街吗？",390,416,30,0xF6EFDF);
  Text("退出后，当前旅程不会保存。",391,470,16,0xA8BDC3);
  for(int i=0;i<2;i=i+1)
  {
   float x=410+210*i;
   bool selected=MenuQuitFocus==i;
   bool hover=MenuFrontHit(x,555,180,52);
   int fill=0x17293A;int color=0xB8CCC9;
   if(selected || hover){fill=0x203D43;color=0x7CE2C8;}
   if(i==1 && (selected || hover)){fill=0x3B3033;color=0xFFBE8D;}
   RoundFill(x,555,180,52,fill);
   if(selected)Line(x+23,606,x+157,606,color);
   string label="取消";if(i==1)label="退出";
   Text(label,x+64,568,20,color);
  }
  Text("← / → 选择    ENTER 确认    ESC 取消",391,625,12,0x849FA6);
 }
 static void DrawMenu()
 {
  Art(41,0,0,1250,900);
  DrawMenuAtmosphere();
  if(MenuPage!=0)SoftRect(0,0,1250,900,0x050D17,.57);
  DrawMenuTitle();
  if(MenuPage==0)DrawMenuNavigation();
  if(MenuPage==1)DrawMenuSettings();
  if(MenuPage==2)DrawMenuHelp();
  if(MenuPage==3)DrawMenuQuit();
  DrawMenuFooter();
 }
}
