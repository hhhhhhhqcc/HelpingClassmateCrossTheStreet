import {createIntegratedGameHarness} from './verify-native-game.mjs';
export function verifyIntegratedUI(bundle) {
 const h=createIntegratedGameHarness(bundle),tests=[];
 const check=name=>{
  h.draws.length=0;h.call('Render');
  const labels=h.draws.filter(d=>d.name==='Text'||d.name==='TextBlock');
  if(labels.some(d=>typeof d.args[0]!=='string'||(d.name==='Text'?d.args[3]:d.args[5])<=0||d.args[2]>=900))throw Error('Invalid UI text '+name);
  if(labels.some(d=>/Lv\.|升级|每级|升到三级/.test(d.args[0])))throw Error('Old rank UI '+name);
  tests.push({name,primitives:h.draws.length,labels:labels.length,passed:true});
 };
 for(const mode of [0,1]){
  h.initialize(mode);h.set('Phase',0);check('menu-'+mode);
  h.initialize(mode);
  if(mode===0){h.call('ChooseReward',0);h.call('RouteChoose',1);}
  else {h.call('ChooseReward',0);h.call('ChooseReward',0);}
  check('road-'+mode);h.set('Phase',3);check('pause-'+mode);h.set('Phase',1);h.call('EndGame',1);check('end-'+mode);
  h.initialize(mode);check('reward-'+mode);
 }
 for(const act of [1,2,3]){
  h.initialize(0);h.call('ChooseReward',0);h.set('RouteAct',act);h.set('RouteRow',act===3?5:1);h.set('RouteColumn',1);h.call('RouteMap');check('map-act-'+act);
 }
 for(const kind of [2,3,4]){
  h.initialize(0);h.call('ChooseReward',0);h.get('NodeTypes')[0]=kind;h.call('RouteChoose',0);check('stop-'+kind);
 }
 h.initialize(0);h.call('ChooseReward',0);h.set('BackPhase',5);h.set('Phase',9);
 for(let page=0;page<3;page++){h.set('CollectionPage',page);h.set('CollectionView',0);check('recipes-'+page);}
 for(let page=0;page<2;page++){h.set('CollectionPage',page);h.set('CollectionView',1);check('backpack-'+page);}
 h.initialize(0);h.call('ChooseReward',0);h.set('RouteAct',2);h.set('RouteRow',5);h.set('RouteColumn',1);h.call('RouteChoose',1);
 for(let id=0;id<24;id++)h.call('GrantItem',0,id);
 h.set('RoadblockX',500);h.set('RoadblockY',375);h.set('RoadblockTime',3);h.set('BeaconX',550);h.set('BeaconY',249);h.set('BeaconTime',4);h.set('SignalTime',2);h.call('RoadTick');check('full-bag-and-world-devices');
 return {format:'Shipped integrated Render CIL with drawing wrappers recorded',passed:tests.length,failed:0,tests};
}
