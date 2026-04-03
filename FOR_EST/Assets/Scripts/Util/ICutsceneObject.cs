using System.Collections;
using UnityEngine;

public interface ICutsceneObject
{
    public void SetPosition(Vector2 pos);
    public void SetDirection(bool isRight);
    public void SetAnimation(string anim, bool tnf);
    public IEnumerator SetMoveTarget(Vector2 obj, bool isForceMove);
    public void SetReverse();
    public IEnumerator Fader(bool isFadeIn, float time);
}