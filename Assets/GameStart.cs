using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{

    public GameMode GameMode;

    // Start is called before the first frame update
    void Awake()
    {
        AppConst.GameMode = GameMode;
    }
}
