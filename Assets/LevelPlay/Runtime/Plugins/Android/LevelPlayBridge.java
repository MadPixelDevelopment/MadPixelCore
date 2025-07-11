package com.ironsource.unity.androidbridge;

import static com.ironsource.unity.androidbridge.AndroidBridgeUtilities.postBackgroundTask;

import com.ironsource.mediationsdk.config.ConfigFile;
import com.unity3d.mediation.LevelPlay;
import com.unity3d.mediation.LevelPlayConfiguration;
import com.unity3d.mediation.LevelPlayInitError;
import com.unity3d.mediation.LevelPlayInitListener;
import com.unity3d.mediation.LevelPlayInitRequest;
import com.unity3d.mediation.LevelPlayInitRequest.Builder;
import com.unity3d.mediation.impression.LevelPlayImpressionData;
import com.unity3d.mediation.impression.LevelPlayImpressionDataListener;
import com.unity3d.player.UnityPlayer;
import java.util.ArrayList;
import java.util.List;
import org.json.JSONObject;
import java.util.Iterator;
import com.unity3d.mediation.segment.LevelPlaySegment;

public class LevelPlayBridge implements LevelPlayInitListener, LevelPlayImpressionDataListener {
    private IUnityLevelPlayInitListener mUnityLevelPlayInitListener;
    private UnityImpressionDataListener mUnityImpressionDataListener;

    private static final LevelPlayBridge mInstance = new LevelPlayBridge();
    private LevelPlayBridge() {

    }

    public static synchronized LevelPlayBridge getInstance() {
        return mInstance;
    }
    public void initialize(String appKey, String userId, String[] adFormats, IUnityLevelPlayInitListener listener){
        List<LevelPlay.AdFormat> adFormatList = getAdFormatList(adFormats);

        Builder requestBuilder = new LevelPlayInitRequest.Builder(appKey);
        if (userId != null && userId != "") {
            requestBuilder.withUserId(userId);
        }

        if (adFormatList != null) {
            requestBuilder.withLegacyAdFormats(adFormatList);
        }
        LevelPlayInitRequest initRequest = requestBuilder.build();

        mUnityLevelPlayInitListener = listener;
        LevelPlay.init(UnityPlayer.currentActivity, initRequest, this);
    }

    public void setPluginData(String pluginType, String pluginVersion, String pluginFrameworkVersion) {
        ConfigFile.getConfigFile().setPluginData(pluginType, pluginVersion, pluginFrameworkVersion);
    }

    public boolean setDynamicUserId(String dynamicUserId) {
        return LevelPlay.setDynamicUserId(dynamicUserId);
    }

    public void setUnityImpressionDataListener(UnityImpressionDataListener listener) {
        this.mUnityImpressionDataListener = listener;
    }

    public void validateIntegration() {
        LevelPlay.validateIntegration(UnityPlayer.currentActivity);
    }

    public void launchTestSuite() {
        LevelPlay.launchTestSuite(UnityPlayer.currentActivity);
    }

    public void setNetworkData(String networkKey, String networkData) {
        try {
            JSONObject networkDataJson = new JSONObject(networkData);
            LevelPlay.setNetworkData(networkKey, networkDataJson);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void setAdaptersDebug(boolean enabled) { LevelPlay.setAdaptersDebug(enabled); }

    public void setMetaData(String key, String value) {
        LevelPlay.setMetaData(key, value);
    }

    public void setMetaData(String key, String[] values) {
        List <String> valuesList = new ArrayList<>();
        for (String value: values) {
            valuesList.add(value);
        }
        LevelPlay.setMetaData(key, valuesList);
    }

    public void setConsent(boolean consent) {
        LevelPlay.setConsent(consent);
    }

    public void setSegment(String segmentJson) {
        try {
            JSONObject json = new JSONObject(segmentJson);
            LevelPlaySegment ironSegment = new LevelPlaySegment();

            Iterator<String> iter = json.keys();
            while (iter.hasNext()) {
                String key = iter.next();
                if (key.equals(AndroidBridgeConstants.SEGMENT_LEVEL)) {
                    ironSegment.setLevel(json.optInt(key));
                } else if (key.equals(AndroidBridgeConstants.SEGMENT_PAYING)) {
                    ironSegment.setPaying(json.optInt(key) != 0);
                } else if (key.equals(AndroidBridgeConstants.SEGMENT_CREATION_DATE)) {
                    ironSegment.setUserCreationDate(json.optLong(key));
                } else if (key.equals(AndroidBridgeConstants.SEGMENT_NAME)) {
                    ironSegment.setSegmentName(json.optString(key));
                } else if (key.equals(AndroidBridgeConstants.SEGMENT_IAPT))
                    ironSegment.setIapTotal(json.optDouble(key));
                else // customs
                {
                    ironSegment.setCustom(key, json.optString(key));
                }
            }
            LevelPlay.setSegment(ironSegment);

        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    @Override
    public void onInitFailed(LevelPlayInitError initError) {
        if (mUnityLevelPlayInitListener != null) {
            postBackgroundTask(new Runnable() {
                @Override
                public void run() {
                    mUnityLevelPlayInitListener.onInitFailed(LevelPlayUtils.initErrorToString(initError));
                }
            });
        }
    }

    @Override
    public void onInitSuccess(LevelPlayConfiguration configuration) {
            LevelPlay.addImpressionDataListener(this);
            postBackgroundTask(new Runnable() {
                    @Override
                    public void run() {
                        if (mUnityLevelPlayInitListener != null) {
                            mUnityLevelPlayInitListener.onInitSuccess(LevelPlayUtils.configurationToString(configuration));
                        }
                    }
                });
            }


    @Override
    public void onImpressionSuccess(final LevelPlayImpressionData impressionData) {
        postBackgroundTask(new Runnable() {
            @Override
            public void run() {
                if (mUnityImpressionDataListener != null) {
                    mUnityImpressionDataListener.onImpressionSuccess(AndroidBridgeUtilities.getImpressionDataString(impressionData));
                }
            }
        });
    }

    private List<LevelPlay.AdFormat> getAdFormatList(String[] adFormats) {
        if(adFormats == null)
            return null;
        List<LevelPlay.AdFormat> adFormatList = new ArrayList<>();
        for (String adFormat : adFormats) {
            adFormatList.add(LevelPlay.AdFormat.valueOf(adFormat.toUpperCase()));
        }
        return adFormatList;
    }


}
