using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadTest : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/ui/prefab/uitest.prefab.ab");
        yield return request;

        AssetBundleCreateRequest bgImgResRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/ui/resources/background_01_loading_1920.png.ab");
        yield return bgImgResRequest;
        AssetBundleCreateRequest bgBtnResRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/ui/resources/btn_rectangle_01_n_blue.png.ab");
        yield return bgBtnResRequest;

        AssetBundleRequest bundleRequest = request.assetBundle.LoadAssetAsync("Assets/BuildResources/UI/Prefab/UITest.prefab");
        yield return bundleRequest;

        GameObject go = Instantiate(bundleRequest.asset) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
