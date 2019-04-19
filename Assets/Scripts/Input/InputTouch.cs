using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    public class InputTouch
    {
        private static bool _touchEnable = true;
        public static bool touchEnable
        {
            set
            {
                _touchEnable = value;
            }
            get
            {
                return _touchEnable;
            }
        }

        public static Touch GetTouch(int index)
        {
            Touch res = Input.GetTouch(index);
            if (!_touchEnable)
            {
                res.phase = TouchPhase.Canceled;
            }
            return res;
        }

        public static Touch[] GetTouches()
        {
            Touch[] res = Input.touches;
            if (!_touchEnable)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    res[i].phase = TouchPhase.Canceled;
                }
            }
            return res;
        }
    }
}
