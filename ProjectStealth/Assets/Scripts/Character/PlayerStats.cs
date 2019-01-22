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

    private bool is_invincible = false;
    private float INVINCIBILITY_TIME = 0.3f;
    private float invincibility_counter = 0.0f;

    private bool is_evade_recovering = false;
    private float EVADE_RECOVERY_TIME = 0.10f;
    private float evade_recovery_counter = 0.15f;
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
    [SerializeField]
    private GameObject charge_up_rays;
    [SerializeField]
    private GameObject charge_up_ball;
    [SerializeField]
    private GameObject bullet_prefab;
    [SerializeField]
    private GameObject charged_bullet_prefab;
    [SerializeField]
    private GameObject charged_bullet_impact_prefab;

    private bool is_shooting = false;
    private float shoot_charge_timer = 0.0f;
    private const float SHOOT_FULL_CHARGE_TIME = 2.0f; // seconds

    private CharacterOverlay overlay;

    private GameObject aim_enemy_memory;
    private GameObject aim_auto_target; // closest enemy in facing that is targetable
    private Vector2 aim_auto_reticle_position;
    private Vector2 aim_manual_reticle_position;
    private GameObject aim_reticle;
    #endregion

    private bool is_in_shadow = false;

    private float freeze_timer = 0.0f; // seconds

    #region adrenaline
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
    #endregion

    #region air dash
    public  bool  can_air_hang = true;
    private bool  is_air_hang_enqueued;
    private float air_hang_timer;
    private const float AIR_HANG_DURATION = 1.0f;   // seconds

    private bool is_air_dash_enqueued;
    private Vector2 queued_air_dash_direction;
    public uint air_dash_count;
    private float air_dash_angle;
    private float air_dash_timer;
    private const float AIR_DASH_DURATION = 0.3f;   // seconds
    private const float AIR_DASH_SPEED    = 320.0f; // pixels per second
    private const float AIR_DASH_COST     = 25.0f;
    private const float AIR_DASH_INVINCIBILITY_TIME = 0.3f; // seconds
    #endregion

    #region sprint
    private float sprint_cost = 6.67f; // energy per second
    #endregion

    #region progression
    // progress values
    public bool acquired_mag_grip;
    public bool acquired_ceiling_grip;
    public bool acquired_adrenal_rush;
    public bool acquired_hookshot;
    public bool acquired_hack;
    public bool acquired_explosive;
    public bool acquired_charge_shot;
    public bool acquired_emp;
    public bool acquired_jetboost;
    #endregion

    public GadgetEnum gadget;

    // checkpointing
    public Vector2 checkpoint;

    // Noise Prefab: Set in Editor
    public GameObject noise_prefab;

    private CharacterStats char_stats;
    private CharacterAnimationLogic char_anims;
    private IInputManager input_manager;
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

    /// <summary>Takes away energy</summary>
    /// <params name="energy_to_spend">The amount to take away</params>
    public void SpendEnergy( float energy_to_spend )
    {
        energy = Mathf.Max( 0.0f, energy - energy_to_spend );
    }

    /// <summary>
    /// Gets the energy cost to start sprinting.
    /// </summary>
    public float GetSprintStartupCost
    {
        get { return 5.0f; }
    }

    /// <summary>
    /// Gets the energy cost per second to sprint.
    /// </summary>
    public float GetSprintCost
    {
        get { return sprint_cost; }
    }

    /// <summary>
    /// Determines whether the player is currently sprinting or not.
    /// </summary>
    public bool IsSprinting
    {
        get { return char_stats.current_move_state == CharEnums.MoveState.IsRunning && char_stats.current_master_state == CharEnums.MasterState.DefaultState; }
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
        if ( is_invincible )
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

        // Overlay
        if ( overlay == null ) { return; }
        overlay.StartHurtBlink();
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
        is_evading = false;
        is_invincible = false;
        is_cloaked = false;
        was_hit_this_frame = false;
        can_air_hang = true;
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
        UnfreezePlayer();
        char_anims = this.gameObject.GetComponent<CharacterAnimationLogic>();
        char_anims.Reset();
        // hide charged shot
        Animator charge_animator = charge_up_ball.GetComponent<Animator>();
        if ( charge_animator != null )
        {
            if ( charge_animator.isActiveAndEnabled )
            {
                charge_animator.SetBool( "Charging", false );
                charge_animator.SetBool( "Charged",  false );
                charge_up_ball.SetActive( false );
            }
        }

        // reset movement
        char_stats = this.gameObject.GetComponent<CharacterStats>();
        char_stats.velocity = new Vector2( 0.0f, 0.0f );
        char_stats.current_master_state = CharEnums.MasterState.DefaultState;

        // reset overlay
        if ( overlay != null )
        {
            overlay.HideOverlay();
        }
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
        // Releasing fire button in a non-firable state can lock you in to the charged shooting state. 
        // So, you can start firing, go into a non-firing state WHILE fully charging, then release to "multitask" charge.
        // This is a stopgap to keep things from getting wonky then.
        if ( is_shooting ) { return; }

        is_shooting = true;
        shoot_charge_timer = 0.0f;

        if ( acquired_charge_shot )
        {
            ParticleSystem ray_particles = charge_up_rays.GetComponent<ParticleSystem>();
            ray_particles.Stop();
            ray_particles.Play();

            charge_up_ball.SetActive( true );
            Animator charge_animator = charge_up_ball.GetComponent<Animator>();
            charge_animator.SetBool( "Charging", true );
            charge_animator.SetBool( "Charged", false );
        }
    }

    /// <summary>
    /// Fire weapon
    /// </summary>
    public void Shoot()
    {
        if ( ! is_shooting ) { return; }
        if ( is_evading ) { return; } // no shooting mid-evade
        //animation lock checks?


        bool is_fully_charged = ( shoot_charge_timer >= SHOOT_FULL_CHARGE_TIME ) && acquired_charge_shot;
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

            // Impact graphic
            GameObject impact_obj = Instantiate( charged_bullet_impact_prefab, new Vector3( aim_auto_reticle_position.x, aim_auto_reticle_position.y, charged_bullet_impact_prefab.transform.position.z ), Quaternion.identity );

            // freeze player for 0.5 seconds
            FreezePlayer( 0.5f );
            
            // stop showing the fully charged overlay
            if ( overlay != null )
            {
                overlay.HideOverlay();
            }
        }

        // Reset state.
        is_shooting = false;
        ParticleSystem ray_particles = charge_up_rays.GetComponent<ParticleSystem>();
        ray_particles.Stop();

        Animator charge_animator = charge_up_ball.GetComponent<Animator>();
        charge_animator.SetBool( "Charging", false );
        charge_animator.SetBool( "Charged", false );
        charge_up_ball.SetActive( false ); // hide.
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
    /// Freezes in the air momentarily. Can only be used once per jump / airdash.
    /// </summary>
    public void AirHang()
    {
        if ( ! can_air_hang ) { return; }

        air_hang_timer = AIR_HANG_DURATION;
        char_stats.velocity.x = 0.0f; //?
        char_stats.velocity.y = 0.0f;
        char_stats.current_master_state = CharEnums.MasterState.AirHang;
        can_air_hang = false;
        is_air_hang_enqueued = false;
    }

    /// <summary>
    /// Dashes through the air
    /// </summary>
    /// <returns>True if the air dash was executed, false otherwise.</returns>
    public bool AirDash( Vector2 input_direction )
    {
        is_air_dash_enqueued = false;

        // Need jetboost to do sequential dashes.
        if ( air_dash_count > 0 && ! acquired_jetboost ) { return false; }

        // Energy Cost
        if ( ! is_adrenal_rushing && air_dash_count > 0 )
        {
            if ( GetEnergy() < AIR_DASH_COST )
            {
                Referencer.instance.hud_ui.InsuffienctStamina();
                return false;
            }
            else
            {
                energy -= AIR_DASH_COST;
            }
        }

        // Map input to an 8-directional input.
        float directions = 8.0f;
        input_direction.Normalize();
        float angle = Mathf.Atan2( input_direction.y, input_direction.x ) * Mathf.Rad2Deg;
        angle += 360.0f / ( directions * 2.0f ); // 22.5
        angle = angle % 360.0f;
        while ( angle < 0.0f ) { angle += 360.0f; }
        angle = Mathf.Floor( angle / ( 360.0f / directions ) ) * ( 360.0f / directions ); // closest 45 degrees.

        air_dash_angle = angle;
        if ( acquired_jetboost ) { can_air_hang = true; } // reset air hang, to allow it to be used again.
        air_dash_count++;
        air_dash_timer = AIR_DASH_DURATION;
        char_stats.velocity = new Vector2( 0.0f, 0.0f );
        char_stats.current_master_state = CharEnums.MasterState.AirDash;
        StartIFrames();

        // TODO: animate.
        return true;
    }

    /// <summary>
    /// Determines whether or not the player is performing an air hang
    /// </summary>
    public bool IsAirHanging
    {
        get { return char_stats.current_master_state == CharEnums.MasterState.AirHang; }
    }

    /// <summary>
    /// Determines whether or not the player is performing an air dash
    /// </summary>
    public bool IsAirDashing
    {
        get { return char_stats.current_master_state == CharEnums.MasterState.AirDash; }
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
            else if ( is_invincible )
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

        // direction
        Vector2 input_direction = new Vector2( input_manager.HorizontalAxis, input_manager.VerticalAxis );
        if ( char_stats.IsGrounded )
        {
            is_evading = true;
            is_evade_winding_up = true;
            evade_windup_counter = 0.0f;
            is_invincible = false;
            invincibility_counter = 0.0f;
            is_evade_recovering = false;
            evade_recovery_counter = 0.0f;

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

            if ( ! is_adrenal_rushing ) { energy -= EVADE_COST; }
        }
        else if ( char_stats.IsInMidair )
        {
            AirHang();
            if ( input_direction.x == 0.0f && input_direction.y == 0.0f )
            {
                input_direction = new Vector2( -1.0f * char_stats.GetFacingXComponent(), 0.0f );
            }
            AirDash( input_direction );
        }

        // animate
        if ( char_stats.IsGrounded )
        {
            char_anims.DodgeRollTrigger();
        }
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
        is_invincible = true;
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
        if ( input_manager.IsManualAimOn )
        {
            EngageFreeAim();
            return;
        }

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
            if ( hit.collider == null ) { aim_auto_reticle_position = origin + MAX_RANGE * GetFacingVector(); }
            else
            {
                aim_auto_reticle_position = hit.point;
            }
        }

        aim_reticle.transform.position = new Vector3( aim_auto_reticle_position.x, aim_auto_reticle_position.y, aim_reticle.transform.position.z );
    }

    /// <summary>
    /// Manually targets a location to shoot.
    /// </summary>
    private void EngageFreeAim()
    {
        const float MAX_RANGE = 640.0f; //640 px, 20 tiles //TODO: refactor
        Vector2 origin = GetShotOrigin();
        Vector2 direction;
        float distance;

        if ( input_manager.AimMode == IInputManager.ManualAimMode.angle )
        {
            direction = new Vector2( input_manager.HorizontalAimAxis, input_manager.VerticalAimAxis );
            distance = MAX_RANGE;
        }
        else //if ( input_manager.AimMode == IInputManager.ManualAimMode.position )
        {
            Vector3 aim_position = Camera.main.ViewportToWorldPoint( new Vector3( input_manager.AimPosition.x / (float) Screen.width, input_manager.AimPosition.y / (float) Screen.height, 0.0f ) );
            direction = new Vector2( aim_position.x - origin.x, aim_position.y - origin.y );
            distance = Mathf.Min( direction.magnitude + 1.0f, MAX_RANGE );
        }

        RaycastHit2D hit = Physics2D.Raycast( origin, direction, distance, CollisionMasks.player_shooting_mask );
        if ( hit.collider == null ) { aim_auto_reticle_position = origin + distance * direction.normalized; }
        else
        {
            aim_auto_reticle_position = hit.point;
        }
        aim_reticle.transform.position = new Vector3( aim_auto_reticle_position.x, aim_auto_reticle_position.y, aim_reticle.transform.position.z );
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
    /// Determines if the player is in a state where they can fire their guns.
    /// </summary>
    /// <returns>True if the player can fire, false otherwise.</returns>
    private bool IsInShootState()
    {
        if ( char_stats.current_master_state == CharEnums.MasterState.DefaultState )
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Parses aerial inputs for air hanging and air dashes.
    /// </summary>
    private void AerialInput()
    {
        if ( char_stats.IsGrounded ) { return; }

        // Air hang
        if ( input_manager.JumpInputInst && char_stats.current_master_state == CharEnums.MasterState.DefaultState )
        {
            AirHang();
        }

        // Air dash
        if ( char_stats.current_master_state == CharEnums.MasterState.AirHang )
        {
            if ( Mathf.Abs( input_manager.HorizontalAxis ) > 0.1f || Mathf.Abs( input_manager.VerticalAxis ) > 0.1f )
            {
                AirDash( new Vector2( input_manager.HorizontalAxis, input_manager.VerticalAxis ) );
            }
        }
        // Allow queueing AirHangs and AirDashes from AirDashes
        else if ( char_stats.current_master_state == CharEnums.MasterState.AirDash )
        {
            if ( ! is_air_dash_enqueued )
            {
                if ( ( Mathf.Abs( input_manager.HorizontalAxis ) > 0.1f || Mathf.Abs( input_manager.VerticalAxis ) > 0.1f ) && input_manager.JumpInputInst )
                {
                    queued_air_dash_direction = new Vector2( input_manager.HorizontalAxis, input_manager.VerticalAxis );
                    is_air_dash_enqueued = true;
                }
            }

            if ( input_manager.JumpInputInst && Mathf.Abs( input_manager.HorizontalAxis ) < 0.1f )
            {
                is_air_hang_enqueued = true;
            }
        }

    }

    /// <summary>
    /// Locks the player in a state where they cannot respond to input.
    /// </summary>
    /// <param name="duration">The duration the frozen state should last, in seconds.</param>
    public void FreezePlayer( float duration )
    {
        if ( duration <= 0.0f ) { return; }
        input_manager.IgnoreInput = true;
        freeze_timer = Mathf.Max( duration, freeze_timer );
    }

    /// <summary>
    /// Unlocks the player from the frozen "no-input" state.
    /// </summary>
    public void UnfreezePlayer()
    {
        input_manager.IgnoreInput = false;
        freeze_timer = 0.0f;
    }

    /// <summary>
    /// Early initialization
    /// </summary>
    private void Awake()
    {
        aim_reticle = transform.Find( "Reticle" ).gameObject;

        #region overlay
        GameObject sprite_obj = transform.Find( "Sprites" ).gameObject;
        if ( sprite_obj != null )
        {
            GameObject overlay_obj = sprite_obj.transform.Find( "Overlay" ).gameObject;
            if ( overlay_obj != null )
            {
                overlay = overlay_obj.GetComponent<CharacterOverlay>();
            }
        }
        #if UNITY_EDITOR
        if ( overlay == null )
        {
            Debug.LogError( "Could not find the player overlay in the hierarchy." );
        }
        #endif
        #endregion
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

        #region Timers
        #region Shield
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
        if ( is_invincible )
        {
            //Debug.Log("Evade");
            invincibility_counter += Time.deltaTime * Time.timeScale;
            if ( is_evading && invincibility_counter >= INVINCIBILITY_TIME )
            {
                is_invincible = false;
                is_evade_recovering = true;
                char_stats.current_master_state = CharEnums.MasterState.DefaultState;

                // Start sprint out of dash? (cuts off recovery)
                if ( Mathf.Abs( input_manager.HorizontalAxis ) >= 0.1f && Mathf.Sign( input_manager.HorizontalAxis ) == char_stats.GetFacingXComponent() && ! evade_enqueued )
                {
                    SimpleCharacterCore char_core = GetComponent<SimpleCharacterCore>();
                    if ( char_core != null )
                    {
                        char_core.StartRunning();
                        char_core.FullSpeed();
                    }
                }
            }
            else if ( IsAirDashing && invincibility_counter >= INVINCIBILITY_TIME )
            {
                is_invincible = false;
            }
            else
            {
                is_invincible = false;
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

        if ( is_cloaked || is_evading || IsAirHanging || IsAirDashing || IsSprinting ) { is_energy_regenerating = false; }
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

        #region AirHang
        if ( char_stats.current_master_state == CharEnums.MasterState.AirHang )
        {
            air_hang_timer -= Time.deltaTime * Time.timeScale;
            // Can't split a frame, so timer imprecision here is OK.
            if ( air_hang_timer <= 0.0f )
            {
                char_stats.current_master_state = CharEnums.MasterState.DefaultState;
                char_anims.FallTrigger();
            }
        }
        #endregion

        #region AirDash
        if ( char_stats.current_master_state == CharEnums.MasterState.AirDash )
        {
            air_dash_timer -= Time.deltaTime * Time.timeScale;
            float scalar = AIR_DASH_SPEED * Time.deltaTime * Time.timeScale;
            // End-frame overshoot correction (amount of motion from going over intended time is removed).
            if ( air_dash_timer < 0.0f )
            {
                scalar += AIR_DASH_SPEED * air_dash_timer;
            }
            GetComponent<SimpleCharacterCore>().MoveWithCollision( new Vector3( scalar * Mathf.Cos( air_dash_angle * Mathf.Deg2Rad ), scalar * Mathf.Sin( air_dash_angle * Mathf.Deg2Rad ), 0.0f ) );
            if ( air_dash_timer <= 0.0f )
            {
                bool performed_another_dash = false;
                if ( is_air_dash_enqueued )
                {
                    performed_another_dash = AirDash( queued_air_dash_direction );
                }

                if ( ! performed_another_dash )
                {
                    char_stats.current_master_state = CharEnums.MasterState.DefaultState;
                    
                    if ( is_air_hang_enqueued )
                    {
                        AirHang();
                    }
                    else
                    {
                        char_anims.FallTrigger();
                    }
                }
            }

        }
        #endregion

        #region Freeze
        if ( freeze_timer > 0.0f )
        {
            freeze_timer = Mathf.Max( freeze_timer - Time.deltaTime * Time.timeScale, 0.0f );
            if ( freeze_timer <= 0.0f )
            {
                UnfreezePlayer();
            }
        }
        #endregion
        #endregion

        #region Shooting
        if ( is_shooting )
        {
            if ( acquired_charge_shot )
            {
                // Display charge up ball over player around shot origin point.
                if ( charge_up_ball != null )
                {
                    charge_up_ball.transform.parent.localPosition = new Vector3( GetFacingVector().x * ( char_stats.STANDING_COLLIDER_SIZE.x / 2.0f + 4.0f ), -2.0f, -1.0f );
                    SpriteRenderer sprite_renderer =  charge_up_ball.GetComponent<SpriteRenderer>();
                    if ( sprite_renderer != null )
                    {
                        if ( ! IsInShootState() )
                        {
                            sprite_renderer.enabled = false; // only disable this so we keep correct animation
                        }
                        else
                        {
                            sprite_renderer.enabled = true;
                        }
                    }
                }

                // Track whether fully charged or not.
                float prev_shoot_charge_timer = shoot_charge_timer;
                shoot_charge_timer = Mathf.Min( shoot_charge_timer + Time.deltaTime * Time.timeScale, SHOOT_FULL_CHARGE_TIME );
                if ( ( shoot_charge_timer >= SHOOT_FULL_CHARGE_TIME ) && prev_shoot_charge_timer < shoot_charge_timer )
                {
                    if ( overlay != null )
                    {
                        Color overlay_color;
                        //overlay_color = new Color( 0.0f, 0.70f, 0.35f ); // energy
                        overlay_color = new Color( 0.98f, 0.75f, 1.0f ); // bullet
                        overlay.ShowOverlay( overlay_color );
                    }
                    if ( charge_up_ball != null )
                    {
                        Animator charge_animator = charge_up_ball.GetComponent<Animator>();
                        if ( charge_animator != null )
                        {
                            charge_animator.SetBool( "Charged", true );
                        }
                    }
                }
            }
        }
        #endregion

        AutoTarget();

        AerialInput();

        // Idle check
        was_idle = is_idle;
        is_idle = char_anims.animator.GetCurrentAnimatorStateInfo( 0 ).IsName( "valerie_idle" );
    }

}
