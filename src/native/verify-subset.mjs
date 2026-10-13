
import { ManagedBuilder, Sig } from './build-managed.mjs';
import { compileMethods } from './compile-subset.mjs';

// This simulator validates emitted CIL and game logic without launching an EXE.
// It is deliberately limited to the CIL instructions used by game bodies.
export function executeMethod(method,args=[],state={},methods=[],functions={},budget={remaining:2000000}) {
  const code=method.il.finish(),stack=[],stackTypes=[],locals=(method.locals||[]).map(t=>t[0]===14||t[0]===29?null:0);let pc=0,lastType='int';
  const sigType=s=>s?.[0]===12?'float':s?.[0]===13?'double':s?.[0]===14?'string':s?.[0]===29?'array':'int';
  const push=(v,t)=>{stack.push(v);stackTypes.push(t||((typeof v==='string')?'string':Array.isArray(v)?'array':'int'));};
  const fieldType=t=>sigType(method.il.builder.fields.find(f=>f.token===t)?.signature);
  const pop=()=>{if(!stack.length)throw Error('CIL stack underflow in '+method.name+' at '+(pc-1));lastType=stackTypes.pop();return stack.pop();};
  const i32=()=>{const n=code.readInt32LE(pc);pc+=4;return n;},u32=()=>{const n=code.readUInt32LE(pc);pc+=4;return n;};
  const store=(i)=>{let v=pop();if(method.locals?.[i]?.[0]===12)v=Math.fround(v);locals[i]=v;};
  const call=token=>{
    if(functions[token]){const f=functions[token],a=stack.splice(stack.length-f.argTypes.length);stackTypes.splice(stackTypes.length-f.argTypes.length);const value=f.fn(...a);if(f.returnType!=='void')push(value,f.returnType);return;}
    const callee=methods.find(m=>m.token===token);if(!callee)throw Error('Unknown simulated call '+token.toString(16));
    const a=stack.splice(stack.length-callee.args.length);stackTypes.splice(stackTypes.length-callee.args.length);const value=executeMethod(callee,a,state,methods,functions,budget);
    if(callee.returns[0]!==1)push(value,sigType(callee.returns));
  };
  while(pc<code.length){
    if(--budget.remaining<=0)throw Error('CIL simulation budget exceeded');
    const start=pc,op=code[pc++];
    if(op===0)continue;
    if(op>=2&&op<=5){push(args[op-2],sigType(method.args[op-2]));continue;}
    if(op>=6&&op<=9){push(locals[op-6],sigType(method.locals[op-6]));continue;}
    if(op>=10&&op<=13){store(op-10);continue;}
    if(op===14){const i=code[pc++];push(args[i],sigType(method.args[i]));continue;}
    if(op===16){args[code[pc++]]=pop();continue;}
    if(op===17){const i=code[pc++];push(locals[i],sigType(method.locals[i]));continue;}
    if(op===19){store(code[pc++]);continue;}
    if(op===20){push(null);continue;}
    if(op>=21&&op<=30){push(op-22);continue;}
    if(op===32){push(i32());continue;}
    if(op===34){push(code.readFloatLE(pc),'float');pc+=4;continue;}
    if(op===35){push(code.readDoubleLE(pc),'double');pc+=8;continue;}
    if(op===37){push(stack[stack.length-1],stackTypes[stackTypes.length-1]);continue;}
    if(op===38){pop();continue;}
    if(op===40){call(u32());continue;}
    if(op===42){const value=method.returns[0]===1?undefined:pop();if(stack.length)throw Error('Non-empty CIL stack on return '+method.name+': '+stack.length);return value;}
    if(op===56||op===57||op===58){const offset=i32(),go=op===56?true:op===57?!pop():!!pop();if(go)pc+=offset;continue;}
    if(op>=88&&op<=102){
      if(op===101){const v=pop(),t=lastType;push(t==='int'?(-v|0):-v,t);continue;}if(op===102){push(~pop());continue;}
      const b=pop(),bt=lastType,a=pop(),at=lastType,t=at==='double'||bt==='double'?'double':at==='float'||bt==='float'?'float':'int';let value;
      if(op===88)value=a+b;else if(op===89)value=a-b;else if(op===90)value=a*b;else if(op===91){if(b===0)throw Error('Division by zero');value=a/b;}else if(op===93)value=a%b;else if(op===95)value=a&b;else if(op===96)value=a|b;else if(op===97)value=a^b;else throw Error('Unknown arithmetic CIL '+op);
      push(t==='int'?value|0:value,t);continue;
    }
    if(op===105){push(pop()|0);continue;}if(op===107){push(Math.fround(pop()),'float');continue;}if(op===108){const v=pop();push(v,'double');continue;}
    if(op===114){const token=u32(),offset=token&0xffffff,parts=method.il.builder.users.parts;let pos=0,value=null;for(const part of parts){if(pos===offset){let n=part[0],count=1;if(n&128){if(!(n&64)){n=((n&63)<<8)|part[1];count=2;}else{n=((n&31)*2**24)+(part[1]<<16)+(part[2]<<8)+part[3];count=4;}}value=part.toString('utf16le',count,count+n-1);break;}pos+=part.length;}if(value===null)throw Error('Unknown user string');push(value);continue;}
    if(op===126){const t=u32();push(state[t],fieldType(t));continue;}if(op===128){const token=u32();let v=pop();state[token]=fieldType(token)==='float'?Math.fround(v):v;continue;}
    if(op===141){const token=u32(),n=pop(),name=method.il.builder.typeRefs[(token&0xffffff)-1]?.name;if(n<0||n>1000000)throw Error('Invalid newarr length');push(Array(n).fill(name==='String'?null:0),'array');continue;}
    if(op===142){push(pop().length);continue;}
    if(op===148||op===152||op===154||op===145){const index=pop(),array=pop();if(!array||index<0||index>=array.length||!Number.isInteger(index))throw Error('Array bounds');push(array[index],op===152?'float':op===154?'string':'int');continue;}
    if(op===158||op===160||op===162||op===156){let value=pop();const index=pop(),array=pop();if(!array||index<0||index>=array.length||!Number.isInteger(index))throw Error('Array bounds');if(op===160)value=Math.fround(value);array[index]=value;continue;}
    if(op===254){const sub=code[pc++];if(sub===1||sub===2||sub===4){const b=pop(),a=pop();push(sub===1?(a===b?1:0):sub===2?(a>b?1:0):(a<b?1:0));continue;}
      const index=code.readUInt16LE(pc);pc+=2;
      if(sub===9)push(args[index],sigType(method.args[index]));else if(sub===11)args[index]=pop();else if(sub===12)push(locals[index],sigType(method.locals[index]));else if(sub===14)store(index);else throw Error('Unknown extended CIL '+sub);continue;
    }
    throw Error('Unsupported CIL 0x'+op.toString(16)+' at '+start+' in '+method.name);
  }
  throw Error('CIL falls off method '+method.name);
}
export function runCompilerChecks() {
  const b=new ManagedBuilder('CompilerChecks'),obj=b.typeRef('mscorlib','System','Object'),game=b.defineType('Game',obj);
  const arr=game.field('arr',Sig.array(Sig.float),{static:true}),total=game.field('total',Sig.int,{static:true});
  let touchCount=0;
  const touch=game.method('Touch',Sig.bool,[],{static:true});
  const ext={Touch:{token:touch.token,returnType:'bool',argTypes:[]}};
  const defs=[
    {name:'Loop',returnType:'int',args:[],body:'int i=0; float value=.5; for(i=0;i<5;i++){arr[i]=value; value+=1.0f;} while(i>0){i--; if(i==2){continue;} total+=i; if(total>50){break;}} if(value>=4.0 && i==0){return 0xFF182938;} return 0;'},
    {name:'Logic',returnType:'int',args:[],body:'if(false && Touch()){return 1;} if(true || Touch()){return 2;} return 3;'},
    {name:'Float',returnType:'float',args:[{name:'x',type:'int'}],body:'float y=x; y=(y+0.5f)*2; return y;'},
    {name:'Strings',returnType:'string',args:[],body:'string a="同学\\n别刷了"; return a;'},
    {name:'Post',returnType:'int',args:[],body:'int a=3; int old=a++; int now=++a; return old*100+now*10+a;'},
    {name:'ArrayExpression',returnType:'float',args:[],body:'float x=(arr[0]=2.5); arr[1]+=x; return arr[1];'},
    {name:'Nested',returnType:'int',args:[],body:'int i=0; int j=0; int n=0; for(i=0;i<4;i++){for(j=0;j<3;j++){if(j==1){continue;} n++; if(i==2){break;}}} return n;'},
    {name:'ArgAssign',returnType:'int',args:[{name:'x',type:'int'}],body:'x+=2; return x;'},
    {name:'Casts',returnType:'int',args:[],body:'float x=(float)2; return (int)(x+1.9);'},
    {name:'StringConcat',returnType:'string',args:[],body:'return "街道 "+"1";'},
    {name:'StringCompare',returnType:'bool',args:[],body:'return "same"+" string"=="same string";'},
    {name:'IntegerDiv',returnType:'int',args:[],body:'return (7/2)*10+(-7/2);'},
    {name:'Eq',returnType:'bool',args:[],body:'return 4==4 && 3!=4 && 3<4 && 4<=4 && 5>4 && 4>=4;'}
  ];
  const compiled=compileMethods(b,game,defs,{arr:{token:arr,type:'float[]'},total:{token:total,type:'int'}},ext).compiled;
  const functions={[touch.token]:{argTypes:[],returnType:'bool',fn:()=>{touchCount++;return true;}}};
  for(const [i,m] of b.memberRefs.entries()){const token=0x0a000000|i+1;if(m.name==='op_Equality')functions[token]={argTypes:['string','string'],returnType:'bool',fn:(a,z)=>a===z?1:0};else if(m.name==='Concat')functions[token]={argTypes:['string','string'],returnType:'string',fn:(a,z)=>(a||'')+(z||'')};}
  const state={[arr]:Array(8).fill(0),[total]:0},results=[];
  const run=(name,args,expected)=>{const m=compiled.find(c=>c.definition.name===name).method,value=executeMethod(m,args,state,b.methods,functions);if(value!==expected)throw Error(name+' expected '+expected+', got '+value);results.push({name,result:value,codeBytes:m.il.finish().length});};
  run('Loop',[],0xFF182938|0);if(state[total]!==8)throw Error('Loop total mismatch');if(state[arr][4]!==4.5)throw Error('Float array mismatch');
  run('Logic',[],2);if(touchCount!==0)throw Error('Short circuit called Touch');
  run('Float',[5],11);run('Strings',[],'同学\n别刷了');run('Post',[],355);run('ArrayExpression',[],4);run('Nested',[],7);run('ArgAssign',[7],9);run('Eq',[],1);run('IntegerDiv',[],27);run('Casts',[],3);run('StringConcat',[],'街道 1');run('StringCompare',[],1);
  return results;
}
