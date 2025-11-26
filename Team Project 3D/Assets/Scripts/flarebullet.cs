using UnityEngine;
using System.Collections;

// [중요] MonoBehaviour 대신 Bullet을 상속받습니다.
public class flarebullet : Bullet
{

    private Light flarelight;
    private AudioSource flaresound;
    private ParticleSystemRenderer smokepParSystem;
    private bool myCoroutine;
    private float smooth = 2.4f;
    public float flareTimer = 9;
    public AudioClip flareBurningSound;

    // Use this for initialization
    void Start()
    {

        // 코루틴 시작
        StartCoroutine("flareLightoff");

        // 오디오 재생
        if (GetComponent<AudioSource>() != null && flareBurningSound != null)
            GetComponent<AudioSource>().PlayOneShot(flareBurningSound);

        flarelight = GetComponent<Light>();
        flaresound = GetComponent<AudioSource>();
        smokepParSystem = GetComponent<ParticleSystemRenderer>();

        // Bullet(부모)에도 Destroy가 있지만, 여기서 설정한 시간이 더 짧다면 이 시간이 적용됩니다.
        Destroy(gameObject, flareTimer + 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (myCoroutine == true)
        {
            if (flarelight != null)
                flarelight.intensity = Random.Range(2f, 6.0f);
        }
        else
        {
            if (flarelight != null)
            {
                flarelight.intensity = Mathf.Lerp(flarelight.intensity, 0f, Time.deltaTime * smooth);
                flarelight.range = Mathf.Lerp(flarelight.range, 0f, Time.deltaTime * smooth);
            }

            if (flaresound != null)
                flaresound.volume = Mathf.Lerp(flaresound.volume, 0f, Time.deltaTime * smooth);

            if (smokepParSystem != null)
                smokepParSystem.maxParticleSize = Mathf.Lerp(smokepParSystem.maxParticleSize, 0f, Time.deltaTime * 5);
        }
    }

    IEnumerator flareLightoff()
    {
        myCoroutine = true;
        yield return new WaitForSeconds(flareTimer);
        myCoroutine = false;
    }
}