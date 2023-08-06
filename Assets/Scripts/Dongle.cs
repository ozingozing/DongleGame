using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager manager;
    public ParticleSystem effect;
    public bool isMerge;
    public int level;
    public bool isDrag;
    public bool isAttach;

    public Rigidbody2D rigid;
    CircleCollider2D circle;
    Animator anim;
    SpriteRenderer spriteRenderer;

    float deadTime;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        circle = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
   

    private void OnEnable()
    {
        anim.SetInteger("Level", level);
    }

    private void OnDisable()
    {
        //변수 초기화
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;
        
        //트렌스폼 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;
        
        //물리 초기화
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float leftBorder = 4f + transform.localScale.x / 2f;
            float rightBorder = -4f - transform.localScale.x / 2f; ;

            if (mousePos.x < rightBorder)
            {
                mousePos.x = rightBorder;
            }
            else if (mousePos.x > leftBorder)
            {
                mousePos.x = leftBorder;
            }

            mousePos.z = 0;
            mousePos.y = 8;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
        }
        
    }
    

    public void Drag()
    {
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            if(level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                // 자기 자신과 상대편 위치 가지고 오기
                float meX = transform.position.x; 
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                //1. 내가 아래에 있을 때
                //2. 동일한 높이일 때, 내가 오른쪽에 있을 때
                if(meY < otherY || (meY == otherY && meX > otherX))
                {
                    //상대방은 비활성화
                    other.Hide(this.transform.position);
                    //나는 레벨업
                    LevelUp();
                }
            }
        }
    }

   

    public void Hide(Vector3 targerPos)
    {
        isMerge = true;

        rigid.simulated = false;
        circle.enabled = false;

        if(targerPos == Vector3.up * 300)
        {
            EffectPlay();
        }

        StartCoroutine(HideRoutine(targerPos));
    }

    IEnumerator HideRoutine(Vector3 targerPos)
    {
        int frameCount = 0;
        while(frameCount < 20)
        {
            frameCount++;
            if(targerPos != Vector3.up * 300)
            {
                transform.position = Vector3.Lerp(transform.position, targerPos, 0.5f);
            }
            else if(targerPos == Vector3.up * 300)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }
            
            yield return null;
        }

        manager.score += (int)Mathf.Pow(2, level);

        this.isMerge = false;
        this.gameObject.SetActive(false);
    }

    void LevelUp()
    {
        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());
    }
    
    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1);
        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f);
        level++;

        
        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine("AttachRoutine");
        
    }

    IEnumerator AttachRoutine()
    {
        if (isAttach)
        {
            yield break;
        }

        isAttach = true;
        manager.SfxPlay(GameManager.Sfx.Attach);

        yield return new WaitForSeconds(0.2f);
        isAttach = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;

            if(deadTime > 2)
            {
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }
            if(deadTime > 5)
            {
                manager.GameOver();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
}
