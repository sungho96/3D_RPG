using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poolable : MonoBehaviour
{
    /*
     * 풀링 대상 표시용 컴포넌트
     * - 이 컴포넌트가 붙어 있으면 ResourceManager.Instantiate/Destroy가
     *   "생성/파괴"가 아니라 "Pop/Push"로 동작하도록 분기됨
     *
     * IsUsing:
     * - 현재 사용 중인지 상태를 표시하려는 목적
     * - 일반적으로 Pop 시 true, Push 시 false가 자연스럽지만
     *   현재 Pool.Pop에서 false로 세팅되어 있어 의미 정리가 필요할 수 있음
     */
    public bool IsUsing;
}