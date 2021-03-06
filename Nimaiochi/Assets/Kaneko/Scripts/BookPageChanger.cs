﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KanekoUtilities;
using MyStory.StoryRepair;

[RequireComponent(typeof(BookRenderer))]
public class BookPageChanger : MonoBehaviour
{
    [System.Serializable]
    class PageTexture
    {
        public Texture2D LeftTexture;
        public Texture2D RightTexture;
    }

    [SerializeField]
    PageTexture[] pageTextures = null;

    [SerializeField]
    float swipeSpeed = 0.02f;

    [SerializeField]
    Animator bookAnim = null;

    [SerializeField]
    Animator pageAnim = null;

    [SerializeField]
    SkinnedMeshRenderer pageMeshRend = null;

    BookRenderer bookRenderer = null;

    int maxPageIndex = 3;
    int currentPageIndex = 0;
    float currentRate = 0.0f;
    bool isAnimating;
    private bool isReturn = false;
    
    void Awake()
    {
        bookRenderer = GetComponent<BookRenderer>();
    }

    void Start()
    {
        Init();

        SwipeGetter.Instance.onTouchStart.AddListener((_) =>
        {
            currentRate = 0.0f;
        });

        SwipeGetter.Instance.onSwipe.AddListener((vec) =>
        {
            if (StorySimulator.Instance.PageEndAction != null) return;
            currentRate += vec.x * swipeSpeed * Time.deltaTime * 0.01f;
            currentRate = Mathf.Clamp01(currentRate);
            UpdateMovePage(currentRate);
        });

        SwipeGetter.Instance.onTouchEnd.AddListener((_) =>
        {
            if (currentRate < 0.05f)
            {
                isReturn = true;
                StartCoroutine(ChangePageAnimation(0.0f, currentPageIndex));
            }
            else
            {
                StartCoroutine(ChangePageAnimation(1.0f, currentPageIndex + 1));
            }
        });
    }

    public void Init()
    {
        UpdateTexture(0);
    }

    IEnumerator ChangePageAnimation(float targetRate, int pageIndex)
    {
        isAnimating = true;
        var start = currentRate;
        //t = s / d;
        var duration =  Mathf.Abs(start - targetRate) / 1.0f;

        yield return KKUtilities.FloatLerp(duration, (t) =>
        {
            if (StorySimulator.Instance.PageEndAction != null) return;
            UpdateMovePage(Mathf.Lerp(start, targetRate, t));
        });

        currentPageIndex = pageIndex;
        UpdateTexture(currentPageIndex);
        UpdateMovePage(0.0f);

        isAnimating = false;
    }

    void Update()
    {
        if(pageAnim.gameObject.activeSelf) pageAnim.Play("NextPage", 0, currentRate);
        pageMeshRend.SetBlendShapeWeight(0, Easing.Yoyo(currentRate) * 100.0f);
    }

    void UpdateMovePage(float rate)
    {
        currentRate = rate; 

        if (!StorySimulator.Instance.IsStory) return;
        // 紙芝居アニメーションしてます
        if (StorySimulator.Instance.Chapter && rate > 0) StorySimulator.Instance.Chapter.transform.GetChild(0).localScale = new Vector3(1 - 1 * rate, 1 - 1 * rate, 1 - 1 * rate);
        if (StorySimulator.Instance.Chapter && rate > 0) StorySimulator.Instance.Chapter.transform.GetChild(0).transform.localPosition = new Vector3(0 - 9f * rate, StorySimulator.Instance.Chapter.transform.GetChild(0).transform.localPosition.y, StorySimulator.Instance.Chapter.transform.GetChild(0).transform.localPosition.z);
    }

    void UpdateTexture(int index)
    {
        var temp = Mathf.Min(index + 1, pageTextures.Length - 1);
        if (index + 1 > pageTextures.Length - 1)
        {
            index = 0;
            temp = Mathf.Min(index + 1, pageTextures.Length - 1);
        }
        // 紙芝居を消す
        if (!isReturn && StorySimulator.Instance.Chapter && StorySimulator.Instance.Chapter.transform.GetChild(0).localRotation.x <= 0) StorySimulator.Instance.ResetStoryInfo();
        bookRenderer.SetTextrue(pageTextures[index].LeftTexture, pageTextures[index].RightTexture, pageTextures[temp].LeftTexture, pageTextures[temp].RightTexture);
        isReturn = false;
    }

    int ClampPageIndex(int index)
    {
        return Mathf.Clamp(index, 0, maxPageIndex);
    }
}
