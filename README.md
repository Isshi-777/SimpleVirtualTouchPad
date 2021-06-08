# SimpleVirtualTouchPad

シンプルな仮想タッチパッド  
下記はEventSystemを使用していない方法で作成したため今回はEventsystemを利用する方法  
https://github.com/Isshi-777/VirtualTouchPad  
  
## 概要
**下記の操作を検知し、セットされたイベントを実行する**  
・タッチ（押した瞬間）  
・長押し  
・クリック（タップ）  
・ドラッグ  
・フリック  
  
**設定項目**  
・範囲内のみ更新を有効にするか  
・クリック判定をする範囲の半径  
・長押し判定をする範囲の半径  
・長押し判定をする時間  
・ドラッグ判定する距離  
・フリック判定する距離  
・フリック判定をする時間  

  ## 「範囲内のみ更新を有効にするか」の設定のON/OFF時の挙動
  **・ONの時**  
  下記の場合だとImage範囲外でドラッグおよびリリース（タッチ終了）を行った場合更新されないしイベントも呼ばれない  
  ※タッチし始め（いわゆるPress）はImage上でやらないとイベントは呼ばれない
  ![Videotogif](https://user-images.githubusercontent.com/36006543/121214154-d13ef080-c8b9-11eb-9925-8d1777b9002f.gif)
  
  **・OFFの時**  
  下記の場合だとImage範囲外でドラッグおよびリリース（タッチ終了）を行った場合更新はされるしイベントも呼ばれる  
  ※タッチし始め（いわゆるPress）はImage上でやらないとイベントは呼ばれない
  ![Videotogif (1)](https://user-images.githubusercontent.com/36006543/121214569-3266c400-c8ba-11eb-8fee-0fd17a353dd6.gif)  
  
  
  ## 使用例
  ```cs
  using Isshi777;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    public SimpleVirtualTouchPad touchPad;

    private void Start()
    {
        touchPad.OnPressEvent += () => { Debug.Log("Press"); };
        touchPad.OnLongPressEvent += () => { Debug.Log("LongPress"); };
        touchPad.OnClickEvent += () => { Debug.Log("Click"); };
        touchPad.OnDragEvent += (pressPos, lastPos, currentPos) => { Debug.Log("Drag" + pressPos + " : " + lastPos + " : " + currentPos); };
        touchPad.OnFlickEvent += (direction) => { Debug.Log("Flick" + " : " + direction); }; 
    }
}
  ```
