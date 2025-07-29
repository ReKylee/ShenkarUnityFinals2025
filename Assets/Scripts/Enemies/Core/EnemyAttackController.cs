using Enemies.Interfaces;

namespace Enemies.Core
{
    // Handles all attack behaviors for an enemy
    public class EnemyAttackController : GenericCommandController<IAttackCommand>
    {
    }
}
