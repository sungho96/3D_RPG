using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Animator _anim;
    [SerializeField] Transform _spawn;

    [Header("Pool Prefab path (Resources/Prefabs/")]
    [SerializeField] string _skillPrefabPath = "Skills/Skill1";

    const string skill1_BOOL = "SP SKILL 1";
    bool _busy;

    void Awake()
    {
        if (_anim == null) _anim = GetComponentInChildren<Animator>();
    }
    
    public void UseSkill1()
    {
        if (_busy) return;

        StartCoroutine(PulseBool(skill1_BOOL));

        Vector3 pos = (_spawn != null) ? _spawn.position : transform.position;
        Quaternion rot = (_spawn != null) ? _spawn.rotation : transform.rotation;

        GameObject go = Managers.Resource.Instantiate(_skillPrefabPath);
        if (go != null)
            go.transform.SetPositionAndRotation(pos, rot);

        _busy = true;
        StartCoroutine(UnlockAfter(0.15f));
    }

    IEnumerator PulseBool(string param)
    {
        _anim.SetBool(param, true);
        yield return null;
        _anim.SetBool(param, false);
    }

    IEnumerator UnlockAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        _busy = false;
    }
}
