public static class CharEnums
{
    // this is used throughout all the character scripts
    public enum MasterState { DefaultState, ClimbState, RappelState, AttackState, VaultState, EvadeState };

    // moveState is used by SimpleCharacterCore to determine which movement type is currently being used
    public enum MoveState { IsWalking, IsSneaking, IsRunning };

    // used by CharacterStats and SimpleCharacterCore
    public enum FacingDirection { Left, Right };
}
