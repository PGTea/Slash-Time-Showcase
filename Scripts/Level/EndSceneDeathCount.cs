using UnityEngine;
using UnityEngine.UI;

public class EndSceneDeathCount : MonoBehaviour
{
    [SerializeField] private Image[] levelImageDigit;
    [SerializeField] private Sprite[] NumSprites;
    void Start()
    {
        int[] deathCount = GameOverRestart.Instance.deathCount;
        if(deathCount == null) deathCount = new int[]{1,1,1,1,1,2};
        int level1_Index = 1;
        for(int i = 0; i < levelImageDigit.Length; i++)
        {
            int deathNum = deathCount[(i/2)+level1_Index];
            if(deathNum > 99)
            {
                levelImageDigit[i].sprite = NumSprites[9];
            }
            else
            {
                if(i % 2 == 0) levelImageDigit[i].sprite = NumSprites[deathNum / 10 % 10];
                else levelImageDigit[i].sprite = NumSprites[deathNum % 10];
            }
        }
    }
}
