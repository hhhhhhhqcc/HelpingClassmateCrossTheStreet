import fs from 'node:fs/promises';
import path from 'node:path';

// Original procedural UI / action sounds, PCM16 mono, no network or external assets.
export const MOTION_AUDIO_NAMES = ['牵手','口哨','冲刺','碰撞','获得与通关','道具'];
const SAMPLE_RATE = 22050;
const TAU = Math.PI * 2;
const clamp = (value, lo, hi) => Math.max(lo, Math.min(hi, value));
function randomStream(seed) {
 let state = seed >>> 0;
 return () => { state = (Math.imul(state, 1664525) + 1013904223) >>> 0; return state / 2147483648 - 1; };
}
function voice(t, start, duration, frequency, amplitude=1) {
 const u=t-start;
 if (u<0 || u>=duration) return 0;
 const envelope=Math.min(1,u/0.005)*Math.pow(1-u/duration,2.2);
 return amplitude*envelope*(Math.sin(TAU*frequency*u)+0.15*Math.sin(TAU*frequency*2*u));
}
function makeWav(id) {
 const durations=[0.25,0.16,0.14,0.24,0.43,0.31];
 const duration=durations[id], count=Math.ceil(duration*SAMPLE_RATE);
 const noise=randomStream(0x71c94e3d+id*3571), samples=new Float64Array(count);
 let previousNoise=0, peak=0;
 for(let i=0;i<count;i++) {
  const t=i/SAMPLE_RATE, white=noise();let s=0;
  if(id===0) s=voice(t,0,0.16,523.25,0.74)+voice(t,0.075,0.175,659.25,0.6);
  if(id===1) {
   const u=t/duration, envelope=Math.min(1,t/0.008)*Math.pow(1-u,1.6);
   // Integrating a linear sweep keeps the whistle pitch continuous.
   s=envelope*(Math.sin(TAU*(950*t+0.5*6300*t*t))+0.06*Math.sin(TAU*2100*t));
  }
  if(id===2) {
   const envelope=Math.min(1,t/0.003)*Math.pow(1-t/duration,2.4);
   s=envelope*((white-previousNoise)*0.58+Math.sin(TAU*(980*t-2000*t*t))*0.13);
  }
  if(id===3) {
   const envelope=Math.min(1,t/0.004)*Math.exp(-t*21)*Math.min(1,(duration-t)/0.018);
   s=envelope*(Math.sin(TAU*(118*t-130*t*t))*0.95+white*0.08);
  }
  if(id===4) s=voice(t,0,0.22,659.25,0.65)+voice(t,0.09,0.23,830.61,0.58)+voice(t,0.18,0.25,987.77,0.5);
  if(id===5) s=voice(t,0,0.31,587.33,0.75)+voice(t,0.022,0.265,880,0.25)+voice(t,0.04,0.23,1174.66,0.16);
  previousNoise=white;
  // Extra final taper leaves every file at digital silence and avoids a release click.
  s*=Math.min(1,(duration-t)/0.009);
  samples[i]=s;peak=Math.max(peak,Math.abs(s));
 }
 const targetPeak=[0.19,0.13,0.12,0.17,0.2,0.17][id], gain=targetPeak/Math.max(peak,0.0001);
 const dataBytes=count*2,wav=Buffer.alloc(44+dataBytes);
 wav.write('RIFF',0,'ascii');wav.writeUInt32LE(36+dataBytes,4);wav.write('WAVE',8,'ascii');
 wav.write('fmt ',12,'ascii');wav.writeUInt32LE(16,16);wav.writeUInt16LE(1,20);wav.writeUInt16LE(1,22);
 wav.writeUInt32LE(SAMPLE_RATE,24);wav.writeUInt32LE(SAMPLE_RATE*2,28);wav.writeUInt16LE(2,32);wav.writeUInt16LE(16,34);
 wav.write('data',36,'ascii');wav.writeUInt32LE(dataBytes,40);
 let sumSquares=0;
 for(let i=0;i<count;i++){const n=Math.round(clamp(samples[i]*gain,-1,1)*32767);wav.writeInt16LE(n,44+i*2);sumSquares+=(n/32767)**2;}
 return {wav,report:{id,name:MOTION_AUDIO_NAMES[id],durationSeconds:count/SAMPLE_RATE,sampleRate:SAMPLE_RATE,channels:1,bits:16,peak:targetPeak,rms:Math.sqrt(sumSquares/count),bytes:wav.length}};
}
export async function generateMotionAudio(dir) {
 const audioDir=path.join(dir,'assets','audio');await fs.mkdir(audioDir,{recursive:true});
 const files=[];
 for(let id=0;id<6;id++){
  const {wav,report}=makeWav(id),file=path.join(audioDir,'fx-'+id+'.wav');
  await fs.writeFile(file,wav);files.push({...report,file});
 }
 return {audioDir,files,totalBytes:files.reduce((sum,file)=>sum+file.bytes,0),original:true,format:'PCM16 mono WAV'};
}
