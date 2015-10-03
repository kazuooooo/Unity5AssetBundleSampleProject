using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AssetBundleLoadSample : MonoBehaviour
{
	[SerializeField]
	private string bundleName;
	[SerializeField]
	private string assetName;
	[SerializeField]
	private string sceneName;

	IEnumerator Start ()
	{
		//バンドルをロード
		yield return StartCoroutine (LoadCoroutine (bundleName));
		MakeObj (bundleName, assetName);
	}

	IEnumerator LoadCoroutine (string bundleName)
	{
		//AssetManagerの準備が出来るまで待機
		while (!AssetManager.instance.isReady)
			yield return null;

		//バンドルをロード
		AssetManager.instance.LoadBundle (bundleName);

		//バンドルのロードが終わるまで待機
		while (!AssetManager.instance.IsBundleLoaded (bundleName)) {
			Debug.Log ("Bundle" + bundleName + "Loading");
			yield return null;
		}
		//ロード完了
		Debug.Log (bundleName + "Load Complete");
	}

	private void MakeObj (string bundleName, string assetName)
	{
		//Managerからassetを取得
		GameObject asset = AssetManager.instance.GetAssetFromBundle (bundleName, assetName) as GameObject;
		//生成
		Instantiate (asset);
	}

	private void LoadScene (string bundleName, string sceneName)
	{
		//Managerからsceneを取得
		AssetManager.instance.GetAssetFromBundle (bundleName, sceneName);
		//シーンをロード
		Application.LoadLevel (sceneName);
	}
}
