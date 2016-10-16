public static class CharEnums
{
    // this is used throughout all the character scripts
    public enum MasterState { defaultState, climbState, attackState };

    // moveState is used by simpleCharacterCore to determine which movement type is currently being used
    public enum MoveState { isWalking, isSneaking, isRunning };


}
