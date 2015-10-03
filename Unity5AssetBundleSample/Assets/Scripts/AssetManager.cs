using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AssetManager : MonoBehaviour
{
	public static AssetManager instance;
	public string[] bundleVariants;
	[SerializeField] string pathToBundles;
	Dictionary<string, AssetBundle> bundles;
	AssetBundleManifest manifest = null;
	string platform;
	public float progressVal;

	public bool isReady {
		get {
			return !object.ReferenceEquals (manifest, null);
		}
	}

	void Awake ()
	{
		if (object.ReferenceEquals (instance, null)) {
			instance = this;
		} else if (!object.ReferenceEquals (instance, this)) { 
			////////print ("destroyself");

			Destroy (gameObject);
			return;
		}
		Debug.Log ("start");

		DontDestroyOnLoad (gameObject);

		#if UNITY_IOS
		platform = "iOS";
		#elif UNITY_ANDROID
		platform = "Android";
		#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		platform = "PC";
		#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		platform = "OSX";
		#elif UNITY_WEBPLAYER
		platform = "Web";
		#else
		platform = "error";
		Debug.Log("unsupported platform");
		SetDebugText("unsupported platform");
		#endif
		pathToBundles += platform + "/";
		bundles = new Dictionary<string, AssetBundle> ();
		StartCoroutine ("LoadManifest");
	}

	IEnumerator LoadManifest ()
	{
		Debug.Log ("Loading Manifest");
		Debug.Log ("スタートロード");
		using (WWW www = new WWW (pathToBundles + platform)) {
			yield return www;
			if (!string.IsNullOrEmpty (www.error)) {
				Debug.Log (www.error);

				return false;
			}
			progressVal = www.progress;
			Debug.Log ("progress" + progressVal.ToString ());
			Debug.Log ("プラットフォーム" + platform);
			Debug.Log (pathToBundles + platform);
			Debug.Log ("マニフェストをロード開始");

			manifest = (AssetBundleManifest)www.assetBundle.LoadAsset ("AssetBundleManifest", typeof(AssetBundleManifest));
			yield return null;
			Debug.Log ("バンドルを解放");
			www.assetBundle.Unload (false);

			manifest = (AssetBundleManifest)www.assetBundle.LoadAsset ("AssetBundleManifest", typeof(AssetBundleManifest));

		} 

		if (!isReady) {
			Debug.Log ("There was an error loading manifest");
			Debug.Log ("Manifestをロード出来ませんでした");
		} else {
			Debug.Log ("Manifest loaded successfully");

			Debug.Log ("Manifestをロード完了");
		}
	}

	public bool IsBundleLoaded (string bundleName)
	{
		return bundles.ContainsKey (bundleName);
	}

	public Object GetAssetFromBundle (string bundleName, string assetName)
	{
		if (!IsBundleLoaded (bundleName))
			return null;

		return bundles [bundleName].LoadAsset (assetName);
	}

	public bool LoadBundle (string bundleName)
	{
		////////print ("assetmanager loadbundle" + bundleName);
		//See if bundle is already loaded
		if (IsBundleLoaded (bundleName))
			return true;

		//If bundle isn't loaded, load it
		StartCoroutine (LoadBundleCoroutine (bundleName));
		return false;
	}

	IEnumerator LoadBundleCoroutine (string bundleName)
	{
		//Bundle has already been loaded
		if (IsBundleLoaded (bundleName))
			yield break;
		//ロードしたいbundleに依存するbundleを取得
		string[] dependencies = manifest.GetAllDependencies (bundleName);
		//依存するbundleを全てロードする
		for (int i = 0; i < dependencies.Length; i++)
			yield return StartCoroutine (LoadBundleCoroutine (dependencies [i]));
		bundleName = RemapVariantName (bundleName);
		string url = pathToBundles + bundleName;
		Debug.Log ("Beginning to load " + bundleName + " / " + manifest.GetAssetBundleHash (bundleName));
		//キャッシュに残っていればキャッシュから落とす
		using (WWW www = WWW.LoadFromCacheOrDownload (url, manifest.GetAssetBundleHash (bundleName))) {
			Debug.Log ("progress" + www.progress);
			yield return www;
			if (!string.IsNullOrEmpty (www.error)) {
				Debug.Log (www.error);
				return false;
			}
			bundles.Add (bundleName, www.assetBundle);
		}
		Debug.Log ("Finished loading " + bundleName);
	}

	void OnDisable ()
	{
		Debug.Log ("Unloading Bundles");

		foreach (KeyValuePair<string, AssetBundle> entry in bundles)
			entry.Value.Unload (false);
		bundles.Clear ();

		Debug.Log ("Bundles Unloaded");
	}

	string RemapVariantName (string assetBundleName)
	{
		string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant ();

		if (System.Array.IndexOf (bundlesWithVariant, assetBundleName) < 0)
			return assetBundleName;

		string[] splitBundleName = assetBundleName.Split ('.');
		string[] candidateBundles = System.Array.FindAll (bundlesWithVariant, element => element.StartsWith (splitBundleName [0]));

		int index = -1;
		for (int i = 0; i < bundleVariants.Length; i++) {
			index = System.Array.IndexOf (candidateBundles, splitBundleName [0] + "." + bundleVariants [i]);
			if (index != -1)
				return candidateBundles [index];
		}

		return assetBundleName;
	}
}