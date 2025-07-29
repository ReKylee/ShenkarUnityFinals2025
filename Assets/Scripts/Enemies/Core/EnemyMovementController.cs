using Enemies.Interfaces;

namespace Enemies.Core
{
    // Handles all movement behaviors for an enemy
    public class EnemyMovementController : GenericCommandController<IMovementCommand>
    {
    }
}
