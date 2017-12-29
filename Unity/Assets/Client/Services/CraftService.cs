using UnityEngine;
using System.Collections;

public class CraftService : Service
{
    public bool AttemptEnterCraft(CraftOperator craftOperator, Craft craft, InputGroup inputGroup)
    {
        if (craft.IsOperable(craftOperator))
        {
            EnterCraft(craftOperator, craft, inputGroup);
            return true;
        }

        return false;
    }

    private void EnterCraft(CraftOperator craftOperator, Craft craft, InputGroup inputGroup)
    {
        
    }
}
