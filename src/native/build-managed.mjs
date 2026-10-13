import fs from 'node:fs/promises';
import crypto from 'node:crypto';
import path from 'node:path';
const align=(n,a)=>Math.ceil(n/a)*a;
const u16=n=>{const b=Buffer.alloc(2);b.writeUInt16LE(n);return b;};
const u32=n=>{const b=Buffer.alloc(4);b.writeUInt32LE(n>>>0);return b;};
const u64=n=>{const b=Buffer.alloc(8);b.writeBigUInt64LE(n);return b;};
const cat=(...values)=>Buffer.concat(values.flat(Infinity));
const cstr=s=>Buffer.from(s+'\0','utf8');
const pad=(b,a)=>cat(b,Buffer.alloc(align(b.length,a)-b.length));
export const compressed=n=>n<0x80?Buffer.from([n]):n<0x4000?Buffer.from([(n>>>8)|0x80,n&255]):Buffer.from([(n>>>24)|0xc0,(n>>>16)&255,(n>>>8)&255,n&255]);
const tokenOf=x=>typeof x==='number'?x:x.token;
const typeCode=token=>{token=tokenOf(token);const table=token>>>24,tags={2:0,1:1,27:2};if(!(table in tags))throw Error('Not a type token '+token.toString(16));return ((token&0xffffff)<<2)|tags[table];};
class Heap{
 constructor(kind){this.kind=kind;this.parts=[Buffer.from([0])];this.size=1;this.cache=new Map();}
 add(value){const key=Buffer.isBuffer(value)?value.toString('hex'):value;if(this.cache.has(key))return this.cache.get(key);let data;
 if(this.kind==='strings')data=cstr(value);else if(this.kind==='us'){const chars=Buffer.from(value,'utf16le'),special=Array.from(value).some(c=>{const n=c.charCodeAt(0);return n>255||(n>=1&&n<=8)||(n>=14&&n<=31)||n===39||n===45||n===127;});data=cat(compressed(chars.length+1),chars,Buffer.from([special?1:0]));}else data=cat(compressed(value.length),value);
 const offset=this.size;this.parts.push(data);this.size+=data.length;this.cache.set(key,offset);return offset;}
 buffer(){return cat(this.parts);}
}
export const Sig={void:[1],bool:[2],char:[3],byte:[5],short:[6],int:[8],uint:[9],long:[10],float:[12],double:[13],string:[14],object:[28],nativeInt:[24],array:s=>[29,...s],byref:s=>[16,...s]};
const OPS={nop:0,ldnull:0x14,dup:0x25,pop:0x26,ret:0x2a,br:0x38,brfalse:0x39,brtrue:0x3a,beq:0x3b,bge:0x3c,bgt:0x3d,ble:0x3e,blt:0x3f,bne:0x40,add:0x58,sub:0x59,mul:0x5a,div:0x5b,rem:0x5d,and:0x5f,or:0x60,xor:0x61,neg:0x65,not:0x66,convI4:0x69,convR4:0x6b,convR8:0x6c,throw:0x7a,ldlen:0x8e,ldelemI4:0x94,ldelemR4:0x98,ldelemRef:0x9a,stelemI4:0x9e,stelemR4:0xa0,stelemRef:0xa2,ceq:0xfe01,cgt:0xfe02,clt:0xfe04};
export class IL{
 constructor(builder){this.builder=builder;this.parts=[];this.length=0;this.labels=new Map();this.fixups=[];}
 emit(...chunks){const b=cat(chunks.map(x=>typeof x==='number'?Buffer.from([x]):Buffer.from(x)));this.parts.push(b);this.length+=b.length;return this;}
 op(code,token){if(typeof code==='string')code=OPS[code]??({'ldelem.i4':0x94,'ldelem.r4':0x98,'ldelem.ref':0x9a,'stelem.i4':0x9e,'stelem.r4':0xa0,'stelem.ref':0xa2,'conv.r4':0x6b,'conv.i4':0x69,'conv.r8':0x6c,'ldind.i4':0x4a,'stind.i4':0x54}[code]);if(code===undefined)throw Error('Unknown opcode');if(code>255)this.emit(code>>>8,code&255);else this.emit(code);if(token!==undefined)this.emit(u32(tokenOf(token)));return this;}
 arg(i){return i<=3?this.emit(2+i):i<=255?this.emit(0x0e,i):this.emit(0xfe,9,u16(i));}
 argAddr(i){return i<=255?this.emit(0x0f,i):this.emit(0xfe,10,u16(i));}
 storeArg(i){return i<=255?this.emit(0x10,i):this.emit(0xfe,11,u16(i));}
 local(i){return i<=3?this.emit(6+i):i<=255?this.emit(0x11,i):this.emit(0xfe,12,u16(i));}
 localAddr(i){return i<=255?this.emit(0x12,i):this.emit(0xfe,13,u16(i));}
 store(i){return i<=3?this.emit(10+i):i<=255?this.emit(0x13,i):this.emit(0xfe,14,u16(i));}
 int(n){return n>=-1&&n<=8?this.emit(0x16+n):this.emit(0x20,u32(n));}
 float(n){const b=Buffer.alloc(4);b.writeFloatLE(n);return this.emit(0x22,b);}
 double(n){const b=Buffer.alloc(8);b.writeDoubleLE(n);return this.emit(0x23,b);}
 str(s){return this.op(0x72,0x70000000|this.builder.users.add(s));}
 call(t){return this.op(0x28,t);}callvirt(t){return this.op(0x6f,t);}newobj(t){return this.op(0x73,t);}
 field(t){return this.op(0x7b,t);}fieldAddr(t){return this.op(0x7c,t);}storeField(t){return this.op(0x7d,t);}
 staticField(t){return this.op(0x7e,t);}staticFieldAddr(t){return this.op(0x7f,t);}storeStaticField(t){return this.op(0x80,t);}
 newarr(t){return this.op(0x8d,t);}box(t){return this.op(0x8c,t);}cast(t){return this.op(0x74,t);}ldftn(t){return this.op(0xfe06,t);}
 branch(opcode,name){if(typeof opcode==='string')opcode=OPS[opcode];this.emit(opcode);this.fixups.push({offset:this.length,name});return this.emit(u32(0));}
 label(name){if(this.labels.has(name))throw Error('Duplicate label '+name);this.labels.set(name,this.length);return this;}
 finish(){const b=cat(this.parts);for(const f of this.fixups){if(!this.labels.has(f.name))throw Error('Unknown label '+f.name);b.writeInt32LE(this.labels.get(f.name)-f.offset-4,f.offset);}return b;}
}
for(const[name,code]of Object.entries(OPS))if(!IL.prototype[name])IL.prototype[name]=function(){return this.op(code);};
export class ManagedBuilder{
 constructor(name='TongXueNative'){this.name=name;this.strings=new Heap('strings');this.blobs=new Heap('blob');this.users=new Heap('us');this.typeRefs=[];this.memberRefs=[];this.assemblies=[];this.types=[];this.fields=[];this.methods=[];this.assemblyCache=new Map();this.typeCache=new Map();this.memberCache=new Map();this.moduleRefs=[];this.pinvokes=[];}
 assembly(name,version=[4,0,0,0],key){if(this.assemblyCache.has(name))return this.assemblyCache.get(name);key??=name==='System.Drawing'?[0xb0,0x3f,0x5f,0x7f,0x11,0xd5,0x0a,0x3a]:[0xb7,0x7a,0x5c,0x56,0x19,0x34,0xe0,0x89];this.assemblies.push({name,version,key});const row=this.assemblies.length;this.assemblyCache.set(name,row);return row;}
 typeRef(assembly,ns,name){if(typeof assembly==='string')assembly=this.assembly(assembly);const key=assembly+'|'+ns+'|'+name;if(this.typeCache.has(key))return this.typeCache.get(key);this.typeRefs.push({scope:(assembly<<2)|2,ns,name});const token=0x01000000|this.typeRefs.length;this.typeCache.set(key,token);return token;}
 nestedTypeRef(parent,name){this.typeRefs.push({scope:((tokenOf(parent)&0xffffff)<<2)|3,ns:'',name});return 0x01000000|this.typeRefs.length;}
 sig(token,valueType=false){return[valueType?0x11:0x12,...compressed(typeCode(token))];}
 member(owner,name,returns,args=[],options={}){const ownerToken=tokenOf(owner),key=ownerToken+'|'+name+'|'+returns+'|'+args.join(';')+'|'+!!options.instance;if(this.memberCache.has(key))return this.memberCache.get(key);const table=ownerToken>>>24,tags={2:0,1:1,26:2,6:3,27:4};if(!(table in tags))throw Error('Invalid member owner');const signature=[options.instance?0x20:0,...compressed(args.length),...returns,...args.flat()];this.memberRefs.push({parent:((ownerToken&0xffffff)<<3)|tags[table],name,signature});const token=0x0a000000|this.memberRefs.length;this.memberCache.set(key,token);return token;}
 defineType(name,baseToken,options={}){const b=this,type={token:0x02000000|(this.types.length+2),name,ns:options.namespace||'',base:tokenOf(baseToken),flags:options.flags??0x00100001,fields:[],methods:[]};
 type.field=(name,signature,options={})=>{const f={token:0x04000000|(b.fields.length+1),name,signature,flags:options.flags??((options.public?6:1)|(options.static?0x10:0)),owner:type};b.fields.push(f);type.fields.push(f);return f.token;};
 type.method=(name,returns,args=[],options={},callback)=>{const flags=options.flags??((options.public===false?4:6)|0x80|(options.static?0x10:0)|(options.virtual?0x40:0)|(name==='.ctor'||name==='.cctor'?0x1800:0)|(options.pinvoke?0x2000:0));const m={token:0x06000000|(b.methods.length+1),name,returns,args,options,flags,owner:type,il:new IL(b)};Object.defineProperty(m,'locals',{get(){return m.options.locals;},set(v){m.options.locals=v;}});b.methods.push(m);type.methods.push(m);if(options.pinvoke){let row=b.moduleRefs.indexOf(options.pinvoke.dll)+1;if(!row){b.moduleRefs.push(options.pinvoke.dll);row=b.moduleRefs.length;}b.pinvokes.push({method:m.token,module:row,name:options.pinvoke.name||name,flags:options.pinvoke.flags??0x0100});}if(callback)callback(m.il);return m;};this.types.push(type);return type;}
 async write(outputPath,entryMethod,options={}){const strings=this.strings,blobs=this.blobs,str=x=>strings.add(x),blob=x=>blobs.add(Buffer.from(x)),textRva=0x2000,entryToken=tokenOf(entryMethod);if(entryToken>>>24!==6)throw Error('Entry must be MethodDef');
 for(const entries of[this.fields,this.methods]){const seen=new Set();let last=null;for(const e of entries){if(e.owner!==last){if(seen.has(e.owner))throw Error('Interleaved member declarations: '+e.owner.name);seen.add(e.owner);last=e.owner;}}}
 const localRows=[],methodBodies=[];let methodOffset=72;
 for(const m of this.methods){if(m.options.pinvoke){m.rva=0;m.code=Buffer.alloc(0);continue;}const code=m.il.finish();let localToken=0;if(m.options.locals?.length){localRows.push(blob([7,...compressed(m.options.locals.length),...m.options.locals.flat()]));localToken=0x11000000|localRows.length;}const body=cat(u16(0x3013),u16(m.options.maxStack??128),u32(code.length),u32(localToken),code);m.rva=textRva+methodOffset;m.offset=methodOffset;m.code=code;methodBodies.push(pad(body,4));methodOffset+=align(body.length,4);}
 let staCtor=null;if(options.sta!==false){const staType=this.typeRef('mscorlib','System','STAThreadAttribute');staCtor=this.member(staType,'.ctor',Sig.void,[],{instance:true});}
 const tables=new Map();tables.set(0,[cat(u16(0),u16(str(path.basename(outputPath))),u16(1),u16(0),u16(0))]);tables.set(1,this.typeRefs.map(t=>cat(u16(t.scope),u16(str(t.name)),u16(str(t.ns)))));
 let nextField=1,nextMethod=1;const typeRows=[cat(u32(0),u16(str('<Module>')),u16(0),u16(0),u16(1),u16(1))];for(const t of this.types){typeRows.push(cat(u32(t.flags),u16(str(t.name)),u16(str(t.ns)),u16(typeCode(t.base)),u16(nextField),u16(nextMethod)));nextField+=t.fields.length;nextMethod+=t.methods.length;}tables.set(2,typeRows);
 if(this.fields.length)tables.set(4,this.fields.map(f=>cat(u16(f.flags),u16(str(f.name)),u16(blob([6,...f.signature])))));
 tables.set(6,this.methods.map(m=>cat(u32(m.rva),u16(m.options.pinvoke?0x0080:0),u16(m.flags),u16(str(m.name)),u16(blob([m.options.static?0:0x20,...compressed(m.args.length),...m.returns,...m.args.flat()])),u16(1))));
 tables.set(10,this.memberRefs.map(m=>cat(u16(m.parent),u16(str(m.name)),u16(blob(m.signature)))));
 if(staCtor)tables.set(12,[cat(u16((entryToken&0xffffff)<<5),u16(((staCtor&0xffffff)<<3)|3),u16(blob([1,0,0,0])))]);if(localRows.length)tables.set(17,localRows.map(u16));
 if(this.moduleRefs.length)tables.set(26,this.moduleRefs.map(n=>u16(str(n))));
 if(this.pinvokes.length)tables.set(28,this.pinvokes.map(p=>cat(u16(p.flags),u16(((p.method&0xffffff)<<1)|1),u16(str(p.name)),u16(p.module))));
 tables.set(32,[cat(u32(0x8004),u16(1),u16(0),u16(0),u16(0),u32(0),u16(0),u16(str(this.name)),u16(0))]);tables.set(35,this.assemblies.map(a=>cat(a.version.map(u16),u32(0),u16(blob(a.key)),u16(str(a.name)),u16(0),u16(0))));
 if(strings.size>65535||blobs.size>65535||this.users.size>0xffffff)throw Error('Metadata heap too large');const ids=[...tables.keys()].sort((a,b)=>a-b);let valid=0n;for(const id of ids)valid|=1n<<BigInt(id);
 const tableData=cat(u32(0),Buffer.from([2,0,0,1]),u64(valid),u64(staCtor?1n<<12n:0n),ids.map(id=>u32(tables.get(id).length)),ids.map(id=>cat(tables.get(id))));const guid=crypto.createHash('sha256').update(this.name).update(cat(methodBodies)).digest().subarray(0,16);
 const streams=[['#~',tableData],['#Strings',strings.buffer()],['#US',this.users.buffer()],['#GUID',guid],['#Blob',blobs.buffer()]],version=pad(cstr('v4.0.30319'),4);const prefix=cat(u32(0x424a5342),u16(1),u16(1),u32(0),u32(version.length),version,u16(0),u16(streams.length));
 const streamHeaderSize=streams.reduce((s,[name])=>s+8+align(Buffer.byteLength(name)+1,4),0);let streamOffset=prefix.length+streamHeaderSize;const streamHeaders=[],streamBuffers=[];for(const[name,buffer]of streams){streamHeaders.push(cat(u32(streamOffset),u32(buffer.length),pad(cstr(name),4)));const data=pad(buffer,4);streamBuffers.push(data);streamOffset+=data.length;}const metadata=cat(prefix,streamHeaders,streamBuffers),metadataOffset=methodOffset;
 const importsOffset=align(metadataOffset+metadata.length,4),iltOffset=importsOffset+40,iatOffset=iltOffset+8,hintOffset=iatOffset+8,hint=cat(u16(0),cstr('_CorExeMain')),dllOffset=hintOffset+hint.length,nativeOffset=align(dllOffset+cstr('mscoree.dll').length,4),nativeRva=textRva+nativeOffset,imageBase=0x00400000,textSize=nativeOffset+6,textSection=Buffer.alloc(textSize);
 const clrHeader=cat(u32(72),u16(2),u16(5),u32(textRva+metadataOffset),u32(metadata.length),u32(1),u32(entryToken),Buffer.alloc(48));clrHeader.copy(textSection);cat(methodBodies).copy(textSection,72);metadata.copy(textSection,metadataOffset);
 cat(u32(textRva+iltOffset),u32(0),u32(0),u32(textRva+dllOffset),u32(textRva+iatOffset),Buffer.alloc(20)).copy(textSection,importsOffset);cat(u32(textRva+hintOffset),u32(0)).copy(textSection,iltOffset);cat(u32(textRva+hintOffset),u32(0)).copy(textSection,iatOffset);hint.copy(textSection,hintOffset);cstr('mscoree.dll').copy(textSection,dllOffset);cat(Buffer.from([0xff,0x25]),u32(imageBase+textRva+iatOffset)).copy(textSection,nativeOffset);
 const relocRva=align(textRva+textSize,0x2000),reloc=cat(u32((nativeRva+2)&~0xfff),u32(12),u16(0x3000|((nativeRva+2)&0xfff)),u16(0)),headersSize=0x200,rawTextSize=align(textSize,0x200),headers=Buffer.alloc(headersSize);
 headers.writeUInt16LE(0x5a4d,0);headers.writeUInt32LE(0x80,0x3c);Buffer.from('This program cannot be run in DOS mode.\r\n$','ascii').copy(headers,0x40);headers.writeUInt32LE(0x4550,0x80);const coff=0x84;headers.writeUInt16LE(0x14c,coff);headers.writeUInt16LE(2,coff+2);headers.writeUInt16LE(0xe0,coff+16);headers.writeUInt16LE(0x0102,coff+18);const opt=coff+20;headers.writeUInt16LE(0x10b,opt);headers[opt+2]=8;
 headers.writeUInt32LE(rawTextSize,opt+4);headers.writeUInt32LE(0x200,opt+8);headers.writeUInt32LE(nativeRva,opt+16);headers.writeUInt32LE(textRva,opt+20);headers.writeUInt32LE(relocRva,opt+24);headers.writeUInt32LE(imageBase,opt+28);headers.writeUInt32LE(0x2000,opt+32);headers.writeUInt32LE(0x200,opt+36);headers.writeUInt16LE(4,opt+40);headers.writeUInt16LE(4,opt+48);headers.writeUInt32LE(align(relocRva+reloc.length,0x2000),opt+56);headers.writeUInt32LE(headersSize,opt+60);headers.writeUInt16LE(2,opt+68);headers.writeUInt16LE(0x8540,opt+70);headers.writeUInt32LE(0x100000,opt+72);headers.writeUInt32LE(0x1000,opt+76);headers.writeUInt32LE(0x100000,opt+80);headers.writeUInt32LE(0x1000,opt+84);headers.writeUInt32LE(16,opt+92);
 const dir=(i,rva,size)=>{headers.writeUInt32LE(rva,opt+96+i*8);headers.writeUInt32LE(size,opt+100+i*8);};dir(1,textRva+importsOffset,40);dir(5,relocRva,reloc.length);dir(12,textRva+iatOffset,8);dir(14,textRva,72);
 const section=(offset,name,virtualSize,rva,rawSize,rawPtr,flags)=>{cstr(name).copy(headers,offset);headers.writeUInt32LE(virtualSize,offset+8);headers.writeUInt32LE(rva,offset+12);headers.writeUInt32LE(rawSize,offset+16);headers.writeUInt32LE(rawPtr,offset+20);headers.writeUInt32LE(flags,offset+36);};section(opt+0xe0,'.text',textSize,textRva,rawTextSize,headersSize,0x60000020);section(opt+0xe0+40,'.reloc',reloc.length,relocRva,0x200,headersSize+rawTextSize,0x42000040);
 const executable=cat(headers,pad(textSection,0x200),pad(reloc,0x200));await fs.mkdir(path.dirname(outputPath),{recursive:true});await fs.writeFile(outputPath,executable);const report={format:'PE32 CLR GUI, IL-only AnyCPU',runtime:'.NET Framework 4+',bytes:executable.length,sha256:crypto.createHash('sha256').update(executable).digest('hex'),entrypointToken:'0x'+entryToken.toString(16),types:this.types.length,fields:this.fields.length,methods:this.methods.map(m=>({name:m.owner.name+'.'+m.name,token:'0x'+m.token.toString(16),rva:m.rva,codeBytes:m.code.length})),assemblyReferences:this.assemblies.map(a=>a.name),metadataBytes:metadata.length};return{executable,report,metadata,methods:this.methods,typeRefs:this.typeRefs,memberRefs:this.memberRefs};
 }
}
