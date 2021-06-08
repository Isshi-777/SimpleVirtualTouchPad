using UnityEngine;
using UnityEngine.EventSystems;

namespace Isshi777
{
    /// <summary>
    /// �V���v���ȉ��z�^�b�`�p�b�h
    /// </summary>
    public class SimpleVirtualTouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        /// <summary>
        /// �t���b�N�̌���
        /// </summary>
        public enum EFlickDirection
        {
            Up,
            Down,
            Right,
            Left,
        }

        #region �C�x���g�֘A
        /// <summary>
        /// �^�b�`�̃C�x���g�֐��iPress�AClick�ALongPress�Ŏg�p�j
        /// </summary>
        public delegate void OnCommonTouchEventFunction();

        /// <summary>
        /// �h���b�O�̃C�x���g�֐�
        /// </summary>
        /// <param name="pressPosition">�������ۂ̍��W</param>
        /// <param name="lastPosition">�O�t���[���̍��W</param>
        /// <param name="currentPosition">���݂̍��W</param>
        public delegate void OnDragEventFunction(Vector2 pressPosition, Vector2 lastPosition, Vector2 currentPosition);

        /// <summary>
        /// �t���b�N�̃C�x���g�֐�
        /// </summary>
        /// <param name="direction">�t���b�N�̌���</param>
        public delegate void OnFlickEventFunction(EFlickDirection direction);

        /// <summary>
        /// �^�b�`�����ۂɌĂ΂��C�x���g
        /// </summary>
        public event OnCommonTouchEventFunction OnPressEvent = null;

        /// <summary>
        /// ���������ɌĂ΂��C�x���g
        /// </summary>
        public event OnCommonTouchEventFunction OnLongPressEvent = null;

        /// <summary>
        /// �N���b�N���ɌĂ΂��C�x���g
        /// </summary>
        public event OnCommonTouchEventFunction OnClickEvent = null;

        /// <summary>
        /// �h���b�O���ɌĂ΂��C�x���g
        /// </summary>
        public event OnDragEventFunction OnDragEvent = null;

        /// <summary>
        /// �t���b�N���ɌĂ΂��C�x���g
        /// </summary>
        public event OnFlickEventFunction OnFlickEvent = null;
        #endregion �C�x���g�֘A

        #region �e�����l�ϐ�
        /// <summary>
        /// �͈͓��̂ݍX�V��L���ɂ��邩
        /// </summary>
        [SerializeField]
        private bool updateRangeOnly;

        /// <summary>
        /// �N���b�N���������͈͂̔��a
        /// </summary>
        [SerializeField]
        private float clickRadius;

        /// <summary>
        /// ���������������͈͂̔��a
        /// </summary>
        [SerializeField]
        private float longPressRadius;

        /// <summary>
        /// ��������������鎞��
        /// </summary>
        [SerializeField]
        private float longPressDuration;

        /// <summary>
        /// �h���b�O���肷�鋗��
        /// </summary>
        [SerializeField]
        private float dragDistance;

        /// <summary>
        /// �t���b�N���肷�鋗��
        /// </summary>
        [SerializeField]
        private float flickDistance;

        /// <summary>
        /// �t���b�N��������鎞��
        /// </summary>
        [SerializeField]
        private float flickDuration;
        #endregion �e�����l�ϐ�


        /// <summary>
        /// RectTransform
        /// </summary>
        private RectTransform rectTransfirm;

        /// <summary>
        /// �^�C�}�[
        /// </summary>
        private float timer;

        /// <summary>
        /// �������ۂ̍��W
        /// </summary>
        private Vector2 pressPosition;

        /// <summary>
        /// �O�t���[���̍��W
        /// </summary>
        private Vector2 lastPosition;

        /// <summary>
        /// �������C�x���g���Ăׂ邩�i������(LongPress)�͎w��͈͊O�ɏo��ƌĂׂȂ�����̂ł��̃t���O��p�Ӂj
        /// </summary>
        private bool cantCallLongPress;

        /// <summary>
        /// �X�V�������s����
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

                // �������C�x���g�Ăяo��(OnDrag�͍��W�ړ����Ȃ��ƌĂ΂�Ȃ����ߒ������͂�����ɏ���)
                if (this.timer >= this.longPressDuration && !this.cantCallLongPress)
                {
                    this.OnLongPressEvent.Invoke();
                    this.cantCallLongPress = true;
                }
            }
        }

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="includeEvents">�e�C�x���g�̏����������邩</param>
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
        /// �e�C�x���g�̏�����
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

            // �^�b�`�C�x���g�Ăяo��
            this.OnPressEvent.Invoke();
        }

        public void OnDrag(PointerEventData data)
        {
            if (!this.isUpdate)
            {
                return;
            }

            // �͈͓��̂ݍX�V�\�̐ݒ�Ŕ͈͊O�̏ꍇ
            if (this.updateRangeOnly && !this.IsInRange(data))
            {
                return;
            }

            var currentPosition = data.position;
            var deltaInitial = Vector2.Distance(this.pressPosition, currentPosition);
            var deltaLast = Vector2.Distance(this.lastPosition, currentPosition);

            // ������(LongPress)�͈̔͂𒴂����ꍇ�͌ĂׂȂ��悤�ɂ���
            if (deltaInitial > this.longPressRadius)
            {
                this.cantCallLongPress = true;
            }

            // �h���b�O�C�x���g�Ăяo��
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

            // �͈͓��̂ݍX�V�\�̐ݒ�Ŕ͈͊O�̏ꍇ
            if (this.updateRangeOnly && !this.IsInRange(data))
            {
                return;
            }

            var currentPosition = data.position;
            var deltaInitial = Vector2.Distance(this.pressPosition, currentPosition);

            // �N���b�N�C�x���g�Ăяo��
            if (deltaInitial <= this.clickRadius)
            {
                this.OnClickEvent();
            }

            // �t���b�N�C�x���g�Ăяo��
            if (this.timer < this.flickDuration && deltaInitial > this.flickDistance)
            {
                EFlickDirection flickDirection = EFlickDirection.Up;
                var direction = currentPosition - this.pressPosition;
                Vector2 absDirection = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
                if (absDirection.x <= absDirection.y)// Y�����ւ̃t���b�N
                {
                    // �ő��ɂȂ��Ǝv�����C�R�[���̎����������ʂ�AY������D�悳����
                    flickDirection = (direction.y < 0) ? EFlickDirection.Down : EFlickDirection.Up;
                }
                else// X�����ւ̃t���b�N
                {
                    flickDirection = (direction.x < 0) ? EFlickDirection.Left : EFlickDirection.Right;
                }

                this.OnFlickEvent(flickDirection);
            }

            this.Refresh(false);
        }
    }
}
