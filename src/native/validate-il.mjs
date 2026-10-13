const TOK=x=>typeof x==='number'?x:x.token;
export function validateBuilder(b){
 const diagnostics=[];
 const memberSig=token=>{const table=token>>>24,index=(token&0xffffff)-1;if(table===6){const m=b.methods[index];if(!m)throw Error('Missing MethodDef '+token.toString(16));return {args:m.args.length,instance:!m.options.static,returnsVoid:m.returns[0]===1};}if(table===10){const m=b.memberRefs[index];if(!m)throw Error('Missing MemberRef '+token.toString(16));const s=m.signature;let pos=1,n=s[pos++];if(n&128){if((n&192)===128)n=((n&63)<<8)|s[pos++];else{n=((n&31)<<24)|(s[pos++]<<16)|(s[pos++]<<8)|s[pos++];}}return{args:n,instance:!!(s[0]&32),returnsVoid:s[pos]===1};}throw Error('Bad call token '+token.toString(16));};
 const tokenExists=token=>{const table=token>>>24,row=token&0xffffff;return row>0&&({1:b.typeRefs,2:[null,...b.types],4:b.fields,6:b.methods,10:b.memberRefs}[table]?.length>=row);};
 for(const m of b.methods){if(m.options.pinvoke)continue;const code=m.il.finish(),inst=new Map();let offset=0;
  try{while(offset<code.length){const start=offset;let op=code[offset++];if(op===254)op=(op<<8)|code[offset++];let size=0,pop=0,push=0,flow='next',target=null,token=null;
    if((op>=2&&op<=9)||op===0x11||op===0x0e||op===0x0f||op===0x12||(op>=0x15&&op<=0x23)||op===0x14||op===0x72||op===0x7e||op===0x7f||op===0xfe06||op===0xfe09||op===0xfe0a||op===0xfe0c||op===0xfe0d)push=1;
    else if((op>=10&&op<=13)||op===0x13||op===0x10||op===0x80||op===0xfe0b||op===0xfe0e)pop=1;
    else if(op===0x25){pop=1;push=2;}else if(op===0x26)pop=1;
    else if([0x58,0x59,0x5a,0x5b,0x5d,0x5f,0x60,0x61,0xfe01,0xfe02,0xfe04].includes(op)){pop=2;push=1;}
    else if([0x65,0x66,0x69,0x6b,0x6c,0x8c,0x74,0x8d,0x8e,0x4a].includes(op)){pop=1;push=1;}
    else if(op===0x7b||op===0x7c){pop=1;push=1;}else if(op===0x7d)pop=2;
    else if([0x94,0x98,0x9a].includes(op)){pop=2;push=1;}else if([0x9e,0xa0,0xa2].includes(op))pop=3;
    else if(op===0x54)pop=2;else if(op===0x2a){pop=m.returns[0]===1?0:1;flow='ret';}
    else if(op===0x7a){pop=1;flow='stop';}
    else if(op>=0x38&&op<=0x44){flow=op===0x38?'jump':'cond';pop=op===0x38?0:op<=0x3a?1:2;size=4;target=offset+4+code.readInt32LE(offset);}
    else if([0x28,0x6f,0x73].includes(op)){size=4;token=code.readUInt32LE(offset);const s=memberSig(token);pop=s.args+(op===0x73?0:s.instance?1:0);push=op===0x73||!s.returnsVoid?1:0;}
    else if(op!==0)throw Error('Unsupported opcode 0x'+op.toString(16)+' at '+start);
    if([0x0e,0x0f,0x10,0x11,0x12,0x13,0x1f].includes(op))size=1;
    if([0xfe09,0xfe0a,0xfe0b,0xfe0c,0xfe0d,0xfe0e].includes(op))size=2;
    if([0x20,0x22,0x72,0x7b,0x7c,0x7d,0x7e,0x7f,0x80,0x8c,0x74,0x8d,0xfe06].includes(op))size=4;
    if(op===0x21||op===0x23)size=8;
    if(size===4&&[0x7b,0x7c,0x7d,0x7e,0x7f,0x80,0x8c,0x74,0x8d,0xfe06].includes(op)){const t=code.readUInt32LE(offset);if(!tokenExists(t))throw Error('Unknown metadata token 0x'+t.toString(16));}
    offset+=size;if(offset>code.length)throw Error('Truncated instruction');inst.set(start,{start,next:offset,pop,push,flow,target});
   }
   const states=new Map(),queue=[[0,0]];let max=0;
   while(queue.length){const[p,s]=queue.pop();if(states.has(p)){if(states.get(p)!==s)throw Error('Stack mismatch at '+p+': '+states.get(p)+' vs '+s);continue;}states.set(p,s);const i=inst.get(p);if(!i)throw Error('Branch/fallthrough invalid instruction '+p);if(s<i.pop)throw Error('Stack underflow at '+p+' (have '+s+', need '+i.pop+')');const next=s-i.pop+i.push;max=Math.max(max,next);if(i.flow==='ret'){if(next!==0)throw Error('Nonempty return stack at '+p+': '+next);continue;}if(i.flow==='stop')continue;if(i.flow==='jump')queue.push([i.target,next]);else{queue.push([i.next,next]);if(i.flow==='cond')queue.push([i.target,next]);}}
   if(max>(m.options.maxStack??128))throw Error('maxStack header is too small');diagnostics.push({method:m.owner.name+'.'+m.name,valid:true,codeBytes:code.length,reachableInstructions:states.size,maxStack:max});
  }catch(error){diagnostics.push({method:m.owner.name+'.'+m.name,valid:false,error:error.message});}
 }
 return{valid:diagnostics.every(d=>d.valid),methodsChecked:diagnostics.length,diagnostics};
}
