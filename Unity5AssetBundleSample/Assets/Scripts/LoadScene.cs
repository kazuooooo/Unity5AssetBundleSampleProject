using UnityEngine;
using System.Collections;

public class LoadScene : MonoBehaviour
{
	public string sceneBundle;
	public string sceneName;

	IEnumerator Start ()
	{
		// ("LoadSENE!!!");
		//AssetManagerの準備が出来るまで待機
		while (!AssetManager.instance.isReady)
			yield return null;

		//シーンのバンドルをロード
		AssetManager.instance.LoadBundle (sceneBundle);

		//バンドルのロードが終わるまで待機
		while (!AssetManager.instance.IsBundleLoaded (sceneBundle))
			yield return null;
		//シーンをロード
		Application.LoadLevel (sceneName);
	
	}
}
