using System.Collections.Generic;
using Qonote.Infrastructure.Persistence.Context;

namespace Qonote.Infrastructure.Persistence.Operations.ChangeTracking;

public interface ISidebarImpactEvaluator
{
    HashSet<string> CollectAffectedUserIds(ApplicationDbContext context);
}


