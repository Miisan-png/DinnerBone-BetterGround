using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class Void_Scene_Changer : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private Collider trigger_zone_1;
    [SerializeField] private Collider trigger_zone_2;

    [Header("Player Indicators")]
    [SerializeField] private GameObject player_1_indicator;
    [SerializeField] private GameObject player_2_indicator;

    [Header("Scene Settings")]
    [SerializeField] private string scene_name = "NextScene";
    [SerializeField] private float fade_duration = 1.5f;
    [SerializeField] private float hold_time_before_change = 1f;

    [Header("Player Detection")]
    [SerializeField] private bool use_trigger_zones = true;
    [SerializeField] private Player_Type player_1_type;
    [SerializeField] private Player_Type player_2_type;
    [SerializeField] private float indicator_fade_speed = 0.5f;
    [SerializeField] private float indicator_pulse_scale = 1.2f;
    [SerializeField] private float pulse_duration = 0.8f;

    [Header("Visual Settings")]
    [SerializeField] private AudioSource audio_source;
    [SerializeField] private AudioClip player_enter_sound;
    [SerializeField] private AudioClip both_players_ready_sound;
    [SerializeField] private AudioClip scene_change_sound;

    private bool player_1_in_zone = false;
    private bool player_2_in_zone = false;
    private bool scene_changing = false;

    private Material player_1_material;
    private Material player_2_material;

    private GameObject fade_panel;
    private Canvas fade_canvas;

    private Tween player_1_alpha_tween;
    private Tween player_2_alpha_tween;
    private Tween player_1_pulse_tween;
    private Tween player_2_pulse_tween;

    // NEW: remember original scales so we can restore cleanly
    private Vector3 p1OriginalScale = Vector3.one; // CHANGED / NEW
    private Vector3 p2OriginalScale = Vector3.one; // CHANGED / NEW

    void Start()
    {
        InitializeTriggers();
        InitializeIndicatorMaterials();
        CacheOriginalScales();          // CHANGED / NEW
        SetInitialIndicatorStates();    // CHANGED: now fully hidden + deactivated
        CreateFadePanel();
    }

    private void InitializeTriggers()
    {
        if (trigger_zone_1 != null) trigger_zone_1.isTrigger = true;
        if (trigger_zone_2 != null) trigger_zone_2.isTrigger = true;

        if (audio_source == null)
        {
            audio_source = gameObject.AddComponent<AudioSource>();
        }
    }

    private void InitializeIndicatorMaterials()
    {
        if (player_1_indicator != null)
        {
            var renderer = player_1_indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                player_1_material = new Material(renderer.material);
                renderer.material = player_1_material;
            }
        }

        if (player_2_indicator != null)
        {
            var renderer = player_2_indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                player_2_material = new Material(renderer.material);
                renderer.material = player_2_material;
            }
        }
    }

    private void CacheOriginalScales() // CHANGED / NEW
    {
        if (player_1_indicator != null) p1OriginalScale = player_1_indicator.transform.localScale;
        if (player_2_indicator != null) p2OriginalScale = player_2_indicator.transform.localScale;
    }

    private void CreateFadePanel()
    {
        GameObject canvas_obj = new GameObject("Fade Canvas");
        fade_canvas = canvas_obj.AddComponent<Canvas>();
        fade_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fade_canvas.sortingOrder = 1000;

        canvas_obj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas_obj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        fade_panel = new GameObject("Fade Panel");
        fade_panel.transform.SetParent(canvas_obj.transform);

        var fade_image = fade_panel.AddComponent<UnityEngine.UI.Image>();
        fade_image.color = new Color(0, 0, 0, 0);

        RectTransform rect_transform = fade_panel.GetComponent<RectTransform>();
        rect_transform.anchorMin = Vector2.zero;
        rect_transform.anchorMax = Vector2.one;
        rect_transform.sizeDelta = Vector2.zero;
        rect_transform.anchoredPosition = Vector2.zero;

        canvas_obj.SetActive(false);
    }

    private void SetInitialIndicatorStates() // CHANGED
    {
        // start completely invisible and deactivated
        SetIndicatorAlpha(player_1_material, 0f);
        SetIndicatorAlpha(player_2_material, 0f);

        if (player_1_indicator != null) player_1_indicator.SetActive(false);
        if (player_2_indicator != null) player_2_indicator.SetActive(false);
    }

    private void SetIndicatorAlpha(Material material, float alpha)
    {
        if (material == null) return;

        if (material.HasProperty("_Alpha"))
            material.SetFloat("_Alpha", alpha);
        else if (material.HasProperty("_Opacity"))
            material.SetFloat("_Opacity", alpha);
        else if (material.HasProperty("_Color"))
        {
            Color color = material.GetColor("_Color");
            color.a = alpha;
            material.SetColor("_Color", color);
        }
        else
        {
            Color c = material.color;
            c.a = alpha;
            material.color = c;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player_Controller>();
        if (player == null || scene_changing) return;

        Player_Type player_type = player.Get_Player_Type();

        // NOTE: keeping your existing logic intact
        if (other.GetComponent<Collider>() == trigger_zone_1)
        {
            HandlePlayerEnter(1, player_type);
        }
        else if (other.GetComponent<Collider>() == trigger_zone_2)
        {
            HandlePlayerEnter(2, player_type);
        }
        else
        {
            if ((int)player_type == 0) HandlePlayerEnter(1, player_type);
            else if ((int)player_type == 1) HandlePlayerEnter(2, player_type);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<Player_Controller>();
        if (player == null || scene_changing) return;

        Player_Type player_type = player.Get_Player_Type();

        if (use_trigger_zones)
        {
            if (other.GetComponent<Collider>() == trigger_zone_1) HandlePlayerExit(1);
            else if (other.GetComponent<Collider>() == trigger_zone_2) HandlePlayerExit(2);
        }
        else
        {
            if (player_type.Equals(player_1_type)) HandlePlayerExit(1);
            else if (player_type.Equals(player_2_type)) HandlePlayerExit(2);
        }
    }

    private void HandlePlayerEnter(int zone_id, Player_Type player_type)
    {
        Debug.Log($"Player {player_type} entered zone {zone_id}");

        if (zone_id == 1)
        {
            player_1_in_zone = true;
            AnimateIndicator(player_1_material, player_1_indicator, true, p1OriginalScale); // CHANGED
        }
        else if (zone_id == 2)
        {
            player_2_in_zone = true;
            AnimateIndicator(player_2_material, player_2_indicator, true, p2OriginalScale); // CHANGED
        }

        PlaySound(player_enter_sound);
        CheckBothPlayersReady();
    }

    private void HandlePlayerExit(int zone_id)
    {
        Debug.Log($"Player exited zone {zone_id}");

        if (zone_id == 1)
        {
            player_1_in_zone = false;
            AnimateIndicator(player_1_material, player_1_indicator, false, p1OriginalScale); // CHANGED
        }
        else if (zone_id == 2)
        {
            player_2_in_zone = false;
            AnimateIndicator(player_2_material, player_2_indicator, false, p2OriginalScale); // CHANGED
        }

        if (scene_changing)
        {
            StopAllCoroutines();
            scene_changing = false;
            Debug.Log("Scene change canceled because a player exited a zone.");
            StartCoroutine(FadeFromBlack());
        }
    }

    private IEnumerator FadeFromBlack()
    {
        UnityEngine.UI.Image fade_image = fade_panel.GetComponent<UnityEngine.UI.Image>();
        yield return fade_image.DOFade(0f, fade_duration).SetEase(Ease.OutQuad).WaitForCompletion();
        fade_canvas.gameObject.SetActive(false);
    }

    // CHANGED: show = true -> enable, fade to 1, start pulse
    //           show = false -> fade to 0, stop pulse, deactivate at end
    private void AnimateIndicator(Material material, GameObject indicator, bool show, Vector3 originalScale) // CHANGED
    {
        if (material == null || indicator == null) return;

        // Kill existing tweens
        bool isP1 = (material == player_1_material);
        if (isP1)
        {
            player_1_alpha_tween?.Kill();
            player_1_pulse_tween?.Kill();
        }
        else
        {
            player_2_alpha_tween?.Kill();
            player_2_pulse_tween?.Kill();
        }

        if (show)
        {
            if (!indicator.activeSelf) indicator.SetActive(true); // make visible first
            indicator.transform.localScale = originalScale;       // reset scale

            // ensure starting alpha 0 then fade to 1
            SetIndicatorAlpha(material, 0f);
            Tween alpha_tween = DOTween.To(() => GetCurrentAlpha(material),
                x => SetIndicatorAlpha(material, x), 1f, indicator_fade_speed)
                .SetEase(Ease.OutQuad);

            // pulse
            Tween pulse_tween = indicator.transform
                .DOScale(originalScale * indicator_pulse_scale, pulse_duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            if (isP1)
            {
                player_1_alpha_tween = alpha_tween;
                player_1_pulse_tween = pulse_tween;
            }
            else
            {
                player_2_alpha_tween = alpha_tween;
                player_2_pulse_tween = pulse_tween;
            }
        }
        else
        {
            // fade to 0, stop pulse, shrink back to original scale, then deactivate
            Tween alpha_tween = DOTween.To(() => GetCurrentAlpha(material),
                x => SetIndicatorAlpha(material, x), 0f, indicator_fade_speed)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    indicator.SetActive(false); // fully hidden after fade
                });

            indicator.transform.DOScale(originalScale, indicator_fade_speed * 0.5f)
                .SetEase(Ease.OutQuad);

            if (isP1)
            {
                player_1_alpha_tween = alpha_tween;
                player_1_pulse_tween = null;
            }
            else
            {
                player_2_alpha_tween = alpha_tween;
                player_2_pulse_tween = null;
            }
        }
    }

    private float GetCurrentAlpha(Material material)
    {
        if (material == null) return 0f;

        if (material.HasProperty("_Alpha")) return material.GetFloat("_Alpha");
        else if (material.HasProperty("_Opacity")) return material.GetFloat("_Opacity");
        else if (material.HasProperty("_Color")) return material.GetColor("_Color").a;
        else return material.color.a;
    }

    private void CheckBothPlayersReady()
    {
        if (player_1_in_zone && player_2_in_zone && !scene_changing)
        {
            Debug.Log("Both players ready! Starting scene change...");
            PlaySound(both_players_ready_sound);
            StartCoroutine(ChangeSceneSequence());
        }
    }

    private IEnumerator ChangeSceneSequence()
    {
        scene_changing = true;

        yield return new WaitForSeconds(hold_time_before_change);

        PlaySound(scene_change_sound);

        yield return StartCoroutine(FadeToBlack());

        SceneManager.LoadScene(scene_name);
    }

    private IEnumerator FadeToBlack()
    {
        fade_canvas.gameObject.SetActive(true);
        var fade_image = fade_panel.GetComponent<UnityEngine.UI.Image>();
        yield return fade_image.DOFade(1f, fade_duration).SetEase(Ease.InQuad).WaitForCompletion();
    }

    private void PlaySound(AudioClip clip)
    {
        if (audio_source != null && clip != null)
        {
            audio_source.PlayOneShot(clip);
        }
    }

    void OnDestroy()
    {
        player_1_alpha_tween?.Kill();
        player_2_alpha_tween?.Kill();
        player_1_pulse_tween?.Kill();
        player_2_pulse_tween?.Kill();
    }

    [ContextMenu("Test Scene Change")]
    private void TestSceneChange()
    {
        if (!scene_changing)
        {
            player_1_in_zone = true;
            player_2_in_zone = true;
            CheckBothPlayersReady();
        }
    }

    public bool AreBothPlayersReady() => player_1_in_zone && player_2_in_zone;

    public void ForceSceneChange()
    {
        if (!scene_changing) StartCoroutine(ChangeSceneSequence());
    }
}
