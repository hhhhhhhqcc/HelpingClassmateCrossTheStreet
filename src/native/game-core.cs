
using System;
using System.Drawing;
using System.Windows.Forms;

// Native game logic: compiled to CIL, executed inside the WinForms game window.
// No browser, HTML, WebView, JavaScript interpreter, or external assets.
public partial class GameForm : Form {
  static int Mode, Phase, Level, Round, Lives, Score, Best, Seed, Score1, Score2, ChoiceOwner, SelectedChoice, Winner, Linked;
  static int LaneCount, CarCount, MedianCount, PendingRound, Tired, GraceUsed, Mood, Arrived1, Arrived2, FocusCharges, StageHits;
  static float P1X, P1Y, P2X, P2Y, Stamina, MaxStamina, Energy, MaxEnergy, TimeLeft, Elapsed, Shield, Slow, Force, ForceLock;
  static float EscapeTime, Stun2, Invincible, BuddyInvincible, FocusTime, BurstTime, BurstX, BurstY, GraceTime, RegenDelay;
  static float WhistleCD, ClockCD, ShieldCD, TeaCD, EscapeCD, NotificationCD, DashCD, FocusCD;
  static float MoodWait, MoodTimer, MoodDX, Checkpoint, BubbleTime;
  static float[] CarX, CarY, CarW, CarH, CarSpeed, LaneY, LaneH, LaneSpeed, LaneTimer, MedianY;
  static int[] CarActive, CarDirection, CarColor, CarType, CarLane, LaneDirection, P1Items, P2Items, Choices, Input, PreviousInput;
  static string[] ChoiceNames, ChoiceTags, ChoiceDescriptions, ChoiceCombos;
  static string P1Inventory, P2Inventory, ComboText, StatusText, EndTitle, EndText, Bubble, Modifier;

  static int ToInt(float value) { return (int)value; }
  static int MaxInt(int a,int b) { if(a>b)return a;return b; }
  static bool Pressed(int key) { return Input[key] == 1 && PreviousInput[key] == 0; }
  static bool Held(int key) { return Input[key] == 1; }
  static float Dist() { float dx=P1X-P2X;float dy=P1Y-P2Y;return Sqrt(dx*dx+dy*dy); }
  static bool P1Combo(int a,int b) { return P1Items[a]>0 && P1Items[b]>0; }
  static bool P2Combo(int a,int b) { return P2Items[a]>0 && P2Items[b]>0; }

  static void GameInitialize() {
    Mode=0;Phase=0;Level=1;Round=1;Seed=NewSeed();Lives=4;
    MaxStamina=100;Stamina=100;MaxEnergy=100;Energy=100;TimeLeft=75;
    P1Inventory="过关后，三选一道具";P2Inventory="赛前和局间，各选一道具";ComboText="";
    Bubble="";StatusText="选择模式，按 Enter 出发";EndTitle="";EndText="";
    SetupLevel();
    Invincible=0;
  }
  static void ResetInputs() {
    for(int k=0;k<256;k++){Input[k]=0;if(Key(k))Input[k]=1;PreviousInput[k]=Input[k];}
  }
  static void StartGame(bool newSeed) {
    if(newSeed)Seed=NewSeed();
    for(int i=0;i<12;i++)P1Items[i]=0;
    for(int j=0;j<8;j++)P2Items[j]=0;
    Level=1;Round=1;Score1=0;Score2=0;Score=0;Lives=4;Winner=0;PendingRound=0;
    SetupLevel();Phase=1;ResetInputs();UpdateInventory();
    if(Mode==1){ChoiceOwner=0;MakeChoices();Phase=2;}
    else {Bubble="一起到对岸！空格牵手，E 口哨。";BubbleTime=3;}
  }
  static void Menu() {
    Phase=0;Level=1;Round=1;Lives=4;
    for(int i=0;i<12;i++)P1Items[i]=0;
    for(int j=0;j<8;j++)P2Items[j]=0;
    SetupLevel();ResetInputs();UpdateInventory();
    StatusText="1 单人护送 / 2 双人对抗 · Enter 开始";
  }
  static void SetupLevel() {
    SetRandom(Seed+Level*1009);
    LaneCount=3+ToInt(Level*.5);
    if(Mode==1)LaneCount=2+Round;
    if(LaneCount>6)LaneCount=6;
    MedianCount=ToInt((LaneCount-1)*.5);
    float laneHeight=(400.0-MedianCount*28)/LaneCount;
    float y=120;int mi=0;
    for(int i=0;i<LaneCount;i++){
      LaneY[i]=y+laneHeight/2;LaneH[i]=laneHeight;
      LaneDirection[i]=1;if(i%2==1)LaneDirection[i]=-1;
      LaneSpeed[i]=110+Level*12+i*7+Rand()*25;
      LaneTimer[i]=.8+Rand()*1.7;y+=laneHeight;
      if(i%2==1 && i<LaneCount-1){MedianY[mi]=y+14;mi++;y+=28;}
    }
    for(int c=0;c<96;c++)CarActive[c]=0;
    CarCount=0;
    for(int l=0;l<LaneCount;l++){
      float x=-110+Rand()*180;
      while(x<1250){SpawnCar(l,x);x+=250+Rand()*200-Level*7;}
    }
    P1X=519;P1Y=596;P2X=559;P2Y=607;
    Arrived1=0;Arrived2=0;Checkpoint=594;Elapsed=0;TimeLeft=75;StageHits=0;
    MaxStamina=100+P1Items[7]*30;Stamina=MaxStamina;
    MaxEnergy=100+P2Items[4]*25;Energy=MaxEnergy;
    if(Mode==1)Lives=4+P1Items[8];
    Shield=0;Slow=0;Force=0;ForceLock=0;Invincible=1.1;BuddyInvincible=0;
    EscapeTime=0;Stun2=0;FocusTime=0;FocusCharges=0;BurstTime=0;BurstX=0;BurstY=1;
    WhistleCD=0;ClockCD=0;ShieldCD=0;TeaCD=0;EscapeCD=0;NotificationCD=0;DashCD=0;FocusCD=0;
    Linked=0;Tired=0;GraceUsed=0;GraceTime=0;RegenDelay=0;
    Mood=0;MoodTimer=0;MoodWait=3.3+Rand()*2;MoodDX=1;Bubble="";BubbleTime=0;
    Modifier="正常车流";
    int variant=RandomInt(0,4);
    if(Level==1)Modifier="练习路段";
    else if(Level==4 || variant==1)Modifier="雨天";
    else if(Level==6)Modifier="外卖穿行";
    else if(Level==8 || variant==2)Modifier="上头连播";
    else if(variant==3)Modifier="早高峰";
  }
  static void SpawnCar(int lane,float x) {
    int slot=-1;
    for(int i=0;i<96;i++){if(CarActive[i]==0 && slot<0)slot=i;}
    if(slot<0)return;
    int kind=0;
    if(Level>=3 && Rand()<.14)kind=1;
    if(Level>=6 && Rand()<.20)kind=2;
    CarType[slot]=kind;CarActive[slot]=1;CarLane[slot]=lane;
    CarX[slot]=x;CarY[slot]=LaneY[lane];CarDirection[slot]=LaneDirection[lane];
    CarW[slot]=70+Rand()*18;CarH[slot]=Min(44,LaneH[lane]*.55);CarSpeed[slot]=LaneSpeed[lane];
    if(kind==1){CarW[slot]=126;CarSpeed[slot]*=.78;}
    if(kind==2){CarW[slot]=39;CarH[slot]=22;CarSpeed[slot]*=1.16;}
    int col=RandomInt(0,6);CarColor[slot]=0xF09A5C;
    if(col==1)CarColor[slot]=0x64AAA9;
    if(col==2)CarColor[slot]=0xC7CCDB;
    if(col==3)CarColor[slot]=0xE2D58A;
    if(col==4)CarColor[slot]=0xA6A0CF;
    if(col==5)CarColor[slot]=0xCC7186;
    CarCount++;
  }
  static void Traffic(float dt) {
    for(int l=0;l<LaneCount;l++){
      LaneTimer[l]-=dt;
      if(LaneTimer[l]<=0){
        float start=-145;if(LaneDirection[l]<0)start=1225;
        bool free=true;
        for(int c=0;c<96;c++){if(CarActive[c]==1&&CarLane[c]==l&&Abs(CarX[c]-start)<215)free=false;}
        if(free)SpawnCar(l,start);
        LaneTimer[l]=1.9+Rand()*1.25-Level*.048;
        if(Modifier=="早高峰")LaneTimer[l]*=.8;
      }
    }
    for(int i=0;i<96;i++){
      if(CarActive[i]==1){
        float rate=1;if(Slow>0)rate=.45;
        if(Shield>0 && P1Combo(1,8)){
          float d1=Sqrt((CarX[i]-P1X)*(CarX[i]-P1X)+(CarY[i]-P1Y)*(CarY[i]-P1Y));
          float d2=Sqrt((CarX[i]-P2X)*(CarX[i]-P2X)+(CarY[i]-P2Y)*(CarY[i]-P2Y));
          if(Min(d1,d2)<145)rate*=.65;
        }
        CarX[i]+=CarDirection[i]*CarSpeed[i]*rate*dt;
        if(CarX[i]<-220 || CarX[i]>1300){CarActive[i]=0;CarCount--;}
      }
    }
  }
  static bool Touches(float x,float y,int car) {
    return Abs(x-CarX[car])<CarW[car]/2+7 && Abs(y-6-CarY[car])<CarH[car]/2+7;
  }
  static void HitP1() {
    if(Shield>0){Shield=0;Invincible=1.2+P1Items[10]*.5;Bubble="反光伞挡住了！";BubbleTime=2;return;}
    Lives--;StageHits++;Invincible=2+P1Items[10]*.5;
    if(Lives<=0){if(Mode==1)RoundEnd(2);else EndGame(2);return;}
    P1X=519;P1Y=Checkpoint;P2X=557;P2Y=Min(626,Checkpoint+8);
    Arrived1=0;Arrived2=0;Mood=0;MoodWait=5;
    Force=2;if(Mode==1)Force=0;
    Bubble="哎哟！回到最近的安全岛。";BubbleTime=2.5;
  }
  static void HitP2() {
    Energy=Max(0,Energy-25);Stun2=.7;BuddyInvincible=1.6;EscapeTime=0;BurstTime=0;
    Bubble="撞懵了！捣蛋能量 -25";BubbleTime=2;
  }
  static void Whistle() {
    if(Phase!=1 || WhistleCD>0 || Dist()>260+60*P1Items[5])return;
    WhistleCD=7*(1-Min(.45,.15*P1Items[5]));
    if(Mode==1)WhistleCD=8*(1-Min(.45,.15*P1Items[5]));
    if(Mode==1 && FocusTime>0 && FocusCharges>0){
      FocusCharges--;if(FocusCharges==0)FocusTime=0;
      Bubble="耳机挡住了口哨！";BubbleTime=2;return;
    }
    Force=Max(Force,2.8);
    if(Mode==1){Force=2+.15*P1Items[6];ForceLock=.75;EscapeTime=0;BurstTime=0;}
    Mood=0;MoodWait=4+Rand()*4;Bubble="来了！知道啦。";BubbleTime=2;
  }
  static void P1Skill(int id) {
    if(Phase!=1 || P1Items[id]==0)return;
    if(id==0 && ClockCD<=0){
      Slow=3;if(P1Combo(0,4))Slow=4;
      ClockCD=18*(1-.15*(P1Items[0]-1));Bubble="滴答——车流慢下来了。";BubbleTime=2;
    }
    if(id==1 && ShieldCD<=0){
      Shield=4+.6*(P1Items[1]-1);ShieldCD=16*(1-.15*(P1Items[1]-1));
      Bubble="反光伞，撑开！";BubbleTime=2;
    }
    if(id==2 && TeaCD<=0){
      Stamina=Min(MaxStamina,Stamina+55);Force=Max(Force,3);
      if(P1Combo(2,6))Force=Max(Force,6);
      TeaCD=20*(1-.15*(P1Items[2]-1));Tired=0;Mood=0;MoodWait=5;
      Bubble="这口奶茶值得抬头。";BubbleTime=2;
    }
  }
  static void P2Skill(int id) {
    if(Mode!=1 || Phase!=1 || Stun2>0 || ForceLock>0)return;
    if(id>0 && P2Items[id-1]==0)return;
    float cost=42;float cd=EscapeCD;
    if(id==1){cost=30;cd=NotificationCD;}
    if(id==2){cost=28;cd=DashCD;}
    if(id==3){cost=35;cd=FocusCD;}
    if(cd>0 || Energy<cost)return;
    if(id==1 && Dist()>180+40*P2Items[6])return;
    Energy-=cost;
    if(id==0){
      EscapeTime=Max(.7,1.1+.12*P2Items[3]-.1*P1Items[6]);
      Force=0;ForceLock=0;EscapeCD=8*(1-.12*P2Items[3]);Bubble="别拉！我自己走。";
    }
    if(id==1){
      float drain=24+4*(P2Items[0]-1)+4*P2Items[6];
      if(P2Combo(0,6)){drain+=10;Energy=Min(MaxEnergy,Energy+15);}
      Stamina=Max(0,Stamina-drain);
      NotificationCD=12*(1-.12*(P2Items[0]-1));Bubble="叮！99+ 条新消息。";
    }
    if(id==2){
      float dx=0;float dy=0;
      if(Held(39))dx++;if(Held(37))dx--;if(Held(40))dy++;if(Held(38))dy--;
      float len=Sqrt(dx*dx+dy*dy);if(len>0){BurstX=dx/len;BurstY=dy/len;}
      BurstTime=.28;EscapeTime=Max(EscapeTime,.65);Force=0;
      DashCD=10*(1-.12*(P2Items[1]-1));
      if(P2Combo(1,3)){EscapeTime=Max(EscapeTime,.9);DashCD*=.8;}
      Bubble="我滑——";
    }
    if(id==3){
      FocusTime=4;FocusCharges=1;
      if(P2Combo(2,4)){FocusTime=6;FocusCharges=2;}
      FocusCD=15*(1-.12*(P2Items[2]-1));Bubble="听不见，我戴耳机了。";
    }
    BubbleTime=2;
  }
  static void MoveP2(float dt,float p1Speed) {
    float ox=P2X;float oy=P2Y;float dx=0;float dy=0;
    if(Held(39))dx++;if(Held(37))dx--;if(Held(40))dy++;if(Held(38))dy--;
    float len=Sqrt(dx*dx+dy*dy);
    if(len>0){dx/=len;dy/=len;BurstX=dx;BurstY=dy;}
    if(Stun2>0){dx=0;dy=0;}
    if(Stun2>0 || ((Linked==1 || Force>0) && EscapeTime<=0)){
      float ty=P1Y+25;if(Arrived1==1)ty=76;
      float ax=P1X-P2X;float ay=ty-P2Y;float d=Sqrt(ax*ax+ay*ay);
      float speed=200;if(Linked==1)speed=p1Speed+115;
      if(d>10){float step=Min(d-10,speed*dt);P2X+=ax/d*step;P2Y+=ay/d*step;}
      if(ForceLock<=0 && Stun2<=0){P2X+=dx*30*dt;P2Y+=dy*30*dt;}
    }else{
      float speed=145;if(EscapeTime>0)speed*=1.12;
      P2X+=dx*speed*dt;P2Y+=dy*speed*dt;
    }
    if(BurstTime>0 && Stun2<=0){
      float burst=245*(1-Min(.45,.15*P1Items[11]));
      P2X+=BurstX*burst*dt;P2Y+=BurstY*burst*dt;
    }
    float dist=Dist();
    if(dist>210){float pull=Min(dist-210,(220+(dist-210)*4)*dt);P2X+=(P1X-P2X)/dist*pull;P2Y+=(P1Y-P2Y)/dist*pull;}
    P2X=Min(1042,Max(38,P2X));P2Y=Min(626,Max(62,P2Y));
  }
  static void MoveAI(float dt,float p1Speed) {
    if(Arrived2==1)return;
    if(Linked==0 && Force<=0){
      MoodWait-=dt;
      if(MoodWait<=0 && Mood==0){
        Mood=RandomInt(1,4);MoodTimer=.8+Rand()*.7+Level*.035;MoodDX=1;
        if(Rand()<.5)MoodDX=-1;
        Bubble="等一下！再刷一条。";BubbleTime=2;
      }
    }
    if(Mood>0){
      MoodTimer-=dt;
      if(MoodTimer<=0){
        Mood=0;MoodWait=(5.5+Rand()*3.8)/(1+Level*.025)/(1-Min(.5,.2*P1Items[6]));
        if(Modifier=="上头连播")MoodWait*=.65;
      }
    }
    if(Linked==1 || Force>0 || Mood==0){
      float tx=P1X;float ty=P1Y+25;if(Arrived1==1)ty=76;
      float dx=tx-P2X;float dy=ty-P2Y;float d=Sqrt(dx*dx+dy*dy);
      float speed=155*(1+.1*P1Items[4]);
      if(Force>0)speed=195*(1+.1*P1Items[4]);
      if(Linked==1)speed=p1Speed+115;
      else if(Slow>0 && P1Combo(0,4))speed*=1.2;
      if(Linked==0 && Dist()>230)speed*=.55;
      if(d>10){float step=Min(d-10,speed*dt);P2X+=dx/d*step;P2Y+=dy/d*step;}
    }else if(Mood!=1){
      float drift=1-Min(.65,.25*P1Items[11]);P2X+=MoodDX*31*drift*dt;
      if(Mood==3)P2Y+=27*drift*dt;
    }
    P2X=Min(1042,Max(38,P2X));P2Y=Min(626,Max(62,P2Y));
  }
  static void StepGame(float dt) {
    Elapsed+=dt;TimeLeft=Max(0,75-Elapsed);
    Slow=Max(0,Slow-dt);Shield=Max(0,Shield-dt);Force=Max(0,Force-dt);Invincible=Max(0,Invincible-dt);
    ForceLock=Max(0,ForceLock-dt);EscapeTime=Max(0,EscapeTime-dt);Stun2=Max(0,Stun2-dt);
    BuddyInvincible=Max(0,BuddyInvincible-dt);FocusTime=Max(0,FocusTime-dt);BurstTime=Max(0,BurstTime-dt);
    GraceTime=Max(0,GraceTime-dt);RegenDelay=Max(0,RegenDelay-dt);BubbleTime=Max(0,BubbleTime-dt);
    if(FocusTime<=0)FocusCharges=0;
    WhistleCD=Max(0,WhistleCD-dt);ClockCD=Max(0,ClockCD-dt);ShieldCD=Max(0,ShieldCD-dt);TeaCD=Max(0,TeaCD-dt);
    EscapeCD=Max(0,EscapeCD-dt);NotificationCD=Max(0,NotificationCD-dt);DashCD=Max(0,DashCD-dt);FocusCD=Max(0,FocusCD-dt);
    if(Mode==1 && Stun2<=0)Energy=Min(MaxEnergy,Energy+8*(1+.2*P2Items[7])*dt);
    float dx=0;float dy=0;
    if(Held(68) || (Mode==0 && Held(39)))dx++;
    if(Held(65) || (Mode==0 && Held(37)))dx--;
    if(Held(83) || (Mode==0 && Held(40)))dy++;
    if(Held(87) || (Mode==0 && Held(38)))dy--;
    float len=Sqrt(dx*dx+dy*dy);if(len>0){dx/=len;dy/=len;}
    if(Tired==1 && Stamina>=20)Tired=0;
    Linked=0;
    if(Held(32) && Tired==0 && Dist()<=85+35*P1Items[3] && (Mode==0 || EscapeTime<=0))Linked=1;
    if(Linked==1 && Stamina<=0 && P1Combo(3,7) && GraceUsed==0){GraceUsed=1;GraceTime=1.2;}
    if(Linked==1 && Stamina<=0 && GraceTime<=0){Tired=1;Linked=0;}
    float speed=190;if(Linked==1)speed=170;
    speed*=1+.1*P1Items[4];if(Slow>0 && P1Combo(0,4))speed*=1.2;
    P1X=Min(1042,Max(38,P1X+dx*speed*dt));P1Y=Min(626,Max(62,P1Y+dy*speed*dt));
    if(Linked==1){
      float drain=18*(1-.1*P1Items[11]);if(P1Combo(3,7))drain*=.75;
      if(Mode==1)drain*=1+.12*P2Items[5];
      if(GraceTime<=0)Stamina=Max(0,Stamina-drain*dt);
      RegenDelay=.6;Mood=0;MoodTimer=0;MoodWait=Max(MoodWait,1.7);
    }else if(RegenDelay<=0){
      float recovery=25*(1+.25*P1Items[9]);if(Modifier=="雨天")recovery*=.8;
      Stamina=Min(MaxStamina,Stamina+recovery*dt);
    }
    Arrived1=0;if(P1Y<=86)Arrived1=1;
    if(Mode==1)MoveP2(dt,speed);else MoveAI(dt,speed);
    if(Mode==1){Arrived2=0;if(P2Y<=94)Arrived2=1;}
    else if(P2Y<=94){Arrived2=1;P2Y=80;Mood=0;}
    for(int m=0;m<MedianCount;m++){if(Max(P1Y,P2Y)<=MedianY[m]+5 && Checkpoint>MedianY[m])Checkpoint=MedianY[m];}
    Traffic(dt);
    bool collision=false;
    for(int c=0;c<96;c++){
      if(CarActive[c]==1 && Phase==1 && !collision){
        if(Invincible<=0 && Arrived1==0 && Touches(P1X,P1Y,c)){HitP1();collision=true;}
        else if(Mode==1){
          if(BuddyInvincible<=0 && Arrived2==0 && Touches(P2X,P2Y,c))HitP2();
        }else if(Invincible<=0 && Arrived2==0 && Touches(P2X,P2Y,c)){HitP1();collision=true;}
      }
    }
    if(Phase==1){
      if(Mode==1 && TimeLeft<=0)RoundEnd(2);
      else if(Arrived1==1 && Arrived2==1){if(Mode==1)RoundEnd(1);else ClearLevel();}
    }
    StatusText="看准空隙，一起到对岸";
    if(Linked==1)StatusText="牵手中，同学听话";
    else if(Tired==1)StatusText="手酸了，稍等恢复或喝奶茶";
    else if(Mode==1 && EscapeTime>0)StatusText="2P 正在挣脱，暂时不能牵手";
    else if(Mode==1 && Stun2>0)StatusText="2P 撞懵了，趁现在牵走";
    else if(Force>0)StatusText="口哨 / 奶茶生效中";
    else if(Mood>0)StatusText="同学分心！按 E 提醒";
    else if(Dist()>170)StatusText="回头接同学，别落在车道上";
  }
  static void ClearLevel() {
    Score+=250+MaxInt(0,70-ToInt(Elapsed))*3;
    if(StageHits==0)Score+=50;
    if(Level>=10){EndGame(1);return;}
    if(Level%2==0 && Lives<4+P1Items[8])Lives++;
    ChoiceOwner=0;PendingRound=1;MakeChoices();Phase=2;ResetInputs();
  }
  static void RoundEnd(int winner) {
    if(Phase!=1)return;
    if(winner==1){Score1++;Score+=400+ToInt(TimeLeft)*3;}else Score2++;
    Winner=winner;
    if(Score1>=2 || Score2>=2){EndGame(winner);return;}
    PendingRound=1;ChoiceOwner=0;MakeChoices();Phase=2;ResetInputs();
  }
  static void EndGame(int winner) {
    if(Phase==4)return;
    Winner=winner;Phase=4;ResetInputs();
    if(Mode==0){
      EndTitle="同学，手机先放一放。";if(winner==1)EndTitle="十条马路，全员到达！";
      EndText=Join(Join("得分 ",N(Score)),Join("  ·  到达第 ",Join(N(Level)," 关")));
      if(Score>Best)Best=Score;
    }else{
      EndTitle="2P 手机同学获胜！";if(winner==1)EndTitle="1P 护送员获胜！";
      EndText=Join(Join("最终比分  ",N(Score1)),Join(" : ",N(Score2)));
    }
  }
  static void MakeChoices() {
    SetRandom(Seed+Level*3007+ChoiceOwner*19);SelectedChoice=0;
    int count=12;if(ChoiceOwner==1)count=8;
    for(int i=0;i<3;i++){
      int candidate=RandomInt(0,count);bool valid=false;int guard=0;
      while(!valid && guard<200){
        candidate=RandomInt(0,count);valid=true;
        if(ChoiceOwner==0 && P1Items[candidate]>=3)valid=false;
        if(ChoiceOwner==1 && P2Items[candidate]>=3)valid=false;
        for(int j=0;j<i;j++){if(Choices[j]==candidate)valid=false;}
        guard++;
      }
      Choices[i]=candidate;
    }
    // A new active item is guaranteed in early rewards.
    if(Level<=2){
      int active=RandomInt(0,3);int tries=0;
      while(tries<3 && ((ChoiceOwner==0 && P1Items[active]>0)||(ChoiceOwner==1 && P2Items[active]>0))){active=(active+1)%3;tries++;}
      if(tries<3){
        bool existing=false;for(int k=1;k<3;k++){if(Choices[k]==active)existing=true;}
        if(!existing)Choices[0]=active;
      }
    }
    for(int c=0;c<3;c++){
      ChoiceNames[c]=ItemName(ChoiceOwner,Choices[c]);
      ChoiceDescriptions[c]=ItemDescription(ChoiceOwner,Choices[c]);
      int rank=0;if(ChoiceOwner==0)rank=P1Items[Choices[c]];else rank=P2Items[Choices[c]];
      ChoiceTags[c]=Join(Join(N(ChoiceOwner+1),"P 装备  ·  "),Join("Lv.",N(rank+1)));
      ChoiceCombos[c]=ComboHint(ChoiceOwner,Choices[c]);
    }
  }
  static void ChooseReward(int index) {
    if(Phase!=2 || index<0 || index>2)return;
    int item=Choices[index];
    if(ChoiceOwner==0){
      if(P1Items[item]>=3)return;
      P1Items[item]++;
      if(item==8)Lives++;
      if(Mode==1){ChoiceOwner=1;MakeChoices();UpdateInventory();return;}
      Level++;SetupLevel();Phase=1;ResetInputs();UpdateInventory();return;
    }
    if(P2Items[item]>=3)return;
    P2Items[item]++;
    if(PendingRound==1){Round++;Level=Round;}
    PendingRound=0;SetupLevel();Phase=1;ResetInputs();UpdateInventory();
  }
  static string ItemName(int owner,int id) {
    if(owner==0){
      if(id==0)return "慢时钟 Q";
      if(id==1)return "反光伞 F";
      if(id==2)return "续命奶茶 R";
      if(id==3)return "防走丢手环";
      if(id==4)return "校队跑鞋";
      if(id==5)return "扩音口哨";
      if(id==6)return "乖乖软糖";
      if(id==7)return "大号充电宝";
      if(id==8)return "反光背心";
      if(id==9)return "冰美式";
      if(id==10)return "幸运校徽";
      return "防滑鞋底";
    }
    if(id==0)return "假通知 J";
    if(id==1)return "横滑闪避 K";
    if(id==2)return "沉浸耳机 L";
    if(id==3)return "溜溜拖鞋";
    if(id==4)return "超大屏手机";
    if(id==5)return "原地打团";
    if(id==6)return "信号放大器";
    return "随身小电源";
  }
  static string ItemDescription(int owner,int id) {
    if(owner==0){
      if(id==0)return "车流减速 55%，持续 3 秒。\n冷却 18 秒，升级减少冷却。";
      if(id==1)return "4 秒内抵挡一次碰撞。\n冷却 16 秒，升级更强。";
      if(id==2)return "恢复 55 体力，听话 3 秒。\n冷却 20 秒，升级减少冷却。";
      if(id==3)return "每级增加 35 像素牵手范围。\n靠近同学，再按住空格。";
      if(id==4)return "每级移速增加 10%。\n双人模式只增强 1P。";
      if(id==5)return "口哨冷却每级减少 15%。\n范围每级增加 60 像素。";
      if(id==6 && Mode==1)return "缩短 2P 挣脱，延长口哨。\n一颗糖换一点听话。";
      if(id==6)return "每级减少 20% 怪脾气次数。\n最多减少 50%。";
      if(id==7)return "体力上限每级增加 30。\n立即补满，牵手更持久。";
      if(id==8)return "最大生命和当前生命各 +1。\n升级继续增加。";
      if(id==9)return "体力恢复每级增加 25%。\n松手后快速恢复。";
      if(id==10)return "受伤无敌每级延长 0.5 秒。\n多一点容错。";
      return "牵手消耗每级减少 10%。\n双人模式还能削弱滑步。";
    }
    if(id==0)return "J 靠近时扣 1P 24 点体力。\n耗能 30，冷却 12 秒。";
    if(id==1)return "K 沿当前方向滑步并挣脱。\n耗能 28，冷却 10 秒。";
    if(id==2)return "L 四秒内抵挡一次口哨。\n耗能 35，冷却 15 秒。";
    if(id==3)return "挣脱冷却每级减少 12%。\n免牵时间每级 +0.12 秒。";
    if(id==4)return "能量上限每级增加 25 点。\n够你多刷好几条。";
    if(id==5)return "被牵时增加 1P 体力消耗。\n每级 +12%。";
    if(id==6)return "假通知范围每级 +40 像素。\n额外扣除 4 点体力。";
    return "能量恢复每级增加 20%。\n自然恢复基础 8 点/秒。";
  }
  static string ComboHint(int owner,int id) {
    if(owner==0){
      if((id==0 && P1Items[4]>0)||(id==4 && P1Items[0]>0))return "搭配：赶早八（慢时 + 加速）";
      if((id==1 && P1Items[8]>0)||(id==8 && P1Items[1]>0))return "搭配：移动红绿灯（附近车流减速）";
      if((id==2 && P1Items[6]>0)||(id==6 && P1Items[2]>0))return "搭配：哄学套餐（听话 6 秒）";
      if((id==3 && P1Items[7]>0)||(id==7 && P1Items[3]>0))return "搭配：牢牢牵住（减耗 + 续航）";
    }else{
      if((id==0 && P2Items[6]>0)||(id==6 && P2Items[0]>0))return "搭配：99+ 未读（扣体力 + 返能量）";
      if((id==1 && P2Items[3]>0)||(id==3 && P2Items[1]>0))return "搭配：碎步大师（减冷却 + 免牵）";
      if((id==2 && P2Items[4]>0)||(id==4 && P2Items[2]>0))return "搭配：连播结界（挡两次口哨）";
    }
    return "装备保留到本次挑战结束 · 可升到三级";
  }
  static void UpdateInventory() {
    P1Inventory="";P2Inventory="";ComboText="";
    for(int i=0;i<12;i++){if(P1Items[i]>0)P1Inventory=Join(P1Inventory,Join(Join(ItemName(0,i),"  Lv."),Join(N(P1Items[i]),"\n")));}
    for(int j=0;j<8;j++){if(P2Items[j]>0)P2Inventory=Join(P2Inventory,Join(Join(ItemName(1,j),"  Lv."),Join(N(P2Items[j]),"\n")));}
    if(P1Inventory=="")P1Inventory="背包还是空的\n过关后选择道具";
    if(P2Inventory=="")P2Inventory="赛前和局间选装\nEnter 默认可用";
    if(P1Combo(0,4))ComboText=Join(ComboText,"1P 赶早八\n");
    if(P1Combo(1,8))ComboText=Join(ComboText,"1P 移动红绿灯\n");
    if(P1Combo(2,6))ComboText=Join(ComboText,"1P 哄学套餐\n");
    if(P1Combo(3,7))ComboText=Join(ComboText,"1P 牢牢牵住\n");
    if(P2Combo(0,6))ComboText=Join(ComboText,"2P 99+ 未读\n");
    if(P2Combo(1,3))ComboText=Join(ComboText,"2P 碎步大师\n");
    if(P2Combo(2,4))ComboText=Join(ComboText,"2P 连播结界\n");
    MaxStamina=100+P1Items[7]*30;MaxEnergy=100+P2Items[4]*25;
  }
  static void GameTick(float dt) {
    float step=Min(.05,Max(0,dt));
    int phaseBefore=Phase;
    for(int k=0;k<256;k++){Input[k]=0;if(Key(k))Input[k]=1;}
    if(!HasFocus()){if(Phase==1)Phase=3;}
    else{
      if(Pressed(77) && Phase!=0)Menu();
      if(Pressed(80) || Pressed(27)){if(Phase==1)Phase=3;else if(Phase==3)Phase=1;}
      if(Phase==0){
        if(Pressed(49) || Pressed(37)){Mode=0;SetupLevel();}
        if(Pressed(50) || Pressed(39)){Mode=1;SetupLevel();}
        if(Pressed(13))StartGame(false);
      }else if(Phase==2){
        if(Pressed(37))SelectedChoice=(SelectedChoice+2)%3;
        if(Pressed(39))SelectedChoice=(SelectedChoice+1)%3;
        if(Pressed(49) || Pressed(97))ChooseReward(0);
        else if(Pressed(50) || Pressed(98))ChooseReward(1);
        else if(Pressed(51) || Pressed(99))ChooseReward(2);
        else if(Pressed(13))ChooseReward(SelectedChoice);
      }else if(Phase==3){if(Pressed(13))Phase=1;}
      else if(Phase==4){if(Pressed(13))StartGame(false);else if(Pressed(78))StartGame(true);}
      if(Phase==1 && phaseBefore==1){
        if(Pressed(69))Whistle();
        if(Pressed(81))P1Skill(0);if(Pressed(70))P1Skill(1);if(Pressed(82))P1Skill(2);
        if(Mode==1){if(Pressed(13))P2Skill(0);if(Pressed(74))P2Skill(1);if(Pressed(75))P2Skill(2);if(Pressed(76))P2Skill(3);}
        StepGame(step);
      }
    }
    if(Phase==0){
      for(int i=0;i<96;i++){if(CarActive[i]==1){CarX[i]+=CarDirection[i]*CarSpeed[i]*step*.45;if(CarX[i]>1250)CarX[i]=-150;if(CarX[i]<-150)CarX[i]=1250;}}
    }
    for(int p=0;p<256;p++)PreviousInput[p]=Input[p];
  }
  static void GameClick(float x,float y) {
    if(Phase==0){
      if(y>=410 && y<=506){
        if(x>=180 && x<=480){Mode=0;SetupLevel();}
        if(x>=500 && x<=800){Mode=1;SetupLevel();}
      }
      if(x>=180 && x<=800 && y>=532 && y<=584)StartGame(false);
    }else if(Phase==2 && y>=330 && y<=568){
      for(int i=0;i<3;i++){float left=150+315*i;if(x>=left && x<=left+290)ChooseReward(i);}
    }else if(Phase==3){if(x>=180 && x<=1000 && y>=270 && y<=650)Phase=1;}
    else if(Phase==4 && y>=510 && y<=560){
      if(x>=280 && x<=530)StartGame(false);
      else if(x>=570 && x<=820)Menu();
    }
  }
}
