namespace ModularCharacterController.Core.Abilities
{
    /// <summary>
    ///     Enum of all stats that abilities can modify
    /// </summary>
    public enum StatType
    {
        // Movement
        WalkSpeed,
        RunSpeed,
        GroundAcceleration,
        GroundDeceleration,
        AirAcceleration,
        AirDeceleration,

        // Jump
        JumpVelocity,
        JumpReleaseVelocityMultiplier,
        MaxFallSpeed,
        CoyoteTime,
        JumpBufferTime,

        // Fly
        FloatDescentSpeed,
        FlapImpulse,

        // Physics
        GravityScale,

        // Combat
        AttackDamage,
        AttackRange,
        AttackSpeed,

        // Other
        InhaleRange,
        InhalePower
    }
}
