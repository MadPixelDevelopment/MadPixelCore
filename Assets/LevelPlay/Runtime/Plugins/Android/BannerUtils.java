package com.ironsource.unity.androidbridge;

import static com.ironsource.unity.androidbridge.AndroidBridgeConstants.*;
import android.app.Activity;
import android.util.DisplayMetrics;
import android.view.Display;
import android.view.WindowManager;
import com.unity3d.mediation.LevelPlayAdSize;
import com.unity3d.player.UnityPlayer;

public class BannerUtils {
   public static LevelPlayAdSize getAdaptiveAdSize(int customWidth) {
      return LevelPlayAdSize.createAdaptiveAdSize(UnityPlayer.currentActivity, customWidth);
   }
}
