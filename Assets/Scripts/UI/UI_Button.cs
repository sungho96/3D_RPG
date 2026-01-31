using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class UI_Button : UI_Base
{
    // UI 자동 바인딩용 맵: enum 이름이 Hierarchy 오브젝트 이름과 1:1로 매칭되어야 함
    enum Buttons//button 종류
    {
        pointButton,
    }

    enum Texts//Text 종류
    {
        pointText,
        ScoreText,
    }
    enum GameObjects
    {
        TestObject,
    }
    int _score = 0;

    private void Start()
    {
        // UI 바인딩은 시작 시 1회 수행(이후에는 Get으로 꺼내 쓰는 구조)
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
    }

    public void OnButtonClick()
    {
        _score++;
        
    }
}
