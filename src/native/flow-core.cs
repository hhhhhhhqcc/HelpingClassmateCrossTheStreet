using System;
public partial class GameForm {
 static int RoadRule, LastPhoneWave, CollectionPage, BackPhase, CollectionView;
 static float ObstacleX, ObstacleY, ObstacleW, ObstacleH;
 static string RoadWarning;

 static void RootSetupRoad() {
  RoadRule=0;ObstacleX=0;ObstacleY=0;ObstacleW=0;ObstacleH=0;LastPhoneWave=-1;RoadWarning="";
  if(Mode==0 && NodeKind==5){
   RoadRule=RouteAct;
   if(RoadRule==1)Modifier="校门车队";
   if(RoadRule==2){Modifier="施工绕行";ObstacleX=392;ObstacleY=329;ObstacleW=296;ObstacleH=30;}
   if(RoadRule==3)Modifier="连播路口";
  }
 }
 static void RoadTick() {
  if(Mode!=0 || RoadRule==0){RoadWarning="";return;}
  float cycle=Elapsed%10;
  if(RoadRule==1){
   RoadWarning="车队空隙：看清车流再走";
   if(cycle<2)RoadWarning="车队预告：顺向车道即将提速";
   else if(cycle<5)RoadWarning="车队通过！逆向车道相对缓慢";
  }
  if(RoadRule==2)RoadWarning="施工隔离带：从左右两侧绕过去";
  if(RoadRule==3){
   cycle=Elapsed%12;
   RoadWarning="路口换向：观察箭头与车流空隙";
   if(cycle<5)RoadWarning="顺向车道通行，逆向车道减速";
   else if(cycle<10)RoadWarning="逆向车道通行，顺向车道减速";
   if(cycle<2)RoadWarning="连播预告：同学即将停下，牵手或吹口哨";
   int wave=ToInt(Elapsed/12);
   if(cycle>=2 && LastPhoneWave!=wave){
    LastPhoneWave=wave;
    if(Linked==0 && Force<=0 && P1Items[16]==0){
     Mood=1;MoodTimer=1.25;Bubble="等等！这一条还没看完。";BubbleTime=2;
    }
   }
  }
 }
 static float RoadTrafficRate(int car) {
  if(Mode!=0)return 1;
  float cycle=Elapsed%10;
  if(RoadRule==1 && cycle>=2 && cycle<5){if(CarLane[car]%2==0)return 1.4;return .7;}
  if(RoadRule==3){
   cycle=Elapsed%12;
   if(cycle<5 && CarLane[car]%2==1)return .22;
   if(cycle>=5 && cycle<10 && CarLane[car]%2==0)return .22;
  }
  return 1;
 }
 static void ResolveRoadObstacle(float old1,float old2) {
  if(Mode!=0 || ObstacleH<=0)return;
  if(P1X>ObstacleX-10 && P1X<ObstacleX+ObstacleW+10 && P1Y>ObstacleY-12 && P1Y<ObstacleY+ObstacleH+12){
   if(old1>=ObstacleY+ObstacleH*.5)P1Y=ObstacleY+ObstacleH+12;
   else P1Y=ObstacleY-12;
  }
  if(P2X>ObstacleX-10 && P2X<ObstacleX+ObstacleW+10 && P2Y>ObstacleY-12 && P2Y<ObstacleY+ObstacleH+12){
   if(old2>=ObstacleY+ObstacleH*.5)P2Y=ObstacleY+ObstacleH+12;
   else P2Y=ObstacleY-12;
  }
 }
 static void CollectionKeys() {
 if(Phase!=9)return;
 if(Pressed(9)){CollectionView=1-CollectionView;CollectionPage=0;}
 int pages=3;if(CollectionView==1)pages=2;
 if(Pressed(37))CollectionPage=(CollectionPage+pages-1)%pages;
 if(Pressed(39))CollectionPage=(CollectionPage+1)%pages;
 if(Pressed(49))CollectionPage=0;
 if(Pressed(50))CollectionPage=1;
 if(Pressed(51) && pages==3)CollectionPage=2;
}
 static void CollectionClick(float x,float y) {
 if(Phase!=9)return;if(x>=1060 && x<=1220 && y>=90 && y<=145){Phase=BackPhase;ResetInputs();return;}
 int pages=3;if(CollectionView==1)pages=2;
 if(y>=813 && y<=850){
  for(int page=0;page<pages;page++){float left=471+99*page;if(x>=left && x<=left+79)CollectionPage=page;}
 }
}
 static int RouteCompletedCount() {
  int total=0;
  for(int i=0;i<54;i++){if(NodeVisited[i]==2)total++;}
  return total;
 }
 static void GameInitialize() {

    Mode=0;Phase=0;Level=1;Round=1;Seed=NewSeed();Lives=4;
    MaxStamina=100;Stamina=100;MaxEnergy=100;Energy=100;TimeLeft=75;
    P1Inventory="过关后，三选一道具";P2Inventory="赛前和局间，各选一道具";ComboText="";
    Bubble="";StatusText="选择模式，按 Enter 出发";EndTitle="";EndText="";
    RouteAct=1;RouteRow=0;RouteColumn=-1;CurrentNode=-1;NodeKind=0;Coins=0;RewardContext=0;CollectionPage=0;CollectionView=0;BackPhase=0;ItemResetRun();SetupLevel();
    Invincible=0;
  
 }

 static void StartGame(bool newSeed) {

 if(newSeed)Seed=NewSeed();
 for(int i=0;i<24;i++)P1Items[i]=0;
 for(int j=0;j<8;j++)P2Items[j]=0;
 Level=1;Round=1;Score1=0;Score2=0;Score=0;Lives=4;Winner=0;PendingRound=0;
 RouteAct=1;RouteRow=0;RouteColumn=-1;CurrentNode=-1;NodeKind=0;Coins=0;RewardContext=0;
 ItemResetRun();CollectionPage=0;CollectionView=0;BackPhase=0;ResetInputs();UpdateInventory();
 if(Mode==1){SetupLevel();ChoiceOwner=0;MakeChoices();Phase=2;}
 else RouteStart();

 }

 static void Menu() {

    Phase=0;Level=1;Round=1;Lives=4;
    for(int i=0;i<24;i++)P1Items[i]=0;
    for(int j=0;j<8;j++)P2Items[j]=0;
    RouteAct=1;RouteRow=0;RouteColumn=-1;CurrentNode=-1;NodeKind=0;Coins=0;RewardContext=0;ItemResetRun();SetupLevel();ResetInputs();UpdateInventory();
    StatusText="1 单人护送 / 2 双人对抗 · Enter 开始";
  
 }

 static void SetupLevel() {

    SetRandom(Seed+Level*1009);if(Mode==0)SetRandom(Seed+Level*1009+(CurrentNode+1)*4729);
    LaneCount=3+ToInt(Level*.5);if(Mode==0){LaneCount=3+(RouteAct-1)+ToInt(RouteRow*.5);if(NodeKind==1)LaneCount++;}
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
  ItemSetup();RootSetupRoad();
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
        rate*=ItemTrafficRate(i);rate*=RoadTrafficRate(i);CarX[i]+=CarDirection[i]*CarSpeed[i]*rate*dt;
        if(CarX[i]<-220 || CarX[i]>1300){CarActive[i]=0;CarCount--;}
      }
    }
  
 }

 static void HitP1() {

    if(Shield>0){Shield=0;Invincible=1.2+P1Items[10]*.5;Bubble="反光伞挡住了！";BubbleTime=2;return;}
    if(ItemBeforeHit())return;Lives--;StageHits++;Invincible=2+P1Items[10]*.5;
    if(Lives<=0){if(Mode==1)RoundEnd(2);else EndGame(2);return;}
    P1X=519;P1Y=Checkpoint;P2X=557;P2Y=Min(626,Checkpoint+8);
    Arrived1=0;Arrived2=0;Mood=0;MoodWait=5;
    Force=2;if(Mode==1)Force=0;
    Bubble="哎哟！回到最近的安全岛。";BubbleTime=2.5;
  
 }

 static void Whistle() {

    if(Phase!=1 || WhistleCD>0 || Dist()>ItemWhistleRange())return;
    WhistleCD=7*(1-Min(.45,.15*P1Items[5]));
    if(Mode==1)WhistleCD=8*(1-Min(.45,.15*P1Items[5]));
    if(Mode==1 && FocusTime>0 && FocusCharges>0){
      FocusCharges--;if(FocusCharges==0)FocusTime=0;
      Bubble="耳机挡住了口哨！";BubbleTime=2;return;
    }
    Force=Max(Force,2.8);
    if(Mode==1){Force=2+.15*P1Items[6];ForceLock=.75;EscapeTime=0;BurstTime=0;}
    ItemOnWhistle();Mood=0;MoodWait=4+Rand()*4;Bubble="来了！知道啦。";BubbleTime=2;
  
 }

 static void MoveAI(float dt,float p1Speed) {

    
 if(Arrived2==1)return;
 if(Mode==0 && BeaconTime>0 && Linked==0){
  Mood=0;
  float bx=BeaconX-P2X;float by=BeaconY-P2Y;float bd=Sqrt(bx*bx+by*by);
  if(bd>8){float bs=Min(bd-8,185*(1+.1*P1Items[4])*dt);P2X+=bx/bd*bs;P2Y+=by/bd*bs;}
  P2X=Min(1042,Max(38,P2X));P2Y=Min(626,Max(62,P2Y));return;
 }
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

    float old1=P1Y;float old2=P2Y;Elapsed+=dt;TimeLeft=Max(0,75-Elapsed);RoadTick();
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
    if(Held(32) && Tired==0 && Dist()<=ItemLinkRange() && (Mode==0 || EscapeTime<=0))Linked=1;
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
    ResolveRoadObstacle(old1,old2);Arrived1=0;if(P1Y<=86)Arrived1=1;if(Mode==1){Arrived2=0;if(P2Y<=94)Arrived2=1;}
    else if(P2Y<=94){Arrived2=1;P2Y=80;Mood=0;}
    for(int m=0;m<MedianCount;m++){if(Max(P1Y,P2Y)<=MedianY[m]+5 && Checkpoint>MedianY[m])Checkpoint=MedianY[m];}
    ItemTick(dt);Traffic(dt);
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
if(Mode==0)RouteClearRoad();
 }

 static void EndGame(int winner) {

 if(Phase==4)return;
 Winner=winner;Phase=4;ResetInputs();
 if(Mode==0){
  EndTitle="今天的回家路，还没走完。";if(winner==1)EndTitle="同学已安全送到家！";
  EndText=Join(Join("经过 "+N(RouteCompletedCount())+" 站 · 第 "+N(RouteAct)+" 幕 · 得分 ",N(Score))," · 零钱 "+N(Coins));
  if(Score>Best)Best=Score;
 }else{
  EndTitle="2P 手机同学获胜！";if(winner==1)EndTitle="1P 护送员获胜！";
  EndText="最终比分  "+N(Score1)+" : "+N(Score2);
 }
 }

 static void GameTick(float dt) {

 float step=Min(.05,Max(0,dt));
 int phaseBefore=Phase;
 for(int k=0;k<256;k++){Input[k]=0;if(Key(k))Input[k]=1;}
 if(!HasFocus()){if(Phase==1)Phase=3;}
 else{
  if(Pressed(77) && Phase!=0)Menu();
  if(Mode==0 && Pressed(66) && Phase!=0 && Phase!=4){
   if(Phase==9)Phase=BackPhase;
   else {BackPhase=Phase;Phase=9;CollectionPage=0;CollectionView=0;}
   ResetInputs();
  }
  if(Pressed(80) || Pressed(27)){
   if(Phase==9){Phase=BackPhase;ResetInputs();}
   else if(Phase==1)Phase=3;
   else if(Phase==3)Phase=1;
  }
  if(Phase==0){
   if(Pressed(49) || Pressed(37)){Mode=0;SetupLevel();}
   if(Pressed(50) || Pressed(39)){Mode=1;SetupLevel();}
   if(Pressed(13))StartGame(true);
  }else if(Phase==2){
   if(Pressed(37))SelectedChoice=(SelectedChoice+2)%3;
   if(Pressed(39))SelectedChoice=(SelectedChoice+1)%3;
   if(Pressed(49) || Pressed(97))ChooseReward(0);
   else if(Pressed(50) || Pressed(98))ChooseReward(1);
   else if(Pressed(51) || Pressed(99))ChooseReward(2);
   else if(Pressed(13))ChooseReward(SelectedChoice);
  }else if(Phase==3){if(Pressed(13))Phase=1;}
  else if(Phase==4){if(Pressed(13))StartGame(false);else if(Pressed(78))StartGame(true);}
  else if(Phase>=5 && Phase<=8)RouteKeys();
  else if(Phase==9)CollectionKeys();
  if(Phase==1 && phaseBefore==1){
   if(Pressed(69))Whistle();
   if(Pressed(81))P1Skill(0);if(Pressed(70))P1Skill(1);if(Pressed(82))P1Skill(2);
   if(Mode==0){if(Pressed(9))CycleGadget();if(Pressed(84))GadgetSkill();}
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
if(Phase>=5 && Phase<=8){RouteClick(x,y);return;}if(Phase==9){CollectionClick(x,y);return;}
    if(Phase==0){
      if(y>=410 && y<=506){
        if(x>=180 && x<=480){Mode=0;SetupLevel();}
        if(x>=500 && x<=800){Mode=1;SetupLevel();}
      }
      if(x>=180 && x<=800 && y>=532 && y<=584)StartGame(true);
    }else if(Phase==2 && y>=330 && y<=568){
      for(int i=0;i<3;i++){float left=150+315*i;if(x>=left && x<=left+290)ChooseReward(i);}
    }else if(Phase==3){if(x>=180 && x<=1000 && y>=270 && y<=650)Phase=1;}
    else if(Phase==4 && y>=510 && y<=560){
      if(x>=280 && x<=530)StartGame(false);
      else if(x>=570 && x<=820)Menu();
    }
  
 }
}
