﻿using System;
 using DG.Tweening;
 using FruitSwipeMatch3Kit;
 using GoogleMobileAds.Api;
 using UnityEngine;

public class Admob : MonoBehaviour
{
    private string appId = "ca-app-pub-4337728929525466~6370811938";
    private string bannerId = "ca-app-pub-6115070480367583/4670041120";
    private string interstitialId = "ca-app-pub-4337728929525466/3553076909";
    private string rewardVideoId = "ca-app-pub-4337728929525466/3826756680";
//    private string interstitialId = "ca-app-pub-3940256099942544/1033173712";
//    private string rewardVideoId = "ca-app-pub-3940256099942544/5224354917";
    
    private BannerView _banner;
    private InterstitialAd _interstitial;
    private RewardBasedVideoAd _rewardBasedVideo;
    private Action _onReward;
    private Action _onRewardClose;
    private Action _onInterstitialClose;
    private int _requestCount = 0;
    private int _requestInter = 0;
    private int _gameOverCount = 0;
    private static Admob _instance;
    public static Admob Instance => _instance;
    public bool IsNoAds => PlayerPrefs.GetInt(GameplayConstants.NoAdsPrefKey) > 0;

    private void Awake()
    {
        if (_instance != null) Destroy(gameObject);
        else _instance = this;
    }

    private void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);
        _rewardBasedVideo = RewardBasedVideoAd.Instance;
        _rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
        _rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
        _rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
        _rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        _rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
//        OpenBanner();
        RequestRewardVideo();
        RequestInterstitial();
        DontDestroyOnLoad(gameObject);
    }

    // Returns an ad request with custom ad targeting.
    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
//            .AddTestDevice(AdRequest.TestDeviceSimulator)
//            .AddTestDevice("AB7CD22B866989C25120F046C012FDFE")
//            .AddKeyword("game")
//            .SetGender(Gender.Male)
//            .SetBirthday(new DateTime(1985, 1, 1))
//            .TagForChildDirectedTreatment(false)
//            .AddExtra("color_bg", "9B30FF")
            .Build();
    }

    private void OpenBanner()
    {
        _banner = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);
        _banner.LoadAd(CreateAdRequest());
        _banner.Show();
    }
    
    #region RewardVideoAds
    
    public bool IsRewardLoaded()
    {
        if (_rewardBasedVideo != null)
        {
            return _rewardBasedVideo.IsLoaded();
        }
        return false;
    }
    
    private void RequestRewardVideo()
    {
//        UnityAds.SetGDPRConsentMetaData(true);
        _requestCount = 0;
        _rewardBasedVideo.LoadAd(CreateAdRequest(), rewardVideoId);
    }
    
    public void ShowReward(Action onReward, Action onClose = null)
    {
        if (IsRewardLoaded())
        {
            _onReward = onReward;
            _onRewardClose = onClose;
            _rewardBasedVideo.Show();
        }
        else
        {
            if (_requestCount >= 3)
            {
                RequestRewardVideo();
            }
        }
    }
    
    private void HandleRewardBasedVideoLoaded(object sender, EventArgs e)
    {
        _requestCount = 0;
    }

    private void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        // Reload
        if (_requestCount < 3)
        {
            _requestCount++;
            var seg = DOTween.Sequence();
            seg.AppendInterval(3);
            seg.AppendCallback(RequestRewardVideo);
        }
    }

    private void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    {
        
    }

    private void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        RequestRewardVideo();
        if (_onRewardClose != null) _onRewardClose.Invoke();
    }

    private void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        if (_onReward != null)
        {
            _onReward.Invoke();
        }
    }

    #endregion
    
    #region InterstititalAds

    public bool IsInterstitialLoaded()
    {
        return !IsNoAds && _interstitial.IsLoaded();
    }

    public void ShowInterstitialNow(Action onClose)
    {
        if (IsNoAds)
        {
            if(_interstitial != null) _interstitial.Destroy();
            return;
        }
        if (IsInterstitialLoaded())
        {
            _onInterstitialClose = onClose;
            _interstitial.Show();
        }
        else
        {
            if(onClose != null) onClose.Invoke();
            if(_requestInter >= 3) RequestInterstitial();
        }
    }
    
    public bool ShowInterstitial(Action onClose, int maxCount = 2)
    {
        if (IsNoAds)
        {
            if(_interstitial != null) _interstitial.Destroy();
            return false;
        }
        _gameOverCount++;
        if (IsInterstitialLoaded() && _gameOverCount > maxCount)
        {
            _gameOverCount = 0;
            _onInterstitialClose = onClose;
            _interstitial.Show();
            return true;
        }

        if(_requestInter >= 3) RequestInterstitial();
        return false;
    }

    private void RequestInterstitial()
    {
        if (_interstitial != null)
        {
            _interstitial.Destroy();
        }
        
//        UnityAds.SetGDPRConsentMetaData(true);
        _interstitial = new InterstitialAd(interstitialId);
        _interstitial.OnAdFailedToLoad += OnInterstitialFailToLoad;
        _interstitial.OnAdClosed += OnInterstitialClosed;

        _interstitial.LoadAd(CreateAdRequest());
    }

    private void OnInterstitialFailToLoad(object sender, EventArgs args)
    {
        if (_requestInter < 3)
        {
            _requestInter++;
            var seg = DOTween.Sequence();
            seg.AppendInterval(3);
            seg.AppendCallback(RequestInterstitial);
        }
    }

    private void OnInterstitialClosed(object sender, EventArgs args)
    {
        _requestInter = 0;
        if(_onInterstitialClose != null) _onInterstitialClose.Invoke();
        RequestInterstitial();
    }
    
    #endregion

    public void HideBanner()
    {
        if(_banner != null) _banner.Hide();
    }
    
    public void ShowBanner()
    {
        if(_banner != null) _banner.Show();
    }
}
