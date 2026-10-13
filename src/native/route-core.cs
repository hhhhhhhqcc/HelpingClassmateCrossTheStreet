using System;
using System.Windows.Forms;

// Three acts, branching visible routes, deterministic nodes and priced decisions.
public partial class GameForm : Form {
  static int RouteAct, RouteRow, RouteColumn, CurrentNode, NodeKind, Coins, RewardContext;
  static int RouteEvent, RouteEventItem, RouteRewardSequence;
  static int[] ShopItems, ShopPrices, ShopBought, NodeTypes, NodeVisited;
  static string NodeTitle, NodeText;

  static string RouteNodeName(int kind) {
    if(kind==0)return "护送路段";
    if(kind==1)return "精英路口";
    if(kind==2)return "街头事件";
    if(kind==3)return "便利商店";
    if(kind==4)return "安全驿站";
    if(kind==5)return "区域首领";
    return "";
  }
  static string RouteActName() {
    if(RouteAct==1)return "放学街区";
    if(RouteAct==2)return "商业街区";
    return "回家干道";
  }
  static bool RouteCanChoose(int col) {
    if(Phase!=5 || col<0 || col>2 || RouteRow<0 || RouteRow>5)return false;
    int index=(RouteAct-1)*18+RouteRow*3+col;
    if(NodeTypes[index]<0 || NodeVisited[index]!=0)return false;
    if(RouteColumn>=0 && Abs(col-RouteColumn)>1)return false;
    return true;
  }
  static void RouteStart() {
    RouteAct=1;RouteRow=0;RouteColumn=-1;CurrentNode=-1;NodeKind=0;
    Coins=45;RewardContext=1;RouteRewardSequence=0;SelectedChoice=1;
    SetRandom(Seed+17123);
    for(int i=0;i<54;i++){NodeVisited[i]=0;NodeTypes[i]=0;}
    for(int act=0;act<3;act++){
      int start=act*18;
      for(int row=1;row<5;row++){
        for(int col=0;col<3;col++){
          int roll=RandomInt(0,100);int kind=0;
          if(roll>=34 && roll<52)kind=1;
          else if(roll>=52 && roll<72)kind=2;
          else if(roll>=72 && roll<86)kind=3;
          else if(roll>=86)kind=4;
          NodeTypes[start+row*3+col]=kind;
        }
      }
      // Each act offers a reachable shop, rest stop and elite challenge.
      NodeTypes[start+6]=3;NodeTypes[start+11]=4;NodeTypes[start+13]=1;
      NodeTypes[start+15]=-1;NodeTypes[start+16]=5;NodeTypes[start+17]=-1;
    }
    NodeTitle="出发前，选一件随身装备";
    NodeText="装备每件只能获得一次。沿分岔路线收集搭配，护送同学回家。";
    ChoiceOwner=0;MakeChoices();Phase=2;ResetInputs();
  }
  static void RouteMap() {
    Phase=5;RewardContext=0;
    SelectedChoice=1;
    if(RouteColumn>=0)SelectedChoice=RouteColumn;
    if(!RouteCanChoose(SelectedChoice)){
      for(int col=0;col<3;col++){if(RouteCanChoose(col))SelectedChoice=col;}
    }
    NodeTitle=Join(Join("第 ",N(RouteAct)),Join(" 幕 · ",RouteActName()));
    NodeText="选择本层相邻路线。精英奖励更多；商店能补齐组合；安全驿站恢复状态。";
    ResetInputs();
  }
  static void RouteChoose(int col) {
    if(!RouteCanChoose(col))return;
    CurrentNode=(RouteAct-1)*18+RouteRow*3+col;
    RouteColumn=col;NodeKind=NodeTypes[CurrentNode];NodeVisited[CurrentNode]=1;
    SelectedChoice=0;
    NodeTitle=RouteNodeName(NodeKind);
    SetRandom(Seed+CurrentNode*4729+777);
    if(NodeKind==0 || NodeKind==1 || NodeKind==5){
      Level=(RouteAct-1)*3+ToInt(RouteRow*.5)+1;
      SetupLevel();
      // Unique route seed changes the traffic even at the same difficulty.
      SetRandom(Seed+CurrentNode*4729+2719);
      if(NodeKind==1){
        NodeTitle="精英路口 · 车流夹击";Modifier="早高峰";
        for(int lane=0;lane<LaneCount;lane++){LaneSpeed[lane]*=1.12;LaneTimer[lane]*=.84;}
      }
      if(NodeKind==5){
        NodeTitle="首领 · 校门车队";Modifier="校门车队";
        if(RouteAct==2){NodeTitle="首领 · 施工绕行";Modifier="施工绕行";}
        if(RouteAct==3){NodeTitle="首领 · 连播路口";Modifier="连播路口";}
        for(int lane=0;lane<LaneCount;lane++){LaneSpeed[lane]*=1.16;LaneTimer[lane]*=.76;}
      }
      NodeText="两人抵达对岸后获得金币与一件新装备。生命在整趟旅程中保留。";
      Bubble=NodeTitle;BubbleTime=3;Phase=1;RewardContext=0;ResetInputs();return;
    }
    if(NodeKind==2){RoutePrepareEvent();Phase=6;ResetInputs();return;}
    if(NodeKind==3){RoutePrepareShop();Phase=7;ResetInputs();return;}
    if(NodeKind==4){RoutePrepareRest();Phase=8;ResetInputs();}
  }
  static void RouteClearRoad() {
    if(Mode!=0 || Phase!=1)return;
    Score+=250+MaxInt(0,70-ToInt(Elapsed))*3;
    int income=18+RouteAct*6;
    if(NodeKind==1)income=35+RouteAct*8;
    if(NodeKind==5)income=55+RouteAct*10;
    if(StageHits==0){income+=12;Score+=50;}
    Coins+=income;RouteRewardSequence++;
    NodeText=Join(Join("成功护送，获得 ",N(income))," 金币。选一件未拥有的装备，继续规划路线。");
    NodeTitle="路段奖励";
    ChoiceOwner=0;RewardContext=2;MakeChoices();Phase=2;ResetInputs();
  }
  static void RouteAfterReward() {
    if(RewardContext==1){RouteMap();return;}
    if(RewardContext==2){RouteAdvance();return;}
  }
  static void RouteAdvance() {
    if(CurrentNode>=0)NodeVisited[CurrentNode]=2;
    RewardContext=0;RouteRow++;
    if(RouteRow>5){
      if(RouteAct>=3){EndGame(1);return;}
      RouteAct++;RouteRow=0;RouteColumn=-1;
    }
    CurrentNode=-1;RouteMap();
  }
  static string RouteGiftName() {
    if(RouteEventItem<0)return "20 金币";
    return ItemName(0,RouteEventItem);
  }
  static void RouteGiveGift() {
    if(RouteEventItem<0){Coins+=20;return;}
    GrantItem(0,RouteEventItem);UpdateInventory();
  }
  static void RouteSetCard(int card,string name,string tag,string description,string combo) {
    ChoiceNames[card]=name;ChoiceTags[card]=tag;ChoiceDescriptions[card]=description;ChoiceCombos[card]=combo;
  }
  static void RoutePrepareEvent() {
    RouteEvent=RandomInt(0,4);RouteEventItem=PickUnowned(0);SelectedChoice=0;
    string gift=RouteGiftName();
    if(RouteEvent==0){
      NodeTitle="事件 · 校门口的钱包";
      NodeText="你们捡到一个钱包。失主就在附近，感谢礼已明确展示。";
      RouteSetCard(0,"归还钱包","获得一件装备",Join("失主送给你：\n",gift),"立即获得，整趟旅程保留");
      RouteSetCard(1,"帮忙寻找失主","获得 35 金币","带同学一起跑腿，\n获得 35 金币的感谢费。","用于便利商店补齐组合");
      RouteSetCard(2,"交给门卫","恢复 1 生命","门卫请你们坐下歇一会儿，\n恢复 1 生命，并获得 12 金币。","生命恢复不超过当前上限");
    }else if(RouteEvent==1){
      NodeTitle="事件 · 施工围挡";
      NodeText="工人需要帮忙搬开围挡，报酬明确；生命不足时可选其他方案。";
      RouteSetCard(0,"出力搬运","消耗 1 生命",Join("获得 20 金币和：\n",gift),"需要生命至少 2，避免致命代价");
      RouteSetCard(1,"买杯热饮","支付 25 金币","花费 25 金币，\n恢复 2 生命。","需要足够金币，恢复不超过上限");
      RouteSetCard(2,"按指示绕行","获得 10 金币","帮忙张贴绕行提示，\n获得 10 金币。","无额外代价");
    }else if(RouteEvent==2){
      NodeTitle="事件 · 手机电量告急";
      NodeText="同学终于抬头了。附近店主愿意给你们提供几种帮助。";
      RouteSetCard(0,"购买随身用品","支付 18 金币",Join("获得店主的推荐装备：\n",gift),"购买前已展示具体装备");
      RouteSetCard(1,"坐下充电","恢复 1 生命","让同学安静充电，\n你也恢复 1 生命。","休息结束后继续路线");
      RouteSetCard(2,"替店主送货","消耗 1 生命","获得 38 金币，\n需要生命至少 2。","可用于下一个商店");
    }else{
      NodeTitle="事件 · 排水沟里的手机";
      NodeText="同学差点把手机滑进沟里。帮路人找回另一部手机，可以获得报酬。";
      RouteSetCard(0,"伸手捞出来","消耗 1 生命",Join("获得 30 金币和：\n",gift),"需要生命至少 2，代价明确");
      RouteSetCard(1,"租一把长夹子","支付 20 金币",Join("安全取回，获得：\n",gift),"需要足够金币");
      RouteSetCard(2,"等工作人员","恢复 1 生命","在安全处等待，\n恢复 1 生命。","不花金币，不承担风险");
    }
  }
  static int RouteMaxLives() { return 4+P1Items[8]; }
  static void RouteHeal(int amount) {
    Lives+=amount;
    if(Lives>RouteMaxLives())Lives=RouteMaxLives();
  }
  static bool RoutePayCoins(int amount) {
    if(Coins<amount){NodeText=Join(Join("金币不足：需要 ",N(amount)),Join("，目前拥有 ",N(Coins)));return false;}
    Coins-=amount;return true;
  }
  static bool RoutePayLife() {
    if(Lives<2){NodeText="生命不足：这个选项需要至少 2 生命。可以选择其他方案。";return false;}
    Lives--;return true;
  }
  static void RouteSelect(int index) {
    if(index<0 || index>2)return;
    if(Phase==8){
      if(index==0)RouteHeal(2);
      else if(index==1)Coins+=20;
      else RouteGiveGift();
      UpdateInventory();RouteAdvance();return;
    }
    if(Phase!=6)return;
    if(RouteEvent==0){
      if(index==0)RouteGiveGift();
      else if(index==1)Coins+=35;
      else {RouteHeal(1);Coins+=12;}
    }else if(RouteEvent==1){
      if(index==0){if(!RoutePayLife())return;Coins+=20;RouteGiveGift();}
      else if(index==1){if(!RoutePayCoins(25))return;RouteHeal(2);}
      else Coins+=10;
    }else if(RouteEvent==2){
      if(index==0){if(!RoutePayCoins(18))return;RouteGiveGift();}
      else if(index==1)RouteHeal(1);
      else {if(!RoutePayLife())return;Coins+=38;}
    }else{
      if(index==0){if(!RoutePayLife())return;Coins+=30;RouteGiveGift();}
      else if(index==1){if(!RoutePayCoins(20))return;RouteGiveGift();}
      else RouteHeal(1);
    }
    UpdateInventory();RouteAdvance();
  }
  static int RoutePickStock() {
    int count=P1ItemCount();int first=RandomInt(0,count);
    for(int offset=0;offset<count;offset++){
      int id=(first+offset)%count;bool available=P1Items[id]==0;
      for(int card=0;card<3;card++){if(ShopItems[card]==id)available=false;}
      if(available)return id;
    }
    return -1;
  }
  static void RoutePrepareShop() {
    NodeTitle="便利商店";NodeText="装备只能购买一次。按 1 / 2 / 3 购买，Enter 购买选中商品，0 离开。";
    SelectedChoice=0;
    for(int i=0;i<3;i++){ShopItems[i]=-1;ShopBought[i]=0;ShopPrices[i]=0;}
    for(int card=0;card<3;card++){
      int id=RoutePickStock();ShopItems[card]=id;
      if(id<0){RouteSetCard(card,"货架已空","没有可购买的新装备","这个货架没有更多新装备。\n可选其他商品，或继续前进。","按 0 离开商店");}
      else {
        ShopPrices[card]=26+RouteAct*4+(id%5)*4-ItemShopDiscount();
        if(ShopPrices[card]<1)ShopPrices[card]=1;
        RouteSetCard(card,ItemName(0,id),Join(N(ShopPrices[card])," 金币"),ItemDescription(0,id),ComboHint(0,id));
      }
    }
  }
  static void RouteBuy(int index) {
    if(Phase!=7 || index<0 || index>2)return;
    int id=ShopItems[index];
    if(id<0){NodeText="这个货架没有新装备。可以选择其他商品，或按 0 离开。";return;}
    if(ShopBought[index]==1){NodeText="这件装备已经买过了。可以购买其他商品，或按 0 离开。";return;}
    if(!RoutePayCoins(ShopPrices[index]))return;
    GrantItem(0,id);ShopBought[index]=1;UpdateInventory();
    for(int card=0;card<3;card++){if(ShopItems[card]>=0 && ShopBought[card]==0){ShopPrices[card]=26+RouteAct*4+(ShopItems[card]%5)*4-ItemShopDiscount();if(ShopPrices[card]<1)ShopPrices[card]=1;ChoiceTags[card]=Join(N(ShopPrices[card])," 金币");}}
    ChoiceTags[index]="已购买";
    ChoiceCombos[index]="已经加入背包，不再重复出售";
    NodeText=Join(Join("已获得 ",ItemName(0,id)),"。还可购买其他商品，或按 0 离开。");
    ResetInputs();
  }
  static void RouteLeaveShop() { if(Phase==7)RouteAdvance(); }
  static void RoutePrepareRest() {
    NodeTitle="安全驿站";NodeText="三项服务任选一项，之后继续路线。";
    RouteEventItem=PickUnowned(0);SelectedChoice=0;
    RouteSetCard(0,"休息一下","恢复 2 生命","恢复 2 生命，\n不超过当前生命上限。","带着更好的状态挑战下一路段");
    RouteSetCard(1,"顺手帮忙","获得 20 金币","帮驿站整理物资，\n获得 20 金币。","存钱补齐缺少的组合组件");
    RouteSetCard(2,"翻找失物箱","免费获得失物箱礼物",Join("驿站愿意送给你：\n",RouteGiftName()),"具体奖励已展示，装备整趟旅程保留");
  }
  static void RouteCycle(int direction) {
    for(int step=0;step<3;step++){
      SelectedChoice=(SelectedChoice+direction+3)%3;
      if(Phase!=5 || RouteCanChoose(SelectedChoice))return;
    }
  }
  static void RouteKeys() {
    if(Phase<5 || Phase>8)return;
    if(Pressed(37))RouteCycle(-1);
    if(Pressed(39))RouteCycle(1);
    int selection=-1;
    if(Pressed(49) || Pressed(97))selection=0;
    else if(Pressed(50) || Pressed(98))selection=1;
    else if(Pressed(51) || Pressed(99))selection=2;
    else if(Pressed(13))selection=SelectedChoice;
    if(Phase==5){if(selection>=0)RouteChoose(selection);}
    else if(Phase==7){
      if(Pressed(48) || Pressed(96)){RouteLeaveShop();return;}
      if(selection>=0)RouteBuy(selection);
    }else if(selection>=0)RouteSelect(selection);
  }
  static void RouteClick(float x,float y) {
    if(Phase==5){
      float centerY=620-RouteRow*78;
      if(y<centerY-25 || y>centerY+25)return;
      for(int col=0;col<3;col++){
        float centerX=330+295*col;
        if(x>=centerX-105 && x<=centerX+105){RouteChoose(col);return;}
      }
    }else if(Phase==6 || Phase==7 || Phase==8){
      if(Phase==7 && x>=435 && x<=815 && y>=640 && y<=696){RouteLeaveShop();return;}
      if(y<330 || y>568)return;
      for(int index=0;index<3;index++){
        float left=150+315*index;
        if(x>=left && x<=left+290){
          if(Phase==7)RouteBuy(index);else RouteSelect(index);
          return;
        }
      }
    }
  }
}
