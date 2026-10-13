// Realtime locomotion, feedback and rewards; gameplay remains a native timer-driven simulation.
public static partial class Game
{
 static float MotionClock,MotionRoadClock,MotionPhaseAge,UIApproach,HoverX,HoverY;
 static float Walk1,Walk2,MoveSpeed1,MoveSpeed2,Face1X,Face1Y,Face2X,Face2Y;
 static float DashTime,DashVX,DashVY,P1DashCooldown,LocoVX,LocoVY;
 static float MotionShake,HitFlash,SaveFlash,NearMissFlash,EffectWhistle,EffectShield,EffectTea,EffectClock,EffectGadget;
 static float FeedbackX,FeedbackY,MotionLastX1,MotionLastY1,MotionLastX2,MotionLastY2,MotionTrailTimer;
 static int MotionMuted,MotionLastPhase,MotionFxSerial,MotionFxCursor,MotionTrailCursor,MotionOldLinked,MotionNearCount,StageNearMisses;
 static float MotionOldCheckpoint;
 static float[] FXx,FXy,FXvx,FXvy,FXlife,FXmax,TrailX,TrailY,TrailLife;
 static int[] FXcolor,CarNearState;
 static float ApproachValue(float value,float target,float amount)
 {
  if(value<target)return Min(target,value+amount);
  return Max(target,value-amount);
 }
 static void MotionSound(int id){if(MotionMuted==0)Sound(id);}
 static void MotionResetFeedback()
 {
  LocoVX=0;LocoVY=0;DashTime=0;P1DashCooldown=0;MotionShake=0;HitFlash=0;SaveFlash=0;NearMissFlash=0;
  EffectWhistle=0;EffectShield=0;EffectTea=0;EffectClock=0;EffectGadget=0;
  MoveSpeed1=0;MoveSpeed2=0;Walk1=0;Walk2=0;Face1X=0;Face1Y=-1;Face2X=0;Face2Y=-1;
  MotionLastX1=P1X;MotionLastY1=P1Y;MotionLastX2=P2X;MotionLastY2=P2Y;
  MotionOldLinked=0;MotionOldCheckpoint=Checkpoint;MotionTrailTimer=0;MotionFxCursor=0;MotionTrailCursor=0;StageNearMisses=0;
  for(int i=0;i<64;i=i+1)FXlife[i]=0;
  for(int i=0;i<12;i=i+1)TrailLife[i]=0;
  for(int i=0;i<96;i=i+1)CarNearState[i]=0;
 }
 static void MotionUiTick(float dt)
 {
  MotionClock+=dt;MotionPhaseAge+=dt;UIApproach=Min(1,MotionPhaseAge*5);
  if(HasFocus()){HoverX=MouseX();HoverY=MouseY();}else{HoverX=-100;HoverY=-100;}
 }
 static void MotionFinishTick()
 {
  if(MotionLastPhase!=Phase)
  {
   int old=MotionLastPhase;MotionLastPhase=Phase;MotionPhaseAge=0;UIApproach=0;
   if(Phase==4){if(Winner==1)MotionSound(4);else MotionSound(3);}
   else if(old==1 && Phase==2)MotionSound(4);
   else if(Phase!=3 && Phase!=9)MotionSound(5);
  }
 }
 static void MotionEmit(float x,float y,int color,int count)
 {
  for(int n=0;n<count;n=n+1)
  {
   int slot=MotionFxCursor;MotionFxCursor=(MotionFxCursor+1)%64;MotionFxSerial++;
   float angle=MotionFxSerial*2.399;float speed=36+(Sin(MotionFxSerial*1.7)*.5+.5)*95;
   FXx[slot]=x;FXy[slot]=y;FXvx[slot]=Cos(angle)*speed;FXvy[slot]=Sin(angle)*speed-26;
   FXlife[slot]=.35+(Sin(MotionFxSerial*.7)*.5+.5)*.35;FXmax[slot]=FXlife[slot];FXcolor[slot]=color;
  }
 }
 static void MotionTryDash()
 {
  if(Phase!=1 || P1DashCooldown>0 || Stamina<18 || Arrived1==1)return;
  float dx=0;float dy=0;
  if(Held(68) || (Mode==0 && Held(39)))dx++;
  if(Held(65) || (Mode==0 && Held(37)))dx--;
  if(Held(83) || (Mode==0 && Held(40)))dy++;
  if(Held(87) || (Mode==0 && Held(38)))dy--;
  float distance=Sqrt(dx*dx+dy*dy);
  if(distance>0){DashVX=dx/distance;DashVY=dy/distance;}
  else{DashVX=Face1X;DashVY=Face1Y;if(Abs(DashVX)+Abs(DashVY)<.1)DashVY=-1;}
  Stamina-=18;RegenDelay=Max(RegenDelay,.65);DashTime=.22;P1DashCooldown=1.4;
  MotionTrailTimer=0;MotionEmit(P1X,P1Y,0x63DFC4,9);MotionSound(2);
 }
 static void MotionMovePlayer(float dx,float dy,float speed,float dt)
 {
  if(DashTime>0){LocoVX=DashVX*390;LocoVY=DashVY*390;}
  else
  {
   float accel=2600;if(dx==0 && dy==0)accel=3100;
   LocoVX=ApproachValue(LocoVX,dx*speed,accel*dt);LocoVY=ApproachValue(LocoVY,dy*speed,accel*dt);
  }
  P1X=Min(1042,Max(38,P1X+LocoVX*dt));P1Y=Min(626,Max(62,P1Y+LocoVY*dt));
 }
 static void MotionBeforeRoadFrame(float dt)
 {
  MotionRoadClock+=dt;MotionOldLinked=Linked;MotionOldCheckpoint=Checkpoint;
  DashTime=Max(0,DashTime-dt);P1DashCooldown=Max(0,P1DashCooldown-dt);
  MotionShake=Max(0,MotionShake-dt);HitFlash=Max(0,HitFlash-dt);SaveFlash=Max(0,SaveFlash-dt);NearMissFlash=Max(0,NearMissFlash-dt);
  EffectWhistle=Max(0,EffectWhistle-dt);EffectShield=Max(0,EffectShield-dt);EffectTea=Max(0,EffectTea-dt);EffectClock=Max(0,EffectClock-dt);EffectGadget=Max(0,EffectGadget-dt);
  for(int i=0;i<64;i=i+1)
  {
   if(FXlife[i]>0){FXlife[i]=Max(0,FXlife[i]-dt);FXx[i]+=FXvx[i]*dt;FXy[i]+=FXvy[i]*dt;FXvy[i]+=75*dt;}
  }
  for(int i=0;i<12;i=i+1)TrailLife[i]=Max(0,TrailLife[i]-dt);
  MotionTrailTimer=Max(0,MotionTrailTimer-dt);
  if(Pressed(16))MotionTryDash();
 }
 static void MotionUpdateWalk(float dt)
 {
  float ax=P1X-MotionLastX1;float ay=P1Y-MotionLastY1;float d=Sqrt(ax*ax+ay*ay);
  MoveSpeed1=0;
  if(dt>0 && d<40)
  {
   MoveSpeed1=d/dt;Walk1+=d;
   if(d>.15){Face1X=ax/d;Face1Y=ay/d;}
  }
  ax=P2X-MotionLastX2;ay=P2Y-MotionLastY2;d=Sqrt(ax*ax+ay*ay);MoveSpeed2=0;
  if(dt>0 && d<40)
  {
   MoveSpeed2=d/dt;Walk2+=d;
   if(d>.15){Face2X=ax/d;Face2Y=ay/d;}
  }
  MotionLastX1=P1X;MotionLastY1=P1Y;MotionLastX2=P2X;MotionLastY2=P2Y;
 }
 static void MotionCheckNearMiss()
 {
  if(Mode!=0 || Invincible>0 || Phase!=1)return;
  for(int i=0;i<96;i=i+1)
  {
   if(CarActive[i]==1 && CarSpeed[i]>35 && ItemTrafficRate(i)*RoadTrafficRate(i)>.2)
   {
    float qx=Abs(P1X-CarX[i])-(CarW[i]/2+7);
    float qy=Abs(P1Y-6-CarY[i])-(CarH[i]/2+7);
    if(CarNearState[i]==0 && MoveSpeed1>25 && (qx>=0 || qy>=0) && qx<18 && qy<17 && P1Y>120 && P1Y<520)CarNearState[i]=1;
    if(CarNearState[i]==1 && (CarX[i]-P1X)*CarDirection[i]>CarW[i]/2+38)
    {
     CarNearState[i]=2;StageNearMisses++;MotionNearCount++;Score+=15;Coins+=2;Stamina=Min(MaxStamina,Stamina+6);
     NearMissFlash=1.0;FeedbackX=P1X;FeedbackY=P1Y;MotionEmit(P1X,P1Y,0xFFB575,12);MotionSound(4);
    }
   }
  }
 }
 static void MotionAfterRoadFrame(float dt)
 {
  MotionUpdateWalk(dt);
  if(Linked==1 && MotionOldLinked==0){MotionEmit((P1X+P2X)*.5,(P1Y+P2Y)*.5,0x63DFC4,6);MotionSound(0);}
  if(Checkpoint<MotionOldCheckpoint && Phase==1){SaveFlash=1.1;FeedbackX=(P1X+P2X)*.5;FeedbackY=Checkpoint;MotionEmit(FeedbackX,FeedbackY,0x63DFC4,15);MotionSound(4);}
  if(DashTime>0 && MotionTrailTimer<=0)
  {
   int slot=MotionTrailCursor;MotionTrailCursor=(MotionTrailCursor+1)%12;TrailX[slot]=P1X;TrailY[slot]=P1Y;TrailLife[slot]=.24;MotionTrailTimer=.025;
  }
  MotionCheckNearMiss();
 }
 static void StepGame(float dt){MotionBeforeRoadFrame(dt);MotionStepCore(dt);MotionAfterRoadFrame(dt);}
 static void SetupLevel(){MotionSetupCore();MotionResetFeedback();MotionRoadClock=0;}
 static void StartGame(bool newSeed){MotionNearCount=0;MotionResetFeedback();MotionStartCore(newSeed);}
 static void Menu(){MotionMenuCore();MotionResetFeedback();}
 static void GameInitialize(){MotionClock=0;MotionRoadClock=0;MotionPhaseAge=0;MotionLastPhase=0;MotionMuted=0;MotionNearCount=0;HoverX=-100;HoverY=-100;MotionInitializeCore();}
 static void GameClick(float x,float y){MotionClickCore(x,y);MotionFinishTick();}
 static void SpawnCar(int lane,float x)
 {
  int slot=-1;for(int i=0;i<96;i=i+1){if(CarActive[i]==0 && slot<0)slot=i;}
  MotionSpawnCore(lane,x);if(slot>=0)CarNearState[slot]=0;
 }
 static void HitP1()
 {
  bool blocked=Shield>0;float x=P1X;float y=P1Y;
  for(int i=0;i<96;i=i+1){if(CarActive[i]==1 && Touches(P2X,P2Y,i) && !Touches(P1X,P1Y,i)){x=P2X;y=P2Y;}}
  MotionHitCore();FeedbackX=x;FeedbackY=y;
  if(blocked){EffectShield=.65;MotionShake=.1;MotionEmit(x,y,0x63DFC4,20);MotionSound(5);}
  else{MotionShake=.25;HitFlash=.28;MotionEmit(x,y,0xFFB575,24);MotionSound(3);}
  LocoVX=0;LocoVY=0;DashTime=0;MoveSpeed1=0;MoveSpeed2=0;
  MotionLastX1=P1X;MotionLastY1=P1Y;MotionLastX2=P2X;MotionLastY2=P2Y;
  for(int i=0;i<96;i=i+1){if(CarNearState[i]==1)CarNearState[i]=2;}
  for(int i=0;i<12;i=i+1)TrailLife[i]=0;
 }
 static void HitP2(){FeedbackX=P2X;FeedbackY=P2Y;MotionBuddyHitCore();MotionShake=.14;HitFlash=.18;MotionEmit(P2X,P2Y,0xFFB575,16);MotionSound(3);}
 static void Whistle(){float old=WhistleCD;MotionWhistleCore();if(WhistleCD>old){EffectWhistle=.65;MotionEmit(P1X,P1Y,0x63DFC4,7);MotionSound(1);}}
 static void P1Skill(int id)
 {
  float oldClock=ClockCD;float oldShield=ShieldCD;float oldTea=TeaCD;MotionP1SkillCore(id);
  if(ClockCD>oldClock){EffectClock=.65;MotionEmit(P1X,P1Y,0xB9AAF1,14);MotionSound(5);}
  if(ShieldCD>oldShield){EffectShield=.7;MotionEmit(P1X,P1Y,0x63DFC4,14);MotionSound(5);}
  if(TeaCD>oldTea){EffectTea=.7;MotionEmit(P2X,P2Y,0xFFB575,13);MotionSound(5);}
 }
 static void GadgetSkill(){float old=GadgetCD;MotionGadgetCore();if(GadgetCD>old){EffectGadget=.7;MotionEmit(P1X,P1Y,0x63DFC4,13);MotionSound(5);}}
 static void MotionStepCore(float dt){

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
    MotionMovePlayer(dx,dy,speed,dt);if(DashTime>0)speed=390;
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
 static void GameTick(float dt){

 float step=Min(.05,Max(0,dt));
 int phaseBefore=Phase;MotionUiTick(step);
 for(int k=0;k<256;k++){Input[k]=0;if((k>=65 && k<=90) || (k>=37 && k<=40) || (k>=48 && k<=51) || (k>=96 && k<=99) || k==9 || k==13 || k==16 || k==27 || k==32 || k==112){if(Key(k))Input[k]=1;}}
 if(!HasFocus()){if(Phase==1)Phase=3;}
 else{
  if(Pressed(86)){MotionMuted=1-MotionMuted;}
  if(Pressed(112))ToggleFullscreen();
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
 MotionFinishTick();
 for(int p=0;p<256;p++)PreviousInput[p]=Input[p];

 }
 static void MotionSetupCore() {

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
 static void MotionStartCore(bool newSeed) {

 if(newSeed)Seed=NewSeed();
 for(int i=0;i<24;i++)P1Items[i]=0;
 for(int j=0;j<8;j++)P2Items[j]=0;
 Level=1;Round=1;Score1=0;Score2=0;Score=0;Lives=4;Winner=0;PendingRound=0;
 RouteAct=1;RouteRow=0;RouteColumn=-1;CurrentNode=-1;NodeKind=0;Coins=0;RewardContext=0;
 ItemResetRun();CollectionPage=0;CollectionView=0;BackPhase=0;ResetInputs();UpdateInventory();
 if(Mode==1){SetupLevel();ChoiceOwner=0;MakeChoices();Phase=2;}
 else RouteStart();

 }
 static void MotionMenuCore() {

    Phase=0;Level=1;Round=1;Lives=4;
    for(int i=0;i<24;i++)P1Items[i]=0;
    for(int j=0;j<8;j++)P2Items[j]=0;
    RouteAct=1;RouteRow=0;RouteColumn=-1;CurrentNode=-1;NodeKind=0;Coins=0;RewardContext=0;ItemResetRun();SetupLevel();ResetInputs();UpdateInventory();
    StatusText="1 单人护送 / 2 双人对抗 · Enter 开始";
  
 }
 static void MotionInitializeCore() {

    Mode=0;Phase=0;Level=1;Round=1;Seed=NewSeed();Lives=4;
    MaxStamina=100;Stamina=100;MaxEnergy=100;Energy=100;TimeLeft=75;
    P1Inventory="过关后，三选一道具";P2Inventory="赛前和局间，各选一道具";ComboText="";
    Bubble="";StatusText="选择模式，按 Enter 出发";EndTitle="";EndText="";
    RouteAct=1;RouteRow=0;RouteColumn=-1;CurrentNode=-1;NodeKind=0;Coins=0;RewardContext=0;CollectionPage=0;CollectionView=0;BackPhase=0;ItemResetRun();SetupLevel();
    Invincible=0;
  
 }
 static void MotionClickCore(float x,float y) {
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
 static void MotionHitCore() {

    if(Shield>0){Shield=0;Invincible=1.2+P1Items[10]*.5;Bubble="反光伞挡住了！";BubbleTime=2;return;}
    if(ItemBeforeHit())return;Lives--;StageHits++;Invincible=2+P1Items[10]*.5;
    if(Lives<=0){if(Mode==1)RoundEnd(2);else EndGame(2);return;}
    P1X=519;P1Y=Checkpoint;P2X=557;P2Y=Min(626,Checkpoint+8);
    Arrived1=0;Arrived2=0;Mood=0;MoodWait=5;
    Force=2;if(Mode==1)Force=0;
    Bubble="哎哟！回到最近的安全岛。";BubbleTime=2.5;
  
 }
 static void MotionWhistleCore() {

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
 static void MotionSpawnCore(int lane,float x) {
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
 static void MotionBuddyHitCore() {
    Energy=Max(0,Energy-25);Stun2=.7;BuddyInvincible=1.6;EscapeTime=0;BurstTime=0;
    Bubble="撞懵了！捣蛋能量 -25";BubbleTime=2;
  }
 static void MotionP1SkillCore(int id) {
    if(Phase!=1 || id<0 || id>2 || P1Items[id]==0)return;
    if(id==0 && ClockCD<=0){
      Slow=3;if(P1Combo(0,4))Slow=4;
      ClockCD=18;Bubble="滴答——车流慢下来了。";BubbleTime=2;
    }
    if(id==1 && ShieldCD<=0){
      Shield=4;ShieldCD=16;
      if(Mode==0 && P1Combo(18,1)){Shield+=1.5;Stamina=Min(MaxStamina,Stamina+20);}
      Bubble="反光伞，撑开！";BubbleTime=2;
    }
    if(id==2 && TeaCD<=0){
      Stamina=Min(MaxStamina,Stamina+55);Force=Max(Force,3);
      if(P1Combo(2,6))Force=Max(Force,6);
      TeaCD=20;if(Mode==0 && P1Combo(21,2))TeaCD=14;
      Tired=0;Mood=0;MoodWait=5;
      if(Mode==0 && P1Combo(14,2)){
        float duration=4;if(P1Combo(14,6) || P1Combo(15,14))duration=6;
        float range=180;if(P1Combo(15,14))range=500;else if(P1Combo(14,6))range=260;
        PlaceBeacon(range,duration);
      }
      Bubble="这口奶茶值得抬头。";BubbleTime=2;
    }
  }
 static void MotionGadgetCore() {
    if(Mode!=0 || Phase!=1 || GadgetCD>0 || ItemActiveT<12 || ItemActiveT>15)return;
    if(P1Items[ItemActiveT]==0)return;
    if(ItemActiveT==12){
      RoadblockX=P1X;RoadblockY=P1Y;RoadblockTime=2.6;GadgetCD=20;
      if(P1Combo(12,17))RoadblockTime=4;
      if(P1Combo(12,13)){SignalLane=NearestSignalLane();SignalTime=1.5;}
      Bubble="路障摆好，抓紧穿过去！";
    }
    if(ItemActiveT==13){
      SignalLane=NearestSignalLane();SignalTime=2.3;GadgetCD=22;
      if(P1Combo(13,0))Slow=Max(Slow,2.3);
      if(P1Combo(12,13)){RoadblockX=P1X;RoadblockY=P1Y;RoadblockTime=2;}
      Bubble="切换红灯，最近车道停下。";
    }
    if(ItemActiveT==14){
      float duration=4;float range=180;
      if(P1Combo(14,6)){duration=6;range=260;}
      if(P1Combo(15,14)){duration=6;range=500;}
      PlaceBeacon(range,duration);GadgetCD=18;
      Bubble="安全岛有奶茶，快来！";
    }
    if(ItemActiveT==15){
      float duration=3.5;float range=330;
      if(P1Combo(15,14)){duration=6;range=500;}
      PlaceBeacon(range,duration);GadgetCD=24;
      Bubble="无人机带你去下一处安全岛。";
    }
    BubbleTime=2;
  }
}
