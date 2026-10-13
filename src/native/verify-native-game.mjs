
import fs from 'node:fs/promises';
import { ManagedBuilder, Sig } from './build-managed.mjs';
const { compileMethods, extractCSharpDefinitions, TypeSignatures } = await import(new URL('./compile-subset.mjs',import.meta.url).href+'?gamecheck=integrated3');
const { executeMethod } = await import(new URL('./verify-subset.mjs',import.meta.url).href+'?gamecheck=2');

export function createGameHarness(source) {
  const parsed=extractCSharpDefinitions(source),b=new ManagedBuilder('NativeGameLogicChecks'),obj=b.typeRef('mscorlib','System','Object'),game=b.defineType('Game',obj),fields={},functions={},external={};
  const keys=new Set();let focus=true,randomState=1;
  const rand=()=>{randomState^=randomState<<13;randomState^=randomState>>>17;randomState^=randomState<<5;return (randomState>>>0)/4294967296;};
  const wrap=(name,returns,argTypes,fn)=>{
    const method=game.method(name,TypeSignatures[returns],argTypes.map(t=>TypeSignatures[t]),{static:true});
    external[name]={token:method.token,returnType:returns,argTypes,instance:false};
    functions[method.token]={returnType:returns,argTypes,fn};
  };
  wrap('Rand','float',[],()=>Math.fround(rand()));wrap('RandomInt','int',['int','int'],(a,z)=>a+Math.floor(rand()*(z-a)));
  wrap('SetRandom','void',['int'],s=>{randomState=s|0;if(!randomState)randomState=1;});wrap('NewSeed','int',[],()=>123456);
  wrap('Key','bool',['int'],k=>keys.has(k)?1:0);wrap('HasFocus','bool',[],()=>focus?1:0);
  wrap('N','string',['int'],n=>String(n|0));wrap('IntText','string',['int'],n=>String(n|0));
  wrap('Join','string',['string','string'],(a,z)=>(a||'')+(z||''));wrap('FloatText','string',['float'],n=>String(n));
  wrap('Min','float',['float','float'],(a,z)=>Math.fround(Math.min(a,z)));wrap('Max','float',['float','float'],(a,z)=>Math.fround(Math.max(a,z)));
  wrap('Abs','float',['float'],x=>Math.fround(Math.abs(x)));wrap('Sqrt','float',['float'],x=>Math.fround(Math.sqrt(x)));
  wrap('ToInt','int',['float'],n=>n|0);wrap('MaxInt','int',['int','int'],(a,z)=>Math.max(a,z)|0);
  for(const f of parsed.fields)fields[f.name]={token:game.field(f.name,TypeSignatures[f.type],{static:true}),type:f.type};
  const result=compileMethods(b,game,parsed.definitions,fields,external);
  for(const m of b.memberRefs){const token=0x0a000000|b.memberRefs.indexOf(m)+1;if(m.name==='op_Equality')functions[token]={returnType:'bool',argTypes:['string','string'],fn:(a,z)=>a===z?1:0};else if(m.name==='Concat')functions[token]={returnType:'string',argTypes:['string','string'],fn:(a,z)=>(a||'')+(z||'')};}
  const byName=Object.fromEntries(result.compiled.map(c=>[c.definition.name,c.method]));
  const state={},arrayLength=name=>name==='Input'||name==='PreviousInput'?256:name==='P1Items'?12:name==='P2Items'?8:name==='Choices'||name.startsWith('Choice')?3:name.startsWith('Car')?96:8;
  for(const [name,f]of Object.entries(fields))state[f.token]=f.type.endsWith('[]')?Array(arrayLength(name)).fill(f.type==='string[]'?'':0):f.type==='string'?'':0;
  const get=name=>state[fields[name].token],set=(name,value)=>state[fields[name].token]=fields[name].type==='float'?Math.fround(value):value;
  const call=(name,...args)=>{const method=byName[name];if(!method)throw Error('Unknown game method '+name);return executeMethod(method,args,state,b.methods,functions,{remaining:3000000});};
  const noCars=()=>{get('CarActive').fill(0);set('CarCount',0);get('LaneTimer').fill(999);};
  const initialize=(mode=0)=>{keys.clear();focus=true;call('GameInitialize');set('Mode',mode);call('StartGame',0);noCars();};
  const choose=()=>{call('ChooseReward',0);noCars();};
  const tick=(dt=.016)=>call('GameTick',dt);
  return {b,fields,functions,state,byName,get,set,call,keys,tick,initialize,noCars,choose,setFocus:value=>focus=value,parsed};
}

export function createIntegratedGameHarness(bundle) {
  const {b,fields,h:initHost,arrayLengths}=bundle,functions={},keys=new Set();let focus=true,randomState=1;
  const draws=[],sounds=[],fullscreenEvents=[],pointer={x:-100,y:-100};
  const rand=()=>{randomState^=randomState<<13;randomState^=randomState>>>17;randomState^=randomState<<5;return (randomState>>>0)/4294967296;};
  const implementations={
    Rand:()=>Math.fround(rand()),RandomInt:(a,z)=>a+Math.floor(rand()*(z-a)),SetRandom:s=>{randomState=s|0;if(!randomState)randomState=1;},NewSeed:()=>123456,
    Key:k=>keys.has(k)?1:0,HasFocus:()=>focus?1:0,N:n=>String(n|0),Join:(a,z)=>(a||'')+(z||''),FloatText:n=>Number(n).toFixed(1),
    Min:(a,z)=>Math.fround(Math.min(a,z)),Max:(a,z)=>Math.fround(Math.max(a,z)),Abs:x=>Math.fround(Math.abs(x)),Sqrt:x=>Math.fround(Math.sqrt(x)),Sin:x=>Math.fround(Math.sin(x)),Cos:x=>Math.fround(Math.cos(x)),MouseX:()=>pointer.x,MouseY:()=>pointer.y,Sound:id=>{if(id<0||id>5)throw Error("Invalid sound");sounds.push(id);},ToggleFullscreen:()=>fullscreenEvents.push(1)
  };
  const types={1:'void',2:'bool',8:'int',12:'float',14:'string'};
  for(const name of ['Rect','Outline','Circle','Line','Text','TextBlock','Art','Ring','SoftRect','BeginWorld','EndWorld'])implementations[name]=(...args)=>{
    if(name==='BeginWorld'||name==='EndWorld'){if(args.some(a=>!Number.isFinite(a)))throw Error('Invalid transform');draws.push({name,args});return;}
    if(name==='Ring'){if(args.some(a=>!Number.isFinite(a))||args[2]<0||args[3]<=0)throw Error('Invalid ring');draws.push({name,args});return;}
    if(name==='SoftRect'){if(args.some(a=>!Number.isFinite(a))||args[2]<0||args[3]<0||args[5]<0||args[5]>1)throw Error('Invalid alpha rect');draws.push({name,args});return;}
    if(name==='TextBlock'){if(typeof args[0]!=='string'||args.slice(1).some(a=>!Number.isFinite(a))||args[3]<=0||args[4]<=0||args[5]<=0)throw Error('Invalid wrapped text');draws.push({name,args});return;}
    if(name==='Art'){if(args.some(a=>!Number.isFinite(a))||args[0]<0||args[0]>40||args[3]<0||args[4]<0)throw Error('Invalid art geometry');draws.push({name,args});return;}
    for(let i=name==='Text'?1:0;i<4;i++)if(!Number.isFinite(args[i]))throw Error('Non-finite drawing geometry in '+name);
    if(name==='Text'&&typeof args[0]!=='string')throw Error('Text received nonstring');
    if((name==='Rect'||name==='Circle'||name==='Outline')&&(args[2]<0||args[3]<0))throw Error('Negative drawing extent '+name);
    draws.push({name,args});
  };
  for(const [name,fn]of Object.entries(implementations)){const m=initHost.methods[name];if(!m)throw Error('Missing integrated wrapper '+name);functions[m.token]={returnType:types[m.returns[0]],argTypes:m.args.map(a=>types[a[0]]),fn};}
  for(const [i,m]of b.memberRefs.entries()){const token=0x0a000000|i+1;if(m.name==='op_Equality')functions[token]={returnType:'bool',argTypes:['string','string'],fn:(a,z)=>a===z?1:0};else if(m.name==='Concat')functions[token]={returnType:'string',argTypes:['string','string'],fn:(a,z)=>(a||'')+(z||'')};}
  const byName=Object.fromEntries(b.methods.map(m=>[m.name,m])),state={};
  for(const f of b.fields)state[f.token]=[14,18,29].includes(f.signature[0])?null:0;
  const get=name=>state[fields[name].token],set=(name,value)=>state[fields[name].token]=fields[name].type==='float'?Math.fround(value):value;
  const call=(name,...args)=>{const method=byName[name];if(!method)throw Error('Unknown integrated method '+name);return executeMethod(method,args,state,b.methods,functions,{remaining:3000000});};
  const noCars=()=>{get('CarActive').fill(0);set('CarCount',0);get('LaneTimer').fill(999);};
  const initialize=(mode=0)=>{keys.clear();focus=true;call('Initialize');set('Mode',mode);call('StartGame',0);noCars();};
  const choose=()=>{call('ChooseReward',0);noCars();};
  const tick=(dt=.016)=>call('GameTick',dt);
  return {b,fields,functions,state,byName,get,set,call,keys,tick,initialize,noCars,choose,setFocus:value=>focus=value,parsed:{definitions:bundle.result.compiled,fields:Object.values(fields)},draws,sounds,fullscreenEvents,pointer,bundle,arrayLengths};
}

export async function verifyNativeGame(sourcePath,bundle) {
  const source=await fs.readFile(sourcePath,'utf8'),h=bundle?createIntegratedGameHarness(bundle):createGameHarness(source),results=[];
  const assert=(condition,message)=>{if(!condition)throw Error(message);};
  const test=(name,fn)=>{fn();results.push({name,passed:true});};
  const approx=(a,z,tolerance=.001)=>Math.abs(a-z)<=tolerance;
  const startDuel=()=>{h.initialize(1);assert(h.get('Phase')===2&&h.get('ChoiceOwner')===0,'Duel missing 1P selection');h.choose();assert(h.get('ChoiceOwner')===1,'Duel missing 2P selection');h.choose();assert(h.get('Phase')===1,'Duel did not start');};
  const nextRound=()=>{h.choose();h.choose();h.noCars();};
  test('single player completes all ten levels with persistent rewards',()=>{
    h.initialize();for(let level=1;level<=10;level++){assert(h.get('Level')===level,'Incorrect level progression '+level);h.set('P1Y',70);h.set('P2Y',76);h.set('Arrived2',1);h.call('StepGame',0);if(level<10){assert(h.get('Phase')===2,'Missing reward on level '+level);h.choose();assert(h.get('Phase')===1,'Failed to resume');}else assert(h.get('Phase')===4&&h.get('Winner')===1,'Ten level completion did not win');}
    assert(h.get('Score')>0,'No score');assert(h.get('Best')===h.get('Score'),'Best score was not saved in memory');
    assert(h.get('P1Items').reduce((a,z)=>a+z,0)===9,'Rewards did not persist');
  });
  test('both players must reach the far sidewalk',()=>{h.initialize();h.set('P1Y',70);h.set('P2Y',400);h.set('Mood',1);h.call('StepGame',0);assert(h.get('Phase')===1&&h.get('Arrived1')===1&&h.get('Arrived2')===0,'Player1 alone cleared stage');});
  test('duel 1P wins two rounds to zero',()=>{startDuel();h.call('RoundEnd',1);assert(h.get('Score1')===1&&h.get('Phase')===2,'First duel win incorrect');nextRound();assert(h.get('Round')===2,'Round did not advance');h.call('RoundEnd',1);assert(h.get('Phase')===4&&h.get('Score1')===2&&h.get('Score2')===0&&h.get('Winner')===1,'Duel 2-0 result incorrect');});
  test('duel 2P wins a two to one match',()=>{startDuel();h.call('RoundEnd',2);nextRound();h.call('RoundEnd',1);nextRound();assert(h.get('Round')===3,'Third round missing');h.call('RoundEnd',2);assert(h.get('Phase')===4&&h.get('Score1')===1&&h.get('Score2')===2&&h.get('Winner')===2,'Duel 1-2 result incorrect');});
  test('duel timeout awards the round to 2P',()=>{startDuel();h.set('Elapsed',75);h.call('StepGame',0);assert(h.get('Score2')===1&&h.get('Phase')===2,'Timeout failed');});
  test('hand holding follows the escort and consumes stamina',()=>{h.initialize();h.get('Input')[32]=1;h.get('Input')[87]=1;const y=h.get('P1Y'),s=h.get('Stamina');h.call('StepGame',.05);assert(h.get('Linked')===1,'Nearby buddy was not linked');assert(h.get('P1Y')<y,'Escort did not move');assert(h.get('Stamina')<s,'Link did not consume stamina');assert(h.get('Mood')===0,'Linked buddy remains disobedient');});
  test('ordinary exhausted hands release and recover',()=>{h.initialize();h.set('Stamina',0);h.get('Input')[32]=1;h.call('StepGame',.05);assert(h.get('Linked')===0&&h.get('Tired')===1,'Exhaustion did not release');h.get('Input')[32]=0;h.set('RegenDelay',0);h.call('StepGame',.05);assert(h.get('Stamina')>0,'Stamina did not recover');});
  test('hand band and power bank combination adds grace when exhausted',()=>{h.initialize();h.get('P1Items')[3]=1;h.get('P1Items')[7]=1;h.set('Stamina',0);h.get('Input')[32]=1;h.call('StepGame',.05);assert(h.get('Linked')===1&&h.get('GraceUsed')===1&&h.get('GraceTime')>0,'Hand combo missing exhaustion grace');});
  test('whistle forces cooperation and applies cooldown',()=>{h.initialize();h.set('Mood',2);h.call('Whistle');assert(h.get('Force')>2&&h.get('WhistleCD')>0&&h.get('Mood')===0,'Whistle did not work');const cd=h.get('WhistleCD');h.call('Whistle');assert(h.get('WhistleCD')===cd,'Whistle cooldown ignored');});
  test('duel escape grants a period where linking is disabled',()=>{startDuel();h.set('P1X',519);h.set('P1Y',596);h.set('P2X',559);h.set('P2Y',607);h.set('ForceLock',0);h.set('EscapeCD',0);h.set('Energy',100);h.call('P2Skill',0);assert(h.get('EscapeTime')>0&&h.get('Energy')===58,'Escape costs energy and grants immunity');h.get('Input')[32]=1;h.call('StepGame',.05);assert(h.get('Linked')===0,'Escaping 2P can still be linked');});
  test('collision reduces a life and returns both people to safety',()=>{h.initialize();h.set('Invincible',0);h.set('P1Y',300);h.set('P2Y',300);h.get('CarActive')[0]=1;h.get('CarX')[0]=h.get('P1X');h.get('CarY')[0]=h.get('P1Y')-6;h.get('CarW')[0]=90;h.get('CarH')[0]=44;h.get('CarSpeed')[0]=0;h.get('CarDirection')[0]=1;h.call('StepGame',0);assert(h.get('Lives')===3&&h.get('StageHits')===1,'Collision did not deduct life');assert(h.get('P1Y')===h.get('Checkpoint')&&h.get('Invincible')>0,'Collision did not respawn safely');});
  test('shield absorbs a collision without losing a life',()=>{h.initialize();h.set('Shield',3);h.call('HitP1');assert(h.get('Lives')===4&&h.get('Shield')===0&&h.get('Invincible')>0,'Shield failed');});
  test('2P collision stuns and drains energy',()=>{startDuel();h.set('Energy',100);h.call('HitP2');assert(h.get('Energy')===75&&h.get('Stun2')>0&&h.get('BuddyInvincible')>0,'2P collision consequences incorrect');});
  test('slow clock plus shoes extends slowing',()=>{h.initialize();h.get('P1Items')[0]=1;h.get('P1Items')[4]=1;h.call('P1Skill',0);assert(h.get('Slow')===4&&h.get('ClockCD')>0,'Clock/shoes combo failed');});
  test('milk tea plus candy forces six seconds of cooperation',()=>{h.initialize();h.get('P1Items')[2]=1;h.get('P1Items')[6]=1;h.set('Stamina',20);h.call('P1Skill',2);assert(h.get('Force')===6&&h.get('Stamina')===75,'Tea/candy combo failed');});
  test('fake notifications plus signal booster drains more and refunds energy',()=>{startDuel();h.get('P2Items')[0]=1;h.get('P2Items')[6]=1;h.set('Energy',100);h.set('Stamina',100);h.set('NotificationCD',0);h.call('P2Skill',1);assert(h.get('Energy')===85&&h.get('Stamina')===62,'Notification combo incorrect');});
  test('earbuds plus giant phone blocks two whistles',()=>{startDuel();h.get('P2Items')[2]=1;h.get('P2Items')[4]=1;h.set('Energy',100);h.set('FocusCD',0);h.call('P2Skill',3);assert(h.get('FocusTime')===6&&h.get('FocusCharges')===2,'Earbuds combo incorrect');h.set('WhistleCD',0);h.call('Whistle');assert(h.get('FocusCharges')===1&&h.get('Force')===0,'First whistle not blocked');h.set('WhistleCD',0);h.call('Whistle');assert(h.get('FocusCharges')===0&&h.get('Force')===0,'Second whistle not blocked');});
  test('keyboard pause, focus loss and resume preserve positions',()=>{h.initialize();h.keys.add(80);h.tick();assert(h.get('Phase')===3,'P did not pause');const y=h.get('P1Y');h.keys.clear();h.keys.add(87);h.tick();assert(h.get('P1Y')===y,'Paused game moved');h.keys.clear();h.tick();h.keys.add(13);h.tick();assert(h.get('Phase')===1,'Enter did not resume');h.keys.clear();h.setFocus(false);h.tick();assert(h.get('Phase')===3,'Focus loss did not pause');h.setFocus(true);h.keys.add(13);h.tick();assert(h.get('Phase')===1,'Focus restoration could not resume');});
  test('held Enter confirms 2P equipment without escaping on subsequent frames',()=>{h.initialize(1);h.choose();h.keys.add(13);h.tick();assert(h.get('Phase')===1,'2P Enter did not confirm');for(let heldFrame=0;heldFrame<3;heldFrame++){h.tick();assert(h.get('Energy')===h.get('MaxEnergy')&&h.get('EscapeTime')===0&&h.get('EscapeCD')===0,'Held Enter repeated as escape after equipment confirmation');}});
  test('held Enter starts a duel without automatically selecting equipment',()=>{h.initialize(1);h.call('Menu');h.set('Mode',1);h.keys.add(13);h.tick();assert(h.get('Phase')===2&&h.get('ChoiceOwner')===0,'Held Enter failed to start duel selection');for(let heldFrame=0;heldFrame<3;heldFrame++){h.tick();assert(h.get('Phase')===2&&h.get('ChoiceOwner')===0&&h.get('P1Items').reduce((a,z)=>a+z,0)===0,'Start Enter automatically selected equipment on subsequent frame');}h.keys.clear();h.tick();h.keys.add(13);h.tick();assert(h.get('Phase')===2&&h.get('ChoiceOwner')===1,'New Enter press did not select 1P equipment');for(let heldFrame=0;heldFrame<3;heldFrame++){h.tick();assert(h.get('Phase')===2&&h.get('ChoiceOwner')===1&&h.get('P2Items').reduce((a,z)=>a+z,0)===0,'1P selection Enter automatically selected 2P equipment');}});
  test('difficulty increases traffic lanes with the level',()=>{h.initialize();const low=h.get('LaneCount');h.set('Level',10);h.call('SetupLevel');assert(h.get('LaneCount')>low&&h.get('LaneCount')===6,'Lane difficulty did not scale');assert(h.get('MedianCount')===2,'Median count incorrect');for(let i=0;i<h.get('LaneCount');i++)assert(h.get('LaneH')[i]>0&&h.get('LaneY')[i]<520,'Road geometry out of range');});
  test('shield and vest combination slows nearby cars',()=>{h.initialize();h.get('P1Items')[1]=1;h.get('P1Items')[8]=1;h.set('Shield',3);h.get('CarActive')[0]=1;h.get('CarDirection')[0]=1;h.get('CarSpeed')[0]=100;h.get('CarX')[0]=h.get('P1X');h.get('CarY')[0]=h.get('P1Y');const x=h.get('CarX')[0];h.call('Traffic',.1);assert(approx(h.get('CarX')[0]-x,6.5),'Shield/vest did not reduce nearby traffic speed');});
  test('dash and slippers combination extends escape and reduces cooldown',()=>{startDuel();h.get('P2Items')[1]=1;h.get('P2Items')[3]=1;h.set('DashCD',0);h.set('Energy',100);h.call('P2Skill',2);assert(approx(h.get('DashCD'),8)&&approx(h.get('EscapeTime'),.9)&&h.get('BurstTime')>0,'Dash/slippers combo failed');});
  test('car free stage can be completed using held movement and hand keys',()=>{h.initialize();h.keys.add(87);h.keys.add(32);let frames=0;while(h.get('Phase')===1&&frames<600){h.tick(.016);frames++;}assert(h.get('Phase')===2&&h.get('Arrived1')===1&&h.get('Arrived2')===1,'Keyboard escort could not clear car-free stage');assert(frames<400,'Escort crossed unexpectedly slowly');});
  test('keyboard controls 2P separately in the opposing direction',()=>{startDuel();h.keys.add(87);h.keys.add(40);const p1=h.get('P1Y'),p2=h.get('P2Y');h.tick(.016);assert(h.get('P1Y')<p1&&h.get('P2Y')>p2,'Independent 2P keyboard motion failed');});
  test('simultaneous opposite escort keys cancel movement',()=>{h.initialize();h.keys.add(87);h.keys.add(83);h.keys.add(65);h.keys.add(68);const x=h.get('P1X'),y=h.get('P1Y');h.tick(.016);assert(h.get('P1X')===x&&h.get('P1Y')===y,'Opposing keys did not cancel');});
  test('same seed and level reproduce the road',()=>{h.initialize();h.set('Seed',314159);h.call('SetupLevel');const a=JSON.stringify([h.get('LaneSpeed'),h.get('CarX'),h.get('CarType')]);h.call('SetupLevel');const z=JSON.stringify([h.get('LaneSpeed'),h.get('CarX'),h.get('CarType')]);assert(a===z,'Seed is not reproducible');});
  if(bundle){test('integrated Initialize allocates every array with its shipped length',()=>{h.call('Initialize');for(const [name,length] of Object.entries(bundle.arrayLengths)){assert(Array.isArray(h.get(name))&&h.get(name).length===length,'Integrated wrong array '+name);}});for(const mode of [0,1]){for(const phase of [0,1,2,3,4]){test('integrated Render draws mode '+mode+' phase '+phase,()=>{h.initialize(mode);h.set('Phase',phase);h.set('Mode',mode);if(phase===2){h.set('ChoiceOwner',mode);h.call('MakeChoices');}if(phase===4)h.call('EndGame',1);h.draws.length=0;h.call('Render');assert(h.draws.length>100,'Render produced too few drawing calls');assert(h.draws.some(d=>d.name==='Text'),'Render missing text');});}}test('integrated HUD renders all upgraded items and seven combination effects',()=>{h.initialize(1);h.get('P1Items').fill(3);h.get('P2Items').fill(3);h.call('UpdateInventory');h.set('Phase',1);h.set('Shield',2);h.set('Slow',2);h.set('Force',2);h.set('EscapeTime',2);h.set('Stun2',1);h.set('Linked',1);h.set('Bubble','我来试试');h.set('BubbleTime',2);h.draws.length=0;h.call('Render');assert(h.draws.length>100,'Full HUD missing');assert(h.get('ComboText').split('\n').length-1===7,'Seven combinations missing');});}
  return {format:bundle?'CIL simulation of the integrated shipped native EXE methods':'CIL simulated execution of actual native game source',checks:results.length,passed:results.length,results,gameMethods:h.parsed.definitions.length,fields:h.parsed.fields.length};
}
