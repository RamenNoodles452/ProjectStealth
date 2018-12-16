using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    #region vars
    public float health;
    public float health_max = 1.0f;

    #region Shield
    private float shield;
    private float shield_max = 100.0f;

    private bool was_hit_this_frame;
    private bool is_regenerating;
    private float SHIELD_REGENERATION_DELAY = 2.0f;
    private float shield_delay_counter = 0.0f;
    private float shield_regeneration_time = 4.0f; // 25% per second
    #endregion

    #region Energy
    private bool is_energy_regenerating;
    private float energy = 100.0f;
    private float energy_max = 100.0f;
    //private float EnergyRegenerationDelay = 0.0f;
    //private float EnergyDelayCounter = 0.0f;
    private float energy_regeneration_time = 4.0f; // 25% per second
    #endregion

    #region Evade
    private bool evade_enqueued = false;
    public float EVADE_COST = 20.0f;
    private Vector2 evade_direction;

    private bool is_evading = false;

    private bool is_evade_winding_up = false;
    private float EVADE_WINDUP_TIME = 0.10f;
    private float evade_windup_counter = 0.0f;

    private bool invincible = false;
    private float INVINCIBILITY_TIME = 0.3f;
    private float invincibility_counter = 0.0f;

    private bool is_evade_recovering = false;
    private float EVADE_RECOVERY_TIME = 0.10f;
    private float evade_recovery_counter = 0.15f;

    //TODO: evade speed in direction, backstep from neutral (2 states here) chord with direction
    // [ ] aerial 4 directional dodge movement (no iframes)
    // [X] sliders for total time, windup time, i frames (not for aerial), recovery time / skip for enqueued windup, speed in pixels per second.
    // [X] disable normal movement when dodging
    // [ ] don't collide with enemies while dodging
    #endregion

    #region Cloak
    private bool is_cloaked = false;
    private const float CLOAK_COST = 35.0f;
    private const float CLOAK_DRAIN_PER_SECOND = 6.5f; //10s
    //TODO: may need to put a CD on cloak to prevent spamming if regen makes even huge cost spammable.
    // Just change input mapping to require a couple seconds of sneaking in place.
    #endregion

    #region Silencer
    // Not sure if this should be consolidated with stealth / UI needs simplification here....
    // Potentially concerning
    private float silencer = 0.0f;
    private float silencer_max = 3.0f;
    private float silencer_regen = 1.0f;
    #endregion

    #region Shoot
    public GameObject charge_up_rays;
    public GameObject charge_up_ball;

    private bool is_shooting = false;
    private float shoot_charge_timer = 0.0f;
    private const float SHOOT_FULL_CHARGE_TIME = 2.0f; // seconds

    private GameObject aim_enemy_memory;
    private GameObject aim_auto_target; // closest enemy in facing that is targetable
    private Vector2 aim_auto_reticle_position;
    private Vector2 aim_manual_reticle_position;
    private GameObject aim_reticle;
    #endregion

    private bool is_in_shadow = false;

    [SerializeField]
    private bool is_adrenal_rushing = false;
    private bool is_adrenaline_fading_in = false;
    private bool is_adrenaline_fading_out = false;
    private const float ADRENALINE_FADE_DURATION =  0.5f; // seconds to transition (ignores time scaling)
    private const float ADRENAL_RUSH_DURATION    =  3.0f; // seconds, duration will be ~2x as long
    private const float ADRENAL_RUSH_COOLDOWN    = 15.0f; // seconds
    private const float ADRENALINE_DESATURATION  = 0.75f; // percent desaturation
    private float adrenal_rush_timer = ADRENAL_RUSH_COOLDOWN;
    private float adrenal_fade_timer = 0.0f;

    // progress values
    public  bool acquired_mag_grip;
    public  bool acquired_adrenal_rush;
    private bool acquired_hookshot;
    private bool acquired_hack;
    private bool acquired_explosive;
    private bool acquired_charge_shot;

    // checkpointing
    public Vector2 checkpoint;

    // Noise Prefab: Set in Editor
    public GameObject noise_prefab;
    public GameObject bullet_prefab;
    public GameObject charged_bullet_prefab;

    private CharacterStats char_stats;
    private CharacterAnimationLogic char_anims;
    IInputManager input_manager;
    private float walk_animation_timer;

    private bool is_idle  = true;
    private bool was_idle = true;
    #endregion

    #region stat accessors
    // I don't like having accessors phony encapsulation, but we'll keep things together

    /// <returns>The player's current amount of shields</returns>
    public float GetShields()
    {
        return shield;
    }

    /// <returns>The player's maximum amount of shields</returns>
    public float GetShieldsMax()
    {
        return shield_max;
    }

    /// <returns>The player's current amount of energy</returns>
    public float GetEnergy()
    {
        return energy;
    }

    /// <returns>The player's maximum amount of energy</returns>
    public float GetEnergyMax()
    {
        return energy_max;
    }

    /// <returns>How full the adrenaline charge is</returns>
    public float PercentAdrenalineCharge
    {
        get
        {
            if ( ! is_adrenal_rushing ) { return adrenal_rush_timer / ADRENAL_RUSH_COOLDOWN; }
            else { return 1.0f - adrenal_rush_timer / ADRENAL_RUSH_DURATION; }
        }
    }
    #endregion

    /// <summary>
    /// Sets the animation timer for noise generation when the player starts walking
    /// </summary>
    public void StartWalking()
    {
        walk_animation_timer = 0.15f;
    }

    /// <summary>
    /// Returns true if the player is in the idle animation.
    /// </summary>
    public bool IsIdle
    {
        get { return is_idle; }
    }

    /// <summary>
    /// Returns true if the player became idle this frame.
    /// </summary>
    public bool BecameIdleThisFrame
    {
        get { return is_idle && !was_idle; }
    }

    /// <summary>
    /// Sets location to respawn at
    /// </summary>
    /// <param name="coordinates">coordinates</param>
    public void SetCheckpoint( Vector2 coordinates )
    {
        checkpoint = coordinates;
        Debug.Log( "Checkpoint!: " + coordinates );
    }

    /// <summary>
    /// Does damage to the player's health / shields
    /// </summary>
    /// <param name="damage">the amount of damage</param>
    public void Hit( float damage )
    {
        if ( invincible )
        {
            return;
        }

        // Interrupt shield recharge
        was_hit_this_frame = true;

        // Deal damage
        if ( shield > 0.0f )
        {
            Referencer.instance.hud_ui.OnHit();
            shield = Mathf.Max( shield - damage, 0.0f );
            if ( shield <= 0.0f )
            {
                // shield break
                Referencer.instance.hud_ui.OnShieldBreak();
            }
        }
        else
        {
            health = Mathf.Max( health - damage, 0.0f );
            if ( health <= 0.0f )
            {
                // kill
                Respawn(); //TODO: put an ani and delay on this
            }
        }
    }

    /// <summary>
    /// Respawns the player at the last checkpoint. Also, resets the level.
    /// </summary>
    public void Respawn()
    {
        // TODO: reach into gamestate and force reset enemies.

        // Teleport to checkpoint
        // value type
        /*if ( checkpoint == null )
        {
            // find and set a default checkpoint?
            Debug.LogError( "Player died without a checkpoint set!" );
        }
        else*/
        //{
        this.gameObject.transform.position = new Vector3( checkpoint.x, checkpoint.y, this.gameObject.transform.position.z );
        Camera.main.GetComponent<CameraMovement>().SnapToFocalPoint();
        //}

        // Reset stats
        ResetState();
    }

    /// <summary>
    /// Reset state to prevent bugs when changing levels / respawning (may need to split into 2)
    /// Also refill resources
    /// </summary>
    private void ResetState()
    {
        evade_enqueued = false;
        invincible = false;
        is_cloaked = false;
        was_hit_this_frame = false;
        CleanupAdrenalineChanges();

        health = health_max;
        shield = shield_max;

        // TODO: interrupt everything, stop animations, reset all that
        //reset state from climbing, etc.
        if ( Referencer.instance.player.GetComponent<CharacterStats>().current_master_state == CharEnums.MasterState.ClimbState )
        {
            Referencer.instance.player.GetComponent<MagGripUpgrade>().StopClimbing();
        }
        if ( Referencer.instance.player.GetComponent<CharacterStats>().current_master_state == CharEnums.MasterState.RappelState )
        {
            Referencer.instance.player.GetComponent<GrapplingHook>().ResetState();
        }

        input_manager = GetComponent<IInputManager>();
        char_anims = this.gameObject.GetComponent<CharacterAnimationLogic>();
        char_anims.Reset();
        // reset movement
        char_stats = this.gameObject.GetComponent<CharacterStats>();
        char_stats.velocity = new Vector2( 0.0f, 0.0f );
    }

    /// <summary>
    /// Accessor for if the player is in shadow or light.
    /// </summary>
    public bool IsInShadow
    {
        get { return is_in_shadow; }
        set { is_in_shadow = value; }
    }

    /// <summary>
    /// Accessor for shield regeneration
    /// </summary>
    /// <returns>True if the player is regenerating shields</returns>
    public bool IsRegenerating
    {
        get { return is_regenerating; }
    }

    /// <summary>
    /// Accessor for the time it takes after being hit for shield regeneration to begin.
    /// </summary>
    public float RegenerationDelay
    {
        get { return SHIELD_REGENERATION_DELAY; }
    }

    /// <summary>
    /// Accessor for whether the player is in adrenaline rush mode (slows time, "infinite stamina")
    /// </summary>
    public bool IsAdrenalRushing
    {
        get { return is_adrenal_rushing; }
    }

    /// <summary>
    /// Begins charging weapon in preparation to shoot.
    /// </summary>
    public void StartShoot()
    {
        is_shooting = true;
        shoot_charge_timer = 0.0f;

        ParticleSystem ray_particles = charge_up_rays.GetComponent<ParticleSystem>();
        ParticleSystem ball_particles = charge_up_ball.GetComponent<ParticleSystem>();
        ray_particles.Stop();
        ray_particles.Play();
        ball_particles.Stop();
        ball_particles.Play();
    }

    /// <summary>
    /// Fire weapon
    /// </summary>
    public void Shoot()
    {
        if ( ! is_shooting ) { return; }
        if ( is_evading ) { return; } // no shooting mid-evade
        //animation lock checks?


        bool is_fully_charged = shoot_charge_timer >= SHOOT_FULL_CHARGE_TIME;
        //make noise?
        //use silencer meter / shooting when cloaked makes no noise
        if ( silencer >= 1.0f && ! is_fully_charged )
        {
            // Suppressed shot
            silencer -= 1.0f;
        }
        else
        {
            // Go loud
            GameObject noise_obj = GameObject.Instantiate( noise_prefab, this.gameObject.transform.position, Quaternion.identity );
            Noise noise = noise_obj.GetComponent<Noise>();
            noise.lifetime = 0.25f; // seconds
            noise.radius = 200.0f;
            // TODO: if ( is_fully_charged ) { } // custom noise?
        }

        if ( is_cloaked ) { is_cloaked = false; } // attacking breaks stealth (even silenced?)

        // Actually fire bullets
        // Start with closest tagged enemies (if any)?

        // spawn bullet prefab, set appropriate charge level.
        Vector2 origin =  GetShotOrigin();
        GameObject bullet_obj = null;

        if ( ! is_fully_charged )
        {
            // Normal bullet
            bullet_obj = Instantiate( bullet_prefab, new Vector3( origin.x, origin.y, transform.position.z ), Quaternion.identity );
            BulletRay bullet = bullet_obj.GetComponent<BulletRay>();
            bullet.is_player_owned = true;
            bullet.angle = Mathf.Atan2( aim_auto_reticle_position.y - origin.y, aim_auto_reticle_position.x - origin.x );
            bullet.damage = 50.0f;
        }
        else
        {
            Camera.main.GetComponent<CameraMovement>().ShakeScreen( 10.0f, 0.5f );
            // Charged bullet
            bullet_obj = Instantiate( charged_bullet_prefab, new Vector3( origin.x, origin.y, transform.position.z ), Quaternion.identity );
            BulletChargedRay bullet = bullet_obj.GetComponent<BulletChargedRay>();
            // damage must be > fire rate * uncharged damage.
            bullet.Setup( origin, aim_auto_reticle_position, 500.0f, true );
        }

        // Reset state.
        is_shooting = false;
        ParticleSystem ray_particles = charge_up_rays.GetComponent<ParticleSystem>();
        ParticleSystem ball_particles = charge_up_ball.GetComponent<ParticleSystem>();
        ray_particles.Stop();
        ball_particles.Stop();
    }

    /// <summary>
    /// Light, fast attack combo
    /// </summary>
    public void Attack()
    {
        if ( is_evading ) { return; } // no attacking mid-evade
        //animation lock checks?

        if ( is_cloaked ) { is_cloaked = false; } // attacking breaks stealth

        //enqueueing? + comboing
        //tag enemies for auto aim
    }

    /// <summary>
    /// Heavy attack / insta kill
    /// </summary>
    public void Assassinate()
    {
        if ( is_evading ) { return; } // no assassinating mid-evade
        //animation lock checks?

        //need to be positioned
        //look at enemy positions
        if ( is_cloaked ) { is_cloaked = false; } // attacking breaks stealth

        //enqueueing?
    }

    /// <summary>
    /// Activates super mode, granting infinite stamina and time dilation.
    /// </summary>
    public void AdrenalRush()
    {
        if ( ! is_adrenal_rushing )
        {
            if ( adrenal_rush_timer >= ADRENAL_RUSH_COOLDOWN )
            {
                is_adrenal_rushing = true;
                adrenal_rush_timer = 0.0f;
                energy = energy_max;
                is_adrenaline_fading_in = true;
                adrenal_fade_timer = 0.0f;
            }
            else
            {
                Referencer.instance.hud_ui.InsufficientAdrenaline();
            }
        }
    }

    /// <summary>
    /// Evades
    /// </summary>
    public void Evade()
    {
        if ( is_evading )
        {
            // enqueue if within grace period
            if ( is_evade_recovering )
            {
                evade_enqueued = true;
            }
            else if ( invincible )
            {
                if ( INVINCIBILITY_TIME - invincibility_counter <= 0.1f ) { evade_enqueued = true; }
            }

            return;
        }

        if ( energy < EVADE_COST && ! is_adrenal_rushing )
        {
            Referencer.instance.hud_ui.InsuffienctStamina();
            return;
        } // insufficient resources. play sound?

        // check if stuck in a non-cancellable animation

        is_evading = true;
        is_evade_winding_up = true;
        evade_windup_counter = 0.0f;
        invincible = false;
        invincibility_counter = 0.0f;
        is_evade_recovering = false;
        evade_recovery_counter = 0.0f;

        // direction
        Vector2 input_direction = new Vector2( input_manager.HorizontalAxis, input_manager.VerticalAxis );
        if ( char_stats.IsGrounded )
        {
            if ( input_direction.x == 0.0f ) // backstep
            {
                evade_direction = new Vector2( -1.0f * char_stats.GetFacingXComponent(), 0.0f );
            }
            else if ( input_direction.x > 0.0f )
            {
                evade_direction = Vector2.right;
            }
            else if ( input_direction.x < 0.0f )
            {
                evade_direction = Vector2.left;
            }
        }
        else if ( char_stats.IsInMidair )
        {
            if ( input_direction == Vector2.zero )
            {
                evade_direction = new Vector2( char_stats.GetFacingXComponent(), 0.0f );
            }
            else if ( Mathf.Abs( input_direction.y ) >= Mathf.Abs( input_direction.x ) )
            {
                if ( input_direction.y > 0.0f ) { evade_direction = Vector2.up; }
                else { evade_direction = Vector2.down; }
            }
            else
            {
                if ( input_direction.x > 0.0f ) { evade_direction = Vector2.right; }
                else { evade_direction = Vector2.left; }
            }
        }

        if ( ! is_adrenal_rushing ) { energy -= EVADE_COST; }

        // animate
        if ( char_stats.IsGrounded )
        {
            char_anims.DodgeRollTrigger();
        }
        else
        {
            char_anims.DodgeRollAerialTrigger();
        }

        //return true/false? based on abort / already evading / stuck in recovery / resource insuffiency / success
    }

    /// <returns>Whether the player is evading or not</returns>
    public bool IsEvading()
    {
        return is_evading;
    }

    /// <summary>
    /// Moves the player during evasion.
    /// </summary>
    private void EvasiveAction()
    {
        float speed = 360.0f; // pixels / second
        if ( char_stats.IsInMidair ) { speed = 360.0f; }

        float scalar = speed * Time.deltaTime * Time.timeScale;
        GetComponent<SimpleCharacterCore>().MoveWithCollision( new Vector3( scalar * evade_direction.x, scalar * evade_direction.y, 0.0f ) );
    }

    #region Cloak
    /// <summary>
    /// Turn on cloaking
    /// </summary>
    public void Cloak()
    {
        if ( is_cloaked ) { return; } // Decloak?

        // check if unlocked
        // animation lock checks?

        if ( energy >= CLOAK_COST )
        {
            energy = energy - CLOAK_COST;
        }
        else
        {
            return;
        }

        is_cloaked = true;
        //animate
    }

    /// <summary>
    /// Turn off cloaking
    /// </summary>
    public void Decloak()
    {
        is_cloaked = false;
        //animate
    }

    /// <returns>Whether the player is currently cloaked or not</returns>
    public bool IsCloaking() { return is_cloaked; }
    #endregion

    /// <summary>
    /// Begins dodging invincibility frames
    /// </summary>
    private void StartIFrames()
    {
        invincible = true;
        invincibility_counter = 0.0f;
    }

    /// <summary>
    /// Restores adrenaline rush mode changes (time scale and camera desaturation) to normal.
    /// </summary>
    private void CleanupAdrenalineChanges()
    {
        is_adrenal_rushing = false;
        is_adrenaline_fading_in = false;
        is_adrenaline_fading_out = false;
        adrenal_rush_timer = 0.0f;
        Time.timeScale = 1.0f;
        Camera.main.GetComponent<RenderEffects>().desaturation = 0.0f;
    }

    /// <summary>
    /// Automatically targets an enemy.
    /// </summary>
    private void AutoTarget()
    {
        const float MAX_RANGE = 640.0f; //640 px, 20 tiles
        Vector2 origin = GetShotOrigin();

        // If you fired at an enemy, stay locked on (if in range).
        if ( aim_enemy_memory != null )
        {
            aim_auto_reticle_position = new Vector2( aim_enemy_memory.transform.position.x, aim_enemy_memory.transform.position.y );
            // TODO: LOS check?
            if ( Vector2.Distance( origin, aim_auto_reticle_position ) <= MAX_RANGE )
            {
                aim_reticle.transform.position = new Vector3( aim_auto_reticle_position.x, aim_auto_reticle_position.y, aim_reticle.transform.position.z );
                return;
            }
        }

        // Find the closest enemy in your facing, within range, with a clear shot to them.
        float minimum_distance = MAX_RANGE;
        foreach ( GameObject enemy in Referencer.instance.enemies )
        {
            // Is enemy in direction you are facing?
            if ( char_stats.facing_direction == CharEnums.FacingDirection.Left )
            {
                if ( enemy.transform.position.x > transform.position.x ) { continue; }
            }
            else
            {
                if ( enemy.transform.position.x < transform.position.x ) { continue; }
            }

            // Is enemy in range?
            Vector2 direction = new Vector2( enemy.transform.position.x - origin.x, enemy.transform.position.y - origin.y );
            float distance_to_enemy = direction.magnitude;
            if ( distance_to_enemy >= minimum_distance )
            {
                continue;
            }

            // Is there a clear shot?
            RaycastHit2D hit = Physics2D.Raycast( origin, direction, distance_to_enemy + 1.0f, CollisionMasks.player_shooting_mask );
            if ( hit.collider == null ) { continue; }

            if ( Utils.IsEnemyCollider( hit.collider ) )
            {
                aim_auto_reticle_position = new Vector2( enemy.transform.position.x, enemy.transform.position.y );
                minimum_distance = distance_to_enemy;
            }
        }

        // Fire straight ahead! / manual mode
        if ( minimum_distance == MAX_RANGE )
        {
            RaycastHit2D hit = Physics2D.Raycast( origin, GetFacingVector(), MAX_RANGE, CollisionMasks.player_shooting_mask );
            if ( hit.collider == null ) { aim_auto_reticle_position = new Vector2( transform.position.x, transform.position.y ) + MAX_RANGE * GetFacingVector(); }
            else
            {
                aim_auto_reticle_position = hit.point;
            }
        }

        aim_reticle.transform.position = new Vector3( aim_auto_reticle_position.x, aim_auto_reticle_position.y, aim_reticle.transform.position.z );
    }



    private void EngageFreeAim()
    {

    }

    /// <summary>
    /// Gets the origin point for bullets fired from the player's weapon
    /// </summary>
    /// <returns>The origin point.</returns>
    private Vector2 GetShotOrigin()
    {
        return new Vector2( transform.position.x, transform.position.y ) + char_stats.STANDING_COLLIDER_SIZE.x / 2.0f * GetFacingVector();
    }

    /// <summary>
    /// Gets the vector for the player's facing.
    /// </summary>
    /// <returns>The vector pointing left or right.</returns>
    private Vector2 GetFacingVector()
    {
        if ( char_stats.facing_direction == CharEnums.FacingDirection.Left )
        {
            return new Vector2( -1.0f, 0.0f );
        }
        else
        {
            return new Vector2( 1.0f, 0.0f );
        }
    }

    /// <summary>
    /// Early initialization
    /// </summary>
    private void Awake()
    {
        aim_reticle = transform.Find( "Reticle" ).gameObject;
    }

    /// <summary>
    /// Initialization upon level entry
    /// </summary>
    void Start()
    {
        ResetState();
    }

    /// <summary>
    /// Timer stuff that gets checked every frame
    /// </summary>
    void Update()
    {
        // Death
        if ( health <= 0.0f )
        {
            Respawn(); //TODO: if this takes >1 frame, need state tracking.
        }

        #region timers
        #region shield
        // Shield regeneration
        if ( was_hit_this_frame )
        {
            was_hit_this_frame = false;
            is_regenerating = false;
            shield_delay_counter = 0.0f;
        }

        if ( is_regenerating )
        {
            // Regenerate to full
            shield = Mathf.Min( shield + ( shield_max / shield_regeneration_time ) * Time.deltaTime * Time.timeScale, shield_max );
            if ( shield == shield_max )
            {
                is_regenerating = false;
            }
        }
        else if ( shield < shield_max )
        {
            // Delay before regen begins
            shield_delay_counter += Time.deltaTime * Time.timeScale;
            if ( shield_delay_counter >= SHIELD_REGENERATION_DELAY )
            {
                is_regenerating = true;
                // play recharge sound?
            }
        }
        #endregion

        #region Evade
        if ( is_evade_winding_up )
        {
            //Debug.Log( "Evade windup" );
            evade_windup_counter += Time.deltaTime * Time.timeScale;
            if ( evade_windup_counter >= EVADE_WINDUP_TIME )
            {
                is_evade_winding_up = false;
                StartIFrames();
                char_stats.current_master_state = CharEnums.MasterState.EvadeState;
            }
        }

        // I frames
        if ( invincible )
        {
            //Debug.Log("Evade");
            invincibility_counter += Time.deltaTime * Time.timeScale;
            if ( invincibility_counter >= INVINCIBILITY_TIME )
            {
                invincible = false;
                is_evade_recovering = true;
                char_stats.current_master_state = CharEnums.MasterState.DefaultState;
            }
        }

        if ( is_evade_recovering )
        {
            //Debug.Log("Evade recovery");
            evade_recovery_counter += Time.deltaTime * Time.timeScale;
            if ( evade_recovery_counter >= EVADE_RECOVERY_TIME )
            {
                is_evade_recovering = false;
                is_evading = false;

                if ( evade_enqueued )
                {
                    evade_enqueued = false;
                    Evade();
                }
            }
        }

        if ( IsEvading() && ! is_evade_winding_up )
        {
            EvasiveAction();
        }
        #endregion

        #region Cloaking
        if ( is_cloaked )
        {
            energy = Mathf.Max( energy - CLOAK_DRAIN_PER_SECOND * Time.deltaTime * Time.timeScale, 0.0f );
            if ( energy <= 0.0f )
            {
                is_cloaked = false;
            }
        }
        #endregion

        #region Silencer
        silencer = Mathf.Min( silencer + silencer_regen * Time.deltaTime * Time.timeScale, silencer_max );
        #endregion

        #region Energy
        is_energy_regenerating = true;

        if ( is_cloaked || is_evading ) { is_energy_regenerating = false; }
        //if ( IsShooting || IsAttacking || IsAssassinating ) { IsEnergyRegenerating = false; }

        if ( is_energy_regenerating )
        {
            energy = Mathf.Min( energy + ( energy_max / energy_regeneration_time ) * Time.deltaTime * Time.timeScale, energy_max );
        }
        #endregion

        #region Adrenaline
        if ( is_adrenal_rushing )
        {
            adrenal_rush_timer += Time.deltaTime * Time.timeScale;
            if ( adrenal_rush_timer > ADRENAL_RUSH_DURATION )
            {
                is_adrenal_rushing = false;
                adrenal_rush_timer = 0.0f;
                is_adrenaline_fading_out = true;
                adrenal_fade_timer = 0.0f;
            }
        }
        else
        {
            if ( adrenal_rush_timer < ADRENAL_RUSH_COOLDOWN )
            {
                adrenal_rush_timer += Time.deltaTime * Time.timeScale;
            }
        }

        if ( is_adrenaline_fading_in )
        {
            adrenal_fade_timer += Time.deltaTime;
            float t = adrenal_fade_timer / ADRENALINE_FADE_DURATION;
            Time.timeScale = 1.0f - Mathf.Min( 0.5f * t, 0.5f );
            Camera.main.GetComponent<RenderEffects>().desaturation = Mathf.Min( t * ADRENALINE_DESATURATION, 1.0f );
            if ( adrenal_fade_timer >= ADRENALINE_FADE_DURATION )
            {
                is_adrenaline_fading_in = false;
                Time.timeScale = 0.5f;
            }
        }

        if ( is_adrenaline_fading_out )
        {
            adrenal_fade_timer += Time.deltaTime;
            float t = (1.0f - adrenal_fade_timer / ADRENALINE_FADE_DURATION);
            Time.timeScale = 1.0f - Mathf.Min( 0.5f * t, 0.5f );
            Camera.main.GetComponent<RenderEffects>().desaturation = Mathf.Min( t * ADRENALINE_DESATURATION, 1.0f );
            if ( adrenal_fade_timer >= ADRENALINE_FADE_DURATION )
            {
                CleanupAdrenalineChanges();
            }
        }
        #endregion

        #region Walking
        if ( char_stats.IsGrounded &&
            ( char_stats.current_move_state == CharEnums.MoveState.IsWalking || char_stats.current_move_state == CharEnums.MoveState.IsRunning ) )
        {
            walk_animation_timer += Time.deltaTime; // t_scale SHOULD be respected? But also need to update animation to play slower.
            if ( walk_animation_timer >= 0.35f )
            {
                walk_animation_timer -= 0.35f;
                // make noise
                GameObject noise_obj = GameObject.Instantiate( noise_prefab, this.gameObject.transform.position + new Vector3( 0.0f, -20.0f, 0.0f ), Quaternion.identity );
                Noise noise = noise_obj.GetComponent<Noise>();
                noise.lifetime = 0.2f; // seconds
                if ( char_stats.current_move_state == CharEnums.MoveState.IsWalking )
                {
                    noise.radius = 25.0f;
                }
                else if ( char_stats.current_move_state == CharEnums.MoveState.IsRunning )
                {
                    noise.radius = 50.0f;
                }
            }
        }
        #endregion
        #endregion

        #region Shooting
        if ( is_shooting )
        {
            shoot_charge_timer = Mathf.Min( shoot_charge_timer + Time.deltaTime * Time.timeScale, SHOOT_FULL_CHARGE_TIME );
        }
        #endregion

        AutoTarget();

        // Idle check
        was_idle = is_idle;
        is_idle = char_anims.animator.GetCurrentAnimatorStateInfo( 0 ).IsName( "valerie_idle" );
    }

}
