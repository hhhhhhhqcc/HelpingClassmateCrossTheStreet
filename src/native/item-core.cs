using System;
public partial class GameForm {
  // All equipment is unique: values are 0 (missing) or 1 (owned).
  static int ItemActiveT, SignalLane, MedkitUsed, CompassRested;
  static float GadgetCD, RoadblockTime, RoadblockX, RoadblockY, SignalTime;
  static float BeaconTime, BeaconX, BeaconY, WatchLastX, WatchLastY, WatchDistance;

  static int P1ItemCount() { if(Mode==0)return 24;return 12; }
  static int P2ItemCount() { return 8; }
  static int ItemOwnedCount(int owner) {
    int total=0;int count=P1ItemCount();if(owner==1)count=P2ItemCount();
    for(int i=0;i<count;i++){if(owner==0){if(P1Items[i]>0)total++;}else if(P2Items[i]>0)total++;}
    return total;
  }
  static int PickUnowned(int owner) {
    int count=P1ItemCount();if(owner==1)count=P2ItemCount();
    int start=RandomInt(0,count);
    for(int i=0;i<count;i++){
      int id=(start+i)%count;
      if(owner==0){if(P1Items[id]==0)return id;}
      else if(P2Items[id]==0)return id;
    }
    return -1;
  }
  static void ItemResetRun() {
    ItemActiveT=-1;MedkitUsed=0;CompassRested=0;
    GadgetCD=0;RoadblockTime=0;SignalTime=0;BeaconTime=0;WatchDistance=0;
  }
  static void ItemSetup() {
    GadgetCD=0;RoadblockTime=0;SignalTime=0;BeaconTime=0;SignalLane=-1;
    CompassRested=0;WatchDistance=0;WatchLastX=P1X;WatchLastY=P1Y;
    if(Mode==1){ItemActiveT=-1;return;}
    if(ItemActiveT<12 || ItemActiveT>15 || P1Items[ItemActiveT]==0){
      ItemActiveT=-1;
      for(int i=12;i<16;i++){if(P1Items[i]>0 && ItemActiveT<0)ItemActiveT=i;}
    }
  }
  static void GrantItem(int owner,int id) {
    int count=P1ItemCount();if(owner==1)count=P2ItemCount();
    if(id<0 || id>=count)return;
    if(owner==0){
      if(P1Items[id]>0)return;
      P1Items[id]=1;
      if(id==8)Lives++;
      if(id==7){MaxStamina=130;Stamina=MaxStamina;}
      if(id>=12 && id<=15)ItemActiveT=id;
      if(id==19)MedkitUsed=0;
    }else{
      if(P2Items[id]>0)return;
      P2Items[id]=1;
      if(id==4){MaxEnergy=125;Energy=MaxEnergy;}
    }
    UpdateInventory();
  }
  static void MakeChoices() {
    int rewardSeed=Seed+Level*3007+ChoiceOwner*19+ItemOwnedCount(0)*37+ItemOwnedCount(1)*71;
    if(Mode==0)rewardSeed+=(CurrentNode+1)*401+RouteRewardSequence*977;
    SetRandom(rewardSeed);
    SelectedChoice=0;
    int count=P1ItemCount();if(ChoiceOwner==1)count=P2ItemCount();
    for(int i=0;i<3;i++){
      int item=-1;int start=RandomInt(0,count);
      for(int n=0;n<count;n++){
        int test=(start+n)%count;bool free=true;
        if(ChoiceOwner==0){if(P1Items[test]>0)free=false;}
        else if(P2Items[test]>0)free=false;
        for(int j=0;j<i;j++){if(Choices[j]==test)free=false;}
        if(free && item<0)item=test;
      }
      if(i==0 && ItemOwnedCount(ChoiceOwner)==0)item=RandomInt(0,3);
      Choices[i]=item;
      if(item<0){
        ChoiceNames[i]="路上补给";ChoiceTags[i]="即时奖励";
        ChoiceDescriptions[i]="本次没有更多新装备可选。\n单人获得 20 金币。\n双人补满双方资源。";
        ChoiceCombos[i]="装备保留到本次挑战结束";
      }else{
        ChoiceNames[i]=ItemName(ChoiceOwner,item);
        ChoiceTags[i]="独特装备 · 未拥有";
        if(ChoiceOwner==1)ChoiceTags[i]="2P 独特装备 · 未拥有";
        ChoiceDescriptions[i]=ItemDescription(ChoiceOwner,item);
        ChoiceCombos[i]=ComboHint(ChoiceOwner,item);
      }
    }
  }
  static void ChooseReward(int index) {
    if(Phase!=2 || index<0 || index>2)return;
    int item=Choices[index];
    if(item<0){
      if(Mode==0)Coins+=20;
      else {Stamina=MaxStamina;Energy=MaxEnergy;}
    }else GrantItem(ChoiceOwner,item);
    if(ChoiceOwner==0){
      if(Mode==1){ChoiceOwner=1;MakeChoices();UpdateInventory();return;}
      UpdateInventory();RouteAfterReward();return;
    }
    if(PendingRound==1){Round++;Level=Round;}
    PendingRound=0;SetupLevel();Phase=1;ResetInputs();UpdateInventory();
  }
  static string ItemName(int owner,int id) {
    if(id<0)return "路上补给";
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
      if(id==11)return "防滑鞋底";
      if(id==12)return "折叠路障";
      if(id==13)return "信号遥控器";
      if(id==14)return "奶茶诱饵";
      if(id==15)return "玩具无人机";
      if(id==16)return "降噪耳塞";
      if(id==17)return "荧光贴";
      if(id==18)return "绝缘雨衣";
      if(id==19)return "急救包";
      if(id==20)return "步数手表";
      if(id==21)return "便利店券";
      if(id==22)return "反向充电线";
      if(id==23)return "回家指南针";
      return "未知道具";
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
      if(id==0)return "Q：车流减速 55%，持续 3 秒。\n冷却 18 秒。";
      if(id==1)return "F：撑伞 4 秒，抵挡一次撞击。\n冷却 16 秒。";
      if(id==2)return "R：恢复 55 体力，听话 3 秒。\n冷却 20 秒。";
      if(id==3)return "牵手距离增加 35。\n离同学稍远也能牵住。";
      if(id==4 && Mode==1)return "护送员移动速度增加 10%。\n更快穿过车流空隙。";
      if(id==4)return "两人移动速度增加 10%。\n更快穿过车流空隙。";
      if(id==5)return "口哨范围增加 60。\n口哨冷却缩短 15%。";
      if(id==6 && Mode==1)return "缩短 2P 挣脱时间。\n口哨听话时间稍微延长。";
      if(id==6)return "同学安静的间隔延长。\n降低突然分心的频率。";
      if(id==7)return "体力上限增加 30。\n获得时立即补满体力。";
      if(id==8)return "最大生命和当前生命各 +1。\n本局只能获得一次。";
      if(id==9)return "松手后体力恢复增加 25%。\n和步数手表形成走路流派。";
      if(id==10)return "受伤后的无敌延长 0.5 秒。\n可以与急救包组合。";
      if(id==11)return "牵手消耗减少 10%。\n降低乱走；双人削弱滑步。";
      if(id==12)return "T：停止周围车流 2.6 秒。\n范围 160，冷却 20 秒。";
      if(id==13)return "T：停止最近车道 2.3 秒。\n冷却 22 秒。";
      if(id==14)return "T：在附近安全处放奶茶。\n引导 4 秒，冷却 18 秒。";
      if(id==15)return "T：引导同学到下一安全岛。\n引导 3.5 秒，冷却 24 秒。";
      if(id==16)return "同学分心持续时间缩短。\n安静时更少突然闹脾气。";
      if(id==17)return "双方附近 100 范围车流\n持续减速 8%。";
      if(id==18)return "抵消雨天的体力恢复惩罚。\n可和鞋底、反光伞组合。";
      if(id==19)return "致命撞击时回到安全点。\n保住 1 生命，每局一次。";
      if(id==20)return "每走 180 步距，主动道具\n和口哨冷却减少 1 秒。";
      if(id==21)return "商店每件装备便宜 6 金币。\n与奶茶搭配再减价与冷却。";
      if(id==22)return "牵手时每秒补回 1.5 体力。\n搭配充电宝提高补给效率。";
      return "双方到达安全岛时恢复\n20 体力，每路段触发一次。";
    }
    if(id==0)return "J：靠近时扣 1P 24 点体力。\n耗能 30，冷却 12 秒。";
    if(id==1)return "K：沿当前方向滑步并挣脱。\n耗能 28，冷却 10 秒。";
    if(id==2)return "L：四秒内抵挡一次口哨。\n耗能 35，冷却 15 秒。";
    if(id==3)return "挣脱冷却减少 12%。\n免牵时间增加 0.12 秒。";
    if(id==4)return "能量上限增加 25。\n获得时补满能量。";
    if(id==5)return "被牵时增加 1P 体力消耗。\n牵手消耗增加 12%。";
    if(id==6)return "假通知范围增加 40。\n额外扣除 4 点体力。";
    return "能量自然恢复增加 20%。\n基础恢复 8 点每秒。";
  }
  static int ComboCount(int owner) { if(owner==0)return 20;return 3; }
  static int ComboPartA(int owner,int index) {
    if(owner==1){if(index==0)return 0;if(index==1)return 1;return 2;}
    if(index==0)return 0;if(index==1)return 1;if(index==2)return 2;if(index==3)return 3;
    if(index==4)return 12;if(index==5)return 13;if(index==6)return 14;if(index==7)return 15;
    if(index==8)return 15;if(index==9)return 16;if(index==10)return 18;if(index==11)return 19;
    if(index==12)return 20;if(index==13)return 21;if(index==14)return 22;if(index==15)return 23;
    if(index==16)return 12;if(index==17)return 18;if(index==18)return 14;return 17;
  }
  static int ComboPartB(int owner,int index) {
    if(owner==1){if(index==0)return 6;if(index==1)return 3;return 4;}
    if(index==0)return 4;if(index==1)return 8;if(index==2)return 6;if(index==3)return 7;
    if(index==4)return 17;if(index==5)return 0;if(index==6)return 6;if(index==7)return 14;
    if(index==8)return 5;if(index==9)return 6;if(index==10)return 11;if(index==11)return 10;
    if(index==12)return 9;if(index==13)return 2;if(index==14)return 7;if(index==15)return 3;
    if(index==16)return 13;if(index==17)return 1;if(index==18)return 2;return 8;
  }
  static bool ComboActive(int owner,int index) {
    if(index<0 || index>=ComboCount(owner))return false;
    int a=ComboPartA(owner,index);int b=ComboPartB(owner,index);
    if(owner==0){if(Mode==1 && (a>=12 || b>=12))return false;return P1Combo(a,b);}
    return P2Combo(a,b);
  }
  static string ComboName(int owner,int index) {
    if(owner==1){if(index==0)return "99+ 未读";if(index==1)return "碎步大师";return "连播结界";}
    if(index==0)return "赶早八";if(index==1)return "移动红绿灯";if(index==2)return "哄学套餐";
    if(index==3)return "牢牢牵住";if(index==4)return "发光施工队";if(index==5)return "借来三秒";
    if(index==6)return "甜蜜路线";if(index==7)return "外卖无人机";if(index==8)return "空中广播";
    if(index==9)return "安心刷一会";if(index==10)return "雨天搭档";if(index==11)return "校医的偏爱";
    if(index==12)return "运动咖啡";if(index==13)return "奶茶会员";if(index==14)return "双向续航";
    if(index==15)return "绝不走丢";if(index==16)return "临时交通局";if(index==17)return "雨中补给";
    if(index==18)return "第二杯半价";return "全员高可见";
  }
  static string ComboRecipe(int owner,int index) {
    return Join(Join(ItemName(owner,ComboPartA(owner,index))," + "),ItemName(owner,ComboPartB(owner,index)));
  }
  static string ComboEffect(int owner,int index) {
    if(owner==1){
      if(index==0)return "通知额外扣 10 体力。\n返还 15 捣蛋能量。";
      if(index==1)return "滑步冷却再缩短 20%。\n免牵时间延长到 0.9 秒。";
      return "耳机持续 6 秒。\n抵挡两次口哨。";
    }
    if(index==0)return "慢时延长至 4 秒。\n慢时中两人再加速 20%。";
    if(index==1)return "撑伞时周围 145 范围\n车流额外减速 35%。";
    if(index==2)return "喝奶茶后听话 6 秒。\n同学不再突然停下。";
    if(index==3)return "牵手消耗再减少 25%。\n力竭可额外坚持 1.2 秒。";
    if(index==4)return "路障范围扩大到 220。\n持续延长到 4 秒。";
    if(index==5)return "遥控同时停相邻车道。\n其余车流短暂减速。";
    if(index==6)return "奶茶引导延长至 6 秒。\n目的地更远，更愿意抬头。";
    if(index==7)return "无人机携带奶茶引路。\n引导 6 秒，能到更远安全岛。";
    if(index==8)return "口哨额外增加 180 范围。\n听话时间延长至 3.8 秒。";
    if(index==9)return "分心时间进一步缩短。\n安静时更不容易分心。";
    if(index==10)return "雨天松手时额外回体力。\n每秒恢复 5 点。";
    if(index==11)return "急救时保住 2 生命。\n获得 4 秒安全无敌。";
    if(index==12)return "步距门槛降到 120。\n每次触发再恢复 8 体力。";
    if(index==13)return "奶茶冷却变为 14 秒。\n商店优惠扩大到 9 金币。";
    if(index==14)return "牵手每秒补回 4 体力。\n比单根充电线更省力。";
    if(index==15)return "安全岛恢复提高到 40。\n牵手距离额外增加 20。";
    if(index==16)return "路障和车道停止一起触发。\n共享装置冷却限制连放。";
    if(index==17)return "开伞立即恢复 20 体力。\n护盾持续额外延长 1.5 秒。";
    if(index==18)return "喝奶茶同时布置诱饵。\n将同学引到附近安全岛。";
    return "双方周围 140 范围车流\n持续减速 28%。";
  }
  static string ComboHint(int owner,int id) {
    int first=-1;
    for(int i=0;i<ComboCount(owner);i++){
      int a=ComboPartA(owner,i);int b=ComboPartB(owner,i);
      if(id==a || id==b){
        if(first<0)first=i;int other=a;if(id==a)other=b;
        bool has=false;
        if(owner==0){if(P1Items[other]>0)has=true;}
        else if(P2Items[other]>0)has=true;
        if(has)return Join("拿取即可组合：",ComboName(owner,i));
      }
    }
    if(first>=0)return Join("配方：",ComboRecipe(owner,first));
    return "独特装备 · 获得后自动搭配";
  }
  static string CurrentDeviceName() {
    if(Mode==1)return "双人专用道具";
    if(ItemActiveT<12 || ItemActiveT>15)return "尚无 T 装置";
    return ItemName(0,ItemActiveT);
  }
  static void UpdateInventory() {
    P1Inventory="";P2Inventory="";ComboText="";
    int count=P1ItemCount();
    for(int i=0;i<count;i++){
      if(P1Items[i]>0)P1Inventory=Join(P1Inventory,Join(ItemName(0,i),"\n"));
    }
    for(int j=0;j<8;j++){
      if(P2Items[j]>0)P2Inventory=Join(P2Inventory,Join(ItemName(1,j),"\n"));
    }
    if(P1Inventory=="")P1Inventory="背包还是空的\n探索路线，寻找装备";
    if(P2Inventory=="")P2Inventory="赛前和局间选装\nEnter 默认可用";
    for(int owner=0;owner<2;owner++){
      for(int k=0;k<ComboCount(owner);k++){
        if(ComboActive(owner,k))ComboText=Join(ComboText,Join(ComboName(owner,k),"\n"));
      }
    }
    MaxStamina=100+P1Items[7]*30;MaxEnergy=100+P2Items[4]*25;
    Stamina=Min(Stamina,MaxStamina);Energy=Min(Energy,MaxEnergy);
  }
  static float ItemLinkRange() {
    float range=85+35*P1Items[3];
    if(Mode==0 && P1Combo(23,3))range+=20;
    return range;
  }
  static float ItemWhistleRange() {
    float range=260+60*P1Items[5];
    if(Mode==0 && P1Combo(15,5))range+=180;
    return range;
  }
  static void ItemOnWhistle() {
    if(Mode==0 && P1Combo(15,5))Force=Max(Force,3.8);
  }
  static int ItemShopDiscount() {
    if(Mode!=0 || P1Items[21]==0)return 0;
    if(P1Combo(21,2))return 9;return 6;
  }
  static int NearestSignalLane() {
    int result=0;float nearest=10000;
    for(int i=0;i<LaneCount;i++){float d=Abs(P1Y-LaneY[i]);if(d<nearest){result=i;nearest=d;}}
    return result;
  }
  static void PlaceBeacon(float range,float duration) {
    float target=76;
    for(int m=0;m<MedianCount;m++){
      if(MedianY[m]<P2Y-12 && MedianY[m]>target)target=MedianY[m];
    }
    if(Abs(target-P2Y)>range)target=Checkpoint;
    BeaconX=Min(1000,Max(80,P1X));BeaconY=target;BeaconTime=duration;
    Mood=0;MoodWait=Max(MoodWait,duration);
  }
  static void CycleGadget() {
    if(Mode!=0)return;
    int candidate=ItemActiveT;
    if(candidate<12 || candidate>15)candidate=11;
    for(int i=0;i<4;i++){
      candidate++;if(candidate>15)candidate=12;
      if(P1Items[candidate]>0){
        ItemActiveT=candidate;Bubble=Join("T 装置：",CurrentDeviceName());BubbleTime=1.8;return;
      }
    }
    ItemActiveT=-1;
  }
  static void GadgetSkill() {
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
  static void P1Skill(int id) {
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
  static void ItemTick(float dt) {
    if(Mode!=0)return;
    GadgetCD=Max(0,GadgetCD-dt);RoadblockTime=Max(0,RoadblockTime-dt);
    SignalTime=Max(0,SignalTime-dt);BeaconTime=Max(0,BeaconTime-dt);
    if(P1Items[16]>0 && Linked==0){
      if(Mood>0){
        float extra=.4;if(P1Combo(16,6))extra=1;
        MoodTimer=Max(.001,MoodTimer-extra*dt);
      }else{
        float wait=.12;if(P1Combo(16,6))wait=.32;
        MoodWait+=wait*dt;
      }
    }
    if(P1Items[18]>0 && Modifier=="雨天" && Linked==0 && RegenDelay<=0){
      Stamina=Min(MaxStamina,Stamina+5*(1+.25*P1Items[9])*dt);
      if(P1Combo(18,11))Stamina=Min(MaxStamina,Stamina+5*dt);
    }
    if(P1Items[22]>0 && Linked==1 && Tired==0){
      float rate=1.5;if(P1Combo(22,7))rate=4;
      Stamina=Min(MaxStamina,Stamina+rate*dt);
    }
    float walkX=P1X-WatchLastX;float walkY=P1Y-WatchLastY;
    float walked=Sqrt(walkX*walkX+walkY*walkY);
    WatchLastX=P1X;WatchLastY=P1Y;
    if(P1Items[20]>0 && walked<25){
      WatchDistance+=walked;
      float threshold=180;if(P1Combo(20,9))threshold=120;
      if(WatchDistance>=threshold){
        WatchDistance-=threshold;
        ClockCD=Max(0,ClockCD-1);ShieldCD=Max(0,ShieldCD-1);TeaCD=Max(0,TeaCD-1);
        WhistleCD=Max(0,WhistleCD-1);GadgetCD=Max(0,GadgetCD-1);
        if(P1Combo(20,9))Stamina=Min(MaxStamina,Stamina+8);
      }
    }
    if(P1Items[23]>0 && CompassRested==0){
      for(int m=0;m<MedianCount;m++){
        if(Abs(P1Y-MedianY[m])<16 && Abs(P2Y-MedianY[m])<16 && CompassRested==0){
          CompassRested=1;float amount=20;if(P1Combo(23,3))amount=40;
          Stamina=Min(MaxStamina,Stamina+amount);
          Bubble="安全岛到了，歇口气。";BubbleTime=2;
        }
      }
    }
  }
  static float ItemTrafficRate(int car) {
    if(Mode!=0)return 1;
    float rate=1;
    if(RoadblockTime>0){
      float radius=160;if(P1Combo(12,17))radius=220;
      float dx=CarX[car]-RoadblockX;float dy=CarY[car]-RoadblockY;
      if(dx*dx+dy*dy<radius*radius)rate=0;
    }
    if(SignalTime>0){
      if(CarLane[car]==SignalLane)rate=0;
      if(P1Combo(13,0) && CarLane[car]==SignalLane+1)rate=0;
      if(P1Combo(13,0) && SignalLane==LaneCount-1 && CarLane[car]==SignalLane-1)rate=0;
    }
    if(P1Items[17]>0){
      float radius=100;float factor=.92;
      if(P1Combo(17,8)){radius=140;factor=.72;}
      float dx=CarX[car]-P1X;float dy=CarY[car]-P1Y;
      float ex=CarX[car]-P2X;float ey=CarY[car]-P2Y;
      if(dx*dx+dy*dy<radius*radius || ex*ex+ey*ey<radius*radius)rate*=factor;
    }
    return rate;
  }
  static bool ItemBeforeHit() {
    if(Mode!=0 || Shield>0 || Lives>1 || P1Items[19]==0 || MedkitUsed>0)return false;
    MedkitUsed=1;Lives=1;Invincible=3;
    if(P1Combo(19,10)){Lives=2;Invincible=4;}
    StageHits++;P1X=519;P1Y=Checkpoint;P2X=557;P2Y=Min(626,Checkpoint+8);
    Arrived1=0;Arrived2=0;Mood=0;MoodWait=5;Force=2;BeaconTime=0;
    Bubble="急救包生效！本局仅此一次。";BubbleTime=2.5;
    return true;
  }
}
