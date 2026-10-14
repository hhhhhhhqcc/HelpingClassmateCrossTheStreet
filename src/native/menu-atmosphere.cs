// Subtle menu atmosphere layered over Art 41. No input or simulation state is changed.
public static partial class Game
{
 static void MenuAtmosphereGlow(float x,float y,float w,float h,int color,float alpha)
 {
  // A softly tapered pool of light; the small layers avoid a hard rectangular patch.
  for(int row=0;row<7;row=row+1)
  {
   float shape=Sin((row+.5)*3.14159265/7);
   float width=w*shape;
   SoftRect(x-width/2,y-h/2+row*h/7,width,h/7+.5,color,alpha*shape);
  }
 }
 static void MenuAtmospherePoint(float x,float y,int color,float alpha)
 {
  SoftRect(x-5,y-5,10,10,color,alpha*.11);
  SoftRect(x-2,y-2,4,4,color,alpha*.28);
  SoftRect(x-.7,y-.7,1.4,1.4,color,alpha);
 }
 static void MenuAtmosphereTraffic(float start,float y,float length,float phase,int color,int direction)
 {
  float t=phase%1;
  float x=start+t*length;
  if(direction<0)x=start+(1-t)*length;
  float light=Sin(t*3.14159265)*.44;
  if(light<0)light=0;
  MenuAtmosphereGlow(x,y,24,7,color,light*.1);
  SoftRect(x-3,y-1.3,2.6,2,color,light);
  SoftRect(x+3,y-1.3,2.6,2,color,light);
  SoftRect(x-direction*14,y+1,14,1.3,color,light*.18);
  SoftRect(x-4,y+8,9,1.4,color,light*.08);
 }
 static void DrawMenuAtmosphere()
 {
  float time=MotionClock;
  float drift=0;
  if(HoverX>=0 && HoverX<=1250)drift=(HoverX-625)*.004;
  // The title and vertical choices occupy the untouched left side.
  // Traffic stays in the gaps beside the two illustrated characters.
  MenuAtmosphereTraffic(656,479,123,time*.044+.09,0xF4D5A2,1);
  MenuAtmosphereTraffic(1081,492,152,time*.038+.51,0xEBAC9B,-1);
  MenuAtmosphereTraffic(1100,483,131,time*.031+.17,0xBADEE4,1);

  // Warm dust and faint rain under the right street light. Never cross either face.
  float lamp=.76+.12*Sin(time*.83);
  for(int dust=0;dust<11;dust=dust+1)
  {
   float fall=(time*(.025+dust%3*.007)+dust*.117)%1;
   float x=1124+dust%4*25+Sin(time*.45+dust*1.73)*5;
   float y=259+fall*375;
   float alpha=(1-fall)*.27*lamp;
   MenuAtmospherePoint(x,y,0xE2C7A7,alpha);
  }
  for(int drop=0;drop<28;drop=drop+1)
  {
   float x=1096+(drop*47+time*8)%132;
   float y=247+(drop*97+time*(98+drop%4*9))%465;
   if(y>260 && y<698)Line(x,y,x-1.7,y+6.5,0x414953);
  }
  for(int dust=0;dust<5;dust=dust+1)
  {
   float age=(time*.032+dust*.189)%1;
   float x=718+dust*11+Sin(time*.4+dust)*3;
   float y=329+age*160;
   MenuAtmospherePoint(x,y,0x90B3B6,(1-age)*.15);
  }

  // Screen glow leaks onto the wet street; the narrow reflections drift a few pixels.
  float phone=.7+.1*Sin(time*1.3)+.04*Sin(time*3.4);
  MenuAtmosphereGlow(1065+drift,748,122,13,0x8DD5E4,.046*phone);
  MenuAtmosphereGlow(1090+drift,776,152,10,0x73BAC9,.026*phone);
  for(int shimmer=0;shimmer<7;shimmer=shimmer+1)
  {
   float x=1040+shimmer*11+Sin(time*.71+shimmer)*2;
   float y=718+shimmer*14;
   SoftRect(x,y,31-shimmer*2,1.1,0x8ED4DE,(.09-shimmer*.009)*phone);
  }
  // Tiny notifications float next to the phone character rather than over its face.
  for(int light=0;light<3;light=light+1)
  {
   float rise=(time*.14+light*.333)%1;
   float x=1076+light*14+Sin(time*.63+light)*3;
   float y=611-rise*61;
   float alpha=Sin(rise*3.14159265)*.42;
   if(alpha<0)alpha=0;
   MenuAtmospherePoint(x,y,0x88D6E7,alpha);
  }

  // Long, barely visible low fog is confined beneath the character silhouettes.
  MenuAtmosphereGlow(955+drift+Sin(time*.17)*9,824,570,33,0x8EA9B5,.012);
  MenuAtmosphereGlow(1046+Cos(time*.13)*12,793,329,23,0x90B7BF,.010);
 }
}
