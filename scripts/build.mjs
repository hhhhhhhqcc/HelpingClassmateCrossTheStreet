import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {buildGame} from '../src/native/build-game.mjs';
import {generateMotionAudio} from '../src/native/generate-motion-audio.mjs';
import {createZip,verifyZip,listPackageFiles} from '../src/native/package-visual-release.mjs';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
export async function buildRelease(output=path.join(root,'dist','同学别刷了_街头行动版')) {
 await fs.mkdir(output,{recursive:true});
 for(const name of await listPackageFiles(path.join(root,'game'))) {
  if(name.endsWith('.exe')) continue;
  const dest=path.join(output,name);
  await fs.mkdir(path.dirname(dest),{recursive:true});
  await fs.copyFile(path.join(root,'game',name),dest);
 }
 const bundle=await buildGame(path.join(output,'同学别刷了.exe'));
 await generateMotionAudio(output);
 const zip=await createZip(output,output+'.zip');
 const check=await verifyZip(zip.path,output);
 return {executable:bundle.output.report,validCIL:bundle.validation.valid,zip,verifiedFiles:check.passed};
}
if(typeof process!=='undefined'&&process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url)) {
 const output=process.argv[2]?path.resolve(process.argv[2]):undefined;
 console.log(JSON.stringify(await buildRelease(output),null,2));
}
