using UnityEngine;
using UnityEngine.EventSystems;

namespace Isshi777
{
    /// <summary>
    /// シンプルな仮想タッチパッド
    /// </summary>
    public class SimpleVirtualTouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        /// <summary>
        /// フリックの向き
        /// </summary>
        public enum EFlickDirection
        {
            Up,
            Down,
            Right,
            Left,
        }

        #region イベント関連
        /// <summary>
        /// タッチのイベント関数（Press、Click、LongPressで使用）
        /// </summary>
        public delegate void OnCommonTouchEventFunction();

        /// <summary>
        /// ドラッグのイベント関数
        /// </summary>
        /// <param name="pressPosition">押した際の座標</param>
        /// <param name="lastPosition">前フレームの座標</param>
        /// <param name="currentPosition">現在の座標</param>
        public delegate void OnDragEventFunction(Vector2 pressPosition, Vector2 lastPosition, Vector2 currentPosition);

        /// <summary>
        /// フリックのイベント関数
        /// </summary>
        /// <param name="direction">フリックの向き</param>
        public delegate void OnFlickEventFunction(EFlickDirection direction);

        /// <summary>
        /// タッチした際に呼ばれるイベント
        /// </summary>
        public event OnCommonTouchEventFunction OnPressEvent = null;

        /// <summary>
        /// 長押し時に呼ばれるイベント
        /// </summary>
        public event OnCommonTouchEventFunction OnLongPressEvent = null;

        /// <summary>
        /// クリック時に呼ばれるイベント
        /// </summary>
        public event OnCommonTouchEventFunction OnClickEvent = null;

        /// <summary>
        /// ドラッグ時に呼ばれるイベント
        /// </summary>
        public event OnDragEventFunction OnDragEvent = null;

        /// <summary>
        /// フリック時に呼ばれるイベント
        /// </summary>
        public event OnFlickEventFunction OnFlickEvent = null;
        #endregion イベント関連

        #region 各条件値変数
        /// <summary>
        /// 範囲内のみ更新を有効にするか
        /// </summary>
        [SerializeField]
        private bool updateRangeOnly;

        /// <summary>
        /// クリックを許容する範囲の半径
        /// </summary>
        [SerializeField]
        private float clickRadius;

        /// <summary>
        /// 長押し判定を許容する範囲の半径
        /// </summary>
        [SerializeField]
        private float longPressRadius;

        /// <summary>
        /// 長押し判定をする時間
        /// </summary>
        [SerializeField]
        private float longPressDuration;

        /// <summary>
        /// ドラッグ判定する距離
        /// </summary>
        [SerializeField]
        private float dragDistance;

        /// <summary>
        /// フリック判定する距離
        /// </summary>
        [SerializeField]
        private float flickDistance;

        /// <summary>
        /// フリック判定をする時間
        /// </summary>
        [SerializeField]
        private float flickDuration;
        #endregion 各条件値変数


        /// <summary>
        /// RectTransform
        /// </summary>
        private RectTransform rectTransfirm;

        /// <summary>
        /// タイマー
        /// </summary>
        private float timer;

        /// <summary>
        /// 押した際の座標
        /// </summary>
        private Vector2 pressPosition;

        /// <summary>
        /// 前フレームの座標
        /// </summary>
        private Vector2 lastPosition;

        /// <summary>
        /// 長押しイベントを呼べるか（長押し(LongPress)は指定範囲外に出ると呼べなくするのでこのフラグを用意）
        /// </summary>
        private bool cantCallLongPress;

        /// <summary>
        /// 更新処理を行うか
        /// </summary>
        private bool isUpdate;


        private void Awake()
        {
            this.rectTransfirm = this.transform as RectTransform;
            this.Refresh(true);
        }

        private void Update()
        {
            if (this.isUpdate)
            {
                this.timer += Time.deltaTime;

                // 長押しイベント呼び出し(OnDragは座標移動がないと呼ばれないため長押しはこちらに書く)
                if (this.timer >= this.longPressDuration && !this.cantCallLongPress)
                {
                    this.OnLongPressEvent.Invoke();
                    this.cantCallLongPress = true;
                }
            }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="includeEvents">各イベントの初期化もするか</param>
        public void Refresh(bool includeEvents)
        {
            this.timer = 0f;
            this.isUpdate = false;
            this.pressPosition = Vector2.zero;
            this.lastPosition = Vector2.zero;
            this.cantCallLongPress = false;

            if (includeEvents)
            {
                this.RefreshEvents();
            }
        }

        /// <summary>
        /// 各イベントの初期化
        /// </summary>
        public void RefreshEvents()
        {
            this.OnPressEvent = null;
            this.OnLongPressEvent = null;
            this.OnClickEvent = null;
            this.OnDragEvent = null;
            this.OnFlickEvent = null;
        }

        private bool IsInRange(PointerEventData data)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(this.rectTransfirm, data.position, data.pressEventCamera);
        }

        public void OnPointerDown(PointerEventData data)
        {
            this.isUpdate = true;
            this.pressPosition = data.position;
            this.lastPosition = data.position;

            // タッチイベント呼び出し
            this.OnPressEvent.Invoke();
        }

        public void OnDrag(PointerEventData data)
        {
            if (!this.isUpdate)
            {
                return;
            }

            // 範囲内のみ更新可能の設定で範囲外の場合
            if (this.updateRangeOnly && !this.IsInRange(data))
            {
                return;
            }

            var currentPosition = data.position;
            var deltaInitial = Vector2.Distance(this.pressPosition, currentPosition);
            var deltaLast = Vector2.Distance(this.lastPosition, currentPosition);

            // 長押し(LongPress)の範囲を超えた場合は呼べないようにする
            if (deltaInitial > this.longPressRadius)
            {
                this.cantCallLongPress = true;
            }

            // ドラッグイベント呼び出し
            if (deltaInitial > this.dragDistance)
            {
                this.OnDragEvent.Invoke(this.pressPosition, this.lastPosition, currentPosition);
            }

            this.lastPosition = currentPosition;
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (!this.isUpdate)
            {
                return;
            }

            // 範囲内のみ更新可能の設定で範囲外の場合
            if (this.updateRangeOnly && !this.IsInRange(data))
            {
                return;
            }

            var currentPosition = data.position;
            var deltaInitial = Vector2.Distance(this.pressPosition, currentPosition);

            // クリックイベント呼び出し
            if (deltaInitial <= this.clickRadius)
            {
                this.OnClickEvent();
            }

            // フリックイベント呼び出し
            if (this.timer < this.flickDuration && deltaInitial > this.flickDistance)
            {
                EFlickDirection flickDirection = EFlickDirection.Up;
                var direction = currentPosition - this.pressPosition;
                Vector2 absDirection = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
                if (absDirection.x <= absDirection.y)// Y方向へのフリック
                {
                    // 滅多にないと思うがイコールの時もこちらを通り、Y方向を優先させる
                    flickDirection = (direction.y < 0) ? EFlickDirection.Down : EFlickDirection.Up;
                }
                else// X方向へのフリック
                {
                    flickDirection = (direction.x < 0) ? EFlickDirection.Left : EFlickDirection.Right;
                }

                this.OnFlickEvent(flickDirection);
            }

            this.Refresh(false);
        }
    }
}
