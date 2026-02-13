using System;
using System.Collections.Generic;
using UnityEngine;

#region Data Classes

/// <summary>
/// 단일 스탯 레코드
/// JSON에서 stats 배열의 원소 1개에 해당
/// </summary>
[Serializable]
public class Stat
{
    public int level;   // 레벨(딕셔너리 Key로 사용)
    public int hp;      // 체력
    public int mp;      // 마나
    public int attack;  // 공격력
}

/// <summary>
/// 스탯 데이터 로더
/// JSON 구조(리스트)를 받아서 level -> Stat 딕셔너리로 변환
/// </summary>
[Serializable]
public class StatData : ILoader<int, Stat>
{
    
    public List<Stat> stats = new List<Stat>();

    /// <summary>
    /// level을 Key로 딕셔너리 변환
    /// 같은 level이 중복되면 Add에서 예외 발생 가능(데이터 중복 방지)
    /// </summary>
    public Dictionary<int, Stat> MakeDict()
    {
        Dictionary<int, Stat> dict = new Dictionary<int, Stat>();

        foreach (Stat stat in stats)
            dict.Add(stat.level, stat);

        return dict;
    }
}
#endregion