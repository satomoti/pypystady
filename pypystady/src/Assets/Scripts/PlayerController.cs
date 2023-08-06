using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
   
    AnimationController _animationController = new AnimationController();
    Vector2Int _last_position;
    RotState _last_rotate = RotState.Up;
    const int TRANS_TIME = 3;
    const int ROT_TIME = 3;
    //const float TRANS_TIME = 0.05f;
    //const float ROT_TIME = 0.05f;
    const int FALL_COUNT_UNIT = 120;
    const int FALL_COUNT_SPD = 10;
    const int FALL_COUNT_FAST_SPD = 20;
    const int GROUND_FRAMES = 50;
    int _fallCount = 0;
    int _groundFrame = GROUND_FRAMES;
    enum RotState
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,
        Invalid = -1,
    }
    [SerializeField] PuyoCantroller[] _puyoControllers = new PuyoCantroller[2] { default!, default! };
    [SerializeField] BoardController boardController = default!;
    Vector2Int _position;
    RotState _rotate = RotState.Up;
    // Start is called before the first frame update
    Logicallnput logicalInput = new();
    void Start()
    {
        _puyoControllers[0].SetPuyoType(PuyoType.Green);
        _puyoControllers[1].SetPuyoType(PuyoType.Red);
        _position = new Vector2Int(2, 12);
        _rotate = RotState.Up;
        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position,_rotate);
        _puyoControllers[1].SetPos(new Vector3((float)_position.x, (float)_position.y + 1.0f, 0.0f));
    }
    static readonly Vector2Int[] rotate_tbl = new Vector2Int[]
    {
        Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left
    };
    private static Vector2Int CalcChildPuyoPos(Vector2Int pos,RotState rot)
    {
        return pos + rotate_tbl[(int)rot];
    }
    // Update is called once per frame
    private bool CanMove(Vector2Int pos,RotState rot)
    {
        if (!boardController.CanSettele(pos)) return false;
        if (!boardController.CanSettele(CalcChildPuyoPos(pos,rot)))return false;
        return true;
    }
    void Update()
    {
        if (!_animationController.Update())
        {
            Control();
        }
        float anim_rate = _animationController.GetNormalized();
        _puyoControllers[0].SetPos(Interpolate(_position, RotState.Invalid, _last_position, RotState.Invalid, anim_rate));
        _puyoControllers[1].SetPos(Interpolate(_position, _rotate, _last_position, _last_rotate, anim_rate));
    }
    void Settle()
    {
        bool is_set0 = boardController.Settle(_position, (int)_puyoControllers[0].GetPuyoType());
        Debug.Assert(is_set0);
        bool is_set1 = boardController.Settle(CalcChildPuyoPos(_position,_rotate), (int)_puyoControllers[1].GetPuyoType());
        Debug.Assert(is_set1);
        gameObject.SetActive(false);
    }
    void QuickDrop()
    {
        Vector2Int pos = _position;
        do
        {
            pos += Vector2Int.down;
        } while (CanMove(pos, _rotate));
        pos -= Vector2Int.down;
        _position = pos;
        Settle();
    }
    bool Rotate(bool is_right)
    {
        RotState rot = (RotState)(((int)_rotate + (is_right ? +1 : +3)) & 3);
        Vector2Int pos = _position;
        switch (rot)
        {
            case RotState.Down:
                if(!boardController.CanSettele(pos + Vector2Int.down)||
                   !boardController.CanSettele(pos + new Vector2Int(is_right ? 1 : -1, -1)))
                {
                    pos += Vector2Int.up;
                }break;
            case RotState.Right:
                if (!boardController.CanSettele(pos + Vector2Int.right)) pos += Vector2Int.left;
                break;
            case RotState.Left:
                if (!boardController.CanSettele(pos + Vector2Int.left)) pos += Vector2Int.right;
                break;
            case RotState.Up:
                break;
            default:
                Debug.Assert(false);
                break;
        }
        if (!CanMove(pos, rot)) return false;
        SetTransition(pos, rot, ROT_TIME);
        return true;
    }
    void SetTransition(Vector2Int pos,RotState rot,int time)
    {
        _last_position = _position;
        _last_rotate = _rotate;
        _position = pos;
        _last_rotate = rot;
        _animationController.Set(time);
    }
    private bool Translate(bool is_right)
    {
        Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);
        if (!CanMove(pos, _rotate)) return false;
        //_position = pos;
        SetTransition(pos, _rotate, TRANS_TIME);
        return true;
    }
    static Vector3 Interpolate(Vector2Int pos,RotState rot,Vector2Int pos_last,RotState rot_last,float rate)
    {
        Vector3 p = Vector3.Lerp(
            new Vector3((float)pos.x, (float)pos.y, 0.0f),
            new Vector3((float)pos_last.x, (float)pos_last.y, 0.0f),rate);
        if (rot == RotState.Invalid) return p;
        //回転
        float theta0 = 0.5f * Mathf.PI * (float)(int)rot;
        float theta1 = 0.5f * Mathf.PI * (float)(int)rot_last;
        float theta = theta1 - theta0;
        //近いほうに回る
        if (+Mathf.PI < theta) theta = theta - 2.0f * Mathf.PI;
        if (theta < -Mathf.PI) theta = theta + 2.0f * Mathf.PI;
        theta = theta0 + rate * theta;
        return p + new Vector3(Mathf.Sin(theta), Mathf.Cos(theta), 0.0f);
    }
    static readonly KeyCode[] key_code_tbl = new KeyCode[(int)Logicallnput.Key.MAX] {
    KeyCode.RightArrow,
    KeyCode.LeftArrow,
    KeyCode.X,
    KeyCode.Z,
    KeyCode.UpArrow,
    KeyCode.DownArrow,
    };
    void UpdateInput()
    {
        Logicallnput.Key inputDev = 0;
        for(int i = 0; i < (int)Logicallnput.Key.MAX; i++)
        {
            if (Input.GetKey(key_code_tbl[i]))
            {
                inputDev |= (Logicallnput.Key)(1 << i);
            }
        }
        logicalInput.Update(inputDev);
    }
    void Control()
    {
        if (!Fall(logicalInput.IsRaw(Logicallnput.Key.Down))) return;
        
        if (_animationController.Update()) return;
        //idou
        if (logicalInput.IsRepeat(Logicallnput.Key.Right))
        {
            if (Translate(true)) return;
        }
        if (logicalInput.IsRepeat(Logicallnput.Key.Left))
        {
            if (Translate(false)) return;
        }
        //kaitenn
        if (logicalInput.IsRepeat(Logicallnput.Key.RotR))
        {
            if (Rotate(true)) return;
        }
        if (logicalInput.IsRepeat(Logicallnput.Key.RotL))
        {
            if (Rotate(false)) return;
        }
        //kuixtuku
        if (logicalInput.IsRepeat(Logicallnput.Key.QuickDrop))
        {
            QuickDrop();
        }
    }
    private void FixedUpdate()
    {
        UpdateInput();
        //if (!_animationController.Update())
        //{
        Control();
        //}
        Vector3 dy = Vector3.up * (float)_fallCount / (float)FALL_COUNT_UNIT;
        float anim_rate = _animationController.GetNormalized();
        _puyoControllers[0].SetPos(dy + Interpolate(_position, RotState.Invalid, _last_position, RotState.Invalid, anim_rate));
        _puyoControllers[1].SetPos(dy + Interpolate(_position, _rotate, _last_position, _last_rotate, anim_rate));
    }
    bool Fall(bool is_fast)
    {
        _fallCount -= is_fast ? FALL_COUNT_FAST_SPD : FALL_COUNT_SPD;
        while(_fallCount<0)
        {
            if (!CanMove(_position + Vector2Int.down, _rotate))
            {
                _fallCount = 0;
                if (0 < --_groundFrame) return true;
                Settle();
                return false;
            }
            _position += Vector2Int.down;
            _last_position += Vector2Int.down;
            _fallCount += FALL_COUNT_UNIT;
        }
        return true;
    }
}