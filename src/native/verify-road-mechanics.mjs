import {buildGame} from './build-game.mjs';
import {createIntegratedGameHarness} from './verify-native-game.mjs';

export function verifyRoadMechanics(bundle) {
 const h=createIntegratedGameHarness(bundle),tests=[];
 const assert=(value,message)=>{if(!value)throw new Error(message);};
 const near=(a,b)=>Math.abs(a-b)<.002;
 const test=(name,fn)=>{fn();tests.push({name,passed:true});};
 const road=()=>{
  h.initialize(0);h.call('ChooseReward',0);h.call('RouteChoose',0);h.noCars();
 };
 const boss=act=>{
  h.initialize(0);h.call('ChooseReward',0);h.set('RouteAct',act);h.set('RouteRow',5);h.set('RouteColumn',1);h.set('Phase',5);
  h.call('RouteChoose',1);h.noCars();
 };
 test('all three final road nodes select distinct actual mechanics',()=>{
  for(let act=1;act<=3;act++){boss(act);assert(h.get('Phase')===1&&h.get('NodeKind')===5,'Boss road did not start');assert(h.get('RoadRule')===act,'Incorrect boss rule');assert(h.get('LaneCount')>=5&&h.get('LaneCount')<=6,'Invalid boss lanes');}
 });
 test('school convoy gives a warning before its timed lane speed change',()=>{
  boss(1);h.set('Elapsed',1);h.call('RoadTick');assert(h.get('RoadWarning').includes('预告'),'Missing convoy warning');
  h.get('CarLane')[0]=0;h.get('CarLane')[1]=1;assert(near(h.call('RoadTrafficRate',0),1),'Convoy sped up before warning interval');
  h.set('Elapsed',3);assert(near(h.call('RoadTrafficRate',0),1.4)&&near(h.call('RoadTrafficRate',1),.7),'Convoy lane rates incorrect');
  h.set('Elapsed',6);assert(near(h.call('RoadTrafficRate',0),1),'Convoy failed to end');
 });
 test('convoy changes actual car displacement through Traffic',()=>{
  boss(1);h.set('Elapsed',3);h.get('CarActive')[0]=1;h.get('CarLane')[0]=0;h.get('CarX')[0]=500;h.get('CarY')[0]=h.get('LaneY')[0];h.get('CarDirection')[0]=1;h.get('CarSpeed')[0]=100;
  h.call('Traffic',.05);assert(near(h.get('CarX')[0],507),'Convoy hook was not applied to traffic');
 });
 test('construction blocks direct crossing and both people can move around it',()=>{
  boss(2);h.set('P1X',519);h.set('P2X',559);h.set('P1Y',400);h.set('P2Y',415);
  h.keys.add(87);h.keys.add(32);
  for(let i=0;i<40;i++)h.tick(.05);
  assert(h.get('P1Y')>=h.get('ObstacleY')+h.get('ObstacleH')+11,'Escort passed through construction');
  assert(h.get('P2Y')>=h.get('ObstacleY')+h.get('ObstacleH')+11,'Classmate passed through construction');
  h.keys.add(65);
  for(let i=0;i<45;i++)h.tick(.05);
  assert(h.get('P1Y')<h.get('ObstacleY')&&h.get('P2Y')<h.get('ObstacleY'),'Construction could not be bypassed');
 });
 test('final junction alternates actual lane windows',()=>{
  boss(3);h.get('CarLane')[0]=0;h.get('CarLane')[1]=1;
  h.set('Elapsed',1);assert(near(h.call('RoadTrafficRate',0),1)&&near(h.call('RoadTrafficRate',1),.22),'First junction window incorrect');
  h.set('Elapsed',6);assert(near(h.call('RoadTrafficRate',0),.22)&&near(h.call('RoadTrafficRate',1),1),'Junction did not alternate');
 });
 test('junction distraction has a preview and respects handholding or earplugs',()=>{
  boss(3);h.set('Elapsed',1);h.call('RoadTick');assert(h.get('RoadWarning').includes('预告'),'Phone wave lacks preview');assert(h.get('Mood')===0,'Phone distraction started early');
  h.set('Elapsed',2.1);h.set('Linked',0);h.set('Force',0);h.call('RoadTick');assert(h.get('Mood')===1,'Phone wave did not start');
  h.set('Mood',0);h.set('Linked',1);h.set('Elapsed',14.1);h.call('RoadTick');assert(h.get('Mood')===0,'Handholding failed to prevent phone wave');
  h.set('Linked',0);h.call('GrantItem',0,16);h.set('Elapsed',26.1);h.call('RoadTick');assert(h.get('Mood')===0,'Earplugs failed to prevent phone wave');
 });
 test('normal escort road is playable with real held keys and awards route loot',()=>{
  road();let frames=0;const coins=h.get('Coins');
  h.keys.add(87);h.keys.add(32);
  while(h.get('Phase')===1&&frames<100){h.tick(.05);frames++;}
  assert(h.get('Phase')===2&&h.get('RewardContext')===2,'Held movement could not clear the road');
  assert(h.get('Coins')>coins&&h.get('Score')>0,'Road did not award income');
  h.keys.clear();h.call('ChooseReward',0);
  assert(h.get('Phase')===5&&h.get('RouteRow')===1,'Loot failed to return to the next map layer');
 });
 test('phone distraction still interrupts autonomous following and whistle restores it',()=>{
  road();h.set('P1Y',520);h.set('P2Y',607);h.set('Mood',1);h.set('MoodTimer',1);h.set('Force',0);
  const y=h.get('P2Y');h.keys.add(87);h.tick(.05);assert(near(h.get('P2Y'),y),'Distracted classmate did not stop');
  h.keys.add(69);h.tick(.05);assert(h.get('P2Y')<y&&h.get('Force')>0,'Whistle did not restore movement');
 });
 test('same difficulty on different map branches changes initial traffic',()=>{
  road();h.set('CurrentNode',0);h.call('SetupLevel');const first=h.get('CarX')[0];
  h.set('CurrentNode',1);h.call('SetupLevel');assert(!near(first,h.get('CarX')[0]),'Branching only changed a label');
 });
 return {format:'Integrated native CIL road mechanics',passed:tests.length,failed:0,tests};
}
export async function runRoadChecks(){return verifyRoadMechanics(await buildGame());}
