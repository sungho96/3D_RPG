using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON 로더 표준 인터페이스
/// 로드된 데이터(리스트 등)를 Dictionary로 변환하는 규칙을 강제
/// </summary>
public interface ILoader<Key, Value>
{
    // 데이터 구조(List 등)를 "Key -> Value" 딕셔너리로 변환
    Dictionary<Key, Value> MakeDict();
}

/// <summary>
/// 게임 데이터 관리 매니저
/// Resources/Data 경로의 JSON(TextAsset)을 읽어 Dictionary로 캐싱
/// Init은 게임 시작 시 1회만 호출하는 것을 전제로 함
/// </summary>
public class DataManager
{
    // 스탯 테이블: level -> Stat
    public Dictionary<int, Data.Stat> StatDict { get; private set; } = new Dictionary<int, Data.Stat>();

    /// <summary>
    /// 데이터 초기 로드(1회)
    /// StatData.json을 로드해서 level 기준 딕셔너리로 변환해 캐싱
    /// </summary>
    public void Init()
    {
        StatDict = LoadJson<Data.StatData, int, Data.Stat>("StatData").MakeDict();
    }

    /// <summary>
    /// Resources/Data/{path}의 JSON을 읽어서 Loader 타입으로 역직렬화
    /// Loader는 ILoader를 구현해야 함
    /// </summary>
    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        // Resources 폴더 기준 로드: Resources/Data/StatData
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");

        // JSON 문자열을 Loader 객체로 변환
        return JsonUtility.FromJson<Loader>(textAsset.text);
    }
}