// Formal native main menu and settings. Road mechanics stay in Motion*Core.
public static partial class Game
{
 static int MenuPage,MenuFocus,MenuSettingFocus,MenuQuitFocus,MenuFullscreen;
 static int MenuHadFocus,MenuFullscreenUsed,MenuSoundUsed,MenuConfirmBlock;
 static void MenuToggleFullscreen()
 {
  if(MenuFullscreenUsed==1)return;
  MenuFullscreenUsed=1;ToggleFullscreen();MenuFullscreen=1-MenuFullscreen;
 }
 static void MenuToggleSound()
 {
  if(MenuSoundUsed==1)return;
  MenuSoundUsed=1;MotionMuted=1-MotionMuted;
 }
 static void MenuSetFocus(int focus)
 {
  MenuFocus=focus;
  if(focus==0 || focus==1)Mode=focus;
 }
 static void MenuOpenPage(int page)
 {
  MenuPage=page;MenuConfirmBlock=1;
  if(page==1)MenuSettingFocus=0;
  if(page==3)MenuQuitFocus=0;
  MotionPhaseAge=0;UIApproach=0;ResetInputs();MotionSound(5);
 }
 static void MenuActivate()
 {
  if(MenuPage==0)
  {
   if(MenuFocus==0 || MenuFocus==1)
   {
    Mode=MenuFocus;MenuConfirmBlock=1;StartGame(true);ResetInputs();return;
   }
   if(MenuFocus==2)MenuOpenPage(1);
   else if(MenuFocus==3)MenuOpenPage(2);
   else if(MenuFocus==4)MenuOpenPage(3);
  }
  else if(MenuPage==1)
  {
   if(MenuSettingFocus==0)MenuToggleSound();
   else MenuToggleFullscreen();
  }
  else if(MenuPage==2)MenuOpenPage(0);
  else if(MenuPage==3)
  {
   if(MenuQuitFocus==1)QuitGame();
   else MenuOpenPage(0);
  }
 }
 static void MenuKeys()
 {
  if(MenuPage==0)
  {
   if(Pressed(38) || Pressed(87))MenuSetFocus((MenuFocus+4)%5);
   else if(Pressed(40) || Pressed(83))MenuSetFocus((MenuFocus+1)%5);
   if(Pressed(49) || Pressed(97))MenuSetFocus(0);
   else if(Pressed(50) || Pressed(98))MenuSetFocus(1);
   if(Pressed(13))MenuActivate();
  }
  else if(MenuPage==1)
  {
   if(Pressed(27) || Pressed(37)){MenuOpenPage(0);return;}
   if(Pressed(38) || Pressed(87) || Pressed(40) || Pressed(83))MenuSettingFocus=1-MenuSettingFocus;
   if(Pressed(13) || Pressed(39))MenuActivate();
  }
  else if(MenuPage==2)
  {
   if(Pressed(27) || Pressed(37) || Pressed(13))MenuOpenPage(0);
  }
  else if(MenuPage==3)
  {
   if(Pressed(27)){MenuOpenPage(0);return;}
   if(Pressed(37) || Pressed(65))MenuQuitFocus=0;
   else if(Pressed(39) || Pressed(68))MenuQuitFocus=1;
   if(Pressed(13))MenuActivate();
  }
 }
 static void MenuClick(float x,float y)
 {
  if(MenuPage==0)
  {
   if(x>=100 && x<=460)
   {
    for(int i=0;i<5;i++)
    {
     float top=415+60*i;
     if(y>=top && y<=top+48){MenuSetFocus(i);MenuActivate();return;}
    }
   }
   return;
  }
  if(x>=150 && x<=310 && y>=685 && y<=735){MenuOpenPage(0);return;}
  if(MenuPage==1)
  {
   if(x>=160 && x<=1040 && y>=415 && y<=475){MenuSettingFocus=0;MenuActivate();return;}
   if(x>=160 && x<=1040 && y>=490 && y<=550){MenuSettingFocus=1;MenuActivate();return;}
  }
  else if(MenuPage==3 && y>=555 && y<=607)
  {
   if(x>=410 && x<=590){MenuQuitFocus=0;MenuActivate();}
   else if(x>=620 && x<=800){MenuQuitFocus=1;MenuActivate();}
  }
 }
 static void Menu()
 {
  MotionMenuCore();MotionResetFeedback();MenuPage=0;MenuFocus=Mode;MenuSettingFocus=0;MenuQuitFocus=0;MenuConfirmBlock=1;
  MotionPhaseAge=0;UIApproach=0;ResetInputs();
 }
 static void GameInitialize()
 {
  MotionClock=0;MotionRoadClock=0;MotionPhaseAge=0;MotionLastPhase=0;MotionMuted=0;MotionNearCount=0;HoverX=-100;HoverY=-100;
  MenuPage=0;MenuFocus=0;MenuSettingFocus=0;MenuQuitFocus=0;MenuFullscreen=0;MenuHadFocus=0;MenuFullscreenUsed=0;MenuSoundUsed=0;MenuConfirmBlock=0;
  if(HasFocus())MenuHadFocus=1;
  MotionInitializeCore();
 }
 static void GameClick(float x,float y)
 {
  MenuFullscreenUsed=0;MenuSoundUsed=0;
  if(!HasFocus())return;
  if(MenuHadFocus==0){MenuHadFocus=1;ResetInputs();}
  if(Phase==0)MenuClick(x,y);
  else MotionClickCore(x,y);
  MotionFinishTick();
 }
 static void GameTick(float dt)
 {
  float step=Min(.05,Max(0,dt));
  int phaseBefore=Phase;MotionUiTick(step);MenuFullscreenUsed=0;MenuSoundUsed=0;
  // VK122 is F11. VK112 (F1) deliberately has no fullscreen behavior.
  for(int k=0;k<256;k++)
  {
   Input[k]=0;
   if((k>=65 && k<=90) || (k>=37 && k<=40) || (k>=48 && k<=51) || (k>=96 && k<=99) || k==9 || k==13 || k==16 || k==27 || k==32 || k==122)
   {
    if(Key(k))Input[k]=1;
   }
  }
  // A page change or start consumes Enter until its physical release.
  if(MenuConfirmBlock==1)
  {
   if(Input[13]==0)MenuConfirmBlock=0;
   else Input[13]=0;
  }
  if(!HasFocus())
  {
   MenuHadFocus=0;
   if(Phase==1)Phase=3;
  }
  else
  {
   // Resynchronise held keys on menu focus return before accepting new press events.
   if(MenuHadFocus==0){MenuHadFocus=1;if(Phase==0)ResetInputs();}
   if(Pressed(86))MenuToggleSound();
   if(Pressed(122))MenuToggleFullscreen();
   if(Phase==0)MenuKeys();
   else
   {
    if(Pressed(77))Menu();
    if(Mode==0 && Pressed(66) && Phase!=0 && Phase!=4)
    {
     if(Phase==9)Phase=BackPhase;
     else{BackPhase=Phase;Phase=9;CollectionPage=0;CollectionView=0;}
     ResetInputs();
    }
    if(Pressed(80) || Pressed(27))
    {
     if(Phase==9){Phase=BackPhase;ResetInputs();}
     else if(Phase==1)Phase=3;
     else if(Phase==3)Phase=1;
    }
    if(Phase==2)
    {
     if(Pressed(37))SelectedChoice=(SelectedChoice+2)%3;
     if(Pressed(39))SelectedChoice=(SelectedChoice+1)%3;
     if(Pressed(49) || Pressed(97))ChooseReward(0);
     else if(Pressed(50) || Pressed(98))ChooseReward(1);
     else if(Pressed(51) || Pressed(99))ChooseReward(2);
     else if(Pressed(13))ChooseReward(SelectedChoice);
    }
    else if(Phase==3){if(Pressed(13))Phase=1;}
    else if(Phase==4){if(Pressed(13))StartGame(false);else if(Pressed(78))StartGame(true);}
    else if(Phase>=5 && Phase<=8)RouteKeys();
    else if(Phase==9)CollectionKeys();
    if(Phase==1 && phaseBefore==1)
    {
     if(Pressed(69))Whistle();
     if(Pressed(81))P1Skill(0);if(Pressed(70))P1Skill(1);if(Pressed(82))P1Skill(2);
     if(Mode==0){if(Pressed(9))CycleGadget();if(Pressed(84))GadgetSkill();}
     if(Mode==1){if(Pressed(13))P2Skill(0);if(Pressed(74))P2Skill(1);if(Pressed(75))P2Skill(2);if(Pressed(76))P2Skill(3);}
     StepGame(step);
    }
   }
  }
  if(Phase==0)
  {
   for(int i=0;i<96;i++)
   {
    if(CarActive[i]==1){CarX[i]+=CarDirection[i]*CarSpeed[i]*step*.45;if(CarX[i]>1250)CarX[i]=-150;if(CarX[i]<-150)CarX[i]=1250;}
   }
  }
  MotionFinishTick();
  for(int p=0;p<256;p++)PreviousInput[p]=Input[p];
 }
}
