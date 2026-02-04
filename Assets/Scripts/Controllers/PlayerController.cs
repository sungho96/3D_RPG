using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float _speed = 10.0f;//플레이어 이동속도

	bool _moveToDest = false;//이동여부 판단
	Vector3 _destPos;//mouse 목적지

	UI_Inven _inven;
	UI_HUD _hud;

    void Start()
    {	//키보드로 이동 함수 호출(Action)
		Managers.Input.KeyAction -= OnKeyboard;
		Managers.Input.KeyAction += OnKeyboard;

		//마우스 클릭 함수 호출
		Managers.Input.MouseAction -= OnMouseClicked;
		Managers.Input.MouseAction += OnMouseClicked;

		//Inven생성
		_inven = Managers.UI.ShowSceneUI<UI_Inven>();
        _inven.gameObject.SetActive(false);

		//HUD생성
        _hud = Managers.UI.ShowSceneUI<UI_HUD>();


    }

    void Update()
    {
        OnKeyboard();

        if (_moveToDest)
		{
			//목적에서 방향값 가져오기
			Vector3 dir = _destPos - transform.position;//마우스-플레이어
			if (dir.magnitude < 0.0001f)//도착했을 때 발생하는 0에 근접한 오류 방지
            {
				_moveToDest = false;
			}
			else
			{	
				//오버슈트방지 목적 Mathf.Clamp 사용
				float moveDist = Mathf.Clamp(_speed * Time.deltaTime, 0, dir.magnitude);
				transform.position += dir.normalized * moveDist;
				//이동시 그쪽방향을 보고 이동하게끔LookRotation과 자연스러운 회전 보간 사용Slerp
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
			}
		}
    }

    void OnKeyboard()
    {
		if (Input.GetKey(KeyCode.W))
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.forward), 0.2f);
			transform.position += Vector3.forward * Time.deltaTime * _speed;
		}

		if (Input.GetKey(KeyCode.S))
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.back), 0.2f);
			transform.position += Vector3.back * Time.deltaTime * _speed;
		}

		if (Input.GetKey(KeyCode.A))
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.left), 0.2f);
			transform.position += Vector3.left * Time.deltaTime * _speed;
		}

		if (Input.GetKey(KeyCode.D))
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right), 0.2f);
			transform.position += Vector3.right * Time.deltaTime * _speed;
		}

        if (Input.GetKeyDown(KeyCode.I))
            _inven.gameObject.SetActive(!_inven.gameObject.activeSelf);

        _moveToDest = false;
	}

	void OnMouseClicked(Define.MouseEvent evt)
	{
		if (evt != Define.MouseEvent.Click)
			return;

        //화면으로부터 일직선을 쏴서 현재 마우스좌표를 가져와라
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
		//Scen에서만 보이는 ray 경로하긴하기위해  
		Debug.DrawLine(Camera.main.transform.position, ray.direction * 100f, Color.red);
		RaycastHit hit;//hit에 저장
		
		//만약 RayCast(범위,layermask=Wall)이면 destpos= pos ,moveTo=true를 반환한다.
		if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask("Wall")))
		{
			_destPos = hit.point;
			_moveToDest = true;
			//Debug.Log($"Raycast camera @{hit.collider.gameObject.tag}");
		}
	}
}
