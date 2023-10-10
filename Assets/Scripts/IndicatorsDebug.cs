using Mirror;
using UnityEngine;

public class IndicatorsDebug : NetworkBehaviour
{
    private Color white = new Color(1, 1, 1, 0.5f);
    private Color red = new Color(1, 0, 0, 0.5f);

    private float staminaScale;

    public GameObject lefto;
    public GameObject righto;
    public GameObject rightSwing;
    public GameObject leftSwing;
    public GameObject centralSwing;
    public GameObject staminaBar;
    private void Start()
    {
        staminaScale = staminaBar.transform.localScale.x;
    }
    public void AttackDirrection(bool isToRight)
    {
        if(isToRight)
        {
            lefto.GetComponent<SpriteRenderer>().color = white;
            righto.GetComponent<SpriteRenderer>().color = red;
        }
        else
        {
            lefto.GetComponent<SpriteRenderer>().color = red;
            righto.GetComponent<SpriteRenderer>().color = white;
        }
    }
    public void AttackWindUpIndicators(bool isToRight, bool isWindingUp)
    {
        if (isToRight)
        {
            rightSwing.GetComponent<SpriteRenderer>().color = isWindingUp ? Color.blue : Color.white;
            leftSwing.GetComponent<SpriteRenderer>().color = Color.white;
        }
        else
        {
            rightSwing.GetComponent<SpriteRenderer>().color = Color.white;
            leftSwing.GetComponent<SpriteRenderer>().color = isWindingUp ? Color.blue : Color.white;
        }
    }
    public void ThrustAttackWindUpIndicators(bool isWindingUp)
    {
        rightSwing.GetComponent<SpriteRenderer>().color = Color.white;
        leftSwing.GetComponent<SpriteRenderer>().color = Color.white;
        centralSwing.GetComponent<SpriteRenderer>().color = isWindingUp ? Color.blue : Color.white;
    }
    public void StaminaChanger(float stamina)
    {
        float newScale = stamina * staminaScale / 100;
        Debug.Log(newScale);
        staminaBar.transform.localScale = new Vector3(newScale, staminaBar.transform.localScale.y, staminaBar.transform.localScale.z);
    }
}
