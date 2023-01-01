using System.Collections;

public interface IDefense
{
    void Defense();

    void SetDefensing(DefensePos nowDefensePos, float setRotation);

    void ReleaseDefense();
}

public interface IChangePhase
{
    IEnumerator ChangePhase();
}