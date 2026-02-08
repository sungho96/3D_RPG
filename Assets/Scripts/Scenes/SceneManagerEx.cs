using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
   public BaseScene CurrentScene
    {
        get { return GameObject.FindFirstObjectByType<BaseScene>(); }
    }

   //Define안에있는 Type만 
   public void LoadScene(Define.Scene type)
   {
        CurrentScene.Clear();
        SceneManager.LoadScene(GetSceneName(type));
   }

   string GetSceneName(Define.Scene type)
   {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
   }
}
