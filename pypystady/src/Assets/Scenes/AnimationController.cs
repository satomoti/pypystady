using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnimationController
{
    const float DELTA_TIME_MAX = 1.0f;
    int _time = 0;
    float _inv_time_max = 1.0f;
    public void Set(int max_time)
    {
        Debug.Assert(0 < max_time);
        _time = max_time;
        _inv_time_max = 1.0f / (float)max_time;
    }
    public bool Update()
    {
        //if (DELTA_TIME_MAX < delta_time) delta_time = DELTA_TIME_MAX;
        //_time -= delta_time;
        _time = Math.Max(--_time, 0);
       // if (_time <= 0.0f)
        //{
          //  _time = 0.0f;
            //return false;
        //}
        return (0<_time);
    }
    public float GetNormalized()
    {
        //return _time * _inv_time_max;
        return _inv_time_max*(float)_time;
    }
    // Start is called before the first frame update
}
