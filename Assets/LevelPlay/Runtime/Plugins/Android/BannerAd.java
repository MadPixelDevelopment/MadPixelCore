package com.ironsource.unity.androidbridge;

import android.app.Activity;
import android.content.res.Resources;
import android.graphics.Color;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.view.ViewGroup.OnHierarchyChangeListener;
import android.widget.FrameLayout;
import android.widget.FrameLayout.LayoutParams;
import com.unity3d.mediation.LevelPlayAdError;
import com.unity3d.mediation.LevelPlayAdInfo;
import com.unity3d.mediation.LevelPlayAdSize;
import com.unity3d.mediation.banner.LevelPlayBannerAdView;
import com.unity3d.mediation.banner.LevelPlayBannerAdViewListener;
import com.unity3d.player.UnityPlayer;

public class BannerAd {
    Activity mActivity;
    LevelPlayBannerAdView mBannerAdView;
    int mBannerAdViewVisibilityState = View.INVISIBLE;

    public BannerAd(
        String adUnitId,
        LevelPlayAdSize size,
        String position, float x, float y,
        String placementName,
        boolean displayOnLoad,
        boolean respectSafeArea,
        IUnityBannerAdListener bannerListener
    ) {
        this.mActivity = UnityPlayer.currentActivity;

        this.mBannerAdView = new LevelPlayBannerAdView(mActivity, adUnitId);

        if (size != null) {
            this.mBannerAdView.setAdSize(size);
        }

        if (placementName != null && placementName != "") {
            this.mBannerAdView.setPlacementName(placementName);
        }

        this.mBannerAdView.setBackgroundColor(Color.TRANSPARENT);

        if(displayOnLoad) {
            mBannerAdView.setVisibility(View.VISIBLE);
            mBannerAdViewVisibilityState = View.VISIBLE;
        } else{
            mBannerAdView.setVisibility(View.GONE);
            mBannerAdViewVisibilityState = View.GONE;
        }

        if (respectSafeArea && android.os.Build.VERSION.SDK_INT >= 28){
            mBannerAdView.setFitsSystemWindows(true);
            mBannerAdView.setSystemUiVisibility(View.SYSTEM_UI_FLAG_LAYOUT_STABLE | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN);
            mBannerAdView.setOnApplyWindowInsetsListener((v, insets) -> {
                setPosition(position, x, y, true);

                return insets;
            });
        }

        setPosition(position, x, y, respectSafeArea);

        this.mBannerAdView.setBannerListener(new LevelPlayBannerAdViewListener() {
            @Override
            public void onAdLoaded(LevelPlayAdInfo levelPlayAdInfo) {
                if(bannerListener != null)
                    bannerListener.onAdLoaded(LevelPlayUtils.adInfoToString(levelPlayAdInfo));
            }

            @Override
            public void onAdLoadFailed(LevelPlayAdError adError) {
                if(bannerListener != null)
                    bannerListener.onAdLoadFailed(LevelPlayUtils.adErrorToString(adError));
            }

            @Override
            public void onAdDisplayed(LevelPlayAdInfo levelPlayAdInfo) {
                if(bannerListener != null)
                    bannerListener.onAdDisplayed(LevelPlayUtils.adInfoToString(levelPlayAdInfo));
            }

            @Override
            public void onAdDisplayFailed(LevelPlayAdInfo levelPlayAdInfo,
                                          LevelPlayAdError adError) {
                if(bannerListener != null)
                    bannerListener.onAdDisplayFailed(LevelPlayUtils.adInfoToString(levelPlayAdInfo), LevelPlayUtils.adErrorToString(adError));
            }

            @Override
            public void onAdClicked(LevelPlayAdInfo levelPlayAdInfo) {
                if(bannerListener != null)
                    bannerListener.onAdClicked(LevelPlayUtils.adInfoToString(levelPlayAdInfo));
            }

            @Override
            public void onAdExpanded(LevelPlayAdInfo levelPlayAdInfo) {
                if(bannerListener != null)
                    bannerListener.onAdExpanded(LevelPlayUtils.adInfoToString(levelPlayAdInfo));
            }

            @Override
            public void onAdCollapsed(LevelPlayAdInfo levelPlayAdInfo) {
                if(bannerListener != null)
                    bannerListener.onAdCollapsed(LevelPlayUtils.adInfoToString(levelPlayAdInfo));
            }

            @Override
            public void onAdLeftApplication(LevelPlayAdInfo levelPlayAdInfo) {
                if(bannerListener != null)
                    bannerListener.onAdLeftApplication(LevelPlayUtils.adInfoToString(levelPlayAdInfo));
            }
        });
    }

    public void load() {
        this.mBannerAdView.loadAd();
    }

    public void destroy() {
        this.mBannerAdView.destroy();
    }

    public void showAd() {
        mActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                if (mBannerAdView != null) {
                    mBannerAdView.setVisibility(View.VISIBLE);
                }
                mBannerAdViewVisibilityState = View.VISIBLE;
            }
        });
    }

    public void hideAd() {
        mActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                if (mBannerAdView != null) {
                    mBannerAdView.setVisibility(View.GONE);
                }
                mBannerAdViewVisibilityState = View.GONE;
            }
        });
    }

    public void resumeAutoRefresh() {
        this.mBannerAdView.resumeAutoRefresh();
    }

    public void pauseAutoRefresh() {
        this.mBannerAdView.pauseAutoRefresh();
    }

    public String getAdId() {
        return this.mBannerAdView.getAdId();
    }

    private void setPosition(String position, float x, float y, boolean respectSafeArea) {
        mActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                if (mBannerAdView.getParent() == null) {
                    mActivity.addContentView(mBannerAdView, new LayoutParams(
                        ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));
                }

                setPositionInternal(position, x, y, respectSafeArea);

                mBannerAdView.setOnHierarchyChangeListener(new OnHierarchyChangeListener() {
                    @Override
                    public void onChildViewAdded(View parent, View child) {
                        mActivity.runOnUiThread(new Runnable() {
                            @Override
                            public void run() {
                                if(mBannerAdView != null) {
                                    mBannerAdView.setVisibility(mBannerAdViewVisibilityState);
                                }
                                mBannerAdView.requestLayout();
                            }
                        });
                    }

                    @Override
                    public void onChildViewRemoved(View parent, View child) {}
                });
            }
        });
    }

    private void setPositionInternal(String position, float x, float y, boolean respectSafeArea) {
        FrameLayout.LayoutParams adLayoutParams = (FrameLayout.LayoutParams) mBannerAdView.getLayoutParams();
        if (adLayoutParams == null) return;

        adLayoutParams.leftMargin = 0;
        adLayoutParams.topMargin = 0;
        adLayoutParams.rightMargin = 0;
        adLayoutParams.bottomMargin = 0;

        int insetLeft = 0;
        int insetTop = 0;
        int insetRight = 0;
        int insetBottom = 0;

        if (respectSafeArea && android.os.Build.VERSION.SDK_INT >= 28) {
            android.view.WindowInsets insets = mActivity
                .getWindow()
                .getDecorView()
                .getRootWindowInsets();

            if (insets != null) {
                insetLeft = insets.getSystemWindowInsetLeft();
                insetTop = insets.getSystemWindowInsetTop();
                insetRight = insets.getSystemWindowInsetRight();
                insetBottom = insets.getSystemWindowInsetBottom();
            }
        }

        switch (position) {
            case AndroidBridgeConstants.BANNER_POSITION_TOPLEFT:
                adLayoutParams.gravity = Gravity.TOP | Gravity.START;
                if (respectSafeArea) {
                    adLayoutParams.leftMargin = insetLeft;
                    adLayoutParams.topMargin = insetTop;
                }
                break;
            case AndroidBridgeConstants.BANNER_POSITION_TOPCENTER:
                adLayoutParams.gravity = Gravity.TOP | Gravity.CENTER_HORIZONTAL;
                if (respectSafeArea) adLayoutParams.topMargin = insetTop;
                break;
            case AndroidBridgeConstants.BANNER_POSITION_TOPRIGHT:
                adLayoutParams.gravity = Gravity.TOP | Gravity.END;
                if (respectSafeArea) {
                    adLayoutParams.rightMargin = insetRight;
                    adLayoutParams.topMargin = insetTop;
                }
                break;
            case AndroidBridgeConstants.BANNER_POSITION_CENTERLEFT:
                adLayoutParams.gravity = Gravity.CENTER_VERTICAL | Gravity.START;
                if (respectSafeArea) adLayoutParams.leftMargin = insetLeft;
                break;
            case AndroidBridgeConstants.BANNER_POSITION_CENTER:
                adLayoutParams.gravity = Gravity.CENTER;
                break;
            case AndroidBridgeConstants.BANNER_POSITION_CENTERRIGHT:
                adLayoutParams.gravity = Gravity.CENTER_VERTICAL | Gravity.END;
                if (respectSafeArea) adLayoutParams.rightMargin = insetRight;
                break;
            case AndroidBridgeConstants.BANNER_POSITION_BOTTOMLEFT:
                adLayoutParams.gravity = Gravity.BOTTOM | Gravity.START;
                if (respectSafeArea) {
                    adLayoutParams.leftMargin = insetLeft;
                    adLayoutParams.bottomMargin = insetBottom;
                }
                break;
            case AndroidBridgeConstants.BANNER_POSITION_BOTTOMRIGHT:
                adLayoutParams.gravity = Gravity.BOTTOM | Gravity.END;
                if (respectSafeArea) {
                    adLayoutParams.rightMargin = insetRight;
                    adLayoutParams.bottomMargin = insetBottom;
                }
                break;
            case AndroidBridgeConstants.BANNER_POSITION_CUSTOM:
                adLayoutParams.gravity = Gravity.TOP | Gravity.START;
                int xPos = Math.round(x);
                int yPos = Math.round(y);

                if (respectSafeArea) {
                    xPos += insetLeft;
                    yPos += insetTop;
                }

                adLayoutParams.leftMargin = xPos;
                adLayoutParams.topMargin = yPos;
                break;
            default:
                adLayoutParams.gravity = Gravity.BOTTOM | Gravity.CENTER_HORIZONTAL;
                if (respectSafeArea) adLayoutParams.bottomMargin = insetBottom;
                break;
        }

        mBannerAdView.setLayoutParams(adLayoutParams);
    }

    private float pixelsToDp(float px) {
        return px / Resources.getSystem().getDisplayMetrics().density;
    }
}
