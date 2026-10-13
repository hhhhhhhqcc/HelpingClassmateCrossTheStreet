
import fs from 'node:fs/promises';
export async function inspectFramework(path) {
  const data=await fs.readFile(path),pe=data.readUInt32LE(0x3c),coff=pe+4,opt=coff+20,dir=opt+(data.readUInt16LE(opt)===0x20b?112:96);
  const sections=[];for(let i=0;i<data.readUInt16LE(coff+2);i++){const p=opt+data.readUInt16LE(coff+16)+i*40;sections.push({rva:data.readUInt32LE(p+12),size:Math.max(data.readUInt32LE(p+8),data.readUInt32LE(p+16)),offset:data.readUInt32LE(p+20)});}
  const rva=n=>{const s=sections.find(s=>n>=s.rva&&n<s.rva+s.size);if(!s)throw Error('Unknown RVA');return s.offset+n-s.rva;};
  const clr=rva(data.readUInt32LE(dir+112)),root=rva(data.readUInt32LE(clr+8)),align=n=>(n+3)&~3;
  let p=align(root+16+data.readUInt32LE(root+12));const streamCount=data.readUInt16LE(p+2);p+=4;const streams={};
  for(let i=0;i<streamCount;i++){const offset=data.readUInt32LE(p),length=data.readUInt32LE(p+4),end=data.indexOf(0,p+8),name=data.toString('ascii',p+8,end);streams[name]={offset:root+offset,length};p=align(end+1);}
  p=(streams['#~']||streams['#-']).offset;const heaps=data[p+6],valid=data.readBigUInt64LE(p+8);p+=24;const counts={};
  for(let i=0;i<64;i++)if(valid&(1n<<BigInt(i))){counts[i]=data.readUInt32LE(p);p+=4;}
  const schemas={0:['2','s','g','g','g'],1:['ResolutionScope','s','s'],2:['4','s','s','TypeDefOrRef','t4','t6'],3:['t4'],4:['2','s','b'],5:['t6'],6:['4','2','2','s','b','t8'],7:['t8'],8:['2','2','s'],9:['t2','TypeDefOrRef'],10:['MemberRefParent','s','b'],11:['2','HasConstant','b'],12:['HasCustomAttribute','CustomAttributeType','b'],13:['HasFieldMarshal','b'],14:['2','HasDeclSecurity','b'],15:['2','4','t2'],16:['4','t4'],17:['b'],18:['t2','t20'],19:['t20'],20:['2','s','TypeDefOrRef'],21:['t2','t23'],22:['t23'],23:['2','s','b'],24:['2','t6','HasSemantics'],25:['t2','MethodDefOrRef','MethodDefOrRef'],26:['s'],27:['b'],28:['2','MemberForwarded','s','t26'],29:['4','t4'],30:['4','4'],31:['4'],32:['4','2','2','2','2','4','b','s','s'],33:['4'],34:['4','4','4'],35:['2','2','2','2','4','b','s','s','b'],36:['4','t35'],37:['4','4','4','t35'],38:['4','s','b'],39:['4','4','s','s','Implementation'],40:['4','4','s','Implementation'],41:['t2','t2'],42:['2','2','TypeOrMethodDef','s'],43:['MethodDefOrRef','b'],44:['t42','TypeDefOrRef']};
  const coded={ResolutionScope:[2,[0,26,35,1]],TypeDefOrRef:[2,[2,1,27]],MemberRefParent:[3,[2,1,26,6,27]],HasConstant:[2,[4,8,23]],HasCustomAttribute:[5,[6,4,1,2,8,9,10,0,14,23,20,17,26,27,32,35,38,39,40,42,44,43]],CustomAttributeType:[3,[null,null,6,10,null]],HasFieldMarshal:[1,[4,8]],HasDeclSecurity:[2,[2,6,32]],HasSemantics:[1,[20,23]],MethodDefOrRef:[1,[6,10]],MemberForwarded:[1,[4,6]],Implementation:[2,[38,35,39]],TypeOrMethodDef:[1,[2,6]]};
  const width=k=>k==='2'?2:k==='4'?4:k==='s'?(heaps&1?4:2):k==='g'?(heaps&2?4:2):k==='b'?(heaps&4?4:2):k[0]==='t'?((counts[+k.slice(1)]||0)>=65536?4:2):coded[k][1].some(t=>(counts[t]||0)>=2**(16-coded[k][0]))?4:2;
  const tables={};for(let id=0;id<=44;id++){if(!counts[id])continue;const ws=schemas[id].map(width),rows=[];for(let i=0;i<counts[id];i++){const row=[];for(const w of ws){row.push(w===2?data.readUInt16LE(p):data.readUInt32LE(p));p+=w;}rows.push(row);}tables[id]=rows;}
  const str=n=>n?data.toString('utf8',streams['#Strings'].offset+n,data.indexOf(0,streams['#Strings'].offset+n)):'';
  const blob=n=>{let p=streams['#Blob'].offset+n,l=data[p++];if(l&128){if(!(l&64))l=((l&63)<<8)|data[p++];else l=((l&31)*2**24)+(data[p++]<<16)+(data[p++]<<8)+data[p++];}return data.subarray(p,p+l);};
  const fullName=(row)=>row?[str(row[2]),str(row[1])].filter(Boolean).join('.'):'UnknownType';
  const types=(tables[2]||[]).map((row,i)=>({rid:i+1,name:fullName(row),start:row[5],base:row[3]}));
  const typeName=n=>(n&3)===0?types[(n>>>2)-1]?.name:(n&3)===1?fullName(tables[1][(n>>>2)-1]):'TypeSpec';
  const primitive={1:'void',2:'bool',3:'char',4:'sbyte',5:'byte',6:'short',7:'ushort',8:'int',9:'uint',10:'long',11:'ulong',12:'float',13:'double',14:'string',24:'IntPtr',25:'UIntPtr',28:'object'};
  const decode=raw=>{
    let p=0;const number=()=>{const b=raw[p++];if(!(b&128))return b;if(!(b&64))return ((b&63)<<8)|raw[p++];return ((b&31)*2**24)+(raw[p++]<<16)+(raw[p++]<<8)+raw[p++];};
    const type=()=>{const b=raw[p++];if(primitive[b])return primitive[b];if(b===17||b===18)return (b===17?'valuetype ':'class ')+typeName(number());if(b===21){const t=type(),n=number(),args=[];for(let i=0;i<n;i++)args.push(type());return t+'<'+args.join(',')+'>';}if(b===20){const t=type(),rank=number(),sizes=number();for(let i=0;i<sizes;i++)number();const lowers=number();for(let i=0;i<lowers;i++)number();return t+'['+','.repeat(Math.max(0,rank-1))+']';}if(b===16)return type()+'&';if(b===15)return type()+'*';if(b===29)return type()+'[]';if(b===19||b===30)return (b===19?'!':'!!')+number();if(b===31||b===32){number();return type();}return 'Type'+b;};
    const conv=raw[p++];if(conv&16)number();const n=number(),returns=type(),args=[];for(let i=0;i<n;i++)args.push(type());return {instance:!!(conv&32),returns,args};
  };
  const methods=(tables[6]||[]).map((row,i)=>{const rid=i+1,owner=types.find((t,j)=>rid>=t.start&&rid<(types[j+1]?.start||counts[6]+1));return {owner:owner?.name,name:str(row[3]),flags:row[2],signature:decode(blob(row[4])),hex:blob(row[4]).toString('hex')};});
  const refs=(tables[1]||[]).map(r=>({scope:r[0],name:fullName(r)}));
  const memberRefs=(tables[10]||[]).map((row,i)=>({token:0x0a000000|i+1,owner:(row[0]&7)===1?refs[(row[0]>>>3)-1]?.name:(row[0]&7)===0?types[(row[0]>>>3)-1]?.name:'Owner'+row[0],name:str(row[1]),signature:decode(blob(row[2]))}));
  const assemblies=(tables[35]||[]).map(row=>({version:row.slice(0,4).join('.'),name:str(row[6]),token:blob(row[5]).toString('hex')}));
  return {types,methods,memberRefs,assemblies,refs,counts,tables,typeName,sections,streams,clr:{flags:data.readUInt32LE(clr+16),entryToken:data.readUInt32LE(clr+20),metadataSize:data.readUInt32LE(clr+12)},path};
}
