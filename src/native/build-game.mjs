import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {ManagedBuilder, Sig} from './build-managed.mjs';
import {createHost} from './host-native-action-v2.mjs';
import {compileMethods, extractCSharpDefinitions, TypeSignatures} from './compile-subset.mjs';
import {validateBuilder} from './validate-il.mjs';

const here=path.dirname(fileURLToPath(import.meta.url));
const arrayLengths={
 CarX:96,CarY:96,CarW:96,CarH:96,CarSpeed:96,
 CarActive:96,CarDirection:96,CarColor:96,CarType:96,CarLane:96,
 LaneY:6,LaneH:6,LaneSpeed:6,LaneTimer:6,LaneDirection:6,
 MedianY:3,P1Items:24,P2Items:8,Choices:3,
 NodeTypes:54,NodeVisited:54,ShopItems:3,ShopPrices:3,ShopBought:3,
 ChoiceNames:3,ChoiceTags:3,ChoiceDescriptions:3,ChoiceCombos:3,
 FXx:64,FXy:64,FXvx:64,FXvy:64,FXlife:64,FXmax:64,FXcolor:64,
 TrailX:12,TrailY:12,TrailLife:12,CarNearState:96,
 Input:256,PreviousInput:256
};
const signatures={
 Rect:['void','float','float','float','float','int'],
 Circle:['void','float','float','float','float','int'],
 Outline:['void','float','float','float','float','int'],
 Line:['void','float','float','float','float','int'],
 Text:['void','string','float','float','float','int'],
 Key:['bool','int'],HasFocus:['bool'],Rand:['float'],
 RandomInt:['int','int','int'],SetRandom:['void','int'],NewSeed:['int'],
 Join:['string','string','string'],N:['string','int'],
 FloatText:['string','float'],
 Art:['void','int','float','float','float','float'],
 TextBlock:['void','string','float','float','float','float','float','int'],
 Min:['float','float','float'],Max:['float','float','float'],
 Abs:['float','float'],Sqrt:['float','float'],Sin:['float','float'],Cos:['float','float'],
 Ring:['void','float','float','float','float','int'],
 SoftRect:['void','float','float','float','float','int','float'],
 BeginWorld:['void','float','float'],EndWorld:['void'],
 MouseX:['float'],MouseY:['float'],Sound:['void','int'],ToggleFullscreen:['void']
};
export async function buildGame(outputPath) {
 const b=new ManagedBuilder('TongXueNative');
 const type=b.defineType('GameForm',b.typeRef('System.Windows.Forms','System.Windows.Forms','Form'));
 const h=createHost(b,type);
 const sourceNames=['game-core.cs','flow-core.cs','route-core.cs','item-core.cs','render-native.cs','route-render.cs','visual-theme.cs','visual-road.cs','visual-route.cs','motion-core.cs','motion-road.cs','motion-ui.cs','motion-presentation.cs'];
 const parsedSources=await Promise.all(sourceNames.map(async name=>extractCSharpDefinitions(await fs.readFile(path.join(here,name),'utf8'))));
 const allFields=parsedSources.flatMap(s=>s.fields);
 const definitionsByName=new Map();
 for(const parsed of parsedSources)for(const d of parsed.definitions)definitionsByName.set(d.name,d);
 const fields={};
 for(const f of allFields){
  if(fields[f.name])throw new Error('Duplicate field '+f.name);
  fields[f.name]={token:type.field(f.name,TypeSignatures[f.type],{static:true}),type:f.type};
 }
 const externals={};
 for(const [name,[returnType,...argTypes]] of Object.entries(signatures)){
  externals[name]={token:h.methods[name].token,returnType,argTypes,instance:false};
 }
 const result=compileMethods(b,type,[...definitionsByName.values()],fields,externals);
 const init=type.method('Initialize',Sig.void,[],{static:true});
 const elementTokens={
  int:b.typeRef('mscorlib','System','Int32'),
  float:b.typeRef('mscorlib','System','Single'),
  string:b.typeRef('mscorlib','System','String')
 };
 for(const [name,f] of Object.entries(fields)){
  if(f.type.endsWith('[]')){
   const length=arrayLengths[name];
   if(!length)throw new Error('Missing array length: '+name);
   init.il.int(length).newarr(elementTokens[f.type.slice(0,-2)]).storeStaticField(f.token);
  }
 }
 init.il.call(result.methods.GameInitialize.token).ret();
 h.bind({init,update:result.methods.GameTick.token,render:result.methods.Render.token,click:result.methods.GameClick.token});
 const validation=validateBuilder(b);
 if(!validation.valid)throw new Error('Invalid CIL: '+JSON.stringify(validation));
 let output;
 if(outputPath)output=await b.write(outputPath,h.main);
 return {b,type,h,fields,result,init,validation,output,arrayLengths};
}
