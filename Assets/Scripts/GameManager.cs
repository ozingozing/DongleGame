using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("----------------[Core]")]
    public int maxLevel;
    public int score;
    public bool isOver;
    [Header("----------------[Object Pooling]")]
    public GameObject Dongle_Prefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;
    public List<ParticleSystem> effectPool;
    public GameObject effectPrefab;
    public Transform effectGroup;
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Dongle lastDongle;
    [Header("----------------[Audio]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { LevelUp, Next, Attach, Button, Over };
    int sfxCursor;
    [Header("----------------[UI]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;
    [Header("----------------[ETC]")]
    public GameObject line;
    public GameObject bottom;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        
        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for(int i = 0; i < poolSize; i++)
        {
            MakeDongle();
        }

        if (!PlayerPrefs.HasKey("MaxScore")) PlayerPrefs.SetInt("MaxScore", 0);

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }
    public void GameStart()
    {
        //오브젝트 활성화
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);
        //사운드 플레이
        bgmPlayer.Play();
        SfxPlay(Sfx.Button);
        //게임 시작 (동글생성)
        Invoke("NextDongle", 1.5f);
    }

    Dongle MakeDongle()
    {
        //이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect" + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);


        //동글 생성
        GameObject instantDongleObj = Instantiate(Dongle_Prefab, dongleGroup);
        instantDongleObj.name = "Dongle" + donglePool.Count; 
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this;
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);
        return instantDongle;
    }

    Dongle GetDongle()
    {
       
        for (int i = 0; i < donglePool.Count; i++)
        {
            poolCursor = (poolCursor + 1) % donglePool.Count;
            if (!donglePool[poolCursor].gameObject.activeSelf)
            {
                //donglePool[poolCursor].transform.position = Vector3.Lerp(donglePool[poolCursor].transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.2f);
                return donglePool[poolCursor];
            }
        }

        return MakeDongle();
    }

    void NextDongle()
    {
        if(isOver)
        {
            return;
        }

        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine("WaitNext");
    }

    IEnumerator WaitNext()
    {
        while (lastDongle != null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);

        NextDongle();
    }

    public void TouchDown()
    {
        if (lastDongle == null) return;
        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null) return;
        lastDongle.Drop();
        lastDongle = null;
    }

    public void GameOver()
    {
        if(isOver)
        {
            return;
        }
        isOver = true;

        StartCoroutine("GameOverRoutine");
    }    

    IEnumerator GameOverRoutine()
    {
        //1.장면 안에 활성화 되어있는 모든 동글 가져오기
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        //2. 지우기 전에 모든 동글의 물리효과 비활성화
        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rigid.simulated = false;
        }

        //3. 1번 목록을 하나씩 접근해서 지우기
        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].Hide(Vector3.up * 300);
            yield return null; new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);
        //최고점수 갱신
        PlayerPrefs.SetInt("MaxScore", Mathf.Max(score, PlayerPrefs.GetInt("MaxScore")));
        //게임오버 UI 표시
        subScoreText.text = "점수 : " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine("ResetCoroutine");
    }

    IEnumerator ResetCoroutine()
    {
        yield return null; new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }
    public void SfxPlay(Sfx type)
    {
        switch(type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
            default:
                Debug.Log("SfxPlay() Error: Invalid type parameter");
                break;
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }

    private void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
