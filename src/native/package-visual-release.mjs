import fs from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';
import zlib from 'node:zlib';
const crcTable=Array.from({length:256},(_,n)=>{for(let j=0;j<8;j++)n=(n&1)?0xEDB88320^(n>>>1):n>>>1;return n>>>0;});
export const crc32=data=>{let crc=0xFFFFFFFF;for(const v of data)crc=crcTable[(crc^v)&255]^(crc>>>8);return (crc^0xFFFFFFFF)>>>0;};
export async function listPackageFiles(root){
 const results=[];
 async function walk(dir,prefix=''){for(const entry of await fs.readdir(dir,{withFileTypes:true})){const relative=prefix+entry.name;if(entry.isDirectory())await walk(path.join(dir,entry.name),relative+'/');else if(entry.isFile())results.push(relative);}}
 await walk(root);return results.sort();
}
export async function createZip(root,outputPath,folderName=path.basename(root)){
 const files=await listPackageFiles(root),parts=[],directory=[];let offset=0;
 for(const relative of files){
  const source=await fs.readFile(path.join(root,relative)),packed=zlib.deflateRawSync(source,{level:9}),name=Buffer.from(folderName+'/'+relative,'utf8'),crc=crc32(source);
  const local=Buffer.alloc(30);local.writeUInt32LE(0x04034b50);local.writeUInt16LE(20,4);local.writeUInt16LE(0x800,6);local.writeUInt16LE(8,8);local.writeUInt16LE(0x5D4D,12);local.writeUInt32LE(crc,14);local.writeUInt32LE(packed.length,18);local.writeUInt32LE(source.length,22);local.writeUInt16LE(name.length,26);
  const central=Buffer.alloc(46);central.writeUInt32LE(0x02014b50);central.writeUInt16LE(20,4);central.writeUInt16LE(20,6);central.writeUInt16LE(0x800,8);central.writeUInt16LE(8,10);central.writeUInt16LE(0x5D4D,14);central.writeUInt32LE(crc,16);central.writeUInt32LE(packed.length,20);central.writeUInt32LE(source.length,24);central.writeUInt16LE(name.length,28);central.writeUInt32LE(offset,42);
  parts.push(local,name,packed);directory.push(central,name);offset+=local.length+name.length+packed.length;
 }
 const index=Buffer.concat(directory),end=Buffer.alloc(22);end.writeUInt32LE(0x06054b50);end.writeUInt16LE(files.length,8);end.writeUInt16LE(files.length,10);end.writeUInt32LE(index.length,12);end.writeUInt32LE(offset,16);
 const zip=Buffer.concat([...parts,index,end]);await fs.writeFile(outputPath,zip);
 return {path:outputPath,files:files.length,bytes:zip.length,sha256:crypto.createHash('sha256').update(zip).digest('hex')};
}
export async function verifyZip(outputPath,root){
 const zip=await fs.readFile(outputPath);let offset=0;const entries=[];
 while(zip.readUInt32LE(offset)===0x04034b50){
  const flags=zip.readUInt16LE(offset+6),method=zip.readUInt16LE(offset+8),crc=zip.readUInt32LE(offset+14),packedLength=zip.readUInt32LE(offset+18),size=zip.readUInt32LE(offset+22),nameLength=zip.readUInt16LE(offset+26),extra=zip.readUInt16LE(offset+28),name=zip.toString('utf8',offset+30,offset+30+nameLength),start=offset+30+nameLength+extra;
  if(flags!==0x800||method!==8)throw Error('Bad ZIP entry flags');
  const contents=zlib.inflateRawSync(zip.subarray(start,start+packedLength));
  const relative=name.slice(name.indexOf('/')+1);if(relative.includes('..')||path.isAbsolute(relative))throw Error('Unsafe ZIP path');
  const shipped=await fs.readFile(path.join(root,relative));
  if(contents.length!==size||crc32(contents)!==crc||!contents.equals(shipped))throw Error('ZIP data mismatch '+name);
  entries.push(name);offset=start+packedLength;
 }
 if(zip.readUInt32LE(offset)!==0x02014b50)throw Error('Missing ZIP central directory');
 const end=zip.length-22;if(zip.readUInt32LE(end)!==0x06054b50||zip.readUInt16LE(end+10)!==entries.length||zip.readUInt32LE(end+16)!==offset)throw Error('Invalid ZIP index');
 return {passed:entries.length,entries,bytes:zip.length};
}
