import fs from 'node:fs/promises';
import path from 'node:path';
import {createRequire} from 'node:module';
const rect=(x,y,w,h,fill,extra='')=>'<rect x="'+x+'" y="'+y+'" width="'+w+'" height="'+h+'" fill="'+fill+'" '+extra+'/>';
const curve=(d,fill,extra='')=>'<path d="'+d+'" fill="'+fill+'" '+extra+'/>';
const poly=(p,fill,extra='')=>'<polygon points="'+p+'" fill="'+fill+'" '+extra+'/>';
const circle=(x,y,r,fill,extra='')=>'<circle cx="'+x+'" cy="'+y+'" r="'+r+'" fill="'+fill+'" '+extra+'/>';
export function menuSceneSvg(){
 const p=[];
 p.push('<svg xmlns="http://www.w3.org/2000/svg" width="2500" height="1800" viewBox="0 0 1250 900"><defs>'+ 
 '<linearGradient id="sky" x2=".7" y2="1"><stop stop-color="#080E1B"/><stop offset=".5" stop-color="#162238"/><stop offset="1" stop-color="#465066"/></linearGradient>'+ 
 '<linearGradient id="glass" x2="1" y2="1"><stop stop-color="#203249"/><stop offset="1" stop-color="#101B2B"/></linearGradient>'+ 
 '<linearGradient id="street" x2=".3" y2="1"><stop stop-color="#33485A"/><stop offset=".5" stop-color="#111E30"/><stop offset="1" stop-color="#070E1B"/></linearGradient>'+ 
 '<linearGradient id="sidewalk" x2="0" y2="1"><stop stop-color="#263A48"/><stop offset="1" stop-color="#101B2C"/></linearGradient>'+ 
 '<linearGradient id="jacket" x2="1" y2=".8"><stop stop-color="#E9B370"/><stop offset=".55" stop-color="#B47545"/><stop offset="1" stop-color="#614737"/></linearGradient>'+ 
 '<linearGradient id="teal" x2="1" y2="1"><stop stop-color="#73AAA4"/><stop offset=".45" stop-color="#365D69"/><stop offset="1" stop-color="#203A4A"/></linearGradient>'+ 
 '<linearGradient id="skin" x2="1" y2="1"><stop stop-color="#ECC5A8"/><stop offset="1" stop-color="#9B6E61"/></linearGradient>'+ 
 '<linearGradient id="pants" x2="1" y2="1"><stop stop-color="#28354B"/><stop offset="1" stop-color="#111B2C"/></linearGradient>'+ 
 '<linearGradient id="left" x2="1" y2="0"><stop stop-color="#070E1B"/><stop offset=".35" stop-color="#080F1D" stop-opacity=".97"/><stop offset=".6" stop-color="#0A1220" stop-opacity=".66"/><stop offset="1" stop-color="#0A1220" stop-opacity="0"/></linearGradient>'+ 
 '<linearGradient id="bottom" x2="0" y2="1"><stop stop-color="#060D18" stop-opacity="0"/><stop offset="1" stop-color="#060D18" stop-opacity=".85"/></linearGradient>'+ 
 '<radialGradient id="lamp"><stop stop-color="#FFE6B7" stop-opacity=".65"/><stop offset=".18" stop-color="#E7BB7B" stop-opacity=".2"/><stop offset="1" stop-color="#E5B97B" stop-opacity="0"/></radialGradient>'+ 
 '<radialGradient id="mist"><stop stop-color="#729999" stop-opacity=".28"/><stop offset="1" stop-color="#729999" stop-opacity="0"/></radialGradient>'+ 
 '<radialGradient id="phone"><stop stop-color="#90EFDF" stop-opacity=".42"/><stop offset="1" stop-color="#69DDC8" stop-opacity="0"/></radialGradient>'+ 
 '<filter id="blur"><feGaussianBlur stdDeviation="13"/></filter><filter id="soft"><feGaussianBlur stdDeviation="3"/></filter></defs>');
 p.push(rect(0,0,1250,900,'url(#sky)'));
 p.push(circle(1030,350,370,'url(#mist)'),circle(796,422,190,'url(#mist)'));
 const buildings=[[482,257,98,208],[579,214,75,261],[653,313,64,183],[713,143,91,345],[802,197,84,294],[881,90,131,409],[1010,210,77,311],[1089,164,114,399],[1202,241,88,330]];
 for(let i=0;i<buildings.length;i++){const[x,y,w,h]=buildings[i];p.push(rect(x,y,w,h,'url(#glass)'),rect(x+w-9,y,9,h,'#0D1828'),rect(x,y,w,3,'#314359'));
  for(let row=0;row<Math.floor(h/29)-1;row++)for(let col=0;col<Math.floor(w/18)-1;col++){let n=(i*13+row*7+col*17)%19;let lit=n<5;let color=lit?(n<2?'#D3AD81':'#608D9B'):'#284153';p.push(rect(x+11+col*18,y+19+row*29,7,11,color,'opacity="'+(lit?.60:.34)+'"'));}
  for(let j=1;j<5;j++)p.push(rect(x+w*j/5,y,1,h,'#405366','opacity=".25"'));
 }
 p.push(curve('M650 447 Q735 380 827 411 L835 493 L651 504Z','#192F3A'));
 for(let i=0;i<13;i++){let x=650+i*17;let y=435+Math.sin(i*3.17)*17;p.push(circle(x,y,24+(i%3)*7,'#172A35'));}
 p.push(poly('526,483 1250,510 1250,900 398,900','url(#street)'));
 p.push(poly('481,483 718,517 1250,833 1250,900 495,900','url(#sidewalk)'));
 // Wet crosswalk lies on the far lane, with perspective widths.
 for(let i=0;i<7;i++){let y=531+i*38;let x=874-i*32;let w=75+i*20;let hh=12+i*4;p.push(poly(x+','+y+' '+(x+w)+','+(y+15)+' '+(x+w+hh*.4)+','+(y+hh+15)+' '+(x-8)+','+(y+hh),'#BAC4BF','opacity="'+(.15+i*.028)+'"'));}
 p.push(curve('M511 581 L1250 824','none','stroke="#71888D" stroke-width="7" opacity=".5"'));
 p.push(curve('M511 591 L1250 835','none','stroke="#060E19" stroke-width="11" opacity=".8"'));
 for(let i=0;i<8;i++){let x=602+i*82;p.push(curve('M'+x+' 612 L'+(x-60)+' 900','none','stroke="#091727" stroke-width="2" opacity=".55"'));}
 for(let i=0;i<11;i++){let y=627+i*26;p.push(curve('M570 '+y+' L1250 '+(y+148),'none','stroke="#142332" stroke-width="1.5"'));}
 p.push(poly('947,515 985,515 1103,820 1066,820','#D7A86E','opacity=".045"'),poly('732,491 746,491 633,826 579,826','#91DCCC','opacity=".045"'));
 for(let i=0;i<18;i++){let y=536+i*18;p.push(rect(1071-i*2,y,11+(i%5)*10,2,'#E2B57B','opacity="'+(.07+(i%3)*.025)+'"'));}
 // Street lamp, cable and a distant traffic light frame the scene.
 p.push(curve('M1172 209 L1172 774','none','stroke="#0A1726" stroke-width="13"'),curve('M1170 217 Q1145 174 1071 202','none','stroke="#102033" stroke-width="12"'));
 p.push(poly('1049,199 1095,199 1108,213 1039,213','#263A49'),rect(1047,212,51,5,'#FFE0AA'),circle(1074,214,177,'url(#lamp)'));
 p.push(poly('1057,222 1089,222 1206,653 963,629','#D6B17F','opacity=".035"'));
 p.push(curve('M793 375 L793 540','none','stroke="#0C1725" stroke-width="7"'),rect(780,371,27,79,'#101B27','rx="8"'),circle(793,390,7,'#CA8068','opacity=".55"'),circle(793,432,7,'#63CCAE','opacity=".75"'));
 p.push(curve('M701 229 Q938 301 1250 248','none','stroke="#131D2B" stroke-width="2"'));
 p.push('<ellipse cx="878" cy="721" rx="71" ry="17" fill="#040A13" opacity=".8"/><ellipse cx="1018" cy="742" rx="81" ry="17" fill="#040A13" opacity=".8"/>');
 // Escort: turned toward the road, looking back at the phone user.
 p.push(curve('M857 593 L879 594 L877 685 L850 718 L831 715 L850 676Z','url(#pants)'));
 p.push(curve('M879 590 L910 590 L918 681 L933 720 L909 726 L895 688 L873 640Z','url(#pants)'));
 p.push(curve('M833 706 Q846 709 854 718 L850 727 L820 727 Q816 718 833 706Z','#CFD0C6'),curve('M908 714 L927 713 Q942 718 945 726 L938 733 L906 732Z','#D5CFBA'));
 p.push(curve('M849 487 Q872 473 902 483 Q926 498 924 550 L914 613 Q887 625 849 610 L834 531Z','url(#jacket)'));
 p.push(curve('M864 479 L891 481 L895 512 L866 507Z','#D5CCAE'),curve('M846 499 L856 491 L869 610 L856 611Z','#E0AE6C'),curve('M895 493 L896 611','none','stroke="#714B35" stroke-width="2"'));
 p.push(curve('M856 492 Q831 495 822 517 L826 575 Q832 587 849 582 L853 541Z','#294351'),curve('M854 495 Q837 512 839 568','none','stroke="#536C75" stroke-width="6"'),rect(825,531,19,34,'#1A3441','rx="6"'));
 p.push(curve('M898 495 Q915 489 928 505 L950 550 L985 562 L979 580 L935 568 Q917 558 907 533Z','url(#jacket)'),curve('M976 559 Q990 558 998 569 L994 581 L980 579Z','url(#skin)'));
 p.push(curve('M850 511 Q830 519 833 548 L846 587 L858 581 L851 546 L867 527Z','#B47F4F'),curve('M847 580 L857 579 L864 598 L855 604 L848 594Z','url(#skin)'));
 p.push(curve('M868 466 L869 486 Q877 494 890 484 L890 463Z','url(#skin)'));
 p.push(curve('M852 423 Q859 400 880 404 Q902 407 905 433 L899 458 Q887 478 870 467 L857 450Z','url(#skin)'));
 p.push(curve('M850 435 Q843 413 860 399 Q879 386 899 404 Q911 416 906 439 L897 434 L892 416 Q875 426 856 425 L858 444Z','#1B2030'),curve('M855 408 Q875 389 897 407','none','stroke="#44505C" stroke-width="3"'));
 p.push(curve('M899 438 L906 445 L897 448Z','#C58D71'),curve('M887 455 L897 452','none','stroke="#795146" stroke-width="1.6"'),circle(893,433,1.8,'#303447'));
 // Phone user: weight shifted, head inclined over a bright screen.
 p.push(curve('M989 623 L1017 623 L1000 707 L982 749 L957 746 L977 701Z','url(#pants)'),curve('M1012 619 L1046 618 L1054 706 L1039 750 L1016 747 L1027 704 L1009 662Z','url(#pants)'));
 p.push(curve('M958 737 L981 740 L984 752 L975 758 L946 754 Q941 746 958 737Z','#C4CEC5'),curve('M1017 739 L1039 741 L1046 753 L1040 759 L1006 756 Q1002 746 1017 739Z','#CAD2CA'));
 p.push(curve('M986 505 Q1011 494 1034 510 L1060 563 L1049 637 Q1016 653 984 631 L970 568Z','url(#teal)'));
 p.push(curve('M998 502 L1018 509 L1024 538 L1000 533Z','#C1CECB'),curve('M1020 534 L1029 634','none','stroke="#8DAAA8" stroke-width="2"'));
 p.push(curve('M984 514 Q963 516 961 544 L964 594 Q971 607 984 602 L995 564Z','#253E4D'),curve('M986 517 Q972 542 981 587','none','stroke="#8EA49C" stroke-width="6"'),rect(963,550,18,27,'#182F41','rx="5"'));
 p.push(curve('M1040 520 Q1059 525 1059 549 L1068 565 L1053 575 L1035 552 L1026 530Z','#5C8B8C'));
 p.push(curve('M986 532 Q973 549 984 571 L1020 589 L1051 575 L1048 559 L1020 572 L1005 546Z','url(#teal)'),curve('M1044 559 Q1058 551 1067 561 L1065 576 L1052 578 L1045 571Z','url(#skin)'));
 p.push(curve('M1007 482 L1004 507 Q1013 518 1027 510 L1028 488Z','url(#skin)'));
 p.push(curve('M988 438 Q998 419 1021 427 Q1046 436 1041 459 L1046 475 L1032 482 Q1025 501 1008 493 L991 474Z','url(#skin)'));
 p.push(curve('M989 460 Q977 445 986 430 Q1000 410 1021 419 Q1040 422 1049 443 L1041 454 L1030 440 Q1019 447 1000 448 L999 472 L990 468Z','#151D2B'),curve('M989 432 Q1008 415 1033 434','none','stroke="#374A55" stroke-width="3"'));
 p.push(curve('M1030 475 L1040 472','none','stroke="#815A50" stroke-width="1.5"'),curve('M1024 456 L1033 459','none','stroke="#303347" stroke-width="2"'));
 p.push('<g transform="rotate(12 1068 558)">',rect(1058,537,24,43,'#101B2A','rx="4"'),rect(1061,541,18,31,'#93DFCD','rx="2"'),rect(1063,547,13,2,'#DFF8E9'),rect(1063,552,11,2,'#5FAB9C'),rect(1063,557,13,7,'#54A496'),circle(1070,576,1.7,'#7C9291'),'</g>',circle(1070,559,67,'url(#phone)'));
 p.push(curve('M996 622 L1043 637','none','stroke="#63858A" stroke-width="2" opacity=".55"'),curve('M854 594 L872 593','none','stroke="#F3CB8D" stroke-width="2" opacity=".4"'));
 p.push(circle(1217,610,148,'url(#mist)'));
 // Subtle surface grain remains deterministic and is drawn once into the raster asset.
 for(let i=0;i<640;i++){let x=(i*197.63)%1250,y=(i*83.47)%900;p.push(circle(x,y,.55+(i%4)*.2,'#C4D4D8','opacity=".023"'));}
 p.push(rect(0,0,1250,900,'url(#left)'),rect(0,700,1250,200,'url(#bottom)'));
 p.push('</svg>');return p.join('');
}
export async function generateMenuArt(folder,options={}){
 const sharp=options.sharp||createRequire(import.meta.url)('sharp');const assetDir=path.join(folder,'assets');await fs.mkdir(assetDir,{recursive:true});
 const svg=menuSceneSvg();const png=await sharp(Buffer.from(svg)).resize(1250,900).png().toBuffer();await fs.writeFile(path.join(assetDir,'art-41.png'),png);
 const manifestFile=path.join(assetDir,'manifest.json');let manifest={assets:[]};try{manifest=JSON.parse(await fs.readFile(manifestFile,'utf8'));}catch(e){if(e.code!=='ENOENT')throw e;}
 manifest.assets=manifest.assets.filter(a=>a.id!==41);manifest.assets.push({id:41,file:'assets/art-41.png',width:1250,height:900,bytes:png.length,role:'Native main menu street illustration'});await fs.writeFile(manifestFile,JSON.stringify(manifest,null,2));
 return {id:41,file:'assets/art-41.png',bytes:png.length,width:1250,height:900};
}
