using Mirror;
using UnityEngine;

public class IndicatorsDebug : NetworkBehaviour
{
    private Color white = new Color(1, 1, 1, 0.5f);
    private Color red = new Color(1, 0, 0, 0.5f);

    private float staminaBarScale;
    private float healthBarScale;

    [SerializeField] private GameObject lefto;
    [SerializeField] private GameObject righto;
    [SerializeField] private GameObject rightSwing;
    [SerializeField] private GameObject leftSwing;
    [SerializeField] private GameObject centralSwing;
    [SerializeField] private GameObject staminaBar;
    [SerializeField] private GameObject healthBar;
    private void Start()
    {
        staminaBarScale = staminaBar.transform.localScale.x;
        healthBarScale = healthBar.transform.localScale.x;
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
        float newScale = stamina * staminaBarScale / 100;
        staminaBar.transform.localScale = new Vector3(newScale, staminaBar.transform.localScale.y, staminaBar.transform.localScale.z);
    }
    public void HealthChanger(float hp)
    {
        float newScale = hp * healthBarScale / 100;
        healthBar.transform.localScale = new Vector3(newScale, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
    }
}
