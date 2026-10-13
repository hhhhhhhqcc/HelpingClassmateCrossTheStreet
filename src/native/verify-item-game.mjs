import { executeMethod } from './verify-subset.mjs';

const hostSigs={
 Rect:['void','float','float','float','float','int'],Circle:['void','float','float','float','float','int'],
 Outline:['void','float','float','float','float','int'],Line:['void','float','float','float','float','int'],
 Text:['void','string','float','float','float','int'],Key:['bool','int'],HasFocus:['bool'],Rand:['float'],
 RandomInt:['int','int','int'],SetRandom:['void','int'],NewSeed:['int'],Join:['string','string','string'],
 N:['string','int'],FloatText:['string','float'],Min:['float','float','float'],Max:['float','float','float'],
 Abs:['float','float'],Sqrt:['float','float'],Sin:['float','float'],Cos:['float','float'],Sound:['void','int']
};

export function runItemChecks(game){
 const {b,h,fields,result}=game;
 const tests=[];
 let seed=12345, state={};
 const random=()=>{seed=(Math.imul(seed,1664525)+1013904223)>>>0;return seed/4294967296;};
 const functions={};
 const implementations={
  Rect:()=>{},Circle:()=>{},Outline:()=>{},Line:()=>{},Text:()=>{},
  Key:()=>false,HasFocus:()=>true,Rand:random,
  RandomInt:(min,max)=>min+Math.floor(random()*(max-min)),SetRandom:v=>{seed=v>>>0;},
  NewSeed:()=>54321,Join:(a,c)=>(a||'')+(c||''),N:v=>String(v),
  FloatText:v=>Number(v).toFixed(1),Min:Math.min,Max:Math.max,Abs:Math.abs,Sqrt:Math.sqrt,Sin:Math.sin,Cos:Math.cos,Sound:()=>{}
 };
 for(const [name,sig] of Object.entries(hostSigs)){
  functions[h.methods[name].token]={returnType:sig[0],argTypes:sig.slice(1),fn:implementations[name]};
 }
 for(let i=0;i<b.memberRefs.length;i++){if(b.memberRefs[i].name==='op_Equality')functions[0x0a000001+i]={returnType:'bool',argTypes:['string','string'],fn:(a,c)=>a===c};}
 const get=name=>state[fields[name].token];
 const set=(name,value)=>{state[fields[name].token]=value;};
 const call=(name,...args)=>{
  const token=result.methods[name].token;
  return executeMethod(b.methods.find(m=>m.token===token),args,state,b.methods,functions);
 };
 const assert=(value,message)=>{if(!value)throw Error(message);};
 const near=(a,c)=>Math.abs(a-c)<.0001;
 const fresh=(ids=[],mode=0)=>{
  state={};
  for(const [name,f] of Object.entries(fields)){
   if(f.type.endsWith('[]')){
    let n=game.arrayLengths?.[name];
    if(!n)n=/^Car/.test(name)?96:/^Lane/.test(name)?6:name==='MedianY'?3:name==='P1Items'?24:name==='P2Items'?8:/Input$/.test(name)?256:3;
    set(name,Array(n).fill(f.type==='string[]'?'':0));
   }else set(name,f.type==='string'?'':0);
  }
  set('Mode',mode);set('Phase',1);set('Level',1);set('Round',1);set('Lives',4);
  set('MaxStamina',100);set('Stamina',100);set('MaxEnergy',100);set('Energy',100);
  set('P1X',519);set('P1Y',596);set('P2X',559);set('P2Y',607);set('Checkpoint',594);
  set('LaneCount',3);set('MedianCount',1);get('MedianY')[0]=360;
  get('LaneY')[0]=180;get('LaneY')[1]=320;get('LaneY')[2]=460;
  get('LaneTimer').fill(100);
  set('Modifier','正常车流');set('MoodWait',5);
  call('ItemResetRun');
  for(const id of ids)call('GrantItem',0,id);
  call('ItemSetup');
 };
 const test=(name,fn)=>{try{fn();}catch(e){throw Error(name+': '+e.message);}tests.push({name,passed:true});};
 const car=(x,y,lane=0)=>{
  get('CarActive')[0]=1;get('CarX')[0]=x;get('CarY')[0]=y;get('CarLane')[0]=lane;
  get('CarSpeed')[0]=200;get('CarDirection')[0]=1;get('CarW')[0]=70;get('CarH')[0]=40;
 };

 test('All 24 solo items are unique; repeat grant cannot raise rank or life',()=>{
  fresh();
  for(let i=0;i<24;i++){call('GrantItem',0,i);call('GrantItem',0,i);assert(get('P1Items')[i]===1,'Rank must remain one');}
  assert(get('Lives')===5,'Vest life must only be granted once');
  assert(call('ItemOwnedCount',0)===24,'Solo catalog length');
  assert(get('ItemActiveT')===15,'Newest gadget is selected');
 });
 test('Two-player pool keeps 12/8 unique equipment',()=>{
  fresh([],1);call('GrantItem',0,12);assert(get('P1Items')[12]===0,'Solo device leaked to duel');
  for(let i=0;i<8;i++){call('GrantItem',1,i);call('GrantItem',1,i);assert(get('P2Items')[i]===1,'2P item rank');}
  assert(call('P1ItemCount')===12 && call('P2ItemCount')===8,'Duel pool');
 });
 test('Three choices are distinct and never owned',()=>{
  fresh([0,1,2,3,4,5,6,7,8,9,10]);
  for(let i=0;i<15;i++){
   call('MakeChoices');const choices=get('Choices');
   assert(new Set(choices).size===3,'Repeated reward');
   assert(choices.every(id=>id>=0 && get('P1Items')[id]===0),'Owned reward');
   call('GrantItem',0,choices[0]);
   if(call('ItemOwnedCount',0)>=22)break;
  }
 });
 test('Exhausted reward pool uses resource reward rather than negative indexing',()=>{
  fresh(Array.from({length:24},(_,i)=>i));set('ChoiceOwner',0);call('MakeChoices');
  assert(get('Choices').every(id=>id===-1),'Exhausted pool');
  set('Phase',2);set('Coins',0);call('ChooseReward',0);assert(get('Coins')===20,'Fallback gold');
 });
 test('PickUnowned returns -1 when all unique equipment is owned',()=>{
  fresh(Array.from({length:24},(_,i)=>i));assert(call('PickUnowned',0)===-1,'Missing exhausted sentinel');
 });
 test('Tab cycles owned gadgets; shared cooldown prevents cycling spam',()=>{
  fresh([12,14]);assert(get('ItemActiveT')===14,'Latest active');
  call('CycleGadget');assert(get('ItemActiveT')===12,'Tab cycle');
  call('GadgetSkill');const cd=get('GadgetCD');call('CycleGadget');call('GadgetSkill');
  assert(get('ItemActiveT')===14 && get('GadgetCD')===cd && get('BeaconTime')===0,'Cooldown bypass');
 });
 test('0 赶早八 changes slow duration',()=>{
  fresh([0,4]);call('P1Skill',0);assert(get('Slow')===4,'Slow combo did not extend');
 });
 test('1 移动红绿灯 changes nearby car speed while shield is active',()=>{
  fresh([1,8]);set('P1X',400);set('P1Y',180);set('P2X',440);set('P2Y',180);car(500,180);
  call('P1Skill',1);call('Traffic',.5);assert(near(get('CarX')[0],565),'Shield aura did not slow traffic');
 });
 test('2 哄学套餐 increases forced cooperation to six seconds',()=>{
  fresh([2,6]);call('P1Skill',2);assert(get('Force')===6,'Tea and candy combo');
 });
 test('3 牢牢牵住 provides exhaustion grace',()=>{
  fresh([3,7]);set('Stamina',0);get('Input')[32]=1;call('StepGame',.01);
  assert(get('GraceUsed')===1 && get('GraceTime')>1 && get('Linked')===1,'Link grace combo');
 });
 test('4 发光施工队 extends roadblock duration and stop radius',()=>{
  fresh([12,17]);call('GadgetSkill');car(get('P1X')+200,get('P1Y'));
  assert(get('RoadblockTime')===4 && call('ItemTrafficRate',0)===0,'Roadblock combo');
 });
 test('5 借来三秒 stops adjacent lane and slows other cars',()=>{
  fresh([13,0]);call('GadgetSkill');car(100,320,1);
  assert(get('Slow')>2 && call('ItemTrafficRate',0)===0,'Signal combo');
 });
 test('6 甜蜜路线 extends bait guidance',()=>{
  fresh([14,6]);call('GadgetSkill');assert(get('BeaconTime')===6,'Candy bait combo');
 });
 test('7 外卖无人机 reaches next safe island with longer guidance',()=>{
  fresh([15,14]);set('ItemActiveT',15);call('GadgetSkill');
  assert(get('BeaconTime')===6 && get('BeaconY')===360,'Drone bait route');
 });
 test('8 空中广播 increases whistle reach and cooperation',()=>{
  fresh([15,5]);call('ItemOnWhistle');
  assert(call('ItemWhistleRange')===500 && near(get('Force'),3.8),'Drone whistle');
 });
 test('9 安心刷一会 shortens phone distraction',()=>{
  fresh([16,6]);set('Mood',1);set('MoodTimer',2);call('ItemTick',1);assert(get('MoodTimer')===1,'Earplug candy');
 });
 test('10 雨天搭档 adds rain recovery',()=>{
  fresh([18,11]);set('Stamina',20);set('Modifier','雨天');call('ItemTick',1);
  assert(get('Stamina')===30,'Raincoat shoes');
 });
 test('11 校医的偏爱 rescues once with two life and four seconds immunity',()=>{
  fresh([19,10]);set('Lives',1);assert(call('ItemBeforeHit')===1,'Medkit failed');
  assert(get('Lives')===2 && get('Invincible')===4,'Medkit badge');
  call('ItemSetup');set('Lives',1);assert(call('ItemBeforeHit')===0,'Medkit reset at node');
 });
 test('12 运动咖啡 reduces walking threshold and restores stamina',()=>{
  fresh([20,9]);set('Stamina',20);set('WatchDistance',115);set('P1X',get('P1X')+10);set('ClockCD',5);
  call('ItemTick',.05);assert(get('ClockCD')===4 && get('Stamina')===28,'Stepwatch coffee');
 });
 test('13 奶茶会员 shortens tea cooldown and improves shop discount',()=>{
  fresh([21,2]);call('P1Skill',2);assert(get('TeaCD')===14 && call('ItemShopDiscount')===9,'Membership combo');
 });
 test('14 双向续航 refunds four stamina per second while linked',()=>{
  fresh([22,7]);set('Stamina',20);set('Linked',1);call('ItemTick',1);assert(get('Stamina')===24,'Reverse battery');
 });
 test('15 绝不走丢 restores extra stamina once on safe island and extends link',()=>{
  fresh([23,3]);set('Stamina',20);set('P1Y',360);set('P2Y',360);call('ItemTick',1);
  assert(get('Stamina')===60 && call('ItemLinkRange')===140,'Compass band');
  call('ItemTick',1);assert(get('Stamina')===60,'Compass island repeat exploit');
 });
 test('16 临时交通局 activates both roadblock and lane stop',()=>{
  fresh([12,13]);set('ItemActiveT',12);call('GadgetSkill');
  assert(get('RoadblockTime')>0 && get('SignalTime')===1.5,'Traffic agency combo');
 });
 test('17 雨中补给 gives shield duration and stamina on activation',()=>{
  fresh([18,1]);set('Stamina',20);call('P1Skill',1);
  assert(get('Stamina')===40 && get('Shield')===5.5,'Rain shield');
 });
 test('18 第二杯半价 deploys safe bait when tea is consumed',()=>{
  fresh([14,2]);call('P1Skill',2);assert(get('BeaconTime')===4 && get('TeaCD')===20,'Tea bait combo');
 });
 test('19 全员高可见 creates permanent visible traffic slowdown',()=>{
  fresh([17,8]);car(get('P1X')+120,get('P1Y'));assert(near(call('ItemTrafficRate',0),.72),'Vest fluorescent aura');
 });
 test('Three legacy 2P combos retain actual effects without ranks',()=>{
  fresh([],1);
  for(const id of [0,6,1,3,2,4])call('GrantItem',1,id);
  set('P1X',519);set('P2X',539);set('Stamina',100);set('Energy',100);call('P2Skill',1);
  assert(get('Stamina')===62 && get('Energy')===85,'Unread combo');
  set('Energy',100);call('P2Skill',2);assert(near(get('DashCD'),8) && near(get('EscapeTime'),.9),'Slide combo');
  set('Energy',100);call('P2Skill',3);assert(get('FocusTime')===6 && get('FocusCharges')===2,'Headphone combo');
 });
 test('All recipes require both unique items and render bounded descriptions',()=>{
  fresh();
  for(let owner=0;owner<2;owner++){
   const count=call('ComboCount',owner);assert(count===(owner===0?20:3),'Catalog size');
   for(let i=0;i<count;i++){
    const a=call('ComboPartA',owner,i),c=call('ComboPartB',owner,i);
    const arr=get(owner===0?'P1Items':'P2Items');arr.fill(0);arr[a]=1;
    assert(call('ComboActive',owner,i)===0,'Half recipe activated');
    arr[c]=1;assert(call('ComboActive',owner,i)===1,'Full recipe failed');
    assert(call('ComboRecipe',owner,i).length<50,'Recipe long');
    assert(call('ComboEffect',owner,i).split('\n').length<=2,'Effect must be two lines');
   }
  }
 });
 test('Invulnerable shield does not consume emergency medkit',()=>{
  fresh([19,1]);set('Lives',1);call('P1Skill',1);
  assert(call('ItemBeforeHit')===0 && get('MedkitUsed')===0,'Medkit wasted on shield');
 });
 return {passed:tests.length,failed:0,tests};
}


// These checks exercise the real public input, traffic, collision, and shop hooks.
export function runItemIntegrationChecks(bundle,createIntegratedGameHarness){
 const tests=[];
 const assert=(v,message)=>{if(!v)throw Error(message);};
 const close=(a,b)=>Math.abs(a-b)<.001;
 const fresh=(ids=[])=>{
  const h=createIntegratedGameHarness(bundle);h.initialize(0);
  h.get('P1Items').fill(0);h.get('P2Items').fill(0);h.call('ItemResetRun');
  h.set('Phase',1);h.set('Lives',4);h.set('MaxStamina',100);h.set('Stamina',100);
  h.set('P1X',519);h.set('P1Y',596);h.set('P2X',559);h.set('P2Y',607);
  h.set('Checkpoint',594);h.set('MedianCount',1);h.get('MedianY')[0]=360;
  h.set('LaneCount',3);h.get('LaneY')[0]=180;h.get('LaneY')[1]=320;h.get('LaneY')[2]=460;
  h.set('MoodWait',20);h.set('Mood',0);h.set('Invincible',0);
  for(const id of ids)h.call('GrantItem',0,id);
  h.call('ItemSetup');h.call('ResetInputs');h.noCars();
  return h;
 };
 const car=(h,x,y,lane=0)=>{
  h.get('CarActive')[0]=1;h.get('CarX')[0]=x;h.get('CarY')[0]=y;
  h.get('CarLane')[0]=lane;h.get('CarSpeed')[0]=200;h.get('CarDirection')[0]=1;
  h.get('CarW')[0]=70;h.get('CarH')[0]=40;
 };
 const test=(name,fn)=>{try{fn();}catch(e){throw Error(name+': '+e.message);}tests.push({name,passed:true});};

 test('T input actually freezes nearby traffic for roadblock equipment',()=>{
  const h=fresh([12]);const x=h.get('P1X')+120;car(h,x,h.get('P1Y'));h.keys.add(84);h.tick(.05);
  assert(h.get('GadgetCD')>19 && close(h.get('CarX')[0],x),'T-to-traffic hook missing');
 });
 test('T drone beacon moves the classmate toward an actual safe island',()=>{
  const h=fresh([15,14]);h.set('ItemActiveT',15);const y=h.get('P2Y');h.keys.add(84);h.tick(.05);
  assert(h.get('BeaconY')===360 && h.get('P2Y')<y-5,'Beacon AI movement hook missing');
 });
 test('Passive fluorescent vest combo changes traffic displacement every frame',()=>{
  const h=fresh([17,8]);const x=h.get('P1X')+120;car(h,x,h.get('P1Y'));h.tick(.05);
  assert(close(h.get('CarX')[0]-x,7.2),'Passive traffic hook missing');
 });
 test('E input uses drone broadcast reach and extends cooperation',()=>{
  const h=fresh([15,5]);h.set('P2X',919);h.set('P2Y',596);h.keys.add(69);h.tick(.05);
  assert(h.get('WhistleCD')>0 && h.get('Force')>3.7,'Whistle integration missing');
 });
 test('Actual collision invokes emergency rescue and avoids defeat',()=>{
  const h=fresh([19,10]);h.set('Lives',1);car(h,h.get('P1X'),h.get('P1Y'));
  h.get('CarSpeed')[0]=0;h.tick(.05);
  assert(h.get('Lives')===2 && h.get('MedkitUsed')===1 && h.get('Phase')===1,'Collision rescue hook missing');
 });
 test('Reverse charging affects the stamina consumed by actual held space input',()=>{
  const h=fresh([22,7]);h.set('Stamina',20);h.keys.add(32);h.tick(.05);
  assert(h.get('Linked')===1 && close(h.get('Stamina'),19.3),'Reverse charge frame hook missing');
 });
 test('Walking with coffee stepwatch refreshes cooldowns through actual movement input',()=>{
  const h=fresh([20,9]);h.set('ClockCD',10);h.keys.add(68);
  for(let i=0;i<13;i++)h.tick(.05);
  assert(close(h.get('ClockCD'),8.35),'Walking cooldown hook missing');
 });
 test('Convenience membership discount appears in actual shop prices',()=>{
  const h=fresh([21,2]);h.call('RoutePrepareShop');
  for(let i=0;i<3;i++){
   const id=h.get('ShopItems')[i];if(id<0)continue;
   const expected=Math.max(1,26+h.get('RouteAct')*4+(id%5)*4-9);
   assert(h.get('ShopPrices')[i]===expected,'Shop discount hook missing');
  }
 });
 test('Earplug candy distraction expires at a linear rate during real frames',()=>{
  const h=fresh([16,6]);h.set('Mood',1);h.set('MoodTimer',1);h.set('MoodWait',20);
  for(let i=0;i<5;i++)h.tick(.05);
  assert(h.get('Mood')===1 && h.get('MoodTimer')>.49 && h.get('MoodTimer')<.51,'Unexpected exponential timer change');
  for(let i=0;i<7;i++)h.tick(.05);
  assert(h.get('Mood')===0,'Distraction did not expire normally');
 });
 return {passed:tests.length,failed:0,tests};
}
