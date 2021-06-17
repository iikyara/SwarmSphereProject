using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestEditorScript : EditorWindow
{
  [MenuItem("Editor/Sample")]
  private static void Create()
  {
    // 生成
    GetWindow<TestEditorScript>("サンプル");
  }
}
