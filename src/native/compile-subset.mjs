
// Small deterministic C#-style language to CLR IL compiler.
// No external source or code is executed by this module.
export const TypeSignatures = {void:[1],bool:[2],int:[8],float:[12],string:[14],'int[]':[29,8],'float[]':[29,12],'string[]':[29,14],'bool[]':[29,2]};
export class CompileError extends Error {
  constructor(message,token) { super(message+(token?' at '+token.line+':'+token.column:''));this.name='CompileError';this.token=token; }
}
export function tokenize(source) {
  const result=[];let i=0,line=1,column=1;
  const advance=()=>{const c=source[i++];if(c==='\n'){line++;column=1;}else column++;return c;};
  const add=(kind,text,startLine,startColumn)=>result.push({kind,text,line:startLine,column:startColumn});
  while(i<source.length) {
    const c=source[i];
    if(/\s/.test(c)){advance();continue;}
    if(c==='/'&&source[i+1]==='/'){while(i<source.length&&source[i]!=='\n')advance();continue;}
    if(c==='/'&&source[i+1]==='*'){advance();advance();while(i<source.length&&!(source[i]==='*'&&source[i+1]==='/'))advance();if(i===source.length)throw new CompileError('Unclosed comment',{line,column});advance();advance();continue;}
    const ln=line,col=column;
    if(/[A-Za-z_]/.test(c)){let s='';while(i<source.length&&/[A-Za-z0-9_]/.test(source[i]))s+=advance();add('id',s,ln,col);continue;}
    if(/[0-9]/.test(c)||(c==='.'&&/[0-9]/.test(source[i+1]))) {
      if(c==='0'&&/[xX]/.test(source[i+1]||'~')) { let hex=advance()+advance();while(i<source.length&&/[0-9a-fA-F]/.test(source[i]))hex+=advance();if(hex.length===2)throw new CompileError('Invalid hexadecimal literal',{line:ln,column:col});add('number',hex,ln,col);continue; }
      let s='';while(i<source.length&&/[0-9]/.test(source[i]))s+=advance();
      if(source[i]==='.'){s+=advance();while(i<source.length&&/[0-9]/.test(source[i]))s+=advance();}
      if(/[eE]/.test(source[i]||'~')){s+=advance();if(/[+-]/.test(source[i]))s+=advance();while(i<source.length&&/[0-9]/.test(source[i]))s+=advance();}
      if(/[fF]/.test(source[i]||'~'))s+=advance();add('number',s,ln,col);continue;
    }
    if(c==='"') {
      let raw=advance(),closed=false;
      while(i<source.length){const a=advance();raw+=a;if(a==='\\'){if(i>=source.length)break;raw+=advance();}else if(a==='"'){closed=true;break;}}
      if(!closed)throw new CompileError('Unclosed string',{line:ln,column:col});
      let value;try{value=JSON.parse(raw);}catch{throw new CompileError('Invalid string escape',{line:ln,column:col});}
      add('string',value,ln,col);continue;
    }
    const pair=source.slice(i,i+2);
    if(['++','--','+=','-=','*=','/=','%=','<=','>=','==','!=','&&','||'].includes(pair)){advance();advance();add('op',pair,ln,col);continue;}
    if('{}();,[]+-*/%<>=!'.includes(c)){add('op',advance(),ln,col);continue;}
    throw new CompileError('Unexpected character '+c,{line:ln,column:col});
  }
  result.push({kind:'eof',text:'<eof>',line,column});return result;
}
export function parseBody(source) {
  const tokens=tokenize(source);let p=0;
  const peek=()=>tokens[p],take=()=>tokens[p++];
  const is=s=>peek().text===s,eat=s=>{if(is(s)){take();return true;}return false;};
  const expect=s=>{const t=take();if(t.text!==s)throw new CompileError('Expected '+s+', got '+t.text,t);return t;};
  const identifier=()=>{const t=take();if(t.kind!=='id')throw new CompileError('Expected identifier',t);return t;};
  const typeAhead=()=>['int','float','bool','string'].includes(peek().text);
  const expr=()=>assignment();
  function primary() {
    const t=take();let node;
    if(t.kind==='number'){const hex=/^0[xX]/.test(t.text),isFloat=!hex&&/[.eEfF]/.test(t.text);const parsed=Number(hex?t.text:t.text.replace(/[fF]$/,''));const value=hex?parsed|0:parsed;if(!Number.isFinite(value))throw new CompileError('Invalid number',t);node={kind:'literal',type:isFloat?'float':'int',value,token:t};}
    else if(t.kind==='string')node={kind:'literal',type:'string',value:t.text,token:t};
    else if(t.text==='true'||t.text==='false')node={kind:'literal',type:'bool',value:t.text==='true'?1:0,token:t};
    else if(t.text==='null')node={kind:'literal',type:'null',value:null,token:t};
    else if(t.kind==='id')node={kind:'name',name:t.text,token:t};
    else if(t.text==='('){node=expr();expect(')');}
    else throw new CompileError('Expected expression, got '+t.text,t);
    while(true){
      if(eat('(')){if(node.kind!=='name')throw new CompileError('Only named function calls are supported',t);const args=[];if(!is(')')){do{args.push(expr());}while(eat(','));}expect(')');node={kind:'call',name:node.name,args,token:t};}
      else if(eat('[')){const index=expr();expect(']');node={kind:'index',array:node,index,token:t};}
      else if(is('++')||is('--')){const op=take().text;node={kind:'increment',target:node,amount:op==='++'?1:-1,post:true,token:t};}
      else break;
    }
    return node;
  }
  function unary(){if(is('(')&&['int','float','bool','string'].includes(tokens[p+1]?.text)&&tokens[p+2]?.text===')'){const t=take(),type=take().text;expect(')');return {kind:'cast',type,arg:unary(),token:t};}if(['-','!','+','++','--'].includes(peek().text)){const t=take();const arg=unary();return t.text==='++'||t.text==='--'?{kind:'increment',target:arg,amount:t.text==='++'?1:-1,post:false,token:t}:{kind:'unary',op:t.text,arg,token:t};}return primary();}
  const levels=[['||'],['&&'],['==','!='],['<','<=','>','>='],['+','-'],['*','/','%']];
  function binary(level=0){if(level===levels.length)return unary();let left=binary(level+1);while(levels[level].includes(peek().text)){const t=take();left={kind:'binary',op:t.text,left,right:binary(level+1),token:t};}return left;}
  function assignment(){const target=binary();if(['=','+=','-=','*=','/=','%='].includes(peek().text)){const t=take();let value=assignment();if(t.text!=='=')value={kind:'binary',op:t.text[0],left:target,right:value,token:t};return {kind:'assign',target,value,token:t};}return target;}
  function declaration(withSemicolon=true){const t=take();let type=t.text;if(eat('[')){expect(']');type+='[]';}const declarations=[];do{const n=identifier();declarations.push({name:n.text,value:eat('=')?expr():null,token:n});}while(eat(','));if(withSemicolon)expect(';');return {kind:'declare',type,declarations,token:t};}
  function statement(){
    const t=peek();
    if(eat('{')){const body=[];while(!is('}')){if(peek().kind==='eof')throw new CompileError('Unclosed block',t);body.push(statement());}expect('}');return {kind:'block',body,token:t};}
    if(eat(';'))return {kind:'empty',token:t};
    if(eat('if')){expect('(');const condition=expr();expect(')');const yes=statement(),no=eat('else')?statement():null;return {kind:'if',condition,yes,no,token:t};}
    if(eat('while')){expect('(');const condition=expr();expect(')');return {kind:'while',condition,body:statement(),token:t};}
    if(eat('for')){expect('(');let init=null,condition=null,update=null;if(!is(';'))init=typeAhead()?declaration(false):{kind:'expression',expression:expr(),token:t};expect(';');if(!is(';'))condition=expr();expect(';');if(!is(')'))update=expr();expect(')');return {kind:'for',init,condition,update,body:statement(),token:t};}
    if(eat('return')){const value=is(';')?null:expr();expect(';');return {kind:'return',value,token:t};}
    if(eat('break')){expect(';');return {kind:'break',token:t};}
    if(eat('continue')){expect(';');return {kind:'continue',token:t};}
    if(typeAhead())return declaration();
    const expression=expr();expect(';');return {kind:'expression',expression,token:t};
  }
  const body=[];while(peek().kind!=='eof')body.push(statement());return {kind:'block',body,token:tokens[0]};
}
export function compileBody(source, env, il) {
  const ast=typeof source==='string'?parseBody(source):source;
  const fields=env.fields||{},methods=env.methods||{},args=env.args||{},returnType=env.returnType||'void';
  const localTypes=[],localNames=[],scopes=[new Map()],loops=[];
  let serial=0;
  const label=prefix=>'cs_'+prefix+'_'+serial++;
  const sig=t=>{if(!TypeSignatures[t])throw new CompileError('Unsupported type '+t);return [...TypeSignatures[t]];};
  const allocate=(name,type)=>{const index=localTypes.length;localTypes.push(sig(type));localNames.push({name,type,index});return {kind:'local',index,type,name};};
  const fail=(message,node)=>{throw new CompileError(message,node?.token);};
  const lookup=node=>{for(let i=scopes.length-1;i>=0;i--)if(scopes[i].has(node.name))return scopes[i].get(node.name);if(args[node.name])return {kind:'arg',...args[node.name],name:node.name};if(fields[node.name])return {kind:'field',...fields[node.name],name:node.name};fail('Unknown variable '+node.name,node);};
  const functionInfo=node=>{if(!methods[node.name])fail('Unknown function '+node.name,node);return methods[node.name];};
  const numeric=t=>t==='int'||t==='float';
  const numericCommon=(a,b,node)=>{if(!numeric(a)||!numeric(b))fail('Numeric operands required: '+a+', '+b,node);return a==='float'||b==='float'?'float':'int';};
  const typeOf=node=>{
    if(node.kind==='literal'||node.kind==='cast')return node.type;
    if(node.kind==='name')return lookup(node).type;
    if(node.kind==='index'){const t=typeOf(node.array);if(!t.endsWith('[]'))fail('Index requires an array',node);return t.slice(0,-2);}
    if(node.kind==='call')return functionInfo(node).returnType||'void';
    if(node.kind==='assign'||node.kind==='increment')return typeOf(node.target);
    if(node.kind==='unary'){const t=typeOf(node.arg);if(node.op==='!')return 'bool';if(!numeric(t))fail('Unary arithmetic requires a number',node);return t;}
    if(node.kind==='binary'){if(node.op==='+'&&typeOf(node.left)==='string'&&typeOf(node.right)==='string')return 'string';if(['&&','||','<','<=','>','>=','==','!='].includes(node.op))return 'bool';return numericCommon(typeOf(node.left),typeOf(node.right),node);}
    fail('Unsupported expression '+node.kind,node);
  };
  const convert=(from,to,node)=>{
    if(from===to||from==='null'&&(to==='string'||to.endsWith('[]')))return;
    if(from==='int'&&to==='float'){il.op('conv.r4');return;}
    if(from==='bool'&&to==='int'||from==='int'&&to==='bool')return;
    fail('Cannot convert '+from+' to '+to,node);
  };
  const emitAs=(node,to)=>{const actual=emitExpression(node);convert(actual,to,node);};
  const storeName=(target)=>{
    const v=lookup(target);
    if(v.kind==='local')il.store(v.index);
    else if(v.kind==='field')il.storeStaticField(v.token);
    else if(v.kind==='arg'&&typeof il.storeArg==='function')il.storeArg(v.index);
    else fail('Assignment to arguments needs a local copy',target);
  };
  const elementLoad=t=>t==='float'?'ldelem.r4':t==='string'?'ldelem.ref':t==='bool'?'ldelem.u1':'ldelem.i4';
  const elementStore=t=>t==='float'?'stelem.r4':t==='string'?'stelem.ref':t==='bool'?'stelem.i1':'stelem.i4';
  const emitAssignment=(target,value,keep=true)=>{
    const t=typeOf(target);
    if(target.kind==='name'){emitAs(value,t);if(keep)il.op('dup');storeName(target);}
    else if(target.kind==='index'){
      emitExpression(target.array);emitAs(target.index,'int');emitAs(value,t);
      if(keep){const temp=allocate('$assign',t);il.store(temp.index);il.local(temp.index);il.op(elementStore(t));il.local(temp.index);}
      else il.op(elementStore(t));
    }else fail('Invalid assignment target',target);
    return t;
  };
  const emitIncrement=(node)=>{
    const t=typeOf(node.target);if(!numeric(t))fail('Increment requires numeric target',node);
    let old=null;
    if(node.post){old=allocate('$old',t);emitExpression(node.target);il.store(old.index);}
    const value={kind:'binary',op:node.amount===1?'+':'-',left:node.target,right:{kind:'literal',type:'int',value:1,token:node.token},token:node.token};
    emitAssignment(node.target,value,!node.post);if(node.post)il.local(old.index);return t;
  };
  const emitExpression=node=>{
    const t=typeOf(node);
    switch(node.kind){
      case 'literal':
        if(t==='float')il.float(node.value);else if(t==='string')il.str(node.value);else if(t==='null')il.op('ldnull');else il.int(node.value);break;
      case 'name':{const v=lookup(node);if(v.kind==='local')il.local(v.index);else if(v.kind==='arg')il.arg(v.index);else il.staticField(v.token);break;}
      case 'index':emitExpression(node.array);emitAs(node.index,'int');il.op(elementLoad(t));break;
      case 'call':{
        const m=functionInfo(node),expected=m.argTypes||[];
        if(node.args.length!==expected.length)fail('Function '+node.name+' expects '+expected.length+' arguments, got '+node.args.length,node);
        if(m.instance)fail('Only static function wrappers are supported',node);
        node.args.forEach((a,i)=>emitAs(a,expected[i]));il.call(m.token);break;
      }
      case 'cast':{const from=emitExpression(node.arg);if(from!==t){if(numeric(from)&&numeric(t))il.op(t==='int'?'conv.i4':'conv.r4');else convert(from,t,node);}break;}
      case 'assign':emitAssignment(node.target,node.value);break;
      case 'increment':emitIncrement(node);break;
      case 'unary':emitExpression(node.arg);if(node.op==='-')il.op('neg');else if(node.op==='!'){il.int(0);il.ceq();}break;
      case 'binary':{
        const op=node.op;
        if(op==='&&'||op==='||'){
          const early=label('logicEarly'),done=label('logicDone');
          emitExpression(node.left);il.branch(op==='&&'?'brfalse':'brtrue',early);
          emitExpression(node.right);il.int(0);il.ceq();il.int(0);il.ceq();il.branch('br',done);
          il.label(early);il.int(op==='&&'?0:1);il.label(done);break;
        }
        const lt=typeOf(node.left),rt=typeOf(node.right);
        if(op==='+'&&lt==='string'&&rt==='string'){emitExpression(node.left);emitExpression(node.right);let concat=methods.Join?.token;if(!concat){if(!env.builder)fail('String concatenation requires Join wrapper or env.builder',node);const owner=env.builder.typeRef('mscorlib','System','String');concat=env.builder.member(owner,'Concat',TypeSignatures.string,[TypeSignatures.string,TypeSignatures.string]);}il.call(concat);break;}
        const compare=['==','!=','<','<=','>','>='].includes(op);
        let common;
        if(numeric(lt)&&numeric(rt))common=numericCommon(lt,rt,node);
        else if(compare&&['==','!='].includes(op)&&(lt===rt||lt==='null'||rt==='null'))common=lt==='null'?rt:lt;
        else fail('Invalid operands '+lt+' '+op+' '+rt,node);
        emitAs(node.left,common);emitAs(node.right,common);
        if((op==='=='||op==='!=')&&common==='string'){if(!env.builder)fail('String equality requires env.builder',node);const owner=env.builder.typeRef('mscorlib','System','String'),eq=env.builder.member(owner,'op_Equality',TypeSignatures.bool,[TypeSignatures.string,TypeSignatures.string]);il.call(eq);if(op==='!='){il.int(0);il.ceq();}break;}
        if(op==='==')il.ceq();else if(op==='!='){il.ceq();il.int(0);il.ceq();}
        else if(op==='<')il.clt();else if(op==='>')il.cgt();
        else if(op==='<='||op==='>='){op==='<='?il.cgt():il.clt();il.int(0);il.ceq();}
        else {il.op({'+':'add','-':'sub','*':'mul','/':'div','%':'rem'}[op]);if(common==='float')il.op('conv.r4');}
        break;
      }
      default:fail('Unsupported expression '+node.kind,node);
    }
    return t;
  };
  const discard=node=>{if(node.kind==='assign'){emitAssignment(node.target,node.value,false);return;}const t=emitExpression(node);if(t!=='void')il.op('pop');};
  const scope=fn=>{scopes.push(new Map());try{fn();}finally{scopes.pop();}};
  const emitStatement=node=>{
    switch(node.kind){
      case 'empty':break;
      case 'block':scope(()=>node.body.forEach(emitStatement));break;
      case 'declare':
        for(const d of node.declarations){const map=scopes[scopes.length-1];if(map.has(d.name))fail('Duplicate local '+d.name,d);const v=allocate(d.name,node.type);map.set(d.name,v);if(d.value){emitAs(d.value,node.type);il.store(v.index);}}
        break;
      case 'expression':discard(node.expression);break;
      case 'if':{const otherwise=label('else'),done=label('endif');emitExpression(node.condition);il.branch('brfalse',otherwise);scope(()=>emitStatement(node.yes));il.branch('br',done);il.label(otherwise);if(node.no)scope(()=>emitStatement(node.no));il.label(done);break;}
      case 'while':{
        const begin=label('while'),end=label('endwhile');il.label(begin);emitExpression(node.condition);il.branch('brfalse',end);loops.push({break:end,continue:begin});scope(()=>emitStatement(node.body));loops.pop();il.branch('br',begin);il.label(end);break;
      }
      case 'for':scope(()=>{
        const begin=label('for'),update=label('forUpdate'),end=label('endfor');
        if(node.init)emitStatement(node.init);il.label(begin);if(node.condition){emitExpression(node.condition);il.branch('brfalse',end);}
        loops.push({break:end,continue:update});scope(()=>emitStatement(node.body));loops.pop();il.label(update);if(node.update)discard(node.update);il.branch('br',begin);il.label(end);
      });break;
      case 'break':case 'continue':if(!loops.length)fail(node.kind+' requires a loop',node);il.branch('br',loops[loops.length-1][node.kind]);break;
      case 'return':if(returnType==='void'){if(node.value)fail('Void method cannot return a value',node);}else{if(!node.value)fail('Return value required',node);emitAs(node.value,returnType);}il.ret();break;
      default:fail('Unsupported statement '+node.kind,node);
    }
  };
  emitStatement(ast);
  // The default return also gives empty bodies and fall-through branches valid IL.
  if(returnType!=='void'){if(returnType==='float')il.float(0);else if(returnType==='string'||returnType.endsWith('[]'))il.op('ldnull');else il.int(0);}
  il.ret();
  return {ast,locals:localTypes,localNames};
}
export function compileMethods(builder,type,definitions,fields={},externalMethods={}) {
  const methods={...externalMethods},compiled=[];
  for(const definition of definitions){
    const args=definition.args||[],returnType=definition.returnType||'void';
    const method=type.method(definition.name,TypeSignatures[returnType],args.map(a=>TypeSignatures[a.type]),{static:true,public:definition.public!==false,locals:[]});
    methods[definition.name]={token:method.token,returnType,argTypes:args.map(a=>a.type),instance:false};
    compiled.push({definition,method});
  }
  for(const entry of compiled){
    const {definition,method}=entry,args={};
    (definition.args||[]).forEach((a,index)=>args[a.name]={index,type:a.type});
    const result=compileBody(definition.body,{builder,fields,methods,args,returnType:definition.returnType||'void'},method.il);
    if(typeof method.setLocals==='function')method.setLocals(result.locals);else method.locals=result.locals;
    entry.locals=result.localNames;entry.ast=result.ast;
  }
  return {methods,compiled};
}

export function extractCSharpDefinitions(source) {
  const fields=[],definitions=[];
  const fieldPattern=/\bstatic\s+(int|float|bool|string)(\s*\[\s*\])?\s+([^;{}()]+);/g;
  for(const match of source.matchAll(fieldPattern)){const type=match[1]+(match[2]?'[]':'');for(const name of match[3].split(',')){const n=name.trim();if(!/^[A-Za-z_][A-Za-z0-9_]*$/.test(n))throw new CompileError('Unsupported field declaration '+n);fields.push({name:n,type});}}
  const methodPattern=/\bstatic\s+(void|int|float|bool|string)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(([^)]*)\)\s*\{/g;
  let match;
  while((match=methodPattern.exec(source))) {
    const start=methodPattern.lastIndex;let pos=start,depth=1,inString=false,inLine=false,inBlock=false;
    while(pos<source.length&&depth){const c=source[pos],n=source[pos+1];
      if(inLine){if(c==='\n')inLine=false;pos++;continue;}
      if(inBlock){if(c==='*'&&n==='/'){inBlock=false;pos+=2;}else pos++;continue;}
      if(inString){if(c==='\\'){pos+=2;continue;}if(c==='"')inString=false;pos++;continue;}
      if(c==='/'&&n==='/'){inLine=true;pos+=2;continue;}if(c==='/'&&n==='*'){inBlock=true;pos+=2;continue;}
      if(c==='"'){inString=true;pos++;continue;}if(c==='{')depth++;if(c==='}')depth--;pos++;
    }
    if(depth)throw new CompileError('Unclosed method '+match[2]);
    const args=match[3].trim()?match[3].split(',').map(a=>{const pieces=a.trim().split(/\s+/);if(pieces.length!==2)throw new CompileError('Unsupported parameter '+a);return {type:pieces[0],name:pieces[1]};}):[];
    definitions.push({name:match[2],returnType:match[1],args,body:source.slice(start,pos-1)});
    methodPattern.lastIndex=pos;
  }
  return {fields,definitions};
}
